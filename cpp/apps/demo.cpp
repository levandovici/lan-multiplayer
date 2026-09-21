//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Loopback smoke test for the Michitai.Lan C++ port. Exercises:
//    - frame encoding + streaming FrameBuffer with partial reads
//    - GZip compression round-trip
//    - JSON / command / data model round-trips
//    - the full Multiplayer facade: TCP request/response + UDP state sync
//    - a BroadcastServer/BroadcastClient pair over loopback unicast
//  Exit code is the number of failed checks (0 = all passed).
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include <atomic>
#include <chrono>
#include <future>
#include <iostream>
#include <string>
#include <thread>
#include <vector>

#include <Michitai/Lan.hpp>


// Namespace aliases (the C# names `Multiplayer` and `Data` exist as both
// namespaces and class/namespace members — blanket using-directives collide).
namespace Ml  = Michitai::Lan;
namespace Net = Michitai::Lan::Net;
namespace Mp  = Michitai::Lan::Net::Multiplayer;

namespace
{
    int g_failures = 0;

    void Check(bool ok, const std::string& name)
    {
        std::cout << (ok ? "[PASS] " : "[FAIL] ") << name << '\n';
        if (!ok)
            ++g_failures;
    }

    bool WaitFor(const std::atomic<bool>& flag, int timeoutMs = 5000)
    {
        const auto deadline = std::chrono::steady_clock::now() + std::chrono::milliseconds(timeoutMs);
        while (!flag.load() && std::chrono::steady_clock::now() < deadline)
            std::this_thread::sleep_for(std::chrono::milliseconds(5));
        return flag.load();
    }

    constexpr Ml::EPlatform CurrentPlatform()
    {
#if defined(_WIN32)
        return Ml::EPlatform::Windows;
#elif defined(__APPLE__)
        return Ml::EPlatform::MacOS;
#else
        return Ml::EPlatform::Linux;
#endif
    }


    //------------------------------------------------------------------------------------------------------------------------------------------------------------
    //  Codec-level checks (no sockets).
    //------------------------------------------------------------------------------------------------------------------------------------------------------------

    void TestFrame()
    {
        const std::string text = "hello frame";
        const std::vector<std::uint8_t> frame = Net::Frame::Pack(text);

        Check(frame.size() == Net::Frame::HeaderSize + text.size(), "frame size = header + payload");

        // Feed the frame through a FrameBuffer in tiny pieces to exercise partial reads.
        Net::FrameBuffer buffer;
        std::vector<std::uint8_t> payload;
        std::uint8_t flags = 0;

        Check(!buffer.TryRead(payload, flags), "empty buffer yields no frame");

        for (std::uint8_t byte : frame)
        {
            buffer.Write(&byte, 0, 1);
        }

        Check(buffer.BufferedCount() == static_cast<int>(frame.size()), "buffered byte count");
        Check(buffer.TryRead(payload, flags), "frame extracted after partial feeds");
        Check(flags == Net::Frame::FlagNone, "small payload stays uncompressed");
        Check(Net::Frame::Unpack(payload, flags) == text, "frame round-trip");

        // Two frames in a single write must come back in order.
        buffer.Clear();
        const std::vector<std::uint8_t> a = Net::Frame::Pack("first");
        const std::vector<std::uint8_t> b = Net::Frame::Pack("second");
        std::vector<std::uint8_t> both(a.begin(), a.end());
        both.insert(both.end(), b.begin(), b.end());
        buffer.Write(both.data(), 0, static_cast<int>(both.size()));

        std::vector<std::uint8_t> p1, p2;
        std::uint8_t f1 = 0, f2 = 0;
        const bool two = buffer.TryRead(p1, f1) && buffer.TryRead(p2, f2);
        Check(two && Net::Frame::Unpack(p1, f1) == "first" && Net::Frame::Unpack(p2, f2) == "second",
              "two frames per read come back in order");
    }

