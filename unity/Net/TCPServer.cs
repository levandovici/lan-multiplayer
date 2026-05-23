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

namespace Michitai.Lan.Net
{
    /// <summary>
    /// Provides TCP server functionality for managing multiple client connections.
    /// </summary>
    public sealed class TCPServer
    {
        private TcpListener _listner;

        private IPEndPoint _IPEndPoint;

        private int _buffer_size;

        private TCPServerClient[] _clients;

        private bool _closed;

        private readonly object _clients_lock;

        /// <summary>
        /// Event raised when a client connects to the server.
        /// </summary>
        public event Action<string> OnClientConnected;

        /// <summary>
        /// Event raised when a client disconnects from the server.
        /// </summary>
        public event Action<string> OnClientDisconnected;

        /// <summary>
        /// Event raised when a request is received from a client.
        /// </summary>
        public event Action<IdentifiedMessage> OnRequest;

        /// <summary>
        /// Event raised when the server stops.
        /// </summary>
        public event Action OnStop;

        /// <summary>
        /// Gets whether the server is closed.
        /// </summary>
        public bool IsClosed
        {
            get
            {
                return _closed;
            }

            private set
            {
                _closed = value;
            }
        }

        /// <summary>
        /// Gets the IP endpoint the server is listening on.
        /// </summary>
        public IPEndPoint IpEndPoint => _IPEndPoint;

        /// <summary>
        /// Initializes a new instance of TCPServer with the specified IP address and port.
        /// </summary>
        /// <param name="ip">The IP address to listen on.</param>
        /// <param name="port">The port number to listen on.</param>
        /// <param name="buffer_size">The buffer size for data transfer.</param>
        public TCPServer(IPAddress ip, int port, int buffer_size = 4096) :
            this(new IPEndPoint(ip, port), buffer_size)
        {
        }

        /// <summary>
        /// Initializes a new instance of TCPServer with the specified IP endpoint.
        /// </summary>
        /// <param name="ip_end_point">The IP endpoint to listen on.</param>
        /// <param name="buffer_size">The buffer size for data transfer.</param>
        public TCPServer(IPEndPoint ip_end_point, int buffer_size = 4096)
    {
        _IPEndPoint = ip_end_point;

        _buffer_size = buffer_size;

        _listner = new TcpListener(_IPEndPoint);

        _clients = new TCPServerClient[0];

        IsClosed = false;


        _clients_lock = new object();
    }



        /// <summary>
        /// Starts the server and begins accepting client connections.
        /// </summary>
        public void Start()
    {
        _listner.Start();

        BeginAccept();
    }

        /// <summary>
        /// Stops the server and disconnects all clients.
        /// </summary>
        public void Stop()
    {
        if (IsClosed)
            return;

        IsClosed = true;

        _listner.Stop();

        Socket socket = _listner.Server;

        socket.Close();

        socket.Dispose();

        lock (_clients_lock)
        {
            for (int i = 0; i < _clients.Length; i++)
            {
                _clients[i].Stop();
            }
        }

        OnStop?.Invoke();
    }



        /// <summary>
        /// Sends a response to a specific client by ID.
        /// </summary>
        /// <param name="identifiedMessage">The identified message containing the client ID and response content.</param>
        public void Response(IdentifiedMessage identifiedMessage)
    {
        lock (_clients_lock)
        {
            for (int i = 0; i < _clients.Length; i++)
            {
                if (_clients[i].ID == identifiedMessage.ID)
                {
                    _clients[i].Response(identifiedMessage.Message);

                    return;
                }
            }
        }
    }



        /// <summary>
        /// Disconnects a client by ID.
        /// </summary>
        /// <param name="id">The client ID to disconnect.</param>
        public void Disconnect(string id)
    {
        lock (_clients_lock)
        {
            for (int i = 0; i < _clients.Length; i++)
            {
                if (_clients[i].ID == id)
                {
                    _clients[i].Stop();
                    break;
                }
            }
        }

        DeleteClient(id);
    }



        /// <summary>
        /// Begins asynchronously accepting client connections.
        /// </summary>
        private void BeginAccept()
    {
        try
        {
            _listner.BeginAcceptTcpClient(BeginAcceptTcpClientCallback, null);
        }
        catch
        {
            DebugConsole.LogError("[TCP-Server] Begin Accept Error!");
        }
    }

        /// <summary>
        /// Callback for asynchronous accept operations.
        /// </summary>
        /// <param name="result">The asynchronous operation result.</param>
        private void BeginAcceptTcpClientCallback(IAsyncResult result)
    {
        TcpClient client = null;

        try
        {
            client = _listner.EndAcceptTcpClient(result);

            AddClient(new TCPServerClient(client, (im) => OnRequest?.Invoke(im), (id) => Disconnect(id), _buffer_size));
        }
        catch
        {
            DebugConsole.LogError("[TCP-Server] End Accept Error!");
        }

        BeginAccept();
    }

        /// <summary>
        /// Adds a client to the server's client list.
        /// </summary>
        /// <param name="server_client">The client to add.</param>
        private void AddClient(TCPServerClient server_client)
    {
        lock (_clients_lock)
        {
            TCPServerClient[] clients = _clients;

            _clients = new TCPServerClient[clients.Length + 1];


            for (int i = 0; i < clients.Length; i++)
            {
                _clients[i] = clients[i];
            }

            _clients[clients.Length] = server_client;


            OnClientConnected?.Invoke(server_client.ID);
        }
    }

        /// <summary>
        /// Removes a client from the server's client list by ID.
        /// </summary>
        /// <param name="id">The client ID to remove.</param>
        private void DeleteClient(string id)
    {
        lock (_clients_lock)
        {
            int index = -1;

            for (int i = 0; i < _clients.Length; i++)
            {
                if (_clients[i].ID == id)
                {
                    index = i;

                    break;
                }
            }

            if (index != -1)
            {
                TCPServerClient[] clients = _clients;

                _clients = new TCPServerClient[clients.Length - 1];

                for (int i = 0; i < index; i++)
                {
                    _clients[i] = clients[i];
                }

                for (int i = index; i < _clients.Length; i++)
                {
                    _clients[i] = clients[i + 1];
                }

                OnClientDisconnected?.Invoke(id);
            }
        }
    }
}
}
