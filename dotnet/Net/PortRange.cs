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
    /// Represents a range of network ports with predefined ranges and port management functionality.
    /// </summary>
    public struct PortRange
    {
        /// <summary>
        /// System ports (0-1023).
        /// </summary>
        public static PortRange System => new PortRange(0, 1023);

        /// <summary>
        /// Registered ports (1024-49151).
        /// </summary>
        public static PortRange Registered => new PortRange(1024, 49151);

        /// <summary>
        /// Dynamic/private ports (49152-65535).
        /// </summary>
        public static PortRange Dynamic => new PortRange(49152, 65535);

        /// <summary>
        /// All valid ports (Min-Max).
        /// </summary>
        public static PortRange All => new PortRange(Min, Max);

        /// <summary>
        /// Broadcast ports (1024 ports, 64512-65535).
        /// </summary>
        public static PortRange Broadcast => new PortRange(64512, 65535);

        /// <summary>
        /// Simplified broadcast ports (128 ports, 65408-65535).
        /// </summary>
        public static PortRange BroadcastSimplified => new PortRange(65408, 65535);

        /// <summary>
        /// Minimum valid port number.
        /// </summary>
        public const int Min = 1024;

        /// <summary>
        /// Maximum valid port number.
        /// </summary>
        public const int Max = 65535;

        /// <summary>
        /// The first port in the range.
        /// </summary>
        public int First;

        /// <summary>
        /// The last port in the range.
        /// </summary>
        public int Last;

        /// <summary>
        /// Gets the total number of ports in the range.
        /// </summary>
        public int Count => Last - First + 1;

        /// <summary>
        /// Gets a port store for managing available ports in this range.
        /// </summary>
        public Store RangeStore => new Store(this);

        /// <summary>
        /// Initializes a new instance of PortRange with the specified first and last ports.
        /// </summary>
        /// <param name="first">The first port in the range.</param>
        /// <param name="last">The last port in the range.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when first is less than Min or last is greater than Max.</exception>
        public PortRange(int first, int last)
    {
        if (first < Min)
            throw new ArgumentOutOfRangeException("First can't be less than ServerPortRange.Min");

        if (last > Max)
            throw new ArgumentOutOfRangeException("Last can't be more than ServerPortRange.Max");


        First = first;

        Last = last;
    }



    public class Store
    {
        private Random _random;

        private List<int> _ports;

        private int _count;



        /// <summary>
        /// 
        /// </summary>
        public int RandomPort
        {
            get
            {
                if (_count > 0)
                {
                    int id = _random.Next(0, _count);


                    int item = _ports[id];

                    _ports.Remove(item);

                    _count--;


                    return item;
                }
                else
                {
                    throw new IndexOutOfRangeException("ServerPortRange.Store is Empty! All ports are in use!");
                }
            }
        }



        /// <summary>
        /// Initializes a new instance of Store with all ports from the specified range.
        /// </summary>
        /// <param name="range">The port range to populate the store with.</param>
        public Store(PortRange range)
        {
            _random = new Random();

            _ports = new List<int>();

            for (int port = range.First; port <= range.Last; port++)
            {
                _ports.Add(port);

                _count++;
            }
        }
    }
}
}
