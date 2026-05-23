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
    /// Represents game data for a specific client connected to the server.
    /// </summary>
    public sealed class ServerClientGameData
    {
        public JsonStorage data;

        public Credentials credentials;

        private readonly object _data_lock;

        private readonly object _credentials_lock;

        /// <summary>
        /// Gets or sets the client's game data storage.
        /// </summary>
        public JsonStorage Data
    {
        get
        {
            lock (_data_lock)
            {
                return data;
            }
        }

        set
        {
            lock (_data_lock)
            {
                data = value;
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
        /// Gets a public version of the client data with public credentials.
        /// </summary>
        public ServerClientGameData Public => new ServerClientGameData(Data, Credentials.Public);



        /// <summary>
        /// Initializes a new instance of ServerClientGameData with the specified game data and credentials.
        /// </summary>
        /// <param name="gameData">The client's game data storage.</param>
        /// <param name="credentials">The client's credentials.</param>
        public ServerClientGameData(JsonStorage gameData, Credentials credentials)
    {
        data = gameData;

        this.credentials = credentials;


        _data_lock = new object();

        _credentials_lock = new object();
    }

        /// <summary>
        /// Initializes a new instance of ServerClientGameData with default credentials.
        /// </summary>
        public ServerClientGameData()
    {
        credentials = new Credentials("id", "password");


        _data_lock = new object();

        _credentials_lock = new object();
    }



        /// <summary>
        /// Returns a string representation of the client game data.
        /// </summary>
        /// <returns>A string containing the credentials and data.</returns>
        public override string ToString()
    {
        return $"[SERVER-CLIENT-GAME-DATA]{credentials}{data?.ToString()}";
    }
}
}
