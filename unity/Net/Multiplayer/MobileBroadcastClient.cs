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
            public class MobileBroadcastClient
{
    private UdpClient _client = null;

    private CancellationTokenSource _source = null;

    private Task _sender = null;

    private Task _receiver = null;



    public void Start(int clientPort, int serverPort, byte[] request, int sendDelayMilliseconds, int receiveDelayMilliseconds, Action<IPEndPoint, byte[]> onReceive)
    {
        if (_client != null || _source != null || _sender != null || _receiver != null)
            return;


        _source = new CancellationTokenSource();

        CancellationToken token = _source.Token;


        _client = new UdpClient(clientPort);

        _client.EnableBroadcast = true;


        _sender = Task.Run(() =>
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                var count = _client.Send(request, request.Length, new IPEndPoint(IPAddress.Broadcast, serverPort));

                token.ThrowIfCancellationRequested();

                Task.Delay(sendDelayMilliseconds).Wait();
            }

        }, token);

        _receiver = Task.Run(() =>
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                var point = new IPEndPoint(IPAddress.Any, serverPort);

                var bytes = _client.Receive(ref point);

                token.ThrowIfCancellationRequested();

                onReceive(point, bytes);

                Task.Delay(receiveDelayMilliseconds).Wait();
            }

        }, token);
    }

    public void Stop()
    {
        _source?.Cancel();

        _client?.Close();


        _source = null;

        _sender = null;

        _receiver = null;
    }
}
}
