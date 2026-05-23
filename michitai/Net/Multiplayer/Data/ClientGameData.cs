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
            public sealed class ClientGameData
{
    private string _server_id;

    private Credentials _credentials;


    private readonly object _server_id_lock;

    private readonly object _credentials_lock;



    /// <summary>
    /// 
    /// </summary>
    public string Server_ID
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
    public Credentials Credentials
    {
        get
        {
            lock (_credentials_lock)
            {
                return _credentials;
            }
        }

        set
        {
            lock (_credentials_lock)
            {
                _credentials = value;
            }
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="server_id"></param>
    /// <param name="credentials"></param>
    public ClientGameData(string server_id, Credentials credentials)
    {
        _server_id = server_id;

        _credentials = credentials;


        _server_id_lock = new object();

        _credentials_lock = new object();
    }

    /// <summary>
    /// 
    /// </summary>
    public ClientGameData()
    {
        _server_id = "default";

        _credentials = new Credentials("id", "password");


        _server_id_lock = new object();

        _credentials_lock = new object();
    }
}
}
