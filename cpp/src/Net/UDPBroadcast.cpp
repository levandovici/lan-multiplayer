//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include <Michitai/Lan/Net/UDPBroadcast.hpp>
#include <Michitai/Lan/Net/UDPChannel.hpp>
#include <Michitai/Lan/JsonSerializer.hpp>
#include <Michitai/Lan/Debugging/DebugConsole.hpp>
#include "SocketInternal.hpp"

#include <stdexcept>


namespace Michitai::Lan::Net
{
    UDPBroadcast::UDPBroadcast(const IPEndPoint& point)
    {
        _socket = std::make_unique<Detail::Socket>(Detail::Socket::UdpV4());

        if (!_socket->Bind(point))
            Detail::ThrowSocketError("bind");

        _socket->SetBroadcast(true);
    }

    UDPBroadcast::UDPBroadcast(const IPAddress& ip, const PortRange& range)
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
                throw std::runtime_error("UDPBroadcast: no free port in range");
            }

            auto socket = std::make_unique<Detail::Socket>(Detail::Socket::UdpV4());

            try
            {
                if (socket->Bind(IPEndPoint(ip, port)))
                {
                    socket->SetBroadcast(true);
                    _socket = std::move(socket);
                    break;
                }
            }
            catch (const std::exception& e)
            {
                Debug::DebugConsole::LogError(e.what());
            }
        }
    }

    UDPBroadcast::~UDPBroadcast()
    {
        Stop();
    }

    UDPBroadcast::UDPBroadcast(UDPBroadcast&&) noexcept = default;
    UDPBroadcast& UDPBroadcast::operator=(UDPBroadcast&&) noexcept = default;

    void UDPBroadcast::Stop()
    {
        if (_socket)
            _socket->Close();
    }

    void UDPBroadcast::Send(const IPEndPoint& point, const AppMessage& message)
    {
        const std::string json = JsonSerializer::Serialize(message);
        const auto* bytes = reinterpret_cast<const std::uint8_t*>(json.data());

        const int sent = _socket->SendTo(point, bytes, static_cast<int>(json.size()));

        if (sent != static_cast<int>(json.size()))
        {
            throw std::runtime_error("UDPBroadcast: incomplete datagram send");
        }
    }

    LocatedMessage UDPBroadcast::Receive()
    {
        std::vector<std::uint8_t> buffer(UDPChannel::MaxDatagramSize);

        IPEndPoint remote;
        const int received = _socket->ReceiveFrom(buffer.data(), static_cast<int>(buffer.size()), &remote);

        if (received <= 0)
            return LocatedMessage(std::nullopt, std::nullopt);

        try
        {
            AppMessage message = JsonSerializer::Deserialize<AppMessage>(
                std::string(buffer.begin(), buffer.begin() + received));

            return LocatedMessage(remote, std::move(message));
        }
        catch (...)
        {
            return LocatedMessage(remote, std::nullopt);
        }
    }

    LocatedMessage UDPBroadcast::Receive(int timeoutMilliseconds)
    {
        if (!_socket->WaitReadable(timeoutMilliseconds))
            return LocatedMessage(std::nullopt, std::nullopt);

        return Receive();
    }
}
