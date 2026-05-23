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
    /// <summary>
    /// Provides UDP broadcast functionality for sending and receiving AppMessage objects over the network.
    /// </summary>
    public sealed class UDPBroadcast
    {
        private UdpClient _socket;

        /// <summary>
        /// Gets or sets the underlying UDP socket used for broadcast operations.
        /// </summary>
        public UdpClient Socket
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
        /// Initializes a new instance of UDPBroadcast with the specified IP endpoint.
        /// </summary>
        /// <param name="point">The IP endpoint to bind the UDP socket to.</param>
        public UDPBroadcast(IPEndPoint point)
    {
        _socket = new UdpClient(point);

        _socket.EnableBroadcast = true;
    }

        /// <summary>
        /// Initializes a new instance of UDPBroadcast with the specified IP address and port.
        /// </summary>
        /// <param name="ip">The IP address to bind the UDP socket to.</param>
        /// <param name="port">The port number to bind the UDP socket to.</param>
        public UDPBroadcast(IPAddress ip, int port) : this(new IPEndPoint(ip, port))
        {
        }

        /// <summary>
        /// Initializes a new instance of UDPBroadcast with the specified IP address and port range, automatically selecting an available port.
        /// </summary>
        /// <param name="ip">The IP address to bind the UDP socket to.</param>
        /// <param name="range">The port range to search for an available port.</param>
        public UDPBroadcast(IPAddress ip, PortRange range)
    {
        PortRange.Store store = range.RangeStore;

        while (true)
        {
            try
            {
                _socket = new UdpClient(new IPEndPoint(ip, store.RandomPort));

                _socket.EnableBroadcast = true;

                break;
            }
            catch (SocketException e)
            {
                DebugConsole.LogError(e.Message);
            }
        }
    }



        /// <summary>
        /// Stops the UDP broadcast by closing and disposing the socket.
        /// </summary>
        public void Stop()
        {
            Socket.Close();

            Socket.Dispose();
        }



        /// <summary>
        /// Sends an AppMessage to the specified IP endpoint synchronously.
        /// </summary>
        /// <param name="point">The IP endpoint to send the message to.</param>
        /// <param name="message">The message to send.</param>
        /// <exception cref="InvalidDataException">Thrown when the message could not be sent completely.</exception>
        public void Send(IPEndPoint point, AppMessage message)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(message));

        int result = Socket.Send(bytes, bytes.Length, point);


        if (result != bytes.Length)
        {
            throw new InvalidDataException();
        }
    }

        /// <summary>
        /// Sends an AppMessage to the specified IP address and port synchronously.
        /// </summary>
        /// <param name="ip">The IP address to send the message to.</param>
        /// <param name="port">The port number to send the message to.</param>
        /// <param name="message">The message to send.</param>
        public void Send(IPAddress ip, int port, AppMessage message)
        {
            Send(new IPEndPoint(ip, port), message);
        }



        /// <summary>
        /// Receives a message synchronously and returns it with its source location.
        /// </summary>
        /// <returns>A LocatedMessage containing the received message and its source IP endpoint.</returns>
        public LocatedMessage Receive()
    {
        IPEndPoint point = null;

        var bytes = Socket.Receive(ref point);

        try
        {
            var message = JsonUtility.FromJson<AppMessage>(Encoding.UTF8.GetString(bytes));

            return new LocatedMessage(point, message);
        }
        catch
        {
            return new LocatedMessage(point, null);
        }
    }

        /// <summary>
        /// Receives a message synchronously with a timeout, returning it with its source location.
        /// </summary>
        /// <param name="timeoutMilliseconds">The timeout in milliseconds.</param>
        /// <returns>A LocatedMessage containing the received message and its source IP endpoint, or an empty LocatedMessage if timeout occurs.</returns>
        public LocatedMessage Receive(int timeoutMilliseconds)
    {
        Task timeout = Task.Run(() =>
        {
            Task.Delay(timeoutMilliseconds).Wait();
        });

        Task<LocatedMessage> receive = Task.Run(() =>
        {
            return Receive();
        });


        int index = Task.WaitAny(receive, timeout);


        return receive.IsCompleted ? receive.Result : new LocatedMessage(null, null);
    }



        /// <summary>
        /// Sends an AppMessage to the specified IP endpoint asynchronously.
        /// </summary>
        /// <param name="point">The IP endpoint to send the message to.</param>
        /// <param name="message">The message to send.</param>
        /// <exception cref="InvalidDataException">Thrown when the message could not be sent completely.</exception>
        public async Task SendAsync(IPEndPoint point, AppMessage message)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(message));

            int result = await Socket.SendAsync(bytes, bytes.Length, point);


            if (result != bytes.Length)
            {
                throw new InvalidDataException();
            }
        }
        catch
        {
            DebugConsole.LogWarning("Null message");
        }
    }

        /// <summary>
        /// Sends an AppMessage to the specified IP address and port asynchronously.
        /// </summary>
        /// <param name="ip">The IP address to send the message to.</param>
        /// <param name="port">The port number to send the message to.</param>
        /// <param name="message">The message to send.</param>
        public async Task SendAsync(IPAddress ip, int port, AppMessage message)
        {
            await SendAsync(new IPEndPoint(ip, port), message);
        }

        /// <summary>
        /// Sends an AppMessage to all ports in the specified range on the given IP address asynchronously.
        /// </summary>
        /// <param name="ip">The IP address to send the message to.</param>
        /// <param name="range">The port range to send the message to.</param>
        /// <param name="message">The message to send.</param>
        public async Task SendAsync(IPAddress ip, PortRange range, AppMessage message)
    {
        for (int port = range.First; port <= range.Last; port++)
        {
            await SendAsync(ip, port, message);
        }
    }



        /// <summary>
        /// Receives a message asynchronously and returns it with its source location.
        /// </summary>
        /// <returns>A LocatedMessage containing the received message and its source IP endpoint, or null if receiving fails.</returns>
        public async Task<LocatedMessage> ReceiveAsync()
    {
        var result = await Socket.ReceiveAsync();


        try
        {
            var message = JsonUtility.FromJson<AppMessage>(Encoding.UTF8.GetString(result.Buffer));

            return new LocatedMessage(result.RemoteEndPoint, message);
        }
        catch
        {
            return null;
        }
    }

        /// <summary>
        /// Receives a message asynchronously with a timeout, returning it with its source location.
        /// </summary>
        /// <param name="timeoutMilliseconds">The timeout in milliseconds.</param>
        /// <returns>A LocatedMessage containing the received message and its source IP endpoint, or null if timeout occurs or receiving fails.</returns>
        public async Task<LocatedMessage> ReceiveAsync(int timeoutMilliseconds)
    {
        Task timeout = Task.Run(async () =>
        {
            await Task.Delay(timeoutMilliseconds);
        });

        Task<LocatedMessage> receive = Task.Run(async () =>
        {
            return await ReceiveAsync();
        });


        Task task = await Task.WhenAny(receive, timeout);


        return receive.IsCompleted ? receive.Result : null;
    }
}
}
