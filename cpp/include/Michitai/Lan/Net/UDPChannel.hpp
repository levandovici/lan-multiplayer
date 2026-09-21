//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <atomic>
#include <cstdint>
#include <memory>
#include <mutex>
#include <thread>
#include <vector>

#include <Michitai/Lan/Events.hpp>
#include <Michitai/Lan/Net/EndPoint.hpp>
#include <Michitai/Lan/Net/Messages.hpp>
#include <Michitai/Lan/Net/PortRange.hpp>


namespace Michitai::Lan::Net
{
    namespace Detail { class Socket; }

    /// <summary>
    /// Unreliable UDP datagram channel for high-frequency state synchronization
    /// (positions, transforms, inputs) at 30-60+ Hz without TCP head-of-line blocking.
    /// Datagram format: [1-byte flags][payload], payload compressed when beneficial.
    /// Owned via std::shared_ptr — create with UDPChannel::Create().
    /// </summary>
    class UDPChannel : public std::enable_shared_from_this<UDPChannel>
    {
    public:
        using Ptr = std::shared_ptr<UDPChannel>;

        /// <summary>Maximum UDP datagram payload (IPv4). Datagrams larger than this are rejected.</summary>
        static constexpr int MaxDatagramSize = 65507;

        /// <summary>
        /// Datagrams larger than this may be fragmented by IP and lose parts on congested networks.
        /// Keep per-frame state under this size for reliability (Ethernet MTU 1500 minus headers).
        /// </summary>
        static constexpr int SafeDatagramSize = 1472;

        ~UDPChannel();

        UDPChannel(const UDPChannel&) = delete;
        UDPChannel& operator=(const UDPChannel&) = delete;

        /// <summary>Event raised when a datagram is received. First argument is the sender endpoint.</summary>
        Event<const IPEndPoint&, const Message&> OnReceive;

        /// <summary>Creates a channel bound to the specified endpoint. Use port 0 for an ephemeral port.</summary>
        static Ptr Create(const IPEndPoint& point)
        {
            return Ptr(new UDPChannel(point));
        }

        /// <summary>Creates a channel bound to the specified IP and port. Use 0 for an ephemeral port.</summary>
        static Ptr Create(const IPAddress& ip, int port)
        {
            return Ptr(new UDPChannel(IPEndPoint(ip, port)));
        }

        /// <summary>Creates a channel bound to a free port from the specified range.</summary>
        static Ptr Create(const IPAddress& ip, const PortRange& range)
        {
            return Ptr(new UDPChannel(ip, range));
        }

        /// <summary>Gets the local endpoint the channel is bound to.</summary>
        std::optional<IPEndPoint> IpEndPoint() const;

        /// <summary>Gets the endpoints this channel has received datagrams from. Thread-safe snapshot.</summary>
        std::vector<IPEndPoint> Peers() const;

        /// <summary>Gets whether the channel is closed.</summary>
        bool IsClosed() const { return _closed.load(); }

        /// <summary>Starts the asynchronous receive loop.</summary>
        void Start();

        /// <summary>Stops the channel and releases the socket.</summary>
        void Stop();

        /// <summary>
        /// Sends a message to the specified endpoint as a single datagram.
        /// Throws std::length_error when the datagram exceeds MaxDatagramSize.
        /// </summary>
        void Send(const IPEndPoint& target, const Message& message);

        /// <summary>
        /// Sends a string to the specified endpoint as a single datagram.
        /// Throws std::length_error when the datagram exceeds MaxDatagramSize.
        /// </summary>
        void Send(const IPEndPoint& target, const std::string& text);

        /// <summary>Sends a message to all endpoints that have sent datagrams to this channel.</summary>
        void Broadcast(const Message& message);

        /// <summary>Removes an endpoint from the known peers list.</summary>
        void Forget(const IPEndPoint& point);

    private:
        explicit UDPChannel(const IPEndPoint& point);
        UDPChannel(const IPAddress& ip, const PortRange& range);

        void ReceiveLoop();
        void TrackPeer(const IPEndPoint& point);
        void JoinThreads();

        std::unique_ptr<Detail::Socket> _socket;
        std::atomic<bool> _closed{false};

        mutable std::mutex _sendMutex;

        std::vector<IPEndPoint> _peers;
        mutable std::mutex _peersMutex;

        std::thread _receiveThread;
    };
}
