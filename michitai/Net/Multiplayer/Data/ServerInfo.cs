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
            public class ServerInfo
{
    private int _port;

    private int _clients_count;

    private string _server_id;

    private string _name;


    private readonly object _port_lock;

    private readonly object _clients_count_lock;

    private readonly object _server_id_lock;

    private readonly object _name_lock;



    public string Name
    {
        get
        {
            lock (_name_lock)
            {
                return _name;
            }
        }

        set
        {
            lock (_name_lock)
            {
                _name = value;
            }
        }
    }

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

    public int Port
    {
        get
        {
            lock (_port_lock)
            {
                return _port;
            }
        }

        set
        {
            lock (_port_lock)
            {
                _port = value;
            }
        }
    }

    public int ClientsCount
    {
        get
        {
            lock (_clients_count_lock)
            {
                return _clients_count;
            }
        }

        set
        {
            lock (_clients_count_lock)
            {
                _clients_count = value;
            }
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="port"></param>
    /// <param name="name"></param>
    /// <param name="serverID"></param>
    /// <param name="clientsCount"></param>
    public ServerInfo(int port, string name, string serverID, int clientsCount)
    {
        _port = port;

        _name = name;

        _server_id = serverID;

        _clients_count = clientsCount;


        _port_lock = new object();

        _name_lock = new object();

        _server_id_lock = new object();

        _clients_count_lock = new object();
    }

    /// <summary>
    /// 
    /// </summary>
    public ServerInfo()
    {
        _port = 0;

        _name = "default";

        _server_id = "default";

        _clients_count = -1;


        _port_lock = new object();

        _name_lock = new object();

        _server_id_lock = new object();

        _clients_count_lock = new object();
    }
}
}
