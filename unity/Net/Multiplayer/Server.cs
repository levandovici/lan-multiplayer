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

namespace Michitai.Lan.Net.Multiplayer
{
            public sealed class Server
{
    private string _name;

    private TCPServer _server;

    private UDPChannel _data_channel;

    private ServerClients _clients;

    private ServerGameData _server_data;



    /// <summary>
    /// 
    /// </summary>
    public event Action<string> OnClientConnected;

    /// <summary>
    /// 
    /// </summary>
    public event Action<string> OnClientDisconnected;

    /// <summary>
    /// 
    /// </summary>
    public event Action<IdentifiedMessage> OnRequest;

    /// <summary>
    /// Event raised when a state datagram is received over the UDP data channel.
    /// </summary>
    public event Action<IPEndPoint, Message> OnState;

    /// <summary>
    /// 
    /// </summary>
    public event Action OnDisconnected;



    /// <summary>
    /// 
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// 
    /// </summary>
    public ServerGameData PublicServerData => _server_data.Public;

    /// <summary>
    /// 
    /// </summary>
    public ServerGameData PrivateServerData => _server_data;

    /// <summary>
    /// 
    /// </summary>
    public IPEndPoint IPEndPoint => _server.IpEndPoint;

    /// <summary>
    /// Gets the UDP data channel. Bound to the same port as the TCP server
    /// (UDP and TCP port spaces are independent), so discovered ServerInfo
    /// already tells clients where to send state.
    /// </summary>
    public UDPChannel DataChannel => _data_channel;



    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="serverGameData"></param>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="bufferSize"></param>
    public Server(string name, ServerGameData serverGameData, IPAddress ip, int port, int bufferSize = 4096) : this(name, serverGameData, new IPEndPoint(ip, port), bufferSize)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="serverGameData"></param>
    /// <param name="ipEndPoint"></param>
    /// <param name="bufferSize"></param>
    public Server(string name, ServerGameData serverGameData, IPEndPoint ipEndPoint, int bufferSize = 4096)
    {
        _name = name;

        _server_data = serverGameData;

        _clients = new ServerClients();

        _server = new TCPServer(ipEndPoint, bufferSize);

        _server.OnClientConnected += (id) => OnClientConnected?.Invoke(id);

        _server.OnClientDisconnected += (id) =>
        {
            Disconnect(id);

            OnClientDisconnected?.Invoke(id);
        };

        _server.OnRequest += (im) => OnRequest?.Invoke(im);

        _server.OnStop += () => OnDisconnected?.Invoke();
    }

        /// <summary>
        /// Initializes a new instance of Server with the specified name, game data, IP address, and port range, automatically selecting an available port.
        /// </summary>
        /// <param name="name">The name of the server.</param>
        /// <param name="serverGameData">The server's game data.</param>
        /// <param name="ip">The IP address to listen on.</param>
        /// <param name="range">The port range to search for an available port.</param>
        /// <param name="bufferSize">The buffer size for data transfer.</param>
        public Server(string name, ServerGameData serverGameData, IPAddress ip, PortRange range, int bufferSize = 4096)
    {
        _name = name;

        _server_data = serverGameData;

        _clients = new ServerClients();


        PortRange.Store store = range.RangeStore;

        while (true)
        {
            try
            {
                _server = new TCPServer(ip, store.RandomPort, bufferSize);

                break;
            }
            catch (SocketException e)
            {
                DebugConsole.LogError(e.Message);
            }
        }


        _server.OnClientConnected += (id) => OnClientConnected?.Invoke(id);

        _server.OnClientDisconnected += (id) =>
        {
            Disconnect(id);

            OnClientDisconnected?.Invoke(id);
        };

        _server.OnRequest += (im) => OnRequest?.Invoke(im);

        _server.OnStop += () => OnDisconnected?.Invoke();
    }



