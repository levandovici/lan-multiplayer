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

namespace Michitai.Lan.Net.Multiplayer
{
            public sealed class BroadcastServer
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="incoming"></param>
    /// <returns></returns>
    public delegate AppMessage ProcessMessageDelegate(LocatedMessage incoming);



    private UDPBroadcast _socket;



    /// <summary>
    /// 
    /// </summary>
    public UDPBroadcast Socket
    {
        get
        {
            return _socket;
        }

        set
        {
            _socket = value;
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="point"></param>
    public BroadcastServer(IPEndPoint point)
    {
        _socket = new UDPBroadcast(point);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public BroadcastServer(IPAddress ip, int port)
    {
        _socket = new UDPBroadcast(ip, port);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="range"></param>
    public BroadcastServer(IPAddress ip, PortRange range)
    {
        _socket = new UDPBroadcast(ip, range);
    }



    /// <summary>
    /// 
    /// </summary>
    public void Stop()
    {
        Socket.Stop();
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="process"></param>
    public void Broadcast(ProcessMessageDelegate process)
    {
        LocatedMessage message = Socket.Receive();

        Socket.Send(message.IPEndPoint, process.Invoke(message));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="process"></param>
    /// <param name="timeoutMilliseconds"></param>
    /// <returns></returns>
    public bool Broadcast(ProcessMessageDelegate process, int timeoutMilliseconds)
    {
        Task broadcast = Task.Run(() => Broadcast(process));

        return broadcast.Wait(new TimeSpan(timeoutMilliseconds * 10000));
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="process"></param>
    /// <returns></returns>
    public async Task BroadcastAsync(ProcessMessageDelegate process)
    {
        LocatedMessage incoming = await Socket.ReceiveAsync();

        if (incoming == null)
            return;

        await Socket.SendAsync(incoming.IPEndPoint, process.Invoke(incoming));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="process"></param>
    /// <param name="timeoutMilliseconds"></param>
    /// <returns></returns>
    public async Task<bool> BroadcastAsync(ProcessMessageDelegate process, int timeoutMilliseconds)
    {
        Task timeout = Task.Run(async () => await Task.Delay(timeoutMilliseconds));

        Task broadcast = Task.Run(async () => await BroadcastAsync(process));


        Task task = await Task.WhenAny(broadcast, timeout);


        return broadcast.IsCompleted;
    }
}
}
