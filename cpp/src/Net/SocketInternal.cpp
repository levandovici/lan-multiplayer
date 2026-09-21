//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include "SocketInternal.hpp"

#include <mutex>
#include <stdexcept>
#include <system_error>


namespace Michitai::Lan::Net::Detail
{
    void EnsureSocketsInitialized()
    {
#ifdef _WIN32
        static std::once_flag once;
        std::call_once(once, []
        {
            WSADATA data{};
            if (WSAStartup(MAKEWORD(2, 2), &data) != 0)
                throw std::runtime_error("WSAStartup failed");
        });
#endif
    }

    int LastError()
    {
#ifdef _WIN32
        return WSAGetLastError();
#else
        return errno;
#endif
    }

    std::string ErrorText(int error)
    {
#ifdef _WIN32
        char* buffer = nullptr;
        const DWORD length = FormatMessageA(
            FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
            nullptr, static_cast<DWORD>(error), 0,
            reinterpret_cast<LPSTR>(&buffer), 0, nullptr);
        std::string text = (length > 0 && buffer) ? std::string(buffer, length) : "socket error " + std::to_string(error);
        if (buffer) LocalFree(buffer);
        while (!text.empty() && (text.back() == '\r' || text.back() == '\n'))
            text.pop_back();
        return text;
#else
        return std::strerror(error);
#endif
    }

    void ThrowSocketError(const char* operation)
    {
        const int error = LastError();
        throw std::system_error(error, std::system_category(),
                                std::string(operation) + ": " + ErrorText(error));
    }

    sockaddr_in ToSockAddr(const IPEndPoint& point)
    {
        sockaddr_in address{};
        address.sin_family = AF_INET;
        address.sin_port = htons(static_cast<std::uint16_t>(point.Port));
        address.sin_addr.s_addr = htonl(point.Address.Value());
        return address;
    }

    IPEndPoint FromSockAddr(const sockaddr_in& address)
    {
        return IPEndPoint(IPAddress::FromValue(ntohl(address.sin_addr.s_addr)), ntohs(address.sin_port));
    }


    Socket Socket::TcpV4()
    {
        EnsureSocketsInitialized();

        SocketHandle handle = ::socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
        if (handle == InvalidHandle)
            ThrowSocketError("socket");

        return Socket(handle);
    }

    Socket Socket::UdpV4()
    {
        EnsureSocketsInitialized();

        SocketHandle handle = ::socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
        if (handle == InvalidHandle)
            ThrowSocketError("socket");

        return Socket(handle);
    }

    void Socket::Close()
    {
        if (_handle == InvalidHandle)
            return;

#ifdef _WIN32
        ::closesocket(_handle);
#else
        ::close(_handle);
#endif
        _handle = InvalidHandle;
    }

    void Socket::Shutdown()
    {
        if (_handle == InvalidHandle)
            return;

#ifdef _WIN32
        ::shutdown(_handle, SD_BOTH);
#else
        ::shutdown(_handle, SHUT_RDWR);
#endif
    }

    void Socket::SetNoDelay(bool enabled)
    {
        const int value = enabled ? 1 : 0;
        ::setsockopt(_handle, IPPROTO_TCP, TCP_NODELAY,
                     reinterpret_cast<const char*>(&value), sizeof(value));
    }

    void Socket::SetBroadcast(bool enabled)
    {
        const int value = enabled ? 1 : 0;
        ::setsockopt(_handle, SOL_SOCKET, SO_BROADCAST,
                     reinterpret_cast<const char*>(&value), sizeof(value));
    }

    void Socket::SetReuseAddress(bool enabled)
    {
        const int value = enabled ? 1 : 0;
        ::setsockopt(_handle, SOL_SOCKET, SO_REUSEADDR,
                     reinterpret_cast<const char*>(&value), sizeof(value));
    }

    void Socket::SetSendBufferSize(int size)
    {
        ::setsockopt(_handle, SOL_SOCKET, SO_SNDBUF,
                     reinterpret_cast<const char*>(&size), sizeof(size));
    }

    void Socket::SetReceiveBufferSize(int size)
    {
        ::setsockopt(_handle, SOL_SOCKET, SO_RCVBUF,
                     reinterpret_cast<const char*>(&size), sizeof(size));
    }