        /// <summary>
        /// Starts the server, begins accepting client connections, and opens the UDP state channel.
        /// </summary>
        public void Start()
    {
        _server.Start();

        try
        {
            _data_channel = new UDPChannel(_server.IpEndPoint);

            _data_channel.OnReceive += (point, message) =>
            {
                try
                {
                    OnState?.Invoke(point, message);
                }
                catch (Exception e)
                {
                    DebugConsole.LogError($"[Michitai.Lan][STATE-HANDLER-ERROR][{e.Message}]");
                }
            };

            _data_channel.Start();
        }
        catch (Exception e)
        {
            DebugConsole.LogError($"[Michitai.Lan][DATA-CHANNEL-START-ERROR][{e.Message}]");

            _data_channel = null;
        }
    }

        /// <summary>
        /// Stops the server and disconnects all clients.
        /// </summary>
        public void Stop()
    {
        _data_channel?.Stop();

        _data_channel = null;

        _server.Stop();
    }



        /// <summary>
        /// Sends a state datagram to a specific client endpoint over the UDP data channel.
        /// </summary>
        /// <param name="target">The client UDP endpoint (learned from incoming datagrams).</param>
        /// <param name="message">The state message to send.</param>
        public void SendState(IPEndPoint target, Message message)
    {
        _data_channel?.Send(target, message);
    }

        /// <summary>
        /// Broadcasts a state datagram to all clients that have sent state to this server.
        /// </summary>
        /// <param name="message">The state message to broadcast.</param>
        public void BroadcastState(Message message)
    {
        _data_channel?.Broadcast(message);
    }



        /// <summary>
        /// Sends a response message to a specific client.
        /// </summary>
        /// <param name="identified_message">The identified message containing the client ID and response content.</param>
        public void Response(IdentifiedMessage identified_message)
    {
        _server.Response(identified_message);
    }



        /// <summary>
        /// Logs in a player with the specified credentials.
        /// </summary>
        /// <param name="id">The client ID of the player.</param>
        /// <param name="credentials">The player's credentials.</param>
        public void LogInPlayer(string id, Credentials credentials)
    {
        _clients.LogIn(new ServerClient(id, credentials));
    }

        /// <summary>
        /// Disconnects a client by ID and removes their data.
        /// </summary>
        /// <param name="id">The client ID to disconnect.</param>
        public void Disconnect(string id)
    {
        ServerClient client;

        bool exists = _clients.TryGetPlayer(id, out client);

        if (exists)
        {
            _server_data.DeletePlayer(client.Credentials);
        }

        _clients.LogOut(id);

        _server.Disconnect(id);
    }

        /// <summary>
        /// Registers a new player with the specified game data.
        /// </summary>
        /// <param name="data">The player's game data.</param>
        /// <returns>The credentials assigned to the new player.</returns>
        public Credentials RegisterNewPlayer(JsonStorage data)
    {
        Credentials credentials = Credentials.New();

        _server_data.AddPlayer(new ServerClientGameData(data, credentials));

        return credentials;
    }

        /// <summary>
        /// Checks if the server contains a player with the specified credentials.
        /// </summary>
        /// <param name="player">The player's credentials.</param>
        /// <returns>True if the player exists; otherwise, false.</returns>
        public bool Contains(Credentials player)
    {
        return PrivateServerData.Contains(player);
    }

        /// <summary>
        /// Attempts to get the public game data of a logged-in player by ID.
        /// </summary>
        /// <param name="id">The client ID of the player.</param>
        /// <param name="serverClientGameData">When this method returns, contains the player's public data if successful.</param>
        /// <returns>True if the player was found; otherwise, false.</returns>
        public bool TryGetLoggedInPlayerPublicData(string id, out ServerClientGameData serverClientGameData)
    {
        ServerClient serverClient;

        bool success = _clients.TryGetPlayer(id, out serverClient);


        if (success)
        {
            return PrivateServerData.TryGetPublicPlayerData(serverClient.Credentials, out serverClientGameData);
        }


        serverClientGameData = null;

        return false;
    }

