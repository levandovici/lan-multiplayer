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
            public class MobileBroadcastServer
{
    private UdpClient _server = null;

    private CancellationTokenSource _source = null;

    private Task _task = null;



    public void Start(int clientPort, int serverPort, int delayMilliseconds, Func<IPEndPoint, byte[], byte[]> onReceive)
    {
        if (_server != null || _source != null || _task != null)
            return;


        _source = new CancellationTokenSource();

        CancellationToken token = _source.Token;


        _server = new UdpClient(serverPort);

        _server.EnableBroadcast = true;
        

        _task = Task.Run(() =>
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                IPEndPoint point = null;

                byte[] bytes = null;

                try
                {
                    point = new IPEndPoint(IPAddress.Any, clientPort);

                    bytes = _server.Receive(ref point);
                }
                catch(Exception e)
                {
                    DebugConsole.LogError(e.Message);
                }

                token.ThrowIfCancellationRequested();

                try
                {
                    bytes = onReceive(point, bytes);

                    if (bytes != null && bytes.Length > 0)
                    {
                        var count = _server.Send(bytes, bytes.Length, point);
                    }
                }
                catch(Exception e)
                {
                    DebugConsole.LogError(e.Message);
                }

                Task.Delay(delayMilliseconds).Wait();
            }

        }, token);
    }

    public void Stop()
    {
        _source?.Cancel();

        _server?.Close();


        _server = null;

        _source = null;

        _task = null;
    }
}
}
