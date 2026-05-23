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
    /// <summary>
    /// Provides broadcast client functionality for discovering and communicating with servers on the network.
    /// </summary>
    public sealed class BroadcastClient
    {
        /// <summary>
        /// Delegate for handling received broadcast responses.
        /// </summary>
        /// <param name="response">The located message containing the response and its source.</param>
        public delegate void OnReceiveResponseDelegate(LocatedMessage response);

        private UDPBroadcast _socket;

        /// <summary>
        /// Gets or sets the underlying UDP broadcast socket.
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
        /// <param name="point">The IP endpoint to bind the broadcast socket to.</param>
        public BroadcastClient(IPEndPoint point)
    {
        _socket = new UDPBroadcast(point);
    }

        /// <summary>
        /// Initializes a new instance of BroadcastClient with the specified IP address and port.
        /// </summary>
        /// <param name="ip">The IP address to bind the broadcast socket to.</param>
        /// <param name="port">The port number to bind the broadcast socket to.</param>
        public BroadcastClient(IPAddress ip, int port)
    {
        _socket = new UDPBroadcast(ip, port);
    }

        /// <summary>
        /// Initializes a new instance of BroadcastClient with the specified IP address and port range, automatically selecting an available port.
        /// </summary>
        /// <param name="ip">The IP address to bind the broadcast socket to.</param>
        /// <param name="range">The port range to search for an available port.</param>
        public BroadcastClient(IPAddress ip, PortRange range)
    {
        _socket = new UDPBroadcast(ip, range);
    }



        /// <summary>
        /// Stops the broadcast client by closing the underlying socket.
        /// </summary>
        public void Stop()
    {
        Socket.Stop();
    }



        /// <summary>
        /// Sends a broadcast request to a single IP and port, and waits for a response.
        /// </summary>
        /// <param name="ip">The IP address to send the broadcast to.</param>
        /// <param name="port">The port number to send the broadcast to.</param>
        /// <param name="message">The message to broadcast.</param>
        /// <param name="receiveTimeoutMilliseconds">The timeout in milliseconds for receiving a response.</param>
        /// <returns>The located message containing the response.</returns>
        public LocatedMessage BroadcastRequest(IPAddress ip, int port, AppMessage message, int receiveTimeoutMilliseconds)
    {
        Socket.Send(ip, port, message);

        return Socket.Receive(receiveTimeoutMilliseconds);
    }

        /// <summary>
        /// Sends a broadcast request to a port range on a single IP, and waits for a response.
        /// </summary>
        /// <param name="ip">The IP address to send the broadcast to.</param>
        /// <param name="range">The port range to send the broadcast to.</param>
        /// <param name="message">The message to broadcast.</param>
        /// <param name="receiveTimeoutMilliseconds">The timeout in milliseconds for receiving a response.</param>
        /// <returns>The located message containing the response.</returns>
        public LocatedMessage BroadcastRequest(IPAddress ip, PortRange range, AppMessage message, int receiveTimeoutMilliseconds)
    {
        for (int port = range.First; port <= range.Last; port++)
        {
            Socket.Send(ip, port, message);
        }

        return Socket.Receive(receiveTimeoutMilliseconds);
    }



        /// <summary>
        /// Begins an asynchronous broadcast request to a single IP and port, invoking a callback for each response.
        /// </summary>
        /// <param name="ip">The IP address to send the broadcast to.</param>
        /// <param name="port">The port number to send the broadcast to.</param>
        /// <param name="message">The message to broadcast.</param>
        /// <param name="onReceiveResponse">Callback invoked for each received response.</param>
        /// <param name="receiveResponsesMilliseconds">The duration in milliseconds to receive responses.</param>
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
        /// Begins an asynchronous broadcast request to a port range on a single IP, invoking a callback for each response.
        /// </summary>
        /// <param name="ip">The IP address to send the broadcast to.</param>
        /// <param name="range">The port range to send the broadcast to.</param>
        /// <param name="message">The message to broadcast.</param>
        /// <param name="onReceiveResponse">Callback invoked for each received response.</param>
        /// <param name="receiveResponsesMilliseconds">The duration in milliseconds to receive responses.</param>
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
        /// Begins an asynchronous broadcast request to multiple IPs on a single port, invoking a callback for each response.
        /// </summary>
        /// <param name="masks">The array of IP addresses to send the broadcast to.</param>
        /// <param name="port">The port number to send the broadcast to.</param>
        /// <param name="message">The message to broadcast.</param>
        /// <param name="onReceiveResponse">Callback invoked for each received response.</param>
        /// <param name="receiveResponsesMilliseconds">The duration in milliseconds to receive responses.</param>
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
        /// Begins an asynchronous broadcast request to multiple IPs on a port range, invoking a callback for each response.
        /// </summary>
        /// <param name="masks">The array of IP addresses to send the broadcast to.</param>
        /// <param name="range">The port range to send the broadcast to.</param>
        /// <param name="message">The message to broadcast.</param>
        /// <param name="onReceiveResponse">Callback invoked for each received response.</param>
        /// <param name="receiveResponsesMilliseconds">The duration in milliseconds to receive responses.</param>
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
        /// Asynchronously sends a broadcast request to a single IP and port, and waits for a response.
        /// </summary>
        /// <param name="ip">The IP address to send the broadcast to.</param>
        /// <param name="port">The port number to send the broadcast to.</param>
        /// <param name="message">The message to broadcast.</param>
        /// <param name="receiveTimeoutMilliseconds">The timeout in milliseconds for receiving a response.</param>
        /// <returns>The located message containing the response.</returns>
        public async Task<LocatedMessage> BroadcastRequestAsync(IPAddress ip, int port, AppMessage message, int receiveTimeoutMilliseconds)
    {
        await Socket.SendAsync(ip, port, message);

        return await Socket.ReceiveAsync(receiveTimeoutMilliseconds);
    }

        /// <summary>
        /// Asynchronously sends a broadcast request to a port range on a single IP, and waits for a response.
        /// </summary>
        /// <param name="ip">The IP address to send the broadcast to.</param>
        /// <param name="range">The port range to send the broadcast to.</param>
        /// <param name="message">The message to broadcast.</param>
        /// <param name="receiveTimeoutMilliseconds">The timeout in milliseconds for receiving a response.</param>
        /// <returns>The located message containing the response.</returns>
        public async Task<LocatedMessage> BroadcastRequestAsync(IPAddress ip, PortRange range, AppMessage message, int receiveTimeoutMilliseconds)
    {
        await Socket.SendAsync(ip, range, message);

        return await Socket.ReceiveAsync(receiveTimeoutMilliseconds);
    }


        /// <summary>
        /// Asynchronously sends a broadcast request to a single IP and port, invoking a callback for each response.
        /// </summary>
        /// <param name="ip">The IP address to send the broadcast to.</param>
        /// <param name="port">The port number to send the broadcast to.</param>
        /// <param name="message">The message to broadcast.</param>
        /// <param name="onReceiveResponse">Callback invoked for each received response.</param>
        /// <param name="receiveResponsesMilliseconds">The duration in milliseconds to receive responses.</param>
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
        /// Asynchronously sends a broadcast request to a port range on a single IP, invoking a callback for each response.
        /// </summary>
        /// <param name="ip">The IP address to send the broadcast to.</param>
        /// <param name="range">The port range to send the broadcast to.</param>
        /// <param name="message">The message to broadcast.</param>
        /// <param name="onReceiveResponse">Callback invoked for each received response.</param>
        /// <param name="receiveResponsesMilliseconds">The duration in milliseconds to receive responses.</param>
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
        /// Asynchronously sends a broadcast request to multiple IPs on a single port, invoking a callback for each response.
        /// </summary>
        /// <param name="masks">The array of IP addresses to send the broadcast to.</param>
        /// <param name="port">The port number to send the broadcast to.</param>
        /// <param name="message">The message to broadcast.</param>
        /// <param name="onReceiveResponse">Callback invoked for each received response.</param>
        /// <param name="receiveResponsesMilliseconds">The duration in milliseconds to receive responses.</param>
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
        /// Asynchronously sends a broadcast request to multiple IPs on a port range, invoking a callback for each response.
        /// </summary>
        /// <param name="masks">The array of IP addresses to send the broadcast to.</param>
        /// <param name="range">The port range to send the broadcast to.</param>
        /// <param name="message">The message to broadcast.</param>
        /// <param name="onReceiveResponse">Callback invoked for each received response.</param>
        /// <param name="receiveResponsesMilliseconds">The duration in milliseconds to receive responses.</param>
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
