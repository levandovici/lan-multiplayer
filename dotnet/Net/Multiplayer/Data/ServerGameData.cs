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
    /// Represents server game data including server ID and connected client data.
    /// Implements IJsonStorage for JSON serialization.
    /// </summary>
    [Serializable]
    public sealed class ServerGameData : IJsonStorage
    {
        private string _server_id;

        private ServerClientGameData[] _clients;

        private string _json;

        private readonly object _server_id_lock;

        private readonly object _clients_lock;

        private readonly object _json_lock;

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
        /// Gets or sets the array of connected client data. Thread-safe.
        /// </summary>
        public ServerClientGameData[] Clients
    {
        get
        {
            lock (_clients_lock)
            {
                return _clients;
            }
        }

        set
        {
            lock (_clients_lock)
            {
                _clients = value;
            }
        }
    }

        /// <summary>
        /// Gets or sets the JSON representation. Thread-safe.
        /// </summary>
        public string Json
    {
        get
        {
            lock(_json_lock)
            {
                return _json;
            }
        }

        set
        {
            lock(_json_lock)
            {
                _json = value;
            }
        }
    }



        /// <summary>
        /// Gets a public view of the server data (with public client data).
        /// </summary>
        [JsonIgnore]
        public ServerGameData Public
    {
        get
        {
            ServerClientGameData[] clients;


            lock (_clients_lock)
            {
                clients = new ServerClientGameData[_clients.Length];

                for (int i = 0; i < _clients.Length; i++)
                {
                    clients[i] = _clients[i].Public;
                }
            }


            return new ServerGameData(ServerID, clients);
        }
    }



        /// <summary>
        /// Initializes a new instance of ServerGameData with the specified server ID and clients.
        /// </summary>
        /// <param name="server_id">The server ID.</param>
        /// <param name="clients">The array of client data.</param>
        public ServerGameData(string server_id, ServerClientGameData[] clients)
    {
        _server_id = server_id;

        _clients = clients;

        _json = "";


        _server_id_lock = new object();

        _clients_lock = new object();

        _json_lock = new object();
    }

        /// <summary>
        /// Initializes a new instance of ServerGameData with the specified server ID and no clients.
        /// </summary>
        /// <param name="server_id">The server ID.</param>
        public ServerGameData(string server_id) : this(server_id, new ServerClientGameData[0])
    {
    }

        /// <summary>
        /// Initializes a new instance of ServerGameData with default values.
        /// </summary>
        public ServerGameData()
    {
        _server_id = "default";

        _clients = new ServerClientGameData[0];

        _json = "";


        _server_id_lock = new object();

        _clients_lock = new object();

        _json_lock = new object();
    }



        /// <summary>
        /// Adds a player to the server data. Thread-safe.
        /// </summary>
        /// <param name="player">The player data to add.</param>
        public void AddPlayer(ServerClientGameData player)
    {
        lock (_clients_lock)
        {
            ServerClientGameData[] players = new ServerClientGameData[_clients.Length + 1];

            for (int i = 0; i < _clients.Length; i++)
            {
                players[i] = _clients[i];
            }

            players[_clients.Length] = player;

            _clients = players;
        }
    }

        /// <summary>
        /// Deletes a player from the server data. Thread-safe.
        /// </summary>
        /// <param name="player">The credentials of the player to delete.</param>
        public void DeletePlayer(Credentials player)
    {
        lock (_clients_lock)
        {
            ServerClientGameData[] clients = new ServerClientGameData[_clients.Length - 1];

            for (int i = 0, id = 0; i < _clients.Length; i++)
            {
                if (_clients[i].Credentials != player)
                {
                    clients[id++] = _clients[i];
                }
            }

            _clients = clients;
        }
    }

        /// <summary>
        /// Checks if a player with the specified credentials exists. Thread-safe.
        /// </summary>
        /// <param name="player">The credentials to check.</param>
        /// <returns>True if the player exists; otherwise, false.</returns>
        public bool Contains(Credentials player)
    {
        lock (_clients_lock)
        {
            for (int i = 0; i < _clients.Length; i++)
            {
                if (_clients[i].Credentials == player)
                {
                    return true;
                }
            }
        }

        return false;
    }



        /// <summary>
        /// Tries to get the public player data for the specified credentials. Thread-safe.
        /// </summary>
        /// <param name="player">The credentials to look up.</param>
        /// <param name="client">When this method returns, contains the public client data if found.</param>
        /// <returns>True if the player was found; otherwise, false.</returns>
        public bool TryGetPublicPlayerData(Credentials player, out ServerClientGameData client)
    {
        lock (_clients_lock)
        {
            for (int i = 0; i < _clients.Length; i++)
            {
                if (_clients[i].Credentials == player)
                {
                    client = _clients[i].Public;

                    return true;
                }
            }
        }


        client = null;

        return false;
    }

        /// <summary>
        /// Tries to get the private player data for the specified credentials. Thread-safe.
        /// </summary>
        /// <param name="player">The credentials to look up.</param>
        /// <param name="client">When this method returns, contains the private client data if found.</param>
        /// <returns>True if the player was found; otherwise, false.</returns>
        public bool TryGetPrivatePlayerData(Credentials player, out ServerClientGameData client)
    {
        lock (_clients_lock)
        {
            for (int i = 0; i < _clients.Length; i++)
            {
                if (_clients[i].Credentials == player)
                {
                    client = _clients[i];

                    return true;
                }
            }
        }


        client = null;

        return false;
    }



    public T Get<T>()
    {
        return JsonSerializer.Deserialize<T>(Json);
    }

    public void Set<T>(T @object)
    {
        Json = JsonSerializer.Serialize(@object);
    }



        /// <summary>
        /// Returns a string representation of the server game data.
        /// </summary>
        /// <returns>A string containing server ID and player count.</returns>
        public override string ToString()
    {
        return $"[SERVER-GAME-DATA][SERVER-ID][{_server_id}][PLAYERS][{_clients.Length}]";
    }
}
}
