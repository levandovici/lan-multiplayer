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
    /// Provides broadcast server functionality for responding to broadcast requests from clients.
    /// </summary>
    public sealed class BroadcastServer
    {
        /// <summary>
        /// Delegate for processing incoming broadcast messages and generating responses.
        /// </summary>
        /// <param name="incoming">The located message containing the incoming request.</param>
        /// <returns>The app message to send as a response.</returns>
        public delegate AppMessage ProcessMessageDelegate(LocatedMessage incoming);

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
        /// Initializes a new instance of BroadcastServer with the specified IP endpoint.
        /// </summary>
        /// <param name="point">The IP endpoint to bind the broadcast socket to.</param>
        public BroadcastServer(IPEndPoint point)
    {
        _socket = new UDPBroadcast(point);
    }

        /// <summary>
        /// Initializes a new instance of BroadcastServer with the specified IP address and port.
        /// </summary>
        /// <param name="ip">The IP address to bind the broadcast socket to.</param>
        /// <param name="port">The port number to bind the broadcast socket to.</param>
        public BroadcastServer(IPAddress ip, int port)
    {
        _socket = new UDPBroadcast(ip, port);
    }

        /// <summary>
        /// Initializes a new instance of BroadcastServer with the specified IP address and port range, automatically selecting an available port.
        /// </summary>
        /// <param name="ip">The IP address to bind the broadcast socket to.</param>
        /// <param name="range">The port range to search for an available port.</param>
        public BroadcastServer(IPAddress ip, PortRange range)
    {
        _socket = new UDPBroadcast(ip, range);
    }



        /// <summary>
        /// Stops the broadcast server by closing the underlying socket.
        /// </summary>
        public void Stop()
    {
        Socket.Stop();
    }



        /// <summary>
        /// Receives a broadcast message, processes it, and sends a response synchronously.
        /// </summary>
        /// <param name="process">The delegate to process the incoming message.</param>
        public void Broadcast(ProcessMessageDelegate process)
    {
        LocatedMessage message = Socket.Receive();

        Socket.Send(message.IPEndPoint, process.Invoke(message));
    }

        /// <summary>
        /// Receives a broadcast message, processes it, and sends a response synchronously with a timeout.
        /// </summary>
        /// <param name="process">The delegate to process the incoming message.</param>
        /// <param name="timeoutMilliseconds">The timeout in milliseconds.</param>
        /// <returns>True if the broadcast completed within the timeout; otherwise, false.</returns>
        public bool Broadcast(ProcessMessageDelegate process, int timeoutMilliseconds)
    {
        Task broadcast = Task.Run(() => Broadcast(process));

        return broadcast.Wait(new TimeSpan(timeoutMilliseconds * 10000));
    }


        /// <summary>
        /// Asynchronously receives a broadcast message, processes it, and sends a response.
        /// </summary>
        /// <param name="process">The delegate to process the incoming message.</param>
        public async Task BroadcastAsync(ProcessMessageDelegate process)
    {
        LocatedMessage incoming = await Socket.ReceiveAsync();

        if (incoming == null)
            return;

        await Socket.SendAsync(incoming.IPEndPoint, process.Invoke(incoming));
    }

        /// <summary>
        /// Asynchronously receives a broadcast message, processes it, and sends a response with a timeout.
        /// </summary>
        /// <param name="process">The delegate to process the incoming message.</param>
        /// <param name="timeoutMilliseconds">The timeout in milliseconds.</param>
        /// <returns>True if the broadcast completed within the timeout; otherwise, false.</returns>
        public async Task<bool> BroadcastAsync(ProcessMessageDelegate process, int timeoutMilliseconds)
    {
        Task timeout = Task.Run(async () => await Task.Delay(timeoutMilliseconds));

        Task broadcast = Task.Run(async () => await BroadcastAsync(process));


        Task task = await Task.WhenAny(broadcast, timeout);


        return broadcast.IsCompleted;
    }
}
}
