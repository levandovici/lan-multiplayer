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
            public class LocatedServerInfo
{
    public ServerInfo _server_info;

    public IPEndPoint _point;


    private readonly object _server_info_lock;

    private readonly object _point_lock;



    public ServerInfo ServerInfo
    {
        get
        {
            lock (_server_info_lock)
            {
                return _server_info;
            }
        }

        set
        {
            lock (_server_info_lock)
            {
                _server_info = value;
            }
        }
    }

    public IPEndPoint IPEndPoint
    {
        get
        {
            lock (_point_lock)
            {
                return _point;
            }
        }

        set
        {
            lock (_point_lock)
            {
                _point = value;
            }
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="serverInfo"></param>
    /// <param name="point"></param>
    public LocatedServerInfo(ServerInfo serverInfo, IPEndPoint point)
    {
        _server_info = serverInfo;

        _point = point;


        _server_info_lock = new object();

        _point_lock = new object();
    }

    /// <summary>
    /// 
    /// </summary>
    public LocatedServerInfo()
    {
        _server_info = null;

        _point = null;


        _server_info_lock = new object();

        _point_lock = new object();
    }
}
}
