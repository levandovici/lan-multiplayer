//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <atomic>
#include <condition_variable>
#include <cstdint>
#include <functional>
#include <memory>
#include <mutex>
#include <queue>
#include <thread>
#include <vector>

#include <Michitai/Lan/Events.hpp>
#include <Michitai/Lan/Net/EndPoint.hpp>
#include <Michitai/Lan/Net/Frame.hpp>
#include <Michitai/Lan/Net/Messages.hpp>


namespace Michitai::Lan::Net
{
    namespace Detail { class Socket; }

    /// <summary>
    /// Represents a connected TCP client on the server side, using the
    /// length-prefixed frame protocol with NoDelay, dedicated receive/write
    /// threads, queued writes, and GZip payload compression.
    /// Owned via std::shared_ptr — created internally by TCPServer.
    /// </summary>
    class TCPServerClient : public std::enable_shared_from_this<TCPServerClient>
    {
    public:
        using Ptr = std::shared_ptr<TCPServerClient>;
        using RequestHandler = std::function<void(const IdentifiedMessage&)>;
        using StopHandler = std::function<void(const std::string&)>;

        ~TCPServerClient();

        TCPServerClient(const TCPServerClient&) = delete;
        TCPServerClient& operator=(const TCPServerClient&) = delete;

        /// <summary>Gets the unique client ID.</summary>
        const std::string& ID() const { return _id; }

        /// <summary>Gets whether the client is closed.</summary>
        bool IsClosed() const { return _closed.load(); }

        /// <summary>Gets the number of frames queued for writing (backpressure indicator).</summary>
        int PendingWrites() const;

        /// <summary>
        /// Starts the receive loop. Must be called only after the client is
        /// registered with the server (TCPServer.AddClient), otherwise requests
        /// arriving during construction would have their responses dropped.
        /// </summary>
        void Start();

        /// <summary>Stops the client and cleans up resources.</summary>
        void Stop();

        /// <summary>
        /// Sends a response message to the client. Messages are framed, optionally
        /// compressed, and queued so concurrent calls never interleave or get lost.
        /// </summary>
        void Response(const Message& message);

    private:
        friend class TCPServer;

        TCPServerClient(std::unique_ptr<Detail::Socket> socket, RequestHandler onRequest,
                        StopHandler onStop, int bufferSize);

        void ReceiveLoop();
        void WriteLoop();
        void JoinThreads();

        std::string _id;
        int _bufferSize;

        std::unique_ptr<Detail::Socket> _socket;
        FrameBuffer _frameBuffer;

        std::queue<std::vector<std::uint8_t>> _sendQueue;
        mutable std::mutex _sendMutex;
        std::condition_variable _sendCv;

        std::atomic<bool> _closed{false};
        mutable std::mutex _stateMutex;

        std::thread _receiveThread;
        std::thread _writeThread;

        RequestHandler _onRequest;
        StopHandler _onStop;
    };


    /// <summary>
    /// TCP server for managing multiple client connections and message-based communication.
    /// Owned via std::shared_ptr — create with TCPServer::Create().
    /// </summary>
    class TCPServer : public std::enable_shared_from_this<TCPServer>
    {
    public:
        using Ptr = std::shared_ptr<TCPServer>;

        ~TCPServer();

        TCPServer(const TCPServer&) = delete;
        TCPServer& operator=(const TCPServer&) = delete;

        /// <summary>Event raised when a client connects.</summary>
        Event<const std::string&> OnClientConnected;

        /// <summary>Event raised when a client disconnects.</summary>
        Event<const std::string&> OnClientDisconnected;

        /// <summary>Event raised when a request message is received from a client.</summary>
        Event<const IdentifiedMessage&> OnRequest;

        /// <summary>Event raised when the server stops.</summary>
        Event<> OnStop;

        /// <summary>Gets whether the server is closed.</summary>
        bool IsClosed() const { return _closed.load(); }

        /// <summary>Gets the IP endpoint the server is bound to.</summary>
        const IPEndPoint& IpEndPoint() const { return _endPoint; }

        /// <summary>Gets the total number of frames queued for writing across all clients.</summary>
        int PendingWrites() const;

        /// <summary>Creates a TCP server for the specified IP address and port.</summary>
        static Ptr Create(const IPAddress& ip, int port, int bufferSize = 4096)
        {
            return Create(IPEndPoint(ip, port), bufferSize);
        }

        /// <summary>Creates a TCP server bound to the specified endpoint (throws when the bind fails).</summary>
        static Ptr Create(const IPEndPoint& endPoint, int bufferSize = 4096)
        {
            return Ptr(new TCPServer(endPoint, bufferSize));
        }

        /// <summary>Starts the TCP server and begins accepting client connections.</summary>
        void Start();

        /// <summary>Stops the TCP server and disconnects all clients.</summary>
        void Stop();

        /// <summary>Sends a response message to a specific client.</summary>
        void Response(const IdentifiedMessage& identifiedMessage);

        /// <summary>Disconnects a client by its ID.</summary>
        void Disconnect(const std::string& id);

    private:
        TCPServer(const IPEndPoint& endPoint, int bufferSize);

        void AcceptLoop();
        void AddClient(const TCPServerClient::Ptr& client);
        void DeleteClient(const std::string& id);
        void JoinThreads();

        std::unique_ptr<Detail::Socket> _listener;
        IPEndPoint _endPoint;
        int _bufferSize;

        std::vector<TCPServerClient::Ptr> _clients;
        mutable std::mutex _clientsMutex;

        std::atomic<bool> _closed{false};

        std::thread _acceptThread;
    };
}
