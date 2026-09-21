//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <cstdint>
#include <random>
#include <string>


namespace Michitai::Lan
{
    /// <summary>
    /// Generates a random RFC 4122 version 4 UUID string in the same lowercase
    /// hyphenated format as Guid.NewGuid().ToString().
    /// </summary>
    inline std::string NewGuid()
    {
        thread_local std::mt19937_64 rng(std::random_device{}());

        std::uint8_t bytes[16];

        for (int i = 0; i < 16; i += 8)
        {
            const std::uint64_t value = rng();
            for (int j = 0; j < 8; j++)
            {
                bytes[i + j] = static_cast<std::uint8_t>(value >> (j * 8));
            }
        }

        // Version 4, variant 1.
        bytes[6] = static_cast<std::uint8_t>((bytes[6] & 0x0F) | 0x40);
        bytes[8] = static_cast<std::uint8_t>((bytes[8] & 0x3F) | 0x80);

        static constexpr char hex[] = "0123456789abcdef";

        std::string result;
        result.reserve(36);

        for (int i = 0; i < 16; i++)
        {
            if (i == 4 || i == 6 || i == 8 || i == 10)
                result += '-';

            result += hex[bytes[i] >> 4];
            result += hex[bytes[i] & 0x0F];
        }

        return result;
    }
}
