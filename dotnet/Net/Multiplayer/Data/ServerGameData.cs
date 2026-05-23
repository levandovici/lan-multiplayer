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
            public sealed class ServerGameData : IJsonStorage
{
    private string _server_id;

    private ServerClientGameData[] _clients;

    private string _json;


    private readonly object _server_id_lock;

    private readonly object _clients_lock;

    private readonly object _json_lock;



    /// <summary>
    /// 
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
    /// 
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
    /// 
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
    /// 
    /// </summary>
    /// <param name="server_id"></param>
    /// <param name="clients"></param>
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
    /// 
    /// </summary>
    /// <param name="server_id"></param>
    public ServerGameData(string server_id) : this(server_id, new ServerClientGameData[0])
    {
    }

    /// <summary>
    /// 
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
    /// 
    /// </summary>
    /// <param name="player"></param>
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
    /// 
    /// </summary>
    /// <param name="player"></param>
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
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="client"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="client"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"[SERVER-GAME-DATA][SERVER-ID][{_server_id}][PLAYERS][{_clients.Length}]";
    }
}
}