    void TestCompression()
    {
        // Highly compressible payload well above the threshold.
        const std::string big(4096, 'x');
        const std::vector<std::uint8_t> raw(big.begin(), big.end());

        std::vector<std::uint8_t> compressed;
        Check(Net::Compressor::TryCompress(raw, compressed) && compressed.size() < raw.size(),
              "TryCompress shrinks a large repetitive payload");

        const std::vector<std::uint8_t> round = Net::Compressor::Decompress(compressed);
        Check(round == raw, "gzip round-trip");

        const std::vector<std::uint8_t> bigFrame = Net::Frame::Pack(big);
        Check((bigFrame[4] & Net::Frame::FlagCompressed) != 0, "large payload frame is flagged compressed");

        std::vector<std::uint8_t> payload;
        std::uint8_t flags = 0;
        Net::FrameBuffer buffer;
        buffer.Write(bigFrame.data(), 0, static_cast<int>(bigFrame.size()));
        Check(buffer.TryRead(payload, flags) && Net::Frame::Unpack(payload, flags) == big,
              "compressed frame round-trip");
    }

    void TestRejections()
    {
        // A frame header announcing more than MaxPayloadSize must be rejected as corrupt.
        std::vector<std::uint8_t> evil = {0xFF, 0xFF, 0xFF, 0xFF, 0x00};
        Net::FrameBuffer buffer;
        buffer.Write(evil.data(), 0, static_cast<int>(evil.size()));

        std::vector<std::uint8_t> payload;
        std::uint8_t flags = 0;
        bool threw = false;
        try { buffer.TryRead(payload, flags); }
        catch (const std::exception&) { threw = true; }
        Check(threw, "oversized frame rejected");

        // A corrupted gzip stream must fail (inflate error or CRC mismatch).
        std::vector<std::uint8_t> corrupted = Net::Compressor::Compress(std::vector<std::uint8_t>(1024, 'y'));
        corrupted[15] ^= 0xFF;
        threw = false;
        try { Net::Compressor::Decompress(corrupted); }
        catch (const std::exception&) { threw = true; }
        Check(threw, "corrupt gzip payload rejected");
    }

    void TestJsonAndCommands()
    {
        const Net::AppMessage message(1, "Demo", "payload");
        const Net::AppMessage back =
            Net::AppMessage::FromJson(Ml::JsonValue::Parse(message.ToJson().Dump()));
        Check(back.Version == 1 && back.Name == "Demo" && back.Message == "payload",
              "AppMessage JSON round-trip");

        Mp::Commands::Terminal::Ptr terminal =
            Mp::Commands::Terminal::New("say")->Arg("hello")->Next("teleport")->Arg("1 2 3");

        const auto commands = terminal->Commands();
        Check(commands.size() == 2, "terminal holds two commands");
        Check(commands[0]->Arguments().size() == 2 && commands[0]->Arguments()[0] == "/say",
              "command arguments");
        Check(*commands[0] == *Mp::Commands::Command::New("say"), "command equality by name");

        const auto parsed = Mp::Commands::Terminal::FromJson(Ml::JsonValue::Parse(terminal->ToJson().Dump()));
        Check(parsed.Commands().size() == 2, "terminal JSON round-trip");

        const Mp::Data::Credentials credentials("player-1", "secret");
        const Mp::Data::Credentials credentialsBack =
            Mp::Data::Credentials::FromJson(Ml::JsonValue::Parse(credentials.ToJson().Dump()));
        Check(credentialsBack == credentials, "credentials JSON round-trip");
        Check(credentials.Public().Password.empty() && credentials.Public().ID == credentials.ID,
              "public credentials hide the password");
    }


    //------------------------------------------------------------------------------------------------------------------------------------------------------------
    //  Socket-level checks on loopback.
    //------------------------------------------------------------------------------------------------------------------------------------------------------------

