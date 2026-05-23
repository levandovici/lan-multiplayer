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
            public sealed class Client
{
    private TCPClient _client;

    private ClientGameData _client_data;

    private JsonStorage _game_data;

    private ServerGameData _server_data;

    private bool _is_responsed = true;



    /// <summary>
    /// 
    /// </summary>
    public event Action<Message> OnResponse;

    /// <summary>
    /// 
    /// </summary>
    public event Action OnDisconnected;



    /// <summary>
    /// 
    /// </summary>
    public bool IsClosed => _client.IsClosed;

    /// <summary>
    /// 
    /// </summary>
    public bool IsInitialized
    {
        get
        {
            return ClientData != null && GameData != null && ServerData != null;
        }
    }

    /// <summary>
    /// 
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
    /// 
    /// </summary>
    public bool CanRequest
    {
        get
        {
            return IsInitialized && IsResponsed;
        }
    }


    /// <summary>
    /// 
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
    /// 
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
    /// 
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
    /// 
    /// </summary>
    /// <param name="clientGameData"></param>
    /// <param name="ipEndPoint"></param>
    /// <param name="bufferSize"></param>
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
    /// 
    /// </summary>
    /// <param name="clientGameData"></param>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="bufferSize"></param>
    public Client(ClientGameData clientGameData, JsonStorage gameData, IPAddress ip, int port, int bufferSize = 4096) :
        this(clientGameData, gameData, new IPEndPoint(ip, port), bufferSize)
    {
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="clientGameData"></param>
    /// <param name="ipEndPoint"></param>
    /// <param name="bufferSize"></param>
    public Client(ClientGameData clientGameData, IPEndPoint ipEndPoint, int bufferSize = 4096) :
        this(clientGameData, null, ipEndPoint, bufferSize)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="clientGameData"></param>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="bufferSize"></param>
    public Client(ClientGameData clientGameData, IPAddress ip, int port, int bufferSize = 4096) :
        this(clientGameData, null, new IPEndPoint(ip, port), bufferSize)
    {
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="ipEndPoint"></param>
    /// <param name="bufferSize"></param>
    public Client(IPEndPoint ipEndPoint, int bufferSize = 4096) :
        this(null, null, ipEndPoint, bufferSize)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="bufferSize"></param>
    public Client(IPAddress ip, int port, int bufferSize = 4096) :
        this(null, null, new IPEndPoint(ip, port), bufferSize)
    {
    }



    /// <summary>
    /// 
    /// </summary>
    public void Start()
    {
        _client.Start();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Stop()
    {
        _client.Stop();
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
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
