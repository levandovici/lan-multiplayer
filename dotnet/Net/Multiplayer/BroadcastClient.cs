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


namespace Michitai.Lan.Net.Multiplayer
{
    /// <summary>
    /// Broadcast client for discovering and communicating with servers on the LAN.
    /// </summary>
    public sealed class BroadcastClient
    {
        /// <summary>
        /// Delegate for handling received broadcast responses.
        /// </summary>
        /// <param name="response">The located message containing the response.</param>
        public delegate void OnReceiveResponseDelegate(LocatedMessage response);

        private UDPBroadcast _socket;

        /// <summary>
        /// Gets or sets the UDP broadcast socket.
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
        /// Initializes a new instance of BroadcastClient with the specified IP endpoint.
        /// </summary>
        /// <param name="point">The IP endpoint to bind to.</param>
        public BroadcastClient(IPEndPoint point)
    {
        _socket = new UDPBroadcast(point);
    }

        /// <summary>
        /// Initializes a new instance of BroadcastClient with the specified IP address and port.
        /// </summary>
        /// <param name="ip">The IP address to bind to.</param>
        /// <param name="port">The port to bind to.</param>
        public BroadcastClient(IPAddress ip, int port)
    {
        _socket = new UDPBroadcast(ip, port);
    }

        /// <summary>
        /// Initializes a new instance of BroadcastClient with the specified IP and port range.
        /// </summary>
        /// <param name="ip">The IP address to bind to.</param>
        /// <param name="range">The port range to select a port from.</param>
        public BroadcastClient(IPAddress ip, PortRange range)
    {
        _socket = new UDPBroadcast(ip, range);
    }



        /// <summary>
        /// Stops the broadcast client.
        /// </summary>
        public void Stop()
    {
        Socket.Stop();
    }



        /// <summary>
        /// Sends a broadcast request and waits for a response.
        /// </summary>
        /// <param name="ip">The target IP address.</param>
        /// <param name="port">The target port.</param>
        /// <param name="message">The message to broadcast.</param>
        /// <param name="receiveTimeoutMilliseconds">The receive timeout in milliseconds.</param>
        /// <returns>The located message response.</returns>
        public LocatedMessage BroadcastRequest(IPAddress ip, int port, AppMessage message, int receiveTimeoutMilliseconds)
    {
        Socket.Send(ip, port, message);

        return Socket.Receive(receiveTimeoutMilliseconds);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="range"></param>
    /// <param name="message"></param>
    /// <param name="receiveTimeoutMilliseconds"></param>
    /// <returns></returns>
    public LocatedMessage BroadcastRequest(IPAddress ip, PortRange range, AppMessage message, int receiveTimeoutMilliseconds)
    {
        for(int port = range.First; port <= range.Last; port++)
        {
            Socket.Send(ip, port, message);
        }

        return Socket.Receive(receiveTimeoutMilliseconds);
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="message"></param>
    /// <param name="onReceiveResponse"></param>
    /// <param name="receiveResponsesMilliseconds"></param>
    public void BeginBroadcastRequest(IPAddress ip, int port, AppMessage message, OnReceiveResponseDelegate onReceiveResponse, int receiveResponsesMilliseconds)
    {
        Task.Run(() =>
        {
            Socket.Send(ip, port, message);
        });


        CancellationTokenSource source = new CancellationTokenSource();

        CancellationToken token = source.Token;


        Task.Run(() =>
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                onReceiveResponse?.Invoke(Socket.Receive(receiveResponsesMilliseconds));
            }

        }, source.Token);


        Task.Delay(receiveResponsesMilliseconds).Wait();

        source.Cancel();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="masks"></param>
    /// <param name="range"></param>
    /// <param name="message"></param>
    /// <param name="receiveResponsesMilliseconds"></param>
    /// <returns></returns>
    public void BeginBroadcastRequest(IPAddress ip, PortRange range, AppMessage message, OnReceiveResponseDelegate onReceiveResponse, int receiveResponsesMilliseconds)
    {
        Task.Run(() =>
        {
            for (int port = range.First; port <= range.Last; port++)
            {
                Socket.Send(ip, port, message);
            }
        });


        CancellationTokenSource source = new CancellationTokenSource();

        CancellationToken token = source.Token;


        Task.Run(() =>
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                onReceiveResponse?.Invoke(Socket.Receive(receiveResponsesMilliseconds));
            }

        }, source.Token);


        Task.Delay(receiveResponsesMilliseconds).Wait();

        source.Cancel();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="masks"></param>
    /// <param name="port"></param>
    /// <param name="message"></param>
    /// <param name="onReceiveResponse"></param>
    /// <param name="receiveResponsesMilliseconds"></param>
    public void BeginBroadcastRequest(IPAddress[] masks, int port, AppMessage message, OnReceiveResponseDelegate onReceiveResponse, int receiveResponsesMilliseconds)
    {
        Task.Run(() =>
        {
            for (int ip = 0; ip < masks.Length; ip++)
            {
                Socket.Send(masks[ip], port, message);
            }
        });


        CancellationTokenSource source = new CancellationTokenSource();

        CancellationToken token = source.Token;


        Task.Run(() =>
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                onReceiveResponse?.Invoke(Socket.Receive(receiveResponsesMilliseconds));
            }

        }, source.Token);


        Task.Delay(receiveResponsesMilliseconds).Wait();

