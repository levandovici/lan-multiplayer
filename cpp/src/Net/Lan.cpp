//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include <Michitai/Lan/Net/Lan.hpp>
#include "SocketInternal.hpp"

#ifdef _WIN32
    #include <iphlpapi.h>
#else
    #include <ifaddrs.h>
    #include <net/if.h>
    #include <netdb.h>
#endif


namespace Michitai::Lan::Net
{
    std::vector<IPAddress> Lan::LocalIPv4Addresses(EPlatform platform)
    {
        std::vector<IPAddress> addresses;

        if (HasFlag(platform, EPlatform::Windows) ||
            HasFlag(platform, EPlatform::Linux) ||
            HasFlag(platform, EPlatform::MacOS))
        {
            Detail::EnsureSocketsInitialized();

#ifdef _WIN32
            ULONG size = 0;
            if (GetAdaptersAddresses(AF_INET, GAA_FLAG_SKIP_ANYCAST | GAA_FLAG_SKIP_MULTICAST |
                                     GAA_FLAG_SKIP_DNS_SERVER, nullptr, nullptr, &size) != ERROR_BUFFER_OVERFLOW)
            {
                return addresses;
            }

            std::vector<std::uint8_t> buffer(size);
            auto* adapters = reinterpret_cast<IP_ADAPTER_ADDRESSES*>(buffer.data());

            if (GetAdaptersAddresses(AF_INET, GAA_FLAG_SKIP_ANYCAST | GAA_FLAG_SKIP_MULTICAST |
                                     GAA_FLAG_SKIP_DNS_SERVER, nullptr, adapters, &size) != NO_ERROR)
            {
                return addresses;
            }

            for (IP_ADAPTER_ADDRESSES* adapter = adapters; adapter; adapter = adapter->Next)
            {
                if (adapter->OperStatus != IfOperStatusUp)
                    continue;

                for (IP_ADAPTER_UNICAST_ADDRESS* unicast = adapter->FirstUnicastAddress;
                     unicast; unicast = unicast->Next)
                {
                    if (unicast->Address.lpSockaddr->sa_family != AF_INET)
                        continue;

                    auto* ipv4 = reinterpret_cast<sockaddr_in*>(unicast->Address.lpSockaddr);
                    IPAddress address = IPAddress::FromValue(ntohl(ipv4->sin_addr.s_addr));

                    if (!IPAddress::IsLoopback(address))
                        addresses.push_back(address);
                }
            }
#else
            ifaddrs* list = nullptr;
            if (getifaddrs(&list) != 0)
                return addresses;

            for (ifaddrs* item = list; item; item = item->ifa_next)
            {
                if (!item->ifa_addr || item->ifa_addr->sa_family != AF_INET)
                    continue;
                if (!(item->ifa_flags & IFF_UP))
                    continue;

                auto* ipv4 = reinterpret_cast<sockaddr_in*>(item->ifa_addr);
                IPAddress address = IPAddress::FromValue(ntohl(ipv4->sin_addr.s_addr));

                if (!IPAddress::IsLoopback(address))
                    addresses.push_back(address);
            }

            freeifaddrs(list);
#endif
        }
        else if (HasFlag(platform, EPlatform::Android) || HasFlag(platform, EPlatform::IOS))
        {
            Detail::EnsureSocketsInitialized();

            char host[256] = {};
            if (::gethostname(host, sizeof(host) - 1) != 0)
                return addresses;

            addrinfo hints{};
            hints.ai_family = AF_INET;

            addrinfo* result = nullptr;
            if (::getaddrinfo(host, nullptr, &hints, &result) != 0 || !result)
                return addresses;

            for (addrinfo* item = result; item; item = item->ai_next)
            {
                if (item->ai_family != AF_INET)
                    continue;

                auto* ipv4 = reinterpret_cast<sockaddr_in*>(item->ai_addr);
                IPAddress address = IPAddress::FromValue(ntohl(ipv4->sin_addr.s_addr));

                if (!IPAddress::IsLoopback(address))
                    addresses.push_back(address);
            }

            ::freeaddrinfo(result);
        }

        return addresses;
    }

    std::vector<IPAddress> Lan::LocalIPv4Masks(EPlatform platform)
    {
        return LocalIPv4Masks(LocalIPv4Addresses(platform));
    }

    std::vector<IPAddress> Lan::LocalIPv4Masks(const std::vector<IPAddress>& addresses)
    {
        std::vector<IPAddress> masks;
        masks.reserve(addresses.size());

        for (const IPAddress& address : addresses)
        {
            masks.push_back(IPAddress::FromValue(address.Value() | 0xFF));
        }

        return masks;
    }
}
