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
    /// Represents client game data including server ID and credentials.
    /// </summary>
    [Serializable]
    public sealed class ClientGameData
    {
        private string _server_id;

        private Credentials _credentials;

        private readonly object _server_id_lock;

        private readonly object _credentials_lock;

        /// <summary>
        /// Gets or sets the server ID. Thread-safe.
        /// </summary>
        public string Server_ID
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
        /// Gets or sets the client credentials. Thread-safe.
        /// </summary>
        public Credentials Credentials
    {
        get
        {
            lock (_credentials_lock)
            {
                return _credentials;
            }
        }

        set
        {
            lock (_credentials_lock)
            {
                _credentials = value;
            }
        }
    }



        /// <summary>
        /// Initializes a new instance of ClientGameData with the specified server ID and credentials.
        /// </summary>
        /// <param name="server_id">The server ID.</param>
        /// <param name="credentials">The client credentials.</param>
        public ClientGameData(string server_id, Credentials credentials)
    {
        _server_id = server_id;

        _credentials = credentials;


        _server_id_lock = new object();

        _credentials_lock = new object();
    }

        /// <summary>
        /// Initializes a new instance of ClientGameData with default values.
        /// </summary>
        public ClientGameData()
    {
        _server_id = "default";

        _credentials = new Credentials("id", "password");


        _server_id_lock = new object();

        _credentials_lock = new object();
    }
}
}
