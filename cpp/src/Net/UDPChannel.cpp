//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include <Michitai/Lan/Net/UDPChannel.hpp>
#include <Michitai/Lan/Net/Compression.hpp>
#include <Michitai/Lan/Net/Frame.hpp>
#include <Michitai/Lan/Debugging/DebugConsole.hpp>
#include "SocketInternal.hpp"

#include <algorithm>
#include <stdexcept>


namespace Michitai::Lan::Net
{
    UDPChannel::UDPChannel(const IPEndPoint& point)
    {
        _socket = std::make_unique<Detail::Socket>(Detail::Socket::UdpV4());

        if (!_socket->Bind(point))
            Detail::ThrowSocketError("bind");
    }

    UDPChannel::UDPChannel(const IPAddress& ip, const PortRange& range)
    {
        PortRange::Store store = range.RangeStore();

        while (true)
        {
            int port;
            try
            {
                port = store.RandomPort();
            }
            catch (const std::out_of_range&)
            {
                throw std::runtime_error("UDPChannel: no free port in range");
            }

            auto socket = std::make_unique<Detail::Socket>(Detail::Socket::UdpV4());
            if (socket->Bind(IPEndPoint(ip, port)))
            {
                _socket = std::move(socket);
                break;
            }
        }
    }

    UDPChannel::~UDPChannel()
    {
        Stop();
        JoinThreads();
    }

    std::optional<IPEndPoint> UDPChannel::IpEndPoint() const
    {
        try
        {
            return _socket ? _socket->LocalEndPoint() : std::nullopt;
        }
        catch (...)
        {
            return std::nullopt;
        }
    }

    std::vector<IPEndPoint> UDPChannel::Peers() const
    {
        std::lock_guard lock(_peersMutex);
        return _peers;
    }

    void UDPChannel::Start()
    {
        try
        {
            Ptr self = shared_from_this();
            _receiveThread = std::thread([self] { self->ReceiveLoop(); });
        }
        catch (...)
        {
            Stop();
        }
    }

    void UDPChannel::Stop()
    {
        if (_closed.exchange(true))
            return;

        OnReceive = nullptr;

        if (_socket)
            _socket->Close();
    }

    void UDPChannel::Send(const IPEndPoint& target, const Message& message)
    {
        Send(target, message.GetMessage());
    }

    void UDPChannel::Send(const IPEndPoint& target, const std::string& text)
    {
        std::vector<std::uint8_t> payload(text.begin(), text.end());

        const bool compressed = Compressor::TryCompress(payload, payload);

        std::vector<std::uint8_t> datagram(payload.size() + 1);
        datagram[0] = compressed ? Frame::FlagCompressed : Frame::FlagNone;
        std::copy(payload.begin(), payload.end(), datagram.begin() + 1);

        if (static_cast<int>(datagram.size()) > MaxDatagramSize)
        {
            throw std::length_error("Datagram exceeds maximum size (" +
                                    std::to_string(datagram.size()) + " > " +
                                    std::to_string(MaxDatagramSize) + ")");
        }

        if (static_cast<int>(datagram.size()) > SafeDatagramSize)
        {
            Debug::DebugConsole::LogWarning("[Michitai.Lan][UDP-CHANNEL][DATAGRAM-FRAGMENTATION-RISK][" +
                                            std::to_string(datagram.size()) + "]");
        }

        std::lock_guard lock(_sendMutex);

        if (_closed.load())
            return;

        _socket->SendTo(target, datagram.data(), static_cast<int>(datagram.size()));
    }

    void UDPChannel::Broadcast(const Message& message)
    {
        const std::vector<IPEndPoint> peers = Peers();

        for (const IPEndPoint& peer : peers)
        {
            try
            {
                Send(peer, message);
            }
            catch (...)
            {
            }
        }
    }

    void UDPChannel::Forget(const IPEndPoint& point)
    {
        std::lock_guard lock(_peersMutex);
        std::erase(_peers, point);
    }

    void UDPChannel::ReceiveLoop()
    {
        std::vector<std::uint8_t> buffer(MaxDatagramSize);

        while (!IsClosed())
        {
            IPEndPoint remote;
            const int received = _socket->ReceiveFrom(buffer.data(), static_cast<int>(buffer.size()), &remote);

            if (received <= 0)
            {
                if (_closed.load())
                    return;

                Stop();
                return;
            }

            if (received > 1)
            {
                try
                {
                    const std::uint8_t flags = buffer[0];

                    std::vector<std::uint8_t> payload(buffer.begin() + 1, buffer.begin() + received);

                    if ((flags & Frame::FlagCompressed) == Frame::FlagCompressed)
                    {
                        payload = Compressor::Decompress(payload);
                    }

                    TrackPeer(remote);

                    OnReceive.Invoke(remote, Message(std::string(payload.begin(), payload.end())));
                }
                catch (...)
                {
                    Debug::DebugConsole::LogError("[Michitai.Lan][UDP-CHANNEL][RECEIVE-ERROR]");
                }
            }
        }
    }

    void UDPChannel::TrackPeer(const IPEndPoint& point)
    {
        std::lock_guard lock(_peersMutex);

        if (std::find(_peers.begin(), _peers.end(), point) == _peers.end())
        {
            _peers.push_back(point);
        }
    }

    void UDPChannel::JoinThreads()
    {
        if (_receiveThread.joinable())
        {
            if (_receiveThread.get_id() == std::this_thread::get_id())
                _receiveThread.detach();
            else
                _receiveThread.join();
        }
    }
}
