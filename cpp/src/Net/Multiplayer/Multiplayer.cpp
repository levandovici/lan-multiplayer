//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include <Michitai/Lan/Net/Multiplayer/Multiplayer.hpp>
#include <Michitai/Lan/Debugging/DebugConsole.hpp>

#include <chrono>


namespace Michitai::Lan::Net::Multiplayer
{
    // Type aliases — inside Multiplayer member functions the member names
    // (Server, Client, BroadcastServer, BroadcastClient) shadow the types.
    namespace
    {
        using ServerT          = Michitai::Lan::Net::Multiplayer::Server;
        using ClientT          = Michitai::Lan::Net::Multiplayer::Client;
        using BroadcastServerT = Michitai::Lan::Net::Multiplayer::BroadcastServer;
        using BroadcastClientT = Michitai::Lan::Net::Multiplayer::BroadcastClient;
    }

    void Multiplayer::SleepInterruptible(int milliseconds, const std::stop_token& token)
    {
        const auto deadline = std::chrono::steady_clock::now() + std::chrono::milliseconds(milliseconds);

        while (!token.stop_requested())
        {
            const auto remaining = deadline - std::chrono::steady_clock::now();
            if (remaining <= std::chrono::milliseconds(0))
                break;

            std::this_thread::sleep_for(
                std::min<std::chrono::steady_clock::duration>(remaining, std::chrono::milliseconds(10)));
        }
    }

    void Multiplayer::StartServer(EPlatform platform,
                                  const Data::ServerGameDataPtr& serverGameData,
                                  const ProcessMessageDelegate& processMessage,
                                  int receiveRequestsDelayMilliseconds)
    {
        (void)platform; // All platforms share the same socket implementation in the C++ port.

        if (IsServer())
            return;

        Server = ServerT::Create(Name, serverGameData, IpAddress, ServerPortRange);

        IpAddress = Server->IpEndPoint().Address;
        Port = Server->IpEndPoint().Port;

        BroadcastServer = std::make_shared<BroadcastServerT>(IpAddress, BroadcastPortRange);

        _broadcastServerStop = std::stop_source();
        const std::stop_token token = _broadcastServerStop.get_token();
        const BroadcastServerT::Ptr broadcastServer = BroadcastServer;

        _broadcastServerThread = std::thread(
            [token, processMessage, broadcastServer, receiveRequestsDelayMilliseconds]
        {
            while (!token.stop_requested())
            {
                // One receive+respond cycle per iteration; the 100 ms poll keeps
                // the loop responsive to Stop().
                broadcastServer->Broadcast(processMessage, 100);

                SleepInterruptible(receiveRequestsDelayMilliseconds, token);
            }
        });

        Server->OnState += [](const IPEndPoint& point, const Message& message)
        {
            OnState.Invoke(point, message);
        };

        OnStartServer.Invoke();
        Server->Start();
        OnServerStarted.Invoke();
    }

    void Multiplayer::StartClient()
    {
        if (IsClient())
            return;

        Client = ClientT::Create(IpAddress, Port);

        ClientCommands = std::queue<Commands::Command::Ptr>();

        Client->OnState += [](const IPEndPoint& point, const Message& message)
        {
            OnState.Invoke(point, message);
        };

        OnStartClient.Invoke();
        Client->Start();
        OnClientStarted.Invoke();
    }

    void Multiplayer::StopServer()
    {
        Debug::DebugConsole::LogWarning("[MULTIPLAYER] Cancelling BroadcastServerTask...");

        _broadcastServerStop.request_stop();
        if (_broadcastServerThread.joinable())
            _broadcastServerThread.join();

        Debug::DebugConsole::Log("[MULTIPLAYER] BroadcastServerTask canceled.");

        Debug::DebugConsole::LogWarning("[MULTIPLAYER] Stopping Server...");

        if (Server)
            Server->Stop();
        Server = nullptr;

        Debug::DebugConsole::Log("[MULTIPLAYER] Server stopped.");

        Debug::DebugConsole::LogWarning("[MULTIPLAYER] Stopping BroadcastServer...");

        if (BroadcastServer)
            BroadcastServer->Stop();
        BroadcastServer = nullptr;

        Debug::DebugConsole::Log("[MULTIPLAYER] BroadcastServer stopped.");
    }

    void Multiplayer::StopClient()
    {
        if (Client)
            Client->Stop();
        Client = nullptr;
    }

    void Multiplayer::Stop()
    {
        StopClient();
        StopServer();
    }

    void Multiplayer::StartBroadcastClient(EPlatform platform,
                                           const AppMessage& request,
                                           const OnReceiveResponseDelegate& onReceiveResponse,
                                           int receiveResponsesMilliseconds,
                                           int repeatAfterMilliseconds)
    {
        if (_broadcastClientThread.joinable())
            return;

        _broadcastClientStop = std::stop_source();
        const std::stop_token token = _broadcastClientStop.get_token();

        BroadcastClient = std::make_shared<BroadcastClientT>(IpAddress, BroadcastPortRange);
        const BroadcastClientT::Ptr broadcastClient = BroadcastClient;

        _broadcastClientThread = std::thread(
            [token, broadcastClient, platform, request, onReceiveResponse,
             receiveResponsesMilliseconds, repeatAfterMilliseconds]
        {
            while (!token.stop_requested())
            {
                SetBroadcastClientLocating(true);

                std::vector<IPAddress> masks;
                const bool success = Lan::TryGetLocalIPv4Masks(platform, masks);

                // Send the discovery probe to every subnet mask x every broadcast port.
                if (success)
                {
                    for (const IPAddress& mask : masks)
                    {
                        for (int port = BroadcastPortRange.First; port <= BroadcastPortRange.Last; port++)
                        {
                            try { broadcastClient->Socket().Send(mask, port, request); }
                            catch (...) {}
                        }
                    }
                }
                else
                {
                    for (int port = BroadcastPortRange.First; port <= BroadcastPortRange.Last; port++)
                    {
                        try { broadcastClient->Socket().Send(IPAddress::Broadcast(), port, request); }
                        catch (...) {}
                    }
                }

                // Collect responses for the configured window.
                broadcastClient->CollectResponses(onReceiveResponse, receiveResponsesMilliseconds, token);

                SetBroadcastClientLocating(false);

                if (token.stop_requested())
                    break;

                SleepInterruptible(repeatAfterMilliseconds, token);
            }
        });
    }

    void Multiplayer::StopBroadcastClient()
    {
        _broadcastClientStop.request_stop();
        if (_broadcastClientThread.joinable())
            _broadcastClientThread.join();

        if (BroadcastClient)
            BroadcastClient->Stop();
        BroadcastClient = nullptr;

        SetBroadcastClientLocating(false);
    }
}
