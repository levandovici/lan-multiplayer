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
    /// Represents information about a multiplayer server.
    /// </summary>
    [Serializable]
    public class ServerInfo
    {
        private int _port;

        private int _clients_count;

        private string _server_id;

        private string _name;

        private readonly object _port_lock;

        private readonly object _clients_count_lock;

        private readonly object _server_id_lock;

        private readonly object _name_lock;

        /// <summary>
        /// Gets or sets the server name. Thread-safe.
        /// </summary>
        public string Name
    {
        get
        {
            lock (_name_lock)
            {
                return _name;
            }
        }

        set
        {
            lock (_name_lock)
            {
                _name = value;
            }
        }
    }

        /// <summary>
        /// Gets or sets the server ID. Thread-safe.
        /// </summary>
        public string ServerID
    {
        get
        {
            lock (_server_id_lock)
            {
                return _server_id;
            }
        }

        set
        {
            lock (_server_id_lock)
            {
                _server_id = value;
            }
        }
    }

        /// <summary>
        /// Gets or sets the server port. Thread-safe.
        /// </summary>
        public int Port
    {
        get
        {
            lock (_port_lock)
            {
                return _port;
            }
        }

        set
        {
            lock (_port_lock)
            {
                _port = value;
            }
        }
    }

        /// <summary>
        /// Gets or sets the number of connected clients. Thread-safe.
        /// </summary>
        public int ClientsCount
    {
        get
        {
            lock (_clients_count_lock)
            {
                return _clients_count;
            }
        }

        set
        {
            lock (_clients_count_lock)
            {
                _clients_count = value;
            }
        }
    }



        /// <summary>
        /// Initializes a new instance of ServerInfo with the specified parameters.
        /// </summary>
        /// <param name="port">The server port.</param>
        /// <param name="name">The server name.</param>
        /// <param name="serverID">The server ID.</param>
        /// <param name="clientsCount">The number of connected clients.</param>
        public ServerInfo(int port, string name, string serverID, int clientsCount)
    {
        _port = port;

        _name = name;

        _server_id = serverID;

        _clients_count = clientsCount;


        _port_lock = new object();

        _name_lock = new object();

        _server_id_lock = new object();

        _clients_count_lock = new object();
    }

        /// <summary>
        /// Initializes a new instance of ServerInfo with default values.
        /// </summary>
        public ServerInfo()
    {
        _port = 0;

        _name = "default";

        _server_id = "default";

        _clients_count = -1;


        _port_lock = new object();

        _name_lock = new object();

        _server_id_lock = new object();

        _clients_count_lock = new object();
    }
}
}
