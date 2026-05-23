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

using Michitai.Lan;
using Michitai.Lan.Data;
using Michitai.Lan.Net;
using Michitai.Lan.Net.Multiplayer;
using Michitai.Lan.Net.Multiplayer.Chat;
using Michitai.Lan.Net.Multiplayer.Commands;
using Michitai.Lan.Net.Multiplayer.Data;
using Michitai.Lan.Debug;
using UnityEngine;

namespace Michitai.Lan.Net
{
            public struct PortRange
{
    /// <summary>
    /// 
    /// </summary>
    public static PortRange System => new PortRange(0, 1023);


    /// <summary>
    /// 
    /// </summary>
    public static PortRange Registered => new PortRange(1024, 49151);

    /// <summary>
    /// 
    /// </summary>
    public static PortRange Dynamic => new PortRange(49152, 65535);


    /// <summary>
    /// 
    /// </summary>
    public static PortRange All => new PortRange(Min, Max);


    /// <summary>
    /// 1024 ports - inclusive between 64512 - 65535
    /// </summary>
    public static PortRange Broadcast => new PortRange(64512, 65535);

    /// <summary>
    /// 128 ports - inclusive between 65408 - 65535
    /// </summary>
    public static PortRange BroadcastSimplified => new PortRange(65408, 65535);



    /// <summary>
    ///
    /// </summary>
    public const int Min = 1024;

    /// <summary>
    /// 
    /// </summary>
    public const int Max = 65535;



    /// <summary>
    /// 
    /// </summary>
    public int First;

    /// <summary>
    /// 
    /// </summary>
    public int Last;



    /// <summary>
    /// 
    /// </summary>
    public int Count => Last - First + 1;

    /// <summary>
    /// 
    /// </summary>
    public Store RangeStore => new Store(this);



    /// <summary>
    /// </summary>
    /// <param name="first"></param>
    /// <param name="last"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
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
        private System.Random _random;

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



        public Store(PortRange range)
        {
            _random = new System.Random();

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
