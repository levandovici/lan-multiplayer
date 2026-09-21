//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <string>
#include <vector>

#include <Michitai/Lan/EPlatform.hpp>
#include <Michitai/Lan/Net/EndPoint.hpp>


namespace Michitai::Lan::Net
{
    /// <summary>
    /// Provides static methods for LAN network operations and IP address retrieval.
    /// </summary>
    class Lan
    {
    public:
        /// <summary>
        /// Gets all local IPv4 addresses for the specified platform.
        /// </summary>
        /// <param name="platform">The platform to get addresses for.</param>
        /// <returns>Vector of all local non-loopback IPv4 addresses.</returns>
        static std::vector<IPAddress> LocalIPv4Addresses(EPlatform platform);

        /// <summary>
        /// Gets all local IPv4 broadcast masks for the specified platform.
        /// </summary>
        /// <returns>Vector of masks, e.g. 192.168.0.255.</returns>
        static std::vector<IPAddress> LocalIPv4Masks(EPlatform platform);

        /// <summary>Converts IPv4 addresses to broadcast masks.</summary>
        static std::vector<IPAddress> LocalIPv4Masks(const std::vector<IPAddress>& addresses);

        /// <summary>Attempts to get all local IPv4 addresses for the specified platform.</summary>
        static bool TryGetLocalIPv4Addresses(EPlatform platform, std::vector<IPAddress>& addresses)
        {
            addresses = LocalIPv4Addresses(platform);
            return !addresses.empty();
        }

        /// <summary>Attempts to get all local IPv4 broadcast masks for the specified platform.</summary>
        static bool TryGetLocalIPv4Masks(EPlatform platform, std::vector<IPAddress>& masks)
        {
            masks = LocalIPv4Masks(platform);
            return !masks.empty();
        }

        /// <summary>Attempts to get all local IPv4 broadcast masks as strings.</summary>
        static bool TryGetLocalIPv4Masks(EPlatform platform, std::vector<std::string>& masks)
        {
            const std::vector<IPAddress> addresses = LocalIPv4Masks(platform);
            masks.clear();
            masks.reserve(addresses.size());
            for (const IPAddress& address : addresses)
            {
                masks.push_back(address.ToString());
            }
            return !masks.empty();
        }
    };
}
