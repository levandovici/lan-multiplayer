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
            public sealed class ServerClientGameData
{
    public JsonStorage data;

    public Credentials credentials;


    private readonly object _data_lock;

    private readonly object _credentials_lock;



    /// <summary>
    /// 
    /// </summary>
    public JsonStorage Data
    {
        get
        {
            lock (_data_lock)
            {
                return data;
            }
        }

        set
        {
            lock (_data_lock)
            {
                data = value;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public Credentials Credentials
    {
        get
        {
            lock (_credentials_lock)
            {
                return credentials;
            }
        }

        set
        {
            lock (_credentials_lock)
            {
                credentials = value;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public ServerClientGameData Public => new ServerClientGameData(Data, Credentials.Public);



    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameData"></param>
    /// <param name="credentials"></param>
    public ServerClientGameData(JsonStorage gameData, Credentials credentials)
    {
        data = gameData;

        this.credentials = credentials;


        _data_lock = new object();

        _credentials_lock = new object();
    }

    /// <summary>
    /// 
    /// </summary>
    public ServerClientGameData()
    {
        credentials = new Credentials("id", "password");


        _data_lock = new object();

        _credentials_lock = new object();
    }



    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"[SERVER-CLIENT-GAME-DATA]{credentials}{data?.ToString()}";
    }
}
}