        source.Cancel();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="masks"></param>
    /// <param name="range"></param>
    /// <param name="message"></param>
    /// <param name="onReceiveResponse"></param>
    /// <param name="receiveResponsesMilliseconds"></param>
    public void BeginBroadcastRequest(IPAddress[] masks, PortRange range, AppMessage message, OnReceiveResponseDelegate onReceiveResponse, int receiveResponsesMilliseconds)
    {
        Task.Run(() =>
        {
            for (int ip = 0; ip < masks.Length; ip++)
            {
                for (int port = range.First; port <= range.Last; port++)
                {
                    Socket.Send(masks[ip], port, message);
                }
            }
        });


        CancellationTokenSource source = new CancellationTokenSource();

        CancellationToken token = source.Token;

        Task.Run(() =>
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                onReceiveResponse?.Invoke(Socket.Receive(receiveResponsesMilliseconds));
            }

        }, source.Token);


        Task.Delay(receiveResponsesMilliseconds).Wait();

        source.Cancel();
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="message"></param>
    /// <param name="receiveTimeoutMilliseconds"></param>
    /// <returns></returns>
    public async Task<LocatedMessage> BroadcastRequestAsync(IPAddress ip, int port, AppMessage message, int receiveTimeoutMilliseconds)
    {
        await Socket.SendAsync(ip, port, message);

        return await Socket.ReceiveAsync(receiveTimeoutMilliseconds);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<LocatedMessage> BroadcastRequestAsync(IPAddress ip, PortRange range, AppMessage message, int receiveTimeoutMilliseconds)
    {
        await Socket.SendAsync(ip, range, message);

        return await Socket.ReceiveAsync(receiveTimeoutMilliseconds);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="message"></param>
    /// <param name="onReceiveResponse"></param>
    /// <param name="receiveResponsesMilliseconds"></param>
    /// <returns></returns>
    public async Task BroadcastRequestAsync(IPAddress ip, int port, AppMessage message, OnReceiveResponseDelegate onReceiveResponse, int receiveResponsesMilliseconds)
    {
        await Socket.SendAsync(ip, port, message);


        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        CancellationToken token = cancellationTokenSource.Token;


        Task task = Task.Run(async () =>
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                LocatedMessage locatedAppMessage = await Socket.ReceiveAsync();

                if (locatedAppMessage == null)
                    continue;

                onReceiveResponse?.Invoke(locatedAppMessage);
            }

        }, cancellationTokenSource.Token);


        await Task.Delay(receiveResponsesMilliseconds);

        cancellationTokenSource.Cancel();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="range"></param>
    /// <param name="message"></param>
    /// <param name="onReceiveResponse"></param>
    /// <param name="receiveResponsesMilliseconds"></param>
    /// <returns></returns>
    public async Task BroadcastRequestAsync(IPAddress ip, PortRange range, AppMessage message, OnReceiveResponseDelegate onReceiveResponse, int receiveResponsesMilliseconds)
    {
        await Socket.SendAsync(ip, range, message);


        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        CancellationToken token = cancellationTokenSource.Token;

        Task task = Task.Run(async () =>
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                LocatedMessage locatedAppMessage = await Socket.ReceiveAsync();

                if (locatedAppMessage == null)
                    continue;

                onReceiveResponse?.Invoke(locatedAppMessage);
            }

        }, cancellationTokenSource.Token);


        await Task.Delay(receiveResponsesMilliseconds);

        cancellationTokenSource.Cancel();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="masks"></param>
    /// <param name="port"></param>
    /// <param name="message"></param>
    /// <param name="onReceiveResponse"></param>
    /// <param name="receiveResponsesMilliseconds"></param>
    /// <returns></returns>
    public async Task BroadcastRequestAsync(IPAddress[] masks, int port, AppMessage message, OnReceiveResponseDelegate onReceiveResponse, int receiveResponsesMilliseconds)
    {
        for (int i = 0; i < masks.Length; i++)
        {
            await Socket.SendAsync(masks[i], port, message);
        }


        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        CancellationToken token = cancellationTokenSource.Token;

        Task task = Task.Run(async () =>
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                LocatedMessage locatedAppMessage = await Socket.ReceiveAsync();

                if (locatedAppMessage == null)
                    continue;

                onReceiveResponse?.Invoke(locatedAppMessage);
            }

        }, cancellationTokenSource.Token);


        await Task.Delay(receiveResponsesMilliseconds);

        cancellationTokenSource.Cancel();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="masks"></param>
    /// <param name="range"></param>
    /// <param name="message"></param>
    /// <param name="onReceiveResponse"></param>
    /// <param name="receiveResponsesMilliseconds"></param>
    /// <returns></returns>
    public async Task BroadcastRequestAsync(IPAddress[] masks, PortRange range, AppMessage message, OnReceiveResponseDelegate onReceiveResponse, int receiveResponsesMilliseconds)
    {
        for (int i = 0; i < masks.Length; i++)
        {
            await Socket.SendAsync(masks[i], range, message);
        }


        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        CancellationToken token = cancellationTokenSource.Token;

        Task task = Task.Run(async () =>
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                LocatedMessage locatedAppMessage = await Socket.ReceiveAsync();

                if (locatedAppMessage == null)
                    continue;

                onReceiveResponse?.Invoke(locatedAppMessage);
            }

        }, cancellationTokenSource.Token);


        await Task.Delay(receiveResponsesMilliseconds);

        cancellationTokenSource.Cancel();
    }
}
}
