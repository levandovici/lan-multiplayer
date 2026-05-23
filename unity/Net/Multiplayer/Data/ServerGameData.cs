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
            public sealed class ServerGameData : IJsonStorage
{
    public string serverID;

    public ServerClientGameData[] clients;

    public string json;


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
    /// 
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
    /// 
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
    /// 
    /// </summary>
    /// <param name="server_id"></param>
    /// <param name="clients"></param>
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
        serverID = "default";

        clients = new ServerClientGameData[0];

        json = "";


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
    /// 
    /// </summary>
    /// <param name="player"></param>
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
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="client"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="client"></param>
    /// <returns></returns>
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



    public T Get<T>()
    {
        return JsonUtility.FromJson<T>(Json);
    }

    public void Set<T>(T @object)
    {
        Json = JsonUtility.ToJson(@object);
    }



    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"[SERVER-GAME-DATA][SERVER-ID][{serverID}][PLAYERS][{clients.Length}]";
    }
}
}
