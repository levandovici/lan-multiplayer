//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <cstdint>
#include <string>


namespace Michitai::Lan::Net
{
    /// <summary>
    /// IPv4 address equivalent of System.Net.IPAddress (IPv4 only — the library's
    /// LAN discovery is built around IPv4 broadcast).
    /// </summary>
    class IPAddress
    {
    public:
        /// <summary>0.0.0.0</summary>
        IPAddress() = default;

        IPAddress(std::uint8_t a, std::uint8_t b, std::uint8_t c, std::uint8_t d)
            : _value(static_cast<std::uint32_t>(a) << 24 |
                     static_cast<std::uint32_t>(b) << 16 |
                     static_cast<std::uint32_t>(c) << 8  |
                     static_cast<std::uint32_t>(d))
        {
        }

        /// <summary>Parses a dotted-quad string. Throws std::invalid_argument when invalid.</summary>
        static IPAddress Parse(const std::string& text);

        /// <summary>Tries to parse a dotted-quad string.</summary>
        static bool TryParse(const std::string& text, IPAddress& address);

        /// <summary>0.0.0.0 — listens on all interfaces.</summary>
        static IPAddress Any() { return IPAddress(0, 0, 0, 0); }

        /// <summary>255.255.255.255 — limited broadcast.</summary>
        static IPAddress Broadcast() { return IPAddress(255, 255, 255, 255); }

        /// <summary>127.0.0.1</summary>
        static IPAddress Loopback() { return IPAddress(127, 0, 0, 1); }

        /// <summary>Checks whether the address is in 127.0.0.0/8.</summary>
        static bool IsLoopback(const IPAddress& address)
        {
            return (address._value & 0xFF000000u) == 0x7F000000u;
        }

        /// <summary>Address value in host byte order.</summary>
        std::uint32_t Value() const { return _value; }

        /// <summary>Constructs from a host-order 32-bit value.</summary>
        static IPAddress FromValue(std::uint32_t value)
        {
            IPAddress address;
            address._value = value;
            return address;
        }

        std::string ToString() const;

        bool operator==(const IPAddress&) const = default;
        bool operator<(const IPAddress& other) const { return _value < other._value; }

    private:
        std::uint32_t _value = 0;
    };


    /// <summary>
    /// IP endpoint equivalent of System.Net.IPEndPoint.
    /// </summary>
    struct IPEndPoint
    {
        Net::IPAddress Address;
        int Port = 0;

        IPEndPoint() = default;
        IPEndPoint(const Net::IPAddress& address, int port) : Address(address), Port(port) {}

        std::string ToString() const { return Address.ToString() + ":" + std::to_string(Port); }

        bool operator==(const IPEndPoint&) const = default;
        bool operator<(const IPEndPoint& other) const
        {
            if (Address != other.Address)
                return Address < other.Address;
            return Port < other.Port;
        }
    };
}