    bool Socket::Bind(const IPEndPoint& point)
    {
        sockaddr_in address = ToSockAddr(point);
        return ::bind(_handle, reinterpret_cast<sockaddr*>(&address), sizeof(address)) == 0;
    }

    void Socket::Listen(int backlog)
    {
        if (::listen(_handle, backlog) != 0)
            ThrowSocketError("listen");
    }

    void Socket::Connect(const IPEndPoint& point)
    {
        sockaddr_in address = ToSockAddr(point);
        if (::connect(_handle, reinterpret_cast<sockaddr*>(&address), sizeof(address)) != 0)
            ThrowSocketError("connect");
    }

    Socket Socket::Accept(IPEndPoint* remote)
    {
        sockaddr_in address{};
#ifdef _WIN32
        int length = sizeof(address);
#else
        socklen_t length = sizeof(address);
#endif
        SocketHandle handle = ::accept(_handle, reinterpret_cast<sockaddr*>(&address), &length);
        if (handle == InvalidHandle)
            return Socket();

        if (remote)
            *remote = FromSockAddr(address);

        return Socket(handle);
    }

    int Socket::Receive(std::uint8_t* buffer, int count)
    {
#ifdef _WIN32
        const int result = ::recv(_handle, reinterpret_cast<char*>(buffer), count, 0);
#else
        const int result = static_cast<int>(::recv(_handle, buffer, static_cast<std::size_t>(count), 0));
#endif
        return result;
    }

    int Socket::Send(const std::uint8_t* buffer, int count)
    {
#ifdef _WIN32
        return ::send(_handle, reinterpret_cast<const char*>(buffer), count, 0);
#else
        return static_cast<int>(::send(_handle, buffer, static_cast<std::size_t>(count), 0));
#endif
    }

    bool Socket::SendAll(const std::uint8_t* buffer, int count)
    {
        int sent = 0;
        while (sent < count)
        {
            const int result = Send(buffer + sent, count - sent);
            if (result <= 0)
                return false;
            sent += result;
        }
        return true;
    }

    int Socket::SendTo(const IPEndPoint& target, const std::uint8_t* buffer, int count)
    {
        sockaddr_in address = ToSockAddr(target);
#ifdef _WIN32
        return ::sendto(_handle, reinterpret_cast<const char*>(buffer), count, 0,
                        reinterpret_cast<sockaddr*>(&address), sizeof(address));
#else
        return static_cast<int>(::sendto(_handle, buffer, static_cast<std::size_t>(count), 0,
                                         reinterpret_cast<sockaddr*>(&address), sizeof(address)));
#endif
    }

    int Socket::ReceiveFrom(std::uint8_t* buffer, int count, IPEndPoint* remote)
    {
        sockaddr_in address{};
#ifdef _WIN32
        int length = sizeof(address);
        const int result = ::recvfrom(_handle, reinterpret_cast<char*>(buffer), count, 0,
                                      reinterpret_cast<sockaddr*>(&address), &length);
#else
        socklen_t length = sizeof(address);
        const int result = static_cast<int>(::recvfrom(_handle, buffer, static_cast<std::size_t>(count), 0,
                                                       reinterpret_cast<sockaddr*>(&address), &length));
#endif
        if (result >= 0 && remote)
            *remote = FromSockAddr(address);

        return result;
    }

    bool Socket::WaitReadable(int timeoutMilliseconds)
    {
        if (_handle == InvalidHandle)
            return false;

        fd_set set;
        FD_ZERO(&set);
        FD_SET(_handle, &set);

        timeval timeout{};
        timeout.tv_sec = timeoutMilliseconds / 1000;
        timeout.tv_usec = (timeoutMilliseconds % 1000) * 1000;

        const int result = ::select(static_cast<int>(_handle) + 1, &set, nullptr, nullptr, &timeout);
        return result > 0;
    }

    std::optional<IPEndPoint> Socket::LocalEndPoint() const
    {
        if (_handle == InvalidHandle)
            return std::nullopt;

        sockaddr_in address{};
#ifdef _WIN32
        int length = sizeof(address);
#else
        socklen_t length = sizeof(address);
#endif
        if (::getsockname(_handle, reinterpret_cast<sockaddr*>(&address), &length) != 0)
            return std::nullopt;

        return FromSockAddr(address);
    }
}
