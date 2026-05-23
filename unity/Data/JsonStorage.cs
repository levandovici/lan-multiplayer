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

namespace Michitai.Lan.Data
{
            public sealed class JsonStorage : IJsonStorage
{
    public string json;

    private object _json_lock;



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



    public JsonStorage()
    {
        json = "";

        _json_lock = new object();
    }

    public JsonStorage(string json)
    {
        this.json = json;

        _json_lock = new object();
    }



    public T Get<T>()
    {
        return JsonUtility.FromJson<T>(Json);
    }

    public void Set<T>(T @object)
    {
        Json = JsonUtility.ToJson(@object);
    }
}
}
