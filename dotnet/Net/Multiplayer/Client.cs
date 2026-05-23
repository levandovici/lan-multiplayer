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


namespace Michitai.Lan.Net.Multiplayer
{
    /// <summary>
    /// Multiplayer client for connecting to and communicating with a multiplayer server.
    /// </summary>
    public sealed class Client
    {
        private TCPClient _client;

        private ClientGameData _client_data;

        private PlayerGameData _game_data;

        private ServerGameData _server_data;

        private bool _is_responsed = true;

        /// <summary>
        /// Event raised when a response message is received.
        /// </summary>
        public event Action<Message> OnResponse;

        /// <summary>
        /// Event raised when the client is disconnected.
        /// </summary>
        public event Action OnDisconnected;

        /// <summary>
        /// Gets whether the client is closed.
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
        /// Gets or sets whether the client has received a response to the last request.
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
        /// Gets whether the client can send a request (initialized and not waiting for response).
        /// </summary>
        public bool CanRequest
    {
        get
        {
            return IsInitialized && IsResponsed;
        }
    }


        /// <summary>
        /// Gets or sets the client game data.
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
        /// Gets or sets the player game data.
        /// </summary>
        public PlayerGameData GameData
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
        /// Gets or sets the server game data.
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
        /// Initializes a new instance of Client with the specified data and endpoint.
        /// </summary>
        /// <param name="clientGameData">The client game data.</param>
        /// <param name="gameData">The player game data.</param>
        /// <param name="ipEndPoint">The server IP endpoint.</param>
        /// <param name="bufferSize">The buffer size for network operations.</param>
        public Client(ClientGameData clientGameData, PlayerGameData gameData, IPEndPoint ipEndPoint, int bufferSize = 4096)
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
        /// Initializes a new instance of Client with the specified data and IP address.
        /// </summary>
        /// <param name="clientGameData">The client game data.</param>
        /// <param name="gameData">The player game data.</param>
        /// <param name="ip">The server IP address.</param>
        /// <param name="port">The server port.</param>
        /// <param name="bufferSize">The buffer size for network operations.</param>
        public Client(ClientGameData clientGameData, PlayerGameData gameData, IPAddress ip, int port, int bufferSize = 4096) :
        this(clientGameData, gameData, new IPEndPoint(ip, port), bufferSize)
    {
    }


        /// <summary>
        /// Initializes a new instance of Client with client data only.
        /// </summary>
        /// <param name="clientGameData">The client game data.</param>
        /// <param name="ipEndPoint">The server IP endpoint.</param>
        /// <param name="bufferSize">The buffer size for network operations.</param>
        public Client(ClientGameData clientGameData, IPEndPoint ipEndPoint, int bufferSize = 4096) :
        this(clientGameData, null, ipEndPoint, bufferSize)
    {
    }

        /// <summary>
        /// Initializes a new instance of Client with client data and IP address.
        /// </summary>
        /// <param name="clientGameData">The client game data.</param>
        /// <param name="ip">The server IP address.</param>
        /// <param name="port">The server port.</param>
        /// <param name="bufferSize">The buffer size for network operations.</param>
        public Client(ClientGameData clientGameData, IPAddress ip, int port, int bufferSize = 4096) :
        this(clientGameData, null, new IPEndPoint(ip, port), bufferSize)
    {
    }


        /// <summary>
        /// Initializes a new instance of Client with only the server endpoint.
        /// </summary>
        /// <param name="ipEndPoint">The server IP endpoint.</param>
        /// <param name="bufferSize">The buffer size for network operations.</param>
        public Client(IPEndPoint ipEndPoint, int bufferSize = 4096) :
        this(null, null, ipEndPoint, bufferSize)
    {
    }

        /// <summary>
        /// Initializes a new instance of Client with only the server IP address and port.
        /// </summary>
        /// <param name="ip">The server IP address.</param>
        /// <param name="port">The server port.</param>
        /// <param name="bufferSize">The buffer size for network operations.</param>
        public Client(IPAddress ip, int port, int bufferSize = 4096) :
        this(null, null, new IPEndPoint(ip, port), bufferSize)
    {
    }



        /// <summary>
        /// Starts the client and connects to the server.
        /// </summary>
        public void Start()
    {
        _client.Start();
    }

        /// <summary>
        /// Stops the client and disconnects from the server.
        /// </summary>
        public void Stop()
    {
        _client.Stop();
    }



        /// <summary>
        /// Sends a request message to the server.
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