        /// <summary>
        /// Attempts to get the private game data of a logged-in player by ID.
        /// </summary>
        /// <param name="id">The client ID of the player.</param>
        /// <param name="serverClientGameData">When this method returns, contains the player's private data if successful.</param>
        /// <returns>True if the player was found; otherwise, false.</returns>
        public bool TryGetLoggedInPlayerPrivateData(string id, out ServerClientGameData serverClientGameData)
    {
        ServerClient serverClient;

        bool success = _clients.TryGetPlayer(id, out serverClient);


        if (success)
        {
            return PrivateServerData.TryGetPrivatePlayerData(serverClient.Credentials, out serverClientGameData);
        }


        serverClientGameData = null;

        return false;
    }



        /// <summary>
        /// Manages a collection of connected server clients.
        /// </summary>
        [Serializable]
        public class ServerClients
    {
        private ServerClient[] _clients;

        private readonly object _clients_lock;



        /// <summary>
        /// Initializes a new instance of ServerClients.
        /// </summary>
        public ServerClients()
        {
            _clients = new ServerClient[0];

            _clients_lock = new object();
        }



        /// <summary>
        /// Logs in a client to the server.
        /// </summary>
        /// <param name="player">The client to log in.</param>
        public void LogIn(ServerClient player)
        {
            lock (_clients_lock)
            {
                ServerClient[] clients = _clients;

                _clients = new ServerClient[clients.Length + 1];


                for (int i = 0; i < clients.Length; i++)
                {
                    _clients[i] = clients[i];
                }


                _clients[clients.Length] = player;
            }
        }

        /// <summary>
        /// Logs out a client from the server.
        /// </summary>
        /// <param name="id">The client ID to log out.</param>
        public void LogOut(string id)
        {
            if (!Contains(id))
                return;

            lock (_clients_lock)
            {
                ServerClient[] clients = _clients;

                _clients = new ServerClient[_clients.Length - 1];


                for (int i = 0, n = 0; i < clients.Length; i++)
                {
                    if (clients[i].ID != id)
                    {
                        _clients[n++] = clients[i];
                    }
                }
            }
        }



        /// <summary>
        /// Attempts to get a client by ID.
        /// </summary>
        /// <param name="id">The client ID to search for.</param>
        /// <param name="serverClient">When this method returns, contains the client if successful.</param>
        /// <returns>True if the client was found; otherwise, false.</returns>
        public bool TryGetPlayer(string id, out ServerClient serverClient)
        {
            lock (_clients_lock)
            {
                for (int i = 0; i < _clients.Length; i++)
                {
                    if (_clients[i].ID == id)
                    {
                        serverClient = _clients[i];

                        return true;
                    }
                }
            }

            serverClient = null;

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool Contains(string id)
        {
            lock (_clients_lock)
            {
                for (int i = 0; i < _clients.Length; i++)
                {
                    if (_clients[i].ID == id)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Represents a connected server client with credentials.
    /// </summary>
    [Serializable]
    public class ServerClient
    {
        private string _id;

        private Credentials _credentials;

        private readonly object _id_lock;

        private readonly object _credentials_lock;

        /// <summary>
        /// Gets the unique identifier for this client.
        /// </summary>
        public string ID
        {
            get
            {
                lock (_id_lock)
                {
                    return _id;
                }
            }
        }

        /// <summary>
        /// Gets the client's credentials.
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
        }



        /// <summary>
        /// Initializes a new instance of ServerClient with the specified ID and credentials.
        /// </summary>
        /// <param name="id">The unique identifier for the client.</param>
        /// <param name="credentials">The client's credentials.</param>
        public ServerClient(string id, Credentials credentials)
        {
            _id = id;

            _credentials = credentials;


            _id_lock = new object();

            _credentials_lock = new object();
        }
    }
}
}
