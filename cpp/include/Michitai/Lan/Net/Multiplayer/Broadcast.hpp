//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <chrono>
#include <functional>
#include <future>
#include <memory>
#include <optional>
#include <stop_token>
#include <vector>

#include <Michitai/Lan/Net/EndPoint.hpp>
#include <Michitai/Lan/Net/Messages.hpp>
#include <Michitai/Lan/Net/PortRange.hpp>
#include <Michitai/Lan/Net/UDPBroadcast.hpp>


namespace Michitai::Lan::Net::Multiplayer
{
    /// <summary>
    /// Broadcast server for responding to client discovery requests on the LAN.
    /// </summary>
    class BroadcastServer
    {
    public:
        using Ptr = std::shared_ptr<BroadcastServer>;

        /// <summary>Delegate for processing incoming broadcast messages.</summary>
        using ProcessMessageDelegate = std::function<AppMessage(const LocatedMessage&)>;

        /// <summary>Initializes a new instance bound to the specified IP endpoint.</summary>
        explicit BroadcastServer(const IPEndPoint& point) : _socket(point) {}

        /// <summary>Initializes a new instance bound to the specified IP address and port.</summary>
        BroadcastServer(const IPAddress& ip, int port) : _socket(ip, port) {}

        /// <summary>Initializes a new instance bound to a free port from the specified range.</summary>
        BroadcastServer(const IPAddress& ip, const PortRange& range) : _socket(ip, range) {}

        /// <summary>Gets or sets the UDP broadcast socket.</summary>
        UDPBroadcast& Socket() { return _socket; }
        void SetSocket(UDPBroadcast socket) { _socket = std::move(socket); }

        /// <summary>Stops the broadcast server.</summary>
        void Stop() { _socket.Stop(); }

        /// <summary>Processes a single broadcast message and sends the response (blocking).</summary>
        void Broadcast(const ProcessMessageDelegate& process);

        /// <summary>Processes a single broadcast message with a receive timeout.</summary>
        bool Broadcast(const ProcessMessageDelegate& process, int timeoutMilliseconds);

        /// <summary>Asynchronously processes a single broadcast message and sends the response.</summary>
        std::future<void> BroadcastAsync(ProcessMessageDelegate process)
        {
            return std::async(std::launch::async,
                              [this, process = std::move(process)] { Broadcast(process); });
        }

        /// <summary>Asynchronously processes a single broadcast message with a timeout.</summary>
        std::future<bool> BroadcastAsync(ProcessMessageDelegate process, int timeoutMilliseconds)
        {
            return std::async(std::launch::async,
                              [this, process = std::move(process), timeoutMilliseconds]
                              { return Broadcast(process, timeoutMilliseconds); });
        }

    private:
        UDPBroadcast _socket;
    };


    /// <summary>
    /// Broadcast client for discovering and communicating with servers on the LAN.
    /// </summary>
    class BroadcastClient
    {
    public:
        using Ptr = std::shared_ptr<BroadcastClient>;

        /// <summary>Delegate for handling received broadcast responses.</summary>
        using OnReceiveResponseDelegate = std::function<void(const LocatedMessage&)>;

        /// <summary>Initializes a new instance bound to the specified IP endpoint.</summary>
        explicit BroadcastClient(const IPEndPoint& point) : _socket(point) {}

        /// <summary>Initializes a new instance bound to the specified IP address and port.</summary>
        BroadcastClient(const IPAddress& ip, int port) : _socket(ip, port) {}

        /// <summary>Initializes a new instance bound to a free port from the specified range.</summary>
        BroadcastClient(const IPAddress& ip, const PortRange& range) : _socket(ip, range) {}

        /// <summary>Gets or sets the UDP broadcast socket.</summary>
        UDPBroadcast& Socket() { return _socket; }
        void SetSocket(UDPBroadcast socket) { _socket = std::move(socket); }

        /// <summary>Stops the broadcast client.</summary>
        void Stop() { _socket.Stop(); }

        /// <summary>Sends a broadcast request and waits for a response.</summary>
        LocatedMessage BroadcastRequest(const IPAddress& ip, int port, const AppMessage& message,
                                        int receiveTimeoutMilliseconds)
        {
            _socket.Send(ip, port, message);
            return _socket.Receive(receiveTimeoutMilliseconds);
        }

        /// <summary>Sends a broadcast request to a port range and waits for a response.</summary>
        LocatedMessage BroadcastRequest(const IPAddress& ip, const PortRange& range,
                                        const AppMessage& message, int receiveTimeoutMilliseconds)
        {
            for (int port = range.First; port <= range.Last; port++)
            {
                _socket.Send(ip, port, message);
            }
            return _socket.Receive(receiveTimeoutMilliseconds);
        }

        /// <summary>
        /// Sends a broadcast request, then invokes the callback for every response
        /// received within receiveResponsesMilliseconds. Blocks for that duration.
        /// </summary>
        void BeginBroadcastRequest(const IPAddress& ip, int port, const AppMessage& message,
                                   const OnReceiveResponseDelegate& onReceiveResponse,
                                   int receiveResponsesMilliseconds)
        {
            _socket.Send(ip, port, message);
            CollectResponses(onReceiveResponse, receiveResponsesMilliseconds);
        }

