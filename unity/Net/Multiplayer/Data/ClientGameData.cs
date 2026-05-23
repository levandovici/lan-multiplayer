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

namespace Michitai.Lan.Net.Multiplayer.Data
{
    /// <summary>
    /// Represents game data for a multiplayer client, including server ID and credentials.
    /// </summary>
    [Serializable]
    public sealed class ClientGameData
    {
        public string server_id;

        public Credentials credentials;

        private readonly object _server_id_lock;

        private readonly object _credentials_lock;

        /// <summary>
        /// Gets or sets the server ID the client is connected to.
        /// </summary>
        public string Server_ID
    {
        get
        {
            lock (_server_id_lock)
            {
                return server_id;
            }
        }

        set
        {
            lock (_server_id_lock)
            {
                server_id = value;
            }
        }
    }

        /// <summary>
        /// Gets or sets the client's credentials.
        /// </summary>
        public Credentials Credentials
    {
        get
        {
            lock (_credentials_lock)
            {
                return credentials;
            }
        }

        set
        {
            lock (_credentials_lock)
            {
                credentials = value;
            }
        }
    }



        /// <summary>
        /// Initializes a new instance of ClientGameData with the specified server ID and credentials.
        /// </summary>
        /// <param name="server_id">The server ID.</param>
        /// <param name="credentials">The client's credentials.</param>
        public ClientGameData(string server_id, Credentials credentials)
    {
        this.server_id = server_id;

        this.credentials = credentials;


        _server_id_lock = new object();

        _credentials_lock = new object();
    }

        /// <summary>
        /// Initializes a new instance of ClientGameData with default values.
        /// </summary>
        public ClientGameData()
    {
        server_id = "default";

        credentials = new Credentials("id", "password");


        _server_id_lock = new object();

        _credentials_lock = new object();
    }
}
}
