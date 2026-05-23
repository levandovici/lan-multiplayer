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
            public sealed class LocatedMessage
{
    private IPEndPoint _point;

    private AppMessage _message;



    /// <summary>
    /// 
    /// </summary>
    public IPEndPoint IPEndPoint
    {
        get
        {
            return _point;
        }

        set
        {
            _point = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public AppMessage Message
    {
        get
        {
            return _message;
        }

        set
        {
            _message = value;
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="point"></param>
    /// <param name="message"></param>
    public LocatedMessage(IPEndPoint point, AppMessage message)
    {
        _point = point;

        _message = message;
    }



    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"IP End Point: {IPEndPoint}\t App Message: {Message}";
    }
}
}
