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
    /// Represents game data for a multiplayer server, including all connected clients.
    /// Implements IJsonStorage for JSON serialization.
    /// </summary>
    public sealed class ServerGameData : IJsonStorage
    {
        public string serverID;

        public ServerClientGameData[] clients;

        public string json;

        private readonly object _server_id_lock;

        private readonly object _clients_lock;

        private readonly object _json_lock;

        /// <summary>
        /// Gets or sets the server ID.
        /// </summary>
        public string ServerID
    {
        get
        {
            lock (_server_id_lock)
            {
                return serverID;
            }
        }

        set
        {
            lock (_server_id_lock)
            {
                serverID = value;
            }
        }
    }

        /// <summary>
        /// Gets or sets the array of connected client game data.
        /// </summary>
        public ServerClientGameData[] Clients
    {
        get
        {
            lock (_clients_lock)
            {
                return clients;
            }
        }

        set
        {
            lock (_clients_lock)
            {
                clients = value;
            }
        }
    }

        /// <summary>
        /// Gets or sets the JSON string representation of the server data.
        /// </summary>
        public string Json
    {
        get
        {
            lock (_json_lock)
            {
                return json;
            }
        }

        set
        {
            lock (_json_lock)
            {
                json = value;
            }
        }
    }



        /// <summary>
        /// Gets a public version of the server data with only public client data.
        /// </summary>
        public ServerGameData Public
    {
        get
        {
            ServerClientGameData[] clients;


            lock (_clients_lock)
            {
                clients = new ServerClientGameData[this.clients.Length];

                for (int i = 0; i < this.clients.Length; i++)
                {
                    clients[i] = this.clients[i].Public;
                }
            }


            return new ServerGameData(ServerID, clients);
        }
    }



        /// <summary>
        /// Initializes a new instance of ServerGameData with the specified server ID and clients.
        /// </summary>
        /// <param name="server_id">The server ID.</param>
        /// <param name="clients">The array of connected client game data.</param>
        public ServerGameData(string server_id, ServerClientGameData[] clients)
    {
        serverID = server_id;

        this.clients = clients;

        json = "";


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
        serverID = "default";

        clients = new ServerClientGameData[0];

        json = "";


        _server_id_lock = new object();

        _clients_lock = new object();

        _json_lock = new object();
    }



        /// <summary>
        /// Adds a player to the server data.
        /// </summary>
        /// <param name="player">The player's game data to add.</param>
        public void AddPlayer(ServerClientGameData player)
    {
        lock (_clients_lock)
        {
            ServerClientGameData[] players = new ServerClientGameData[clients.Length + 1];

            for (int i = 0; i < clients.Length; i++)
            {
                players[i] = clients[i];
            }

            players[clients.Length] = player;

            clients = players;
        }
    }

        /// <summary>
        /// Deletes a player from the server data by credentials.
        /// </summary>
        /// <param name="player">The credentials of the player to delete.</param>
        public void DeletePlayer(Credentials player)
    {
        lock (_clients_lock)
        {
            ServerClientGameData[] clients = new ServerClientGameData[this.clients.Length - 1];

            for (int i = 0, id = 0; i < this.clients.Length; i++)
            {
                if (this.clients[i].Credentials != player)
                {
                    clients[id++] = this.clients[i];
                }
            }

            this.clients = clients;
        }
    }

        /// <summary>
        /// Checks if the server contains a player with the specified credentials.
        /// </summary>
        /// <param name="player">The player's credentials.</param>
        /// <returns>True if the player exists; otherwise, false.</returns>
        public bool Contains(Credentials player)
    {
        lock (_clients_lock)
        {
            for (int i = 0; i < clients.Length; i++)
            {
                if (clients[i].Credentials == player)
                {
                    return true;
                }
            }
        }

        return false;
    }



        /// <summary>
        /// Attempts to get the public game data of a player by credentials.
        /// </summary>
        /// <param name="player">The player's credentials.</param>
        /// <param name="client">When this method returns, contains the player's public data if successful.</param>
        /// <returns>True if the player was found; otherwise, false.</returns>
        public bool TryGetPublicPlayerData(Credentials player, out ServerClientGameData client)
    {
        lock (_clients_lock)
        {
            for (int i = 0; i < clients.Length; i++)
            {
                if (clients[i].Credentials == player)
                {
                    client = clients[i].Public;

                    return true;
                }
            }
        }


        client = null;

        return false;
    }

        /// <summary>
        /// Attempts to get the private game data of a player by credentials.
        /// </summary>
        /// <param name="player">The player's credentials.</param>
        /// <param name="client">When this method returns, contains the player's private data if successful.</param>
        /// <returns>True if the player was found; otherwise, false.</returns>
        public bool TryGetPrivatePlayerData(Credentials player, out ServerClientGameData client)
    {
        lock (_clients_lock)
        {
            for (int i = 0; i < clients.Length; i++)
            {
                if (clients[i].Credentials == player)
                {
                    client = clients[i];

                    return true;
                }
            }
        }


        client = null;

        return false;
    }



        /// <summary>
        /// Gets the JSON data deserialized to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <returns>The deserialized object.</returns>
        public T Get<T>()
    {
        return JsonUtility.FromJson<T>(Json);
    }

        /// <summary>
        /// Sets the JSON data by serializing the specified object.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="@object">The object to serialize.</param>
        public void Set<T>(T @object)
    {
        Json = JsonUtility.ToJson(@object);
    }



        /// <summary>
        /// Returns a string representation of the server game data.
        /// </summary>
        /// <returns>A string containing the server ID and player count.</returns>
        public override string ToString()
    {
        return $"[SERVER-GAME-DATA][SERVER-ID][{serverID}][PLAYERS][{clients.Length}]";
    }
}
}
