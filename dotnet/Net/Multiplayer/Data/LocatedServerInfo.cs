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


namespace Michitai.Lan.Net.Multiplayer.Data
{
    /// <summary>
    /// Represents a discovered server with its information and network endpoint.
    /// </summary>
    public class LocatedServerInfo
    {
        private ServerInfo _server_info;

        private IPEndPoint _point;

        private readonly object _server_info_lock;

        private readonly object _point_lock;

        /// <summary>
        /// Gets or sets the server information. Thread-safe.
        /// </summary>
        public ServerInfo ServerInfo
    {
        get
        {
            lock (_server_info_lock)
            {
                return _server_info;
            }
        }

        set
        {
            lock (_server_info_lock)
            {
                _server_info = value;
            }
        }
    }

        /// <summary>
        /// Gets or sets the IP endpoint of the server. Thread-safe.
        /// </summary>
        public IPEndPoint IPEndPoint
    {
        get
        {
            lock (_point_lock)
            {
                return _point;
            }
        }

        set
        {
            lock (_point_lock)
            {
                _point = value;
            }
        }
    }



        /// <summary>
        /// Initializes a new instance of LocatedServerInfo with the specified server info and endpoint.
        /// </summary>
        /// <param name="serverInfo">The server information.</param>
        /// <param name="point">The IP endpoint.</param>
        public LocatedServerInfo(ServerInfo serverInfo, IPEndPoint point)
    {
        _server_info = serverInfo;

        _point = point;


        _server_info_lock = new object();

        _point_lock = new object();
    }

        /// <summary>
        /// Initializes a new instance of LocatedServerInfo with null values.
        /// </summary>
        public LocatedServerInfo()
    {
        _server_info = null;

        _point = null;


        _server_info_lock = new object();

        _point_lock = new object();
    }
}
}
