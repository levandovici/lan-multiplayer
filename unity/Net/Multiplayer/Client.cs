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
    /// <summary>
    /// Represents a multiplayer client that connects to a server and manages game data.
    /// </summary>
    public sealed class Client
    {
        private TCPClient _client;

        private ClientGameData _client_data;

        private JsonStorage _game_data;

        private ServerGameData _server_data;

        private bool _is_responsed = true;

        /// <summary>
        /// Event raised when a response message is received from the server.
        /// </summary>
        public event Action<Message> OnResponse;

        /// <summary>
        /// Event raised when the client disconnects from the server.
        /// </summary>
        public event Action OnDisconnected;

        /// <summary>
        /// Gets whether the client connection is closed.
        /// </summary>
        public bool IsClosed => _client.IsClosed;

        /// <summary>
        /// Gets whether the client is initialized with all required data.
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return ClientData != null && GameData != null && ServerData != null;
            }
        }

        /// <summary>
        /// Gets whether the client has received a response from the server.
        /// </summary>
        public bool IsResponsed
        {
            get
            {
                return _is_responsed;
            }

            private set
            {
                _is_responsed = value;
            }
        }

        /// <summary>
        /// Gets whether the client can send a request to the server.
        /// </summary>
        public bool CanRequest
        {
            get
            {
                return IsInitialized && IsResponsed;
            }
        }

        /// <summary>
        /// Gets or sets the client's game data.
        /// </summary>
        public ClientGameData ClientData
        {
            get
            {
                return _client_data;
            }

            set
            {
                _client_data = value;
            }
        }

        /// <summary>
        /// Gets or sets the shared game data storage.
        /// </summary>
        public JsonStorage GameData
        {
            get
            {
                return _game_data;
            }

            set
            {
                _game_data = value;
            }
        }

        /// <summary>
        /// Gets or sets the server's game data.
        /// </summary>
        public ServerGameData ServerData
        {
            get
            {
                return _server_data;
            }

            set
            {
                _server_data = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of Client with the specified game data and IP endpoint.
        /// </summary>
        /// <param name="clientGameData">The client's game data.</param>
        /// <param name="gameData">The shared game data storage.</param>
        /// <param name="ipEndPoint">The IP endpoint to connect to.</param>
        /// <param name="bufferSize">The buffer size for data transfer.</param>
        public Client(ClientGameData clientGameData, JsonStorage gameData, IPEndPoint ipEndPoint, int bufferSize = 4096)
    {
        _client_data = clientGameData;

        _game_data = gameData;

        _client = new TCPClient(ipEndPoint, bufferSize);

        IsResponsed = true;

        _client.OnResponse += (m) =>
        {
            IsResponsed = true;

            OnResponse?.Invoke(m);
        };

        _client.OnStop += () => OnDisconnected?.Invoke();
    }

        /// <summary>
        /// Initializes a new instance of Client with the specified game data, IP address, and port.
        /// </summary>
        /// <param name="clientGameData">The client's game data.</param>
        /// <param name="gameData">The shared game data storage.</param>
        /// <param name="ip">The IP address to connect to.</param>
        /// <param name="port">The port number to connect to.</param>
        /// <param name="bufferSize">The buffer size for data transfer.</param>
        public Client(ClientGameData clientGameData, JsonStorage gameData, IPAddress ip, int port, int bufferSize = 4096) :
            this(clientGameData, gameData, new IPEndPoint(ip, port), bufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of Client with the specified game data and IP endpoint.
        /// </summary>
        /// <param name="clientGameData">The client's game data.</param>
        /// <param name="ipEndPoint">The IP endpoint to connect to.</param>
        /// <param name="bufferSize">The buffer size for data transfer.</param>
        public Client(ClientGameData clientGameData, IPEndPoint ipEndPoint, int bufferSize = 4096) :
            this(clientGameData, null, ipEndPoint, bufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of Client with the specified game data, IP address, and port.
        /// </summary>
        /// <param name="clientGameData">The client's game data.</param>
        /// <param name="ip">The IP address to connect to.</param>
        /// <param name="port">The port number to connect to.</param>
        /// <param name="bufferSize">The buffer size for data transfer.</param>
        public Client(ClientGameData clientGameData, IPAddress ip, int port, int bufferSize = 4096) :
            this(clientGameData, null, new IPEndPoint(ip, port), bufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of Client with the specified IP endpoint.
        /// </summary>
        /// <param name="ipEndPoint">The IP endpoint to connect to.</param>
        /// <param name="bufferSize">The buffer size for data transfer.</param>
        public Client(IPEndPoint ipEndPoint, int bufferSize = 4096) :
            this(null, null, ipEndPoint, bufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of Client with the specified IP address and port.
        /// </summary>
        /// <param name="ip">The IP address to connect to.</param>
        /// <param name="port">The port number to connect to.</param>
        /// <param name="bufferSize">The buffer size for data transfer.</param>
        public Client(IPAddress ip, int port, int bufferSize = 4096) :
            this(null, null, new IPEndPoint(ip, port), bufferSize)
        {
        }

        /// <summary>
        /// Starts the client by connecting to the server.
        /// </summary>
        public void Start()
    {
        _client.Start();
    }

        /// <summary>
        /// Stops the client by disconnecting from the server.
        /// </summary>
        public void Stop()
    {
        _client.Stop();
    }



        /// <summary>
        /// Sends a request message to the server if the client is ready.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void Request(Message message)
    {
        if (IsResponsed)
        {
            IsResponsed = false;

            _client.Request(message);
        }
    }
}
}
