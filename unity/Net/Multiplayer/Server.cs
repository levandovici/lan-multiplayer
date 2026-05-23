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
    /// 
    /// </summary>
    public void Start()
    {
        _server.Start();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Stop()
    {
        _server.Stop();
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="identified_message"></param>
    public void Response(IdentifiedMessage identified_message)
    {
        _server.Response(identified_message);
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="credentials"></param>
    public void LogInPlayer(string id, Credentials credentials)
    {
        _clients.LogIn(new ServerClient(id, credentials));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
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
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public Credentials RegisterNewPlayer(JsonStorage data)
    {
        Credentials credentials = Credentials.New();

        _server_data.AddPlayer(new ServerClientGameData(data, credentials));

        return credentials;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool Contains(Credentials player)
    {
        return PrivateServerData.Contains(player);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="serverClientGameData"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="serverClientGameData"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    [Serializable]
    public class ServerClients
    {
        private ServerClient[] _clients;

        private readonly object _clients_lock;



        /// <summary>
        /// 
        /// </summary>
        public ServerClients()
        {
            _clients = new ServerClient[0];

            _clients_lock = new object();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
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
        /// 
        /// </summary>
        /// <param name="id"></param>
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
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="serverClient"></param>
        /// <returns></returns>
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

    [Serializable]
    public class ServerClient
    {
        private string _id;

        private Credentials _credentials;


        private readonly object _id_lock;

        private readonly object _credentials_lock;



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