        void BeginBroadcastRequest(const IPAddress& ip, const PortRange& range, const AppMessage& message,
                                   const OnReceiveResponseDelegate& onReceiveResponse,
                                   int receiveResponsesMilliseconds)
        {
            for (int port = range.First; port <= range.Last; port++)
            {
                _socket.Send(ip, port, message);
            }
            CollectResponses(onReceiveResponse, receiveResponsesMilliseconds);
        }

        void BeginBroadcastRequest(const std::vector<IPAddress>& masks, int port, const AppMessage& message,
                                   const OnReceiveResponseDelegate& onReceiveResponse,
                                   int receiveResponsesMilliseconds)
        {
            for (const IPAddress& mask : masks)
            {
                _socket.Send(mask, port, message);
            }
            CollectResponses(onReceiveResponse, receiveResponsesMilliseconds);
        }

        void BeginBroadcastRequest(const std::vector<IPAddress>& masks, const PortRange& range,
                                   const AppMessage& message,
                                   const OnReceiveResponseDelegate& onReceiveResponse,
                                   int receiveResponsesMilliseconds)
        {
            for (const IPAddress& mask : masks)
            {
                for (int port = range.First; port <= range.Last; port++)
                {
                    _socket.Send(mask, port, message);
                }
            }
            CollectResponses(onReceiveResponse, receiveResponsesMilliseconds);
        }

        /// <summary>Asynchronously sends a broadcast request and waits for a response.</summary>
        std::future<LocatedMessage> BroadcastRequestAsync(IPAddress ip, int port, AppMessage message,
                                                        int receiveTimeoutMilliseconds)
        {
            return std::async(std::launch::async,
                              [this, ip = std::move(ip), port, message = std::move(message),
                               receiveTimeoutMilliseconds]
                              { return BroadcastRequest(ip, port, message, receiveTimeoutMilliseconds); });
        }

        std::future<LocatedMessage> BroadcastRequestAsync(IPAddress ip, PortRange range, AppMessage message,
                                                        int receiveTimeoutMilliseconds)
        {
            return std::async(std::launch::async,
                              [this, ip = std::move(ip), range, message = std::move(message),
                               receiveTimeoutMilliseconds]
                              { return BroadcastRequest(ip, range, message, receiveTimeoutMilliseconds); });
        }

        /// <summary>
        /// Asynchronously broadcasts a request and invokes the callback for every
        /// response received within receiveResponsesMilliseconds.
        /// </summary>
        std::future<void> BroadcastRequestAsync(IPAddress ip, int port, AppMessage message,
                                                OnReceiveResponseDelegate onReceiveResponse,
                                                int receiveResponsesMilliseconds)
        {
            return std::async(std::launch::async,
                              [this, ip = std::move(ip), port, message = std::move(message),
                               onReceiveResponse = std::move(onReceiveResponse), receiveResponsesMilliseconds]
                              { BeginBroadcastRequest(ip, port, message, onReceiveResponse, receiveResponsesMilliseconds); });
        }

        std::future<void> BroadcastRequestAsync(IPAddress ip, PortRange range, AppMessage message,
                                                OnReceiveResponseDelegate onReceiveResponse,
                                                int receiveResponsesMilliseconds)
        {
            return std::async(std::launch::async,
                              [this, ip = std::move(ip), range, message = std::move(message),
                               onReceiveResponse = std::move(onReceiveResponse), receiveResponsesMilliseconds]
                              { BeginBroadcastRequest(ip, range, message, onReceiveResponse, receiveResponsesMilliseconds); });
        }

        std::future<void> BroadcastRequestAsync(std::vector<IPAddress> masks, int port, AppMessage message,
                                                OnReceiveResponseDelegate onReceiveResponse,
                                                int receiveResponsesMilliseconds)
        {
            return std::async(std::launch::async,
                              [this, masks = std::move(masks), port, message = std::move(message),
                               onReceiveResponse = std::move(onReceiveResponse), receiveResponsesMilliseconds]
                              { BeginBroadcastRequest(masks, port, message, onReceiveResponse, receiveResponsesMilliseconds); });
        }

        std::future<void> BroadcastRequestAsync(std::vector<IPAddress> masks, PortRange range, AppMessage message,
                                                OnReceiveResponseDelegate onReceiveResponse,
                                                int receiveResponsesMilliseconds)
        {
            return std::async(std::launch::async,
                              [this, masks = std::move(masks), range, message = std::move(message),
                               onReceiveResponse = std::move(onReceiveResponse), receiveResponsesMilliseconds]
                              { BeginBroadcastRequest(masks, range, message, onReceiveResponse, receiveResponsesMilliseconds); });
        }

        /// <summary>
        /// Receives responses for up to receiveResponsesMilliseconds, invoking the callback
        /// for each. Stops early when the token requests cancellation.
        /// </summary>
        void CollectResponses(const OnReceiveResponseDelegate& onReceiveResponse,
                              int receiveResponsesMilliseconds,
                              const std::stop_token& stopToken);

        /// <summary>Same as above without cancellation support.</summary>
        void CollectResponses(const OnReceiveResponseDelegate& onReceiveResponse,
                              int receiveResponsesMilliseconds);

    private:
        UDPBroadcast _socket;
    };
}
