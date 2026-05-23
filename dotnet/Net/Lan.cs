//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Net.NetworkInformation;
using System.Text;
using System.Runtime;
using System.Runtime.Serialization;


namespace Michitai.Lan.Net
{
    /// <summary>
    /// Provides static methods for LAN network operations and IP address retrieval.
    /// </summary>
    public static class Lan
    {
        /// <summary>
        /// Gets all local IPv4 addresses for the specified platform.
        /// </summary>
        /// <param name="platform">The platform to get addresses for.</param>
        /// <returns>IPAddress Array of all local NetworkInterfaces</returns>
        public static IPAddress[] LocalIPv4Addresses(EPlatform platform)
    {
        var ip_addresses = new List<IPAddress>();

        int count = 0;


        if ((platform & EPlatform.Windows) == EPlatform.Windows ||
            (platform & EPlatform.Linux) == EPlatform.Linux ||
            (platform & EPlatform.MacOS) == EPlatform.MacOS)
        {
            var ni = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface item in ni)
            {
                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork & !IPAddress.IsLoopback(ip.Address))
                        {
                            ip_addresses.Add(ip.Address);

                            count++;
                        }
                    }
                }
            }
        }
        else if ((platform & EPlatform.Android) == EPlatform.Android ||
                (platform & EPlatform.IOS) == EPlatform.IOS)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                {
                    ip_addresses.Add(ip);

                    count++;
                }
            }
        }


        if (count > 0)
        {
            return ip_addresses.ToArray();
        }
        else
        {
            return new IPAddress[0];
        }
    }

        /// <summary>
        /// Gets all local IPv4 broadcast masks for the specified platform.
        /// </summary>
        /// <param name="platform">The platform to get masks for.</param>
        /// <returns>IPAddress Masks Array of all local NetworkInterfaces. Example 192.168.0.255</returns>
        public static IPAddress[] LocalIPv4Masks(EPlatform platform)
    {
        var ips = LocalIPv4Addresses(platform);

        return LocalIPv4Masks(ips);
    }

        /// <summary>
        /// Converts IPv4 addresses to broadcast masks.
        /// </summary>
        /// <param name="iPv4Addresses">IPAddresses Array. Example 192.168.0.1</param>
        /// <returns>IPAddresses Masks Array. Example 192.168.0.255</returns>
        public static IPAddress[] LocalIPv4Masks(IPAddress[] iPv4Addresses)
    {
        return iPv4Addresses.Select(ip =>
        {
            var bytes = ip.GetAddressBytes();

            bytes[bytes.Length - 1] = 255;

            return new IPAddress(bytes);

        }).ToArray();
    }



        /// <summary>
        /// Attempts to get all local IPv4 addresses for the specified platform.
        /// </summary>
        /// <param name="platform">The platform to get addresses for.</param>
        /// <param name="ipAddresses">When this method returns, contains the IP addresses if successful.</param>
        /// <returns>True if addresses were found; otherwise, false.</returns>
        public static bool TryGetLocalIPv4Addresses(EPlatform platform, out IPAddress[] ipAddresses)
    {
        ipAddresses = LocalIPv4Addresses(platform);

        return ipAddresses.Length > 0;
    }

        /// <summary>
        /// Attempts to get all local IPv4 broadcast masks for the specified platform.
        /// </summary>
        /// <param name="platform">The platform to get masks for.</param>
        /// <param name="masks">When this method returns, contains the masks if successful.</param>
        /// <returns>True if masks were found; otherwise, false.</returns>
        public static bool TryGetLocalIPv4Masks(EPlatform platform, out IPAddress[] masks)
    {
        masks = LocalIPv4Masks(platform);

        return masks.Length > 0;
    }

        /// <summary>
        /// Attempts to get all local IPv4 broadcast masks as strings for the specified platform.
        /// </summary>
        /// <param name="platform">The platform to get masks for.</param>
        /// <param name="masks">When this method returns, contains the mask strings if successful.</param>
        /// <returns>True if masks were found; otherwise, false.</returns>
        public static bool TryGetLocalIPv4Masks(EPlatform platform, out string[] masks)
    {
        masks = LocalIPv4Masks(platform).Select(ip => ip.ToString()).ToArray();

        return masks.Length > 0;
    }
}
}
