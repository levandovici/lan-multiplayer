//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//
//  Internal cross-platform socket wrapper (Winsock2 / BSD sockets).
//  Not part of the public API — all public types use IPEndPoint/IPAddress.
//
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <cstdint>
#include <optional>
#include <string>

#include <Michitai/Lan/Net/EndPoint.hpp>

#ifdef _WIN32
    #ifndef WIN32_LEAN_AND_MEAN
        #define WIN32_LEAN_AND_MEAN
    #endif
    #ifndef NOMINMAX
        #define NOMINMAX
    #endif
    #include <winsock2.h>
    #include <ws2tcpip.h>
    // windows.h defines GetMessage -> GetMessageA/W, which would mangle Message::GetMessage().
    #ifdef GetMessage
        #undef GetMessage
    #endif
#else
    #include <sys/socket.h>
    #include <sys/types.h>
    #include <netinet/in.h>
    #include <netinet/tcp.h>
    #include <arpa/inet.h>
    #include <unistd.h>
    #include <fcntl.h>
    #include <errno.h>
#endif


namespace Michitai::Lan::Net::Detail
{
#ifdef _WIN32
    using SocketHandle = SOCKET;
    inline constexpr SocketHandle InvalidHandle = INVALID_SOCKET;
#else
    using SocketHandle = int;
    inline constexpr SocketHandle InvalidHandle = -1;
#endif

    /// <summary>Initializes the socket subsystem once (WSAStartup on Windows).</summary>
    void EnsureSocketsInitialized();

    /// <summary>Last socket error code (WSAGetLastError / errno).</summary>
    int LastError();

    /// <summary>Error text for a socket error code.</summary>
    std::string ErrorText(int error);

    /// <summary>Throws std::system_error describing the last socket error.</summary>
    [[noreturn]] void ThrowSocketError(const char* operation);

    /// <summary>sockaddr_in conversion helpers.</summary>
    sockaddr_in ToSockAddr(const IPEndPoint& point);
    IPEndPoint FromSockAddr(const sockaddr_in& address);


    /// <summary>
    /// RAII wrapper over a socket handle. Move-only.
    /// </summary>
    class Socket
    {
    public:
        Socket() = default;
        explicit Socket(SocketHandle handle) : _handle(handle) {}

        ~Socket() { Close(); }

        Socket(const Socket&) = delete;
        Socket& operator=(const Socket&) = delete;

        Socket(Socket&& other) noexcept : _handle(other._handle) { other._handle = InvalidHandle; }

        Socket& operator=(Socket&& other) noexcept
        {
            if (this != &other)
            {
                Close();
                _handle = other._handle;
                other._handle = InvalidHandle;
            }
            return *this;
        }

        static Socket TcpV4();
        static Socket UdpV4();

        bool Valid() const { return _handle != InvalidHandle; }
        SocketHandle Handle() const { return _handle; }

        /// <summary>Releases the handle without closing it.</summary>
        SocketHandle Release() { SocketHandle h = _handle; _handle = InvalidHandle; return h; }

        void Close();
        void Shutdown();

        void SetNoDelay(bool enabled);
        void SetBroadcast(bool enabled);
        void SetReuseAddress(bool enabled);
        void SetSendBufferSize(int size);
        void SetReceiveBufferSize(int size);

        /// <summary>Binds the socket. Returns false (instead of throwing) when the address is in use.</summary>
        bool Bind(const IPEndPoint& point);
        void Listen(int backlog = SOMAXCONN);
        void Connect(const IPEndPoint& point);

        /// <summary>Accepts one connection. Returns an invalid Socket on failure.</summary>
        Socket Accept(IPEndPoint* remote = nullptr);

        /// <summary>Receives up to count bytes. Returns bytes read, 0 on orderly close, -1 on error.</summary>
        int Receive(std::uint8_t* buffer, int count);

        /// <summary>Sends up to count bytes. Returns bytes sent or -1 on error.</summary>
        int Send(const std::uint8_t* buffer, int count);

        /// <summary>Sends all bytes; retries short writes. Returns false on error.</summary>
        bool SendAll(const std::uint8_t* buffer, int count);

        /// <summary>Sends one datagram. Returns bytes sent or -1 on error.</summary>
        int SendTo(const IPEndPoint& target, const std::uint8_t* buffer, int count);

        /// <summary>Receives one datagram. Returns bytes received or -1 on error.</summary>
        int ReceiveFrom(std::uint8_t* buffer, int count, IPEndPoint* remote);

        /// <summary>Waits until the socket is readable or the timeout expires. Uses select().</summary>
        bool WaitReadable(int timeoutMilliseconds);

        std::optional<IPEndPoint> LocalEndPoint() const;

    private:
        SocketHandle _handle = InvalidHandle;
    };
}
