//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <atomic>
#include <queue>
#include <stop_token>
#include <string>
#include <thread>

#include <Michitai/Lan/EPlatform.hpp>
#include <Michitai/Lan/Events.hpp>
#include <Michitai/Lan/Net/EndPoint.hpp>
#include <Michitai/Lan/Net/Messages.hpp>
#include <Michitai/Lan/Net/PortRange.hpp>
#include <Michitai/Lan/Net/Lan.hpp>
#include <Michitai/Lan/Net/UDPChannel.hpp>
#include <Michitai/Lan/Net/Multiplayer/Broadcast.hpp>
#include <Michitai/Lan/Net/Multiplayer/Client.hpp>
#include <Michitai/Lan/Net/Multiplayer/Commands.hpp>
#include <Michitai/Lan/Net/Multiplayer/Data.hpp>
#include <Michitai/Lan/Net/Multiplayer/Server.hpp>


namespace Michitai::Lan::Net::Multiplayer
{
    /// <summary>
    /// Static class providing centralized multiplayer game management and configuration.
    /// </summary>
    class Multiplayer
    {
    public:
        // Aliases declared before the members that shadow their type names.
        using ProcessMessageDelegate = BroadcastServer::ProcessMessageDelegate;
        using OnReceiveResponseDelegate = BroadcastClient::OnReceiveResponseDelegate;

        /// <summary>The name of the multiplayer game.</summary>
        static inline std::string Name = "New Multiplayer Game";

        /// <summary>The IP address to which the server or/and client will connect.</summary>
        static inline IPAddress IpAddress = IPAddress::Any();

        /// <summary>The port to which the client will connect.</summary>
        static inline int Port = 50000;

        /// <summary>Port range. One of them will be used by the server. Default 50000-50128.</summary>
        static inline PortRange ServerPortRange{50000, 50128};

        /// <summary>Port range used by Server and Client discovery messages. Default 60000-60128.</summary>
        static inline PortRange BroadcastPortRange{60000, 60128};

        /// <summary>The multiplayer server instance.</summary>
        static inline Server::Ptr Server;

        /// <summary>The multiplayer client instance.</summary>
        static inline Client::Ptr Client;

        /// <summary>The broadcast server instance.</summary>
        static inline BroadcastServer::Ptr BroadcastServer;

        /// <summary>The broadcast client instance.</summary>
        static inline BroadcastClient::Ptr BroadcastClient;

        /// <summary>Gets whether the broadcast client is locating responses from the server.</summary>
        static bool BroadcastClientLocating() { return _broadcastClientLocating.load(); }

        /// <summary>Queued client commands.</summary>
        static inline std::queue<Commands::Command::Ptr> ClientCommands;

        /// <summary>Number of requests that can be sent at the same time.</summary>
        static inline int ClientOnceMaxCommands = 4;

        /// <summary>
        /// Gets the active unreliable UDP data channel (server-side or client-side).
        /// Bound to the same port as the TCP server, so discovered ServerInfo
        /// endpoints already carry the right address for state sync.
        /// </summary>
        static UDPChannel::Ptr DataChannel()
        {
            return IsServer()
                ? (Server ? Server->DataChannel() : nullptr)
                : (Client ? Client->DataChannel() : nullptr);
        }

        /// <summary>
        /// Event raised when a state datagram is received over the UDP data channel.
        /// On the server, the endpoint identifies the sending client; on the client it is the server.
        /// </summary>
        static inline Event<const IPEndPoint&, const Message&> OnState;

        static bool IsServer() { return Server != nullptr; }
        static bool IsClient() { return Client != nullptr; }

        static inline Event<> OnStartServer;
        static inline Event<> OnStartClient;
        static inline Event<> OnClientStarted;
        static inline Event<> OnServerStarted;

        /// <summary>
        /// Starts the game server and the broadcast responder that answers discovery probes.
        /// </summary>
        static void StartServer(EPlatform platform,
                                const Data::ServerGameDataPtr& serverGameData,
                                const ProcessMessageDelegate& processMessage,
                                int receiveRequestsDelayMilliseconds = 100);

        /// <summary>Starts the game client.</summary>
        static void StartClient();

        static void StopServer();
        static void StopClient();
        static void Stop();

        /// <summary>
        /// Sends a state datagram to the server over the unreliable UDP data channel.
        /// Best for 30-60Hz synchronization — no head-of-line blocking, latest-wins.
        /// </summary>
        static void SendState(const Message& message)
        {
            if (Client)
                Client->SendState(message);
        }

        /// <summary>Broadcasts a state datagram to all clients that have sent state to this server.</summary>
        static void BroadcastState(const Message& message)
        {
            if (Server)
                Server->BroadcastState(message);
        }

        /// <summary>Sends a state datagram to a specific client endpoint over the UDP data channel.</summary>
        static void SendState(const IPEndPoint& target, const Message& message)
        {
            if (Server)
                Server->SendState(target, message);
        }

        /// <summary>Clears all lifecycle and state events.</summary>
        static void ClearEvents()
        {
            OnStartServer = nullptr;
            OnServerStarted = nullptr;
            OnStartClient = nullptr;
            OnClientStarted = nullptr;
            OnState = nullptr;
        }

        /// <summary>
        /// Starts broadcasting discovery requests on the LAN and collecting server responses.
        /// </summary>
        static void StartBroadcastClient(EPlatform platform,
                                         const AppMessage& request,
                                         const OnReceiveResponseDelegate& onReceiveResponse,
                                         int receiveResponsesMilliseconds = 5000,
                                         int repeatAfterMilliseconds = 5000);

        /// <summary>Stops the broadcast client.</summary>
        static void StopBroadcastClient();

    private:
        static void SetBroadcastClientLocating(bool value) { _broadcastClientLocating.store(value); }

        /// <summary>Sleeps in small slices so the wait stays responsive to cancellation.</summary>
        static void SleepInterruptible(int milliseconds, const std::stop_token& token);

        static inline std::atomic<bool> _broadcastClientLocating{false};

        static inline std::stop_source _broadcastServerStop;
        static inline std::thread _broadcastServerThread;

        static inline std::stop_source _broadcastClientStop;
        static inline std::thread _broadcastClientThread;
    };
}