    void TestMultiplayerFacade()
    {
        Mp::Multiplayer::IpAddress = Net::IPAddress::Loopback();
        Mp::Multiplayer::Name = "LanDemo";
        Mp::Multiplayer::ServerPortRange = Net::PortRange(51000, 51128);
        Mp::Multiplayer::BroadcastPortRange = Net::PortRange(61000, 61128);

        auto serverData = std::make_shared<Mp::Data::ServerGameData>("demo-server");

        Mp::Multiplayer::StartServer(CurrentPlatform(), serverData,
            [](const Net::LocatedMessage& probe)
            {
                return Net::AppMessage(1, "LanDemo", "server-info");
            });

        Check(Mp::Multiplayer::IsServer(), "facade: server started on " +
              Mp::Multiplayer::IpAddress.ToString() + ":" + std::to_string(Mp::Multiplayer::Port));

        // Echo handler on the real server instance. The small delay widens the
        // in-flight window so the queued-request check below actually overlaps.
        Mp::Multiplayer::Server->OnRequest += [](const Net::IdentifiedMessage& request)
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(30));
            Mp::Multiplayer::Server->Response(
                Net::IdentifiedMessage(Net::Message("echo:" + request.Message.GetMessage()), request.ID));
        };

        // UDP state: the same static event fires for server-side and client-side
        // datagrams in this single-process demo — distinguish by source port.
        std::atomic<bool> serverGotState{false};
        std::atomic<bool> clientGotState{false};
        Mp::Multiplayer::OnState += [&](const Net::IPEndPoint& from, const Net::Message& state)
        {
            if (from.Port == Mp::Multiplayer::Port)
            {
                clientGotState = true;
            }
            else
            {
                serverGotState = true;
                Mp::Multiplayer::BroadcastState(Net::Message("state-echo:" + state.GetMessage()));
            }
        };

        Mp::Multiplayer::StartClient();
        Check(Mp::Multiplayer::IsClient() && !Mp::Multiplayer::Client->IsClosed(),
              "facade: client connected");

        // TCP request/response — all replies go through one synchronized vector
        // so queued-request ordering can be verified too.
        std::mutex responsesMutex;
        std::vector<std::string> responses;
        Mp::Multiplayer::Client->OnResponse += [&](const Net::Message& message)
        {
            std::lock_guard lock(responsesMutex);
            responses.push_back(message.GetMessage());
        };

        auto waitResponses = [&](int count) -> std::vector<std::string>
        {
            const auto deadline = std::chrono::steady_clock::now() + std::chrono::seconds(5);
            while (std::chrono::steady_clock::now() < deadline)
            {
                {
                    std::lock_guard lock(responsesMutex);
                    if (static_cast<int>(responses.size()) >= count)
                        return responses;
                }
                std::this_thread::sleep_for(std::chrono::milliseconds(5));
            }
            std::lock_guard lock(responsesMutex);
            return responses;
        };

        Mp::Multiplayer::Client->Request(Net::Message("ping"));
        Check(waitResponses(1).size() == 1 && responses[0] == "echo:ping",
              "facade: TCP request/response round-trip");

        // Three rapid requests: the second and third must queue behind the
        // in-flight one and still be answered in order (the echo handler's
        // 30 ms delay forces overlap).
        Mp::Multiplayer::Client->Request(Net::Message("q1"));
        Mp::Multiplayer::Client->Request(Net::Message("q2"));
        Mp::Multiplayer::Client->Request(Net::Message("q3"));

        const auto queued = waitResponses(4);
        Check(queued.size() == 4 && queued[1] == "echo:q1" && queued[2] == "echo:q2" &&
              queued[3] == "echo:q3",
              "facade: queued requests answered in order");

        // A payload above the compression threshold travels compressed over the wire.
        const std::string bigPayload(65536, 'z');
        Mp::Multiplayer::Client->Request(Net::Message(bigPayload));
        const auto big = waitResponses(5);
        Check(big.size() == 5 && big[4] == "echo:" + bigPayload,
              "facade: 64KB payload round-trip (compressed)");

        // UDP state channel both directions.
        Mp::Multiplayer::SendState(Net::Message("pos:1,2,3"));
        Check(WaitFor(serverGotState), "facade: server received UDP state");
        Check(WaitFor(clientGotState), "facade: client received broadcast UDP state");

        Mp::Multiplayer::Stop();
        Check(!Mp::Multiplayer::IsServer() && !Mp::Multiplayer::IsClient(), "facade: clean shutdown");
    }

    void TestBroadcastPair()
    {
        // Real subnet broadcast needs a non-loopback NIC; over loopback the same
        // code path is exercised with unicast datagrams.
        const int port = Net::PortRange(62000, 62128).RangeStore().RandomPort();

        Mp::BroadcastServer server(Net::IPAddress::Loopback(), port);
        Mp::BroadcastClient client(Net::IPAddress::Loopback(), 0);

        std::atomic<bool> responded{false};
        std::thread responder([&]
        {
            responded = server.Broadcast(
                [](const Net::LocatedMessage& probe)
                {
                    return Net::AppMessage(1, "LanDemo", "{\"Name\":\"Demo Server\"}");
                },
                4000);
        });

        const Net::LocatedMessage response =
            client.BroadcastRequest(Net::IPAddress::Loopback(), port,
                                    Net::AppMessage(1, "probe", ""), 4000);

        responder.join();

        Check(responded.load(), "broadcast server processed a probe");
        Check(response.Message.has_value() && response.Message->Name == "LanDemo",
              "broadcast client received the server info");

        server.Stop();
        client.Stop();
    }


    //------------------------------------------------------------------------------------------------------------------------------------------------------------
    //  Interop modes — speak the wire protocol with the C# build (see cpp/tests/interop.cs).
    //    lan-demo server <port> [seconds]  — TCP echo + UDP state echo on loopback
    //    lan-demo client <ip> <port>       — send one TCP request + one UDP state, print replies
    //------------------------------------------------------------------------------------------------------------------------------------------------------------

    int RunServer(int port, int seconds)
    {
        const Net::IPEndPoint point(Net::IPAddress::Loopback(), port);

        auto server  = Net::TCPServer::Create(point);
        auto channel = Net::UDPChannel::Create(Net::IPAddress::Loopback(), port);

        server->OnRequest += [&server](const Net::IdentifiedMessage& request)
        {
            server->Response(Net::IdentifiedMessage(
                Net::Message("echo:" + request.Message.GetMessage()), request.ID));
        };

        channel->OnReceive += [&channel](const Net::IPEndPoint& from, const Net::Message& message)
        {
            channel->Send(from, Net::Message("state-echo:" + message.GetMessage()));
        };

        server->Start();
        channel->Start();

        std::cout << "READY " << port << '\n' << std::flush;
        std::this_thread::sleep_for(std::chrono::seconds(seconds));

        channel->Stop();
        server->Stop();
        return 0;
    }

    int RunClient(const std::string& ip, int port)
    {
        const Net::IPAddress address = Net::IPAddress::Parse(ip);

        auto client  = Net::TCPClient::Create(address, port);
        auto channel = Net::UDPChannel::Create(Net::IPAddress::Loopback(), 0);

        std::promise<std::string> responsePromise;
        std::promise<std::string> statePromise;
        std::atomic<bool> responseDone{false};
        std::atomic<bool> stateDone{false};

        client->OnResponse += [&](const Net::Message& message)
        {
            if (!responseDone.exchange(true))
                responsePromise.set_value(message.GetMessage());
        };

        channel->OnReceive += [&](const Net::IPEndPoint& from, const Net::Message& message)
        {
            if (!stateDone.exchange(true))
                statePromise.set_value(message.GetMessage());
        };

        client->Start();
        channel->Start();

        client->Request(Net::Message("ping-from-cpp"));

        std::future<std::string> response = responsePromise.get_future();
        if (response.wait_for(std::chrono::seconds(5)) != std::future_status::ready)
        {
            std::cout << "FAIL tcp timeout\n";
            return 1;
        }
        std::cout << "tcp-response: " << response.get() << '\n';

        channel->Send(Net::IPEndPoint(address, port), Net::Message("state-from-cpp"));

        std::future<std::string> state = statePromise.get_future();
        if (state.wait_for(std::chrono::seconds(5)) != std::future_status::ready)
        {
            std::cout << "FAIL udp timeout\n";
            return 1;
        }
        std::cout << "udp-response: " << state.get() << '\n';

        channel->Stop();
        client->Stop();

        std::cout << "CLIENT-DONE\n";
        return 0;
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------------------
    //  Discovery mode — exercises the real LAN broadcast path:
    //  StartBroadcastClient probes LocalIPv4Masks x BroadcastPortRange and the
    //  facade's broadcast responder answers over UDP.
    //    lan-demo discover <seconds>
    //------------------------------------------------------------------------------------------------------------------------------------------------------------

    int RunDiscover(int seconds)
    {
        const std::vector<Net::IPAddress> addresses =
            Net::Lan::LocalIPv4Addresses(CurrentPlatform());

        if (addresses.empty())
        {
            std::cout << "SKIP: no non-loopback IPv4 addresses\n";
            return 2;
        }

        const std::vector<Net::IPAddress> masks = Net::Lan::LocalIPv4Masks(CurrentPlatform());
        std::cout << "local IPv4: " << addresses[0].ToString()
                  << "  broadcast masks:";
        for (const Net::IPAddress& mask : masks)
            std::cout << ' ' << mask.ToString();
        std::cout << '\n';

        Mp::Multiplayer::IpAddress = addresses[0];
        Mp::Multiplayer::Name = "LanDemo-Discovery";

        auto serverData = std::make_shared<Mp::Data::ServerGameData>("discover-demo");

        Mp::Multiplayer::StartServer(CurrentPlatform(), serverData,
            [](const Net::LocatedMessage& probe)
            {
                return Net::AppMessage(1, "LanDemo", "server-info");
            });

        std::atomic<bool> found{false};
        Mp::Multiplayer::StartBroadcastClient(CurrentPlatform(),
            Net::AppMessage(1, "probe", ""),
            [&](const Net::LocatedMessage& response)
            {
                if (response.IPEndPoint && response.Message)
                {
                    std::cout << "discovered: " << response.IPEndPoint->ToString()
                              << " -> " << response.Message->ToJson().Dump() << '\n';
                    found = true;
                }
            },
            seconds * 1000,  // collect window
            seconds * 1000); // then repeat (irrelevant — we stop after one window)

        const auto deadline = std::chrono::steady_clock::now() +
                              std::chrono::milliseconds(seconds * 1000 + 500);
        while (!found.load() && std::chrono::steady_clock::now() < deadline)
            std::this_thread::sleep_for(std::chrono::milliseconds(50));

        Mp::Multiplayer::StopBroadcastClient();
        Mp::Multiplayer::Stop();

        std::cout << (found.load() ? "DISCOVER-PASS" : "DISCOVER-FAIL") << '\n';
        return found.load() ? 0 : 1;
    }
}


