//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include <Michitai/Lan/Net/EndPoint.hpp>

#include <stdexcept>


namespace Michitai::Lan::Net
{
    IPAddress IPAddress::Parse(const std::string& text)
    {
        IPAddress address;
        if (!TryParse(text, address))
            throw std::invalid_argument("Invalid IPv4 address: " + text);
        return address;
    }

    bool IPAddress::TryParse(const std::string& text, IPAddress& address)
    {
        std::uint32_t parts[4] = {};
        int part = 0;
        std::uint32_t current = 0;
        bool hasDigit = false;

        for (char c : text)
        {
            if (c >= '0' && c <= '9')
            {
                current = current * 10 + static_cast<std::uint32_t>(c - '0');
                if (current > 255)
                    return false;
                hasDigit = true;
            }
            else if (c == '.')
            {
                if (!hasDigit || part >= 3)
                    return false;
                parts[part++] = current;
                current = 0;
                hasDigit = false;
            }
            else
            {
                return false;
            }
        }

        if (!hasDigit || part != 3)
            return false;

        parts[3] = current;

        address = IPAddress(static_cast<std::uint8_t>(parts[0]), static_cast<std::uint8_t>(parts[1]),
                            static_cast<std::uint8_t>(parts[2]), static_cast<std::uint8_t>(parts[3]));
        return true;
    }

    std::string IPAddress::ToString() const
    {
        return std::to_string((_value >> 24) & 0xFF) + "." +
               std::to_string((_value >> 16) & 0xFF) + "." +
               std::to_string((_value >> 8) & 0xFF) + "." +
               std::to_string(_value & 0xFF);
    }
}
