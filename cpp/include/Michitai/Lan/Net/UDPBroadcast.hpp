//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <cstdint>
#include <future>
#include <memory>
#include <optional>
#include <vector>

#include <Michitai/Lan/Net/EndPoint.hpp>
#include <Michitai/Lan/Net/Messages.hpp>
#include <Michitai/Lan/Net/PortRange.hpp>


namespace Michitai::Lan::Net
{
    namespace Detail { class Socket; }

    /// <summary>
    /// Provides UDP broadcast functionality for network discovery and message broadcasting.
    /// Movable but not copyable (owns the socket).
    /// </summary>
    class UDPBroadcast
    {
    public:
        /// <summary>Initializes a new instance bound to the specified IP endpoint.</summary>
        explicit UDPBroadcast(const IPEndPoint& point);

        /// <summary>Initializes a new instance bound to the specified IP address and port.</summary>
        UDPBroadcast(const IPAddress& ip, int port) : UDPBroadcast(IPEndPoint(ip, port)) {}

        /// <summary>Initializes a new instance bound to a free port from the specified range.</summary>
        UDPBroadcast(const IPAddress& ip, const PortRange& range);

        ~UDPBroadcast();

        UDPBroadcast(UDPBroadcast&&) noexcept;
        UDPBroadcast& operator=(UDPBroadcast&&) noexcept;

        UDPBroadcast(const UDPBroadcast&) = delete;
        UDPBroadcast& operator=(const UDPBroadcast&) = delete;

        /// <summary>Stops the UDP broadcast and cleans up resources.</summary>
        void Stop();

        /// <summary>
        /// Sends an application message to the specified endpoint (JSON serialized).
        /// Throws std::runtime_error when not all bytes are sent.
        /// </summary>
        void Send(const IPEndPoint& point, const AppMessage& message);

        /// <summary>Sends an application message to the specified IP address and port.</summary>
        void Send(const IPAddress& ip, int port, const AppMessage& message)
        {
            Send(IPEndPoint(ip, port), message);
        }

        /// <summary>Sends an application message to all ports in the specified range.</summary>
        void Send(const IPAddress& ip, const PortRange& range, const AppMessage& message)
        {
            for (int port = range.First; port <= range.Last; port++)
            {
                Send(ip, port, message);
            }
        }

        /// <summary>Receives a message from any sender (blocking).</summary>
        LocatedMessage Receive();

        /// <summary>Receives a message with a timeout; members are empty on timeout.</summary>
        LocatedMessage Receive(int timeoutMilliseconds);

        /// <summary>Asynchronously sends an application message to the specified endpoint.</summary>
        std::future<void> SendAsync(IPEndPoint point, AppMessage message)
        {
            return std::async(std::launch::async, [this, point = std::move(point), message = std::move(message)]
            {
                Send(point, message);
            });
        }

        /// <summary>Asynchronously sends an application message to the specified IP address and port.</summary>
        std::future<void> SendAsync(IPAddress ip, int port, AppMessage message)
        {
            return SendAsync(IPEndPoint(std::move(ip), port), std::move(message));
        }

        /// <summary>Asynchronously sends an application message to all ports in the specified range.</summary>
        std::future<void> SendAsync(IPAddress ip, PortRange range, AppMessage message)
        {
            return std::async(std::launch::async, [this, ip = std::move(ip), range, message = std::move(message)]
            {
                for (int port = range.First; port <= range.Last; port++)
                {
                    Send(ip, port, message);
                }
            });
        }

        /// <summary>Asynchronously receives a message from any sender.</summary>
        std::future<std::optional<LocatedMessage>> ReceiveAsync()
        {
            return std::async(std::launch::async, [this]
            {
                return std::optional<LocatedMessage>(Receive());
            });
        }

        /// <summary>Asynchronously receives a message with a timeout; empty optional on timeout.</summary>
        std::future<std::optional<LocatedMessage>> ReceiveAsync(int timeoutMilliseconds)
        {
            return std::async(std::launch::async, [this, timeoutMilliseconds]
            {
                LocatedMessage message = Receive(timeoutMilliseconds);
                if (!message.IPEndPoint.has_value())
                    return std::optional<LocatedMessage>();
                return std::optional<LocatedMessage>(std::move(message));
            });
        }

    private:
        std::unique_ptr<Detail::Socket> _socket;
    };
}
