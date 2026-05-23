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


namespace Michitai.Lan.Data
{
            public sealed class PlayerCharacterData : IJsonStorage
{
    private string _json;

    private object _json_lock;



    public string Json
    {
        get
        {
            lock (_json_lock)
            {
                return _json;
            }
        }

        set
        {
            lock (_json_lock)
            {
                _json = value;
            }
        }
    }



    /// <summary>
    /// 
    /// </summary>
    public PlayerCharacterData()
    {
        _json = "";

        _json_lock = new object();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="json"></param>
    public PlayerCharacterData(string json)
    {
        _json = json;

        _json_lock = new object();
    }



    public T Get<T>()
    {
        return JsonSerializer.Deserialize<T>(Json);
    }

    public void Set<T>(T @object)
    {
        Json = JsonSerializer.Serialize(@object);
    }
}
}
