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
    /// TCP client for asynchronous network communication with length-prefixed frame protocol.
    /// Optimized for high-frequency messaging: Nagle disabled (NoDelay), dedicated receive/write
    /// threads, queued writes, GZip payload compression, and a streaming frame decoder.
    /// Instances are owned via std::shared_ptr — create them with TCPClient::Create().
    /// </summary>
    class TCPClient : public std::enable_shared_from_this<TCPClient>
    {
    public:
        using Ptr = std::shared_ptr<TCPClient>;

        ~TCPClient();

        TCPClient(const TCPClient&) = delete;
        TCPClient& operator=(const TCPClient&) = delete;

        /// <summary>Event raised when a response message is received.</summary>
        Event<const Message&> OnResponse;

        /// <summary>Event raised when the client stops.</summary>
        Event<> OnStop;

        /// <summary>Gets whether the client is closed.</summary>
        bool IsClosed() const { return _closed.load(); }

        /// <summary>Gets the number of frames queued for writing (backpressure indicator).</summary>
        int PendingWrites() const;

        /// <summary>Creates a TCP client for the specified IP address and port.</summary>
        static Ptr Create(const IPAddress& ip, int port, int bufferSize = 8192)
        {
            return Create(IPEndPoint(ip, port), bufferSize);
        }

        /// <summary>Creates a TCP client for the specified IP endpoint.</summary>
        static Ptr Create(const IPEndPoint& endPoint, int bufferSize = 8192)
        {
            return Ptr(new TCPClient(endPoint, bufferSize));
        }

        /// <summary>Starts the client: connects to the server and begins the receive loop. Throws on connect failure.</summary>
        void Start();

        /// <summary>Stops the client and cleans up resources.</summary>
        void Stop();

        /// <summary>
        /// Sends a request message to the server. Messages are framed, optionally
        /// compressed, and queued so concurrent calls never interleave or get lost.
        /// </summary>
        void Request(const Message& message);

    private:
        TCPClient(const IPEndPoint& endPoint, int bufferSize);

        void ReceiveLoop();
        void WriteLoop();
        void JoinThreads();

        IPEndPoint _endPoint;
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
    };
}
