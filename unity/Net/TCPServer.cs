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
            public sealed class TCPServer
{
    private TcpListener _listner;

    private IPEndPoint _IPEndPoint;

    private int _buffer_size;

    private TCPServerClient[] _clients;

    private bool _closed;


    private readonly object _clients_lock;



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
    public event Action OnStop;



    /// <summary>
    /// 
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
    /// 
    /// </summary>
    public IPEndPoint IpEndPoint => _IPEndPoint;



    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="buffer_size"></param>
    public TCPServer(IPAddress ip, int port, int buffer_size = 4096) :
        this(new IPEndPoint(ip, port), buffer_size)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip_end_point"></param>
    /// <param name="buffer_size"></param>
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
    /// 
    /// </summary>
    public void Start()
    {
        _listner.Start();

        BeginAccept();
    }

    /// <summary>
    /// 
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
    /// 
    /// </summary>
    /// <param name="identifiedMessage"></param>
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
    /// 
    /// </summary>
    /// <param name="id"></param>
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
    /// 
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
    /// 
    /// </summary>
    /// <param name="result"></param>
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
    /// 
    /// </summary>
    /// <param name="server_client"></param>
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
    /// 
    /// </summary>
    /// <param name="id"></param>
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
