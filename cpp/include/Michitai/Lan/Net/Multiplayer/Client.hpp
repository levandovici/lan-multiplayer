//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <atomic>
#include <memory>
#include <mutex>
#include <optional>
#include <queue>
#include <string>

#include <Michitai/Lan/Events.hpp>
#include <Michitai/Lan/Data/PlayerData.hpp>
#include <Michitai/Lan/Net/EndPoint.hpp>
#include <Michitai/Lan/Net/Messages.hpp>
#include <Michitai/Lan/Net/TCPClient.hpp>
#include <Michitai/Lan/Net/UDPChannel.hpp>
#include <Michitai/Lan/Net/Multiplayer/Data.hpp>


namespace Michitai::Lan::Net::Multiplayer
{
    /// <summary>
    /// Multiplayer client for connecting to and communicating with a multiplayer server.
    /// Requests that arrive while awaiting a response are queued instead of dropped.
    /// Includes an unreliable UDP channel for high-frequency state synchronization.
    /// Owned via std::shared_ptr — create with Client::Create().
    /// </summary>
    class Client : public std::enable_shared_from_this<Client>
    {
    public:
        using Ptr = std::shared_ptr<Client>;

        ~Client();

        Client(const Client&) = delete;
        Client& operator=(const Client&) = delete;

        /// <summary>Event raised when a response message is received.</summary>
        Event<const Message&> OnResponse;

        /// <summary>Event raised when the client is disconnected.</summary>
        Event<> OnDisconnected;

        /// <summary>Event raised when a state datagram is received over the UDP data channel.</summary>
        Event<const IPEndPoint&, const Message&> OnState;

        /// <summary>Gets whether the client is closed.</summary>
        bool IsClosed() const { return _client->IsClosed(); }

        /// <summary>Gets whether the client is initialized with all required data.</summary>
        bool IsInitialized() const
        {
            return _clientData.has_value() && _gameData.has_value() && _serverData.has_value();
        }

        /// <summary>Gets whether the client has received a response to the last request.</summary>
        bool IsResponsed() const { return _isResponsed.load(); }

        /// <summary>Gets whether the client can send a request (initialized and not waiting).</summary>
        bool CanRequest() const { return IsInitialized() && IsResponsed(); }

        /// <summary>Gets the number of queued requests waiting for responses.</summary>
        int PendingRequests() const
        {
            std::lock_guard lock(_pendingMutex);
            return static_cast<int>(_pendingRequests.size());
        }

        /// <summary>Gets the UDP data channel used for unreliable high-frequency state sync.</summary>
        UDPChannel::Ptr DataChannel() const { return _dataChannel; }

        /// <summary>Gets the server IP endpoint this client is connected to.</summary>
        const IPEndPoint& ServerEndPoint() const { return _endPoint; }

        /// <summary>Gets or sets the client game data (nullptr when unset).</summary>
        Data::ClientGameData* ClientData() { return _clientData ? &*_clientData : nullptr; }
        void SetClientData(const Data::ClientGameData& data) { _clientData = data; }

        /// <summary>Gets or sets the player game data (nullptr when unset).</summary>
        Michitai::Lan::Data::PlayerGameData* GameData() { return _gameData ? &*_gameData : nullptr; }
        void SetGameData(const Michitai::Lan::Data::PlayerGameData& data) { _gameData = data; }

        /// <summary>Gets or sets the server game data (nullptr when unset).</summary>
        Data::ServerGameData* ServerData() { return _serverData ? &*_serverData : nullptr; }
        void SetServerData(const Data::ServerGameData& data) { _serverData = data; }

        /// <summary>Creates a client for the specified server endpoint.</summary>
        static Ptr Create(const IPEndPoint& endPoint, int bufferSize = 8192)
        {
            Ptr client = Ptr(new Client(std::nullopt, std::nullopt, endPoint, bufferSize));
            client->Initialize();
            return client;
        }

        /// <summary>Creates a client for the specified server IP and port.</summary>
        static Ptr Create(const IPAddress& ip, int port, int bufferSize = 8192)
        {
            return Create(IPEndPoint(ip, port), bufferSize);
        }

        /// <summary>Creates a client with client data for the specified endpoint.</summary>
        static Ptr Create(const Data::ClientGameData& clientGameData, const IPEndPoint& endPoint,
                          int bufferSize = 8192)
        {
            Ptr client = Ptr(new Client(clientGameData, std::nullopt, endPoint, bufferSize));
            client->Initialize();
            return client;
        }

        /// <summary>Creates a client with client data for the specified IP and port.</summary>
        static Ptr Create(const Data::ClientGameData& clientGameData, const IPAddress& ip, int port,
                          int bufferSize = 8192)
        {
            return Create(clientGameData, IPEndPoint(ip, port), bufferSize);
        }

        /// <summary>Creates a client with client and player data for the specified endpoint.</summary>
        static Ptr Create(const Data::ClientGameData& clientGameData,
                          const Michitai::Lan::Data::PlayerGameData& gameData,
                          const IPEndPoint& endPoint, int bufferSize = 8192)
        {
            Ptr client = Ptr(new Client(clientGameData, gameData, endPoint, bufferSize));
            client->Initialize();
            return client;
        }

        /// <summary>Creates a client with client and player data for the specified IP and port.</summary>
        static Ptr Create(const Data::ClientGameData& clientGameData,
                          const Michitai::Lan::Data::PlayerGameData& gameData,
                          const IPAddress& ip, int port, int bufferSize = 8192)
        {
            return Create(clientGameData, gameData, IPEndPoint(ip, port), bufferSize);
        }

        /// <summary>Starts the client, connects to the server, and opens the UDP state channel.</summary>
        void Start();

        /// <summary>Stops the client and disconnects from the server.</summary>
        void Stop();

        /// <summary>
        /// Sends a request message to the server. If a request is already awaiting
        /// a response, the message is queued and sent when the response arrives.
        /// </summary>
        void Request(const Message& message);

        /// <summary>
        /// Sends a message immediately without waiting for a response.
        /// Use for fire-and-forget commands; for per-frame state prefer SendState (UDP).
        /// </summary>
        void Send(const Message& message) { _client->Request(message); }

        /// <summary>Sends a state datagram to the server over the unreliable UDP data channel.</summary>
        void SendState(const Message& message);

        /// <summary>Sends a state datagram to the server over the unreliable UDP data channel.</summary>
        void SendState(const std::string& state);

    private:
        Client(std::optional<Data::ClientGameData> clientData,
               std::optional<Michitai::Lan::Data::PlayerGameData> gameData,
               const IPEndPoint& endPoint, int bufferSize);

        /// <summary>Wires TCPClient events (needs shared_ptr — runs after construction).</summary>
        void Initialize();

        /// <summary>Sends the next queued request after a response was received.</summary>
        void FlushPending();

        TCPClient::Ptr _client;
        UDPChannel::Ptr _dataChannel;
        IPEndPoint _endPoint;

        std::optional<Data::ClientGameData> _clientData;
        std::optional<Michitai::Lan::Data::PlayerGameData> _gameData;
        std::optional<Data::ServerGameData> _serverData;

        std::atomic<bool> _isResponsed{true};

        std::queue<Message> _pendingRequests;
        mutable std::mutex _pendingMutex;
    };
}
