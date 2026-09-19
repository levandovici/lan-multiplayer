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

namespace Michitai.Lan.Net.Multiplayer
{
    /// <summary>
    /// Multiplayer client for connecting to and communicating with a multiplayer server.
    /// Requests that arrive while awaiting a response are queued instead of dropped.
    /// Includes an unreliable UDP channel for high-frequency state synchronization.
    /// </summary>
    public sealed class Client
    {
        private TCPClient _client;

        private UDPChannel _data_channel;

        private IPEndPoint _ip_end_point;

        private ClientGameData _client_data;

        private PlayerGameData _game_data;

        private ServerGameData _server_data;

        private bool _is_responsed = true;

        private readonly Queue<Message> _pending_requests;

        private readonly object _pending_lock;

        /// <summary>
        /// Event raised when a response message is received.
        /// </summary>
        public event Action<Message> OnResponse;

        /// <summary>
        /// Event raised when the client is disconnected.
        /// </summary>
        public event Action OnDisconnected;

        /// <summary>
        /// Event raised when a state datagram is received over the UDP data channel.
        /// </summary>
        public event Action<IPEndPoint, Message> OnState;

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
        /// Gets whether the client has received a response to the last request.
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
        /// Gets the number of queued requests waiting for responses.
        /// </summary>
        public int PendingRequests
    {
        get
        {
            lock (_pending_lock)
            {
                return _pending_requests.Count;
            }
        }
    }

        /// <summary>
        /// Gets the UDP data channel used for unreliable high-frequency state sync.
        /// </summary>
        public UDPChannel DataChannel => _data_channel;

        /// <summary>
        /// Gets the server IP endpoint this client is connected to.
        /// </summary>
        public IPEndPoint ServerEndPoint => _ip_end_point;


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
        public Client(ClientGameData clientGameData, PlayerGameData gameData, IPEndPoint ipEndPoint, int bufferSize = 8192)
    {
        _client_data = clientGameData;

        _game_data = gameData;

        _ip_end_point = ipEndPoint;

        _pending_requests = new Queue<Message>();

        _pending_lock = new object();

        _client = new TCPClient(ipEndPoint, bufferSize);

        IsResponsed = true;

        _client.OnResponse += (m) =>
        {
            try
            {
                OnResponse?.Invoke(m);
            }
            finally
            {
                FlushPending();
            }
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
        public Client(ClientGameData clientGameData, PlayerGameData gameData, IPAddress ip, int port, int bufferSize = 8192) :
        this(clientGameData, gameData, new IPEndPoint(ip, port), bufferSize)
    {
    }


        /// <summary>
        /// Initializes a new instance of Client with client data only.
        /// </summary>
        /// <param name="clientGameData">The client game data.</param>
        /// <param name="ipEndPoint">The server IP endpoint.</param>
        /// <param name="bufferSize">The buffer size for network operations.</param>
        public Client(ClientGameData clientGameData, IPEndPoint ipEndPoint, int bufferSize = 8192) :
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
        public Client(ClientGameData clientGameData, IPAddress ip, int port, int bufferSize = 8192) :
        this(clientGameData, null, new IPEndPoint(ip, port), bufferSize)
    {
    }


        /// <summary>
        /// Initializes a new instance of Client with only the server endpoint.
        /// </summary>
        /// <param name="ipEndPoint">The server IP endpoint.</param>
        /// <param name="bufferSize">The buffer size for network operations.</param>
        public Client(IPEndPoint ipEndPoint, int bufferSize = 8192) :
        this(null, null, ipEndPoint, bufferSize)
    {
    }

        /// <summary>
        /// Initializes a new instance of Client with only the server IP address and port.
        /// </summary>
        /// <param name="ip">The server IP address.</param>
        /// <param name="port">The server port.</param>
        /// <param name="bufferSize">The buffer size for network operations.</param>
        public Client(IPAddress ip, int port, int bufferSize = 8192) :
        this(null, null, new IPEndPoint(ip, port), bufferSize)
    {
    }



        /// <summary>
        /// Starts the client, connects to the server, and opens the UDP state channel.
        /// </summary>
        public void Start()
    {
        _client.Start();

        try
        {
            _data_channel = new UDPChannel(IPAddress.Any, 0);

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
        /// Stops the client and disconnects from the server.
        /// </summary>
        public void Stop()
    {
        lock (_pending_lock)
        {
            _pending_requests.Clear();
        }

        _data_channel?.Stop();

        _data_channel = null;

        _client.Stop();
    }



        /// <summary>
        /// Sends a request message to the server. If a request is already awaiting
        /// a response, the message is queued and sent when the response arrives.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void Request(Message message)
    {
        bool sendNow;

        lock (_pending_lock)
        {
            sendNow = IsResponsed;

            if (sendNow)
            {
                IsResponsed = false;
            }
            else
            {
                _pending_requests.Enqueue(message);
            }
        }

        if (sendNow)
        {
            _client.Request(message);
        }
    }

        /// <summary>
        /// Sends a message immediately without waiting for a response.
        /// Use for fire-and-forget commands; for per-frame state prefer SendState (UDP).
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void Send(Message message)
    {
        _client.Request(message);
    }

        /// <summary>
        /// Sends a state datagram to the server over the unreliable UDP data channel.
        /// Best for 30-60Hz synchronization — no head-of-line blocking, latest-wins.
        /// </summary>
        /// <param name="message">The state message to send.</param>
        public void SendState(Message message)
    {
        _data_channel?.Send(_ip_end_point, message);
    }

        /// <summary>
        /// Sends a state datagram to the server over the unreliable UDP data channel.
        /// </summary>
        /// <param name="state">The state string to send.</param>
        public void SendState(string state)
    {
        _data_channel?.Send(_ip_end_point, state);
    }



        /// <summary>
        /// Sends the next queued request after a response was received.
        /// </summary>
        private void FlushPending()
    {
        Message next = null;

        lock (_pending_lock)
        {
            if (_pending_requests.Count > 0)
            {
                next = _pending_requests.Dequeue();
            }
            else
            {
                IsResponsed = true;

                return;
            }
        }

        _client.Request(next);
    }
}
}
