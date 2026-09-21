//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <memory>
#include <mutex>
#include <string>
#include <vector>

#include <Michitai/Lan/Events.hpp>
#include <Michitai/Lan/Data/PlayerData.hpp>
#include <Michitai/Lan/Net/EndPoint.hpp>
#include <Michitai/Lan/Net/Messages.hpp>
#include <Michitai/Lan/Net/PortRange.hpp>
#include <Michitai/Lan/Net/TCPServer.hpp>
#include <Michitai/Lan/Net/UDPChannel.hpp>
#include <Michitai/Lan/Net/Multiplayer/Data.hpp>


namespace Michitai::Lan::Net::Multiplayer
{
    /// <summary>
    /// Multiplayer server for managing client connections and game state synchronization.
    /// Includes an unreliable UDP data channel for high-frequency state sync.
    /// Owned via std::shared_ptr — create with Server::Create().
    /// </summary>
    class Server : public std::enable_shared_from_this<Server>
    {
    public:
        using Ptr = std::shared_ptr<Server>;

        ~Server();

        Server(const Server&) = delete;
        Server& operator=(const Server&) = delete;

        /// <summary>Event raised when a client connects.</summary>
        Event<const std::string&> OnClientConnected;

        /// <summary>Event raised when a client disconnects.</summary>
        Event<const std::string&> OnClientDisconnected;

        /// <summary>Event raised when a request message is received from a client.</summary>
        Event<const IdentifiedMessage&> OnRequest;

        /// <summary>Event raised when a state datagram is received over the UDP data channel.</summary>
        Event<const IPEndPoint&, const Message&> OnState;

        /// <summary>Event raised when the server disconnects.</summary>
        Event<> OnDisconnected;

        /// <summary>Gets the server name.</summary>
        const std::string& Name() const { return _name; }

        /// <summary>Gets the public server data.</summary>
        Data::ServerGameDataPtr PublicServerData() const { return _serverData->Public(); }

        /// <summary>Gets the private server data.</summary>
        Data::ServerGameDataPtr PrivateServerData() const { return _serverData; }

        /// <summary>Gets the IP endpoint of the server.</summary>
        const IPEndPoint& IpEndPoint() const;

        /// <summary>
        /// Gets the UDP data channel. Bound to the same port as the TCP server
        /// (UDP and TCP port spaces are independent), so discovered ServerInfo
        /// already tells clients where to send state.
        /// </summary>
        UDPChannel::Ptr DataChannel() const { return _dataChannel; }


        /// <summary>Creates a server for the specified name, data, IP and port.</summary>
        static Ptr Create(const std::string& name, const Data::ServerGameDataPtr& serverGameData,
                          const IPAddress& ip, int port, int bufferSize = 4096)
        {
            return Create(name, serverGameData, IPEndPoint(ip, port), bufferSize);
        }

        /// <summary>Creates a server for the specified name, data and endpoint.</summary>
        static Ptr Create(const std::string& name, const Data::ServerGameDataPtr& serverGameData,
                          const IPEndPoint& endPoint, int bufferSize = 4096)
        {
            Ptr server = Ptr(new Server(name, serverGameData, endPoint, bufferSize));
            server->Initialize();
            return server;
        }

        /// <summary>Creates a server bound to a free port from the specified range.</summary>
        static Ptr Create(const std::string& name, const Data::ServerGameDataPtr& serverGameData,
                          const IPAddress& ip, const PortRange& range, int bufferSize = 4096);


        void Start();
        void Stop();

        /// <summary>Sends a state datagram to a specific client endpoint over the UDP data channel.</summary>
        void SendState(const IPEndPoint& target, const Message& message);

        /// <summary>Broadcasts a state datagram to all clients that have sent state to this server.</summary>
        void BroadcastState(const Message& message);

        /// <summary>Sends a response message to a specific client.</summary>
        void Response(const IdentifiedMessage& identifiedMessage);

        /// <summary>Logs in a player with the specified credentials.</summary>
        void LogInPlayer(const std::string& id, const Data::Credentials& credentials);

        /// <summary>Disconnects a client by ID and removes the player.</summary>
        void Disconnect(const std::string& id);

        /// <summary>Registers a new player and returns fresh credentials.</summary>
        Data::Credentials RegisterNewPlayer(const Michitai::Lan::Data::PlayerGameData& data);

        /// <summary>Checks if a player with the specified credentials exists.</summary>
        bool Contains(const Data::Credentials& player) const;

        /// <summary>Tries to get the public data of a logged-in player.</summary>
        bool TryGetLoggedInPlayerPublicData(const std::string& id, Data::ServerClientGameData& serverClientGameData);

        /// <summary>Tries to get the private data of a logged-in player.</summary>
        bool TryGetLoggedInPlayerPrivateData(const std::string& id, Data::ServerClientGameData& serverClientGameData);


        /// <summary>Represents a logged-in client (ID + credentials pair).</summary>
        class ServerClient
        {
        public:
            std::string ID;
            Data::Credentials Credentials;

            ServerClient(std::string id, Data::Credentials credentials)
                : ID(std::move(id)), Credentials(std::move(credentials))
            {
            }
        };


        /// <summary>Thread-safe registry of logged-in clients.</summary>
        class ServerClients
        {
        public:
            ServerClients() = default;

            void LogIn(const ServerClient& player)
            {
                std::lock_guard lock(_mutex);
                _clients.push_back(player);
            }

            void LogOut(const std::string& id)
            {
                std::lock_guard lock(_mutex);
                std::erase_if(_clients, [&id](const ServerClient& client)
                {
                    return client.ID == id;
                });
            }

            bool TryGetPlayer(const std::string& id, ServerClient& serverClient) const
            {
                std::lock_guard lock(_mutex);
                for (const ServerClient& client : _clients)
                {
                    if (client.ID == id)
                    {
                        serverClient = client;
                        return true;
                    }
                }
                return false;
            }

        private:
            std::vector<ServerClient> _clients;
            mutable std::mutex _mutex;
        };


    private:
        Server(std::string name, Data::ServerGameDataPtr serverGameData,
               const IPEndPoint& endPoint, int bufferSize);

        Server(std::string name, Data::ServerGameDataPtr serverGameData,
               const IPAddress& ip, const PortRange& range, int bufferSize);

        /// <summary>Wires TCPServer events (needs shared_ptr — runs after construction).</summary>
        void Initialize();

        std::string _name;
        TCPServer::Ptr _server;
        UDPChannel::Ptr _dataChannel;
        ServerClients _clients;
        Data::ServerGameDataPtr _serverData;
    };
}