int main(int argc, char** argv)
{
    Ml::Debug::DebugConsole::Enabled = true;
    Ml::Debug::DebugConsole::OnLogError += [](const std::string& line)
    {
        std::cout << "[LOG-ERROR] " << line << '\n';
    };

    // Interop modes.
    if (argc >= 3 && std::string(argv[1]) == "server")
    {
        return RunServer(std::stoi(argv[2]), argc >= 4 ? std::stoi(argv[3]) : 15);
    }
    if (argc >= 4 && std::string(argv[1]) == "client")
    {
        return RunClient(argv[2], std::stoi(argv[3]));
    }
    if (argc >= 2 && std::string(argv[1]) == "discover")
    {
        return RunDiscover(argc >= 3 ? std::stoi(argv[2]) : 8);
    }

    std::cout << "=== Michitai.Lan C++ smoke test ===\n\n";

    try
    {
        TestFrame();
        TestCompression();
        TestRejections();
        TestJsonAndCommands();
        TestBroadcastPair();
        TestMultiplayerFacade();
    }
    catch (const std::exception& e)
    {
        std::cout << "[FAIL] unhandled exception: " << e.what() << '\n';
        ++g_failures;
    }

    std::cout << "\n=== " << (g_failures == 0 ? "ALL CHECKS PASSED"
                                               : std::to_string(g_failures) + " CHECK(S) FAILED")
              << " ===\n";

    return g_failures;
}
