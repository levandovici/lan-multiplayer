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
    /// Represents client game data on the server side, including player data and credentials.
    /// </summary>
    public sealed class ServerClientGameData
    {
        private PlayerGameData _data;

        private Credentials _credentials;

        private readonly object _data_lock;

        private readonly object _credentials_lock;

        /// <summary>
        /// Gets or sets the player game data. Thread-safe.
        /// </summary>
        public PlayerGameData Data
    {
        get
        {
            lock (_data_lock)
            {
                return _data;
            }
        }

        set
        {
            lock (_data_lock)
            {
                _data = value;
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
        /// Gets a public view of the client data (with public credentials only).
        /// </summary>
        [JsonIgnore]
        public ServerClientGameData Public => new ServerClientGameData(Data, Credentials.Public);



        /// <summary>
        /// Initializes a new instance of ServerClientGameData with the specified data and credentials.
        /// </summary>
        /// <param name="gameData">The player game data.</param>
        /// <param name="credentials">The client credentials.</param>
        public ServerClientGameData(PlayerGameData gameData, Credentials credentials)
    {
        _data = gameData;

        _credentials = credentials;


        _data_lock = new object();

        _credentials_lock = new object();
    }

        /// <summary>
        /// Initializes a new instance of ServerClientGameData with default credentials.
        /// </summary>
        public ServerClientGameData()
    {
        _credentials = new Credentials("id", "password");


        _data_lock = new object();

        _credentials_lock = new object();
    }



        /// <summary>
        /// Returns a string representation of the server client game data.
        /// </summary>
        /// <returns>A string containing credentials and data.</returns>
        public override string ToString()
    {
        return $"[SERVER-CLIENT-GAME-DATA]{_credentials}{_data?.ToString()}";
    }
}
}
