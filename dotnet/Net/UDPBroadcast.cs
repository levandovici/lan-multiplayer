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


namespace Michitai.Lan.Net
{
    /// <summary>
    /// Provides UDP broadcast functionality for network discovery and message broadcasting.
    /// </summary>
    public sealed class UDPBroadcast
    {
        private UdpClient _socket;

        /// <summary>
        /// Gets or sets the UDP socket.
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
        /// <param name="point">The IP endpoint to bind to.</param>
        public UDPBroadcast(IPEndPoint point)
    {
        _socket = new UdpClient(point);

        _socket.EnableBroadcast = true;
    }

        /// <summary>
        /// Initializes a new instance of UDPBroadcast with the specified IP address and port.
        /// </summary>
        /// <param name="ip">The IP address to bind to.</param>
        /// <param name="port">The port to bind to.</param>
        public UDPBroadcast(IPAddress ip, int port) : this(new IPEndPoint(ip, port))
    {
    }

        /// <summary>
        /// Initializes a new instance of UDPBroadcast with the specified IP and port range.
        /// </summary>
        /// <param name="ip">The IP address to bind to.</param>
        /// <param name="range">The port range to select a port from.</param>
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
        /// Stops the UDP broadcast and cleans up resources.
        /// </summary>
        public void Stop()
    {
        Socket.Close();

        Socket.Dispose();
    }



        /// <summary>
        /// Sends an application message to the specified endpoint.
        /// </summary>
        /// <param name="point">The target IP endpoint.</param>
        /// <param name="message">The message to send.</param>
        /// <exception cref="InvalidDataException">Thrown when not all bytes are sent.</exception>
        public void Send(IPEndPoint point, AppMessage message)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        int result = Socket.Send(bytes, bytes.Length, point);


        if (result != bytes.Length)
        {
            throw new InvalidDataException();
        }
    }

        /// <summary>
        /// Sends an application message to the specified IP address and port.
        /// </summary>
        /// <param name="ip">The target IP address.</param>
        /// <param name="port">The target port.</param>
        /// <param name="message">The message to send.</param>
        public void Send(IPAddress ip, int port, AppMessage message)
    {
        Send(new IPEndPoint(ip, port), message);
    }



        /// <summary>
        /// Receives a message from any sender.
        /// </summary>
        /// <returns>The located message containing sender endpoint and message.</returns>
        public LocatedMessage Receive()
    {
        IPEndPoint point = null;

        var bytes = Socket.Receive(ref point);

        try
        {
            var message = JsonSerializer.Deserialize<AppMessage>(Encoding.UTF8.GetString(bytes));

            return new LocatedMessage(point, message);
        }
        catch
        {
            return new LocatedMessage(point, null);
        }
    }

        /// <summary>
        /// Receives a message with a timeout.
        /// </summary>
        /// <param name="timeoutMilliseconds">The timeout in milliseconds.</param>
        /// <returns>The located message, or null if timeout occurs.</returns>
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
        /// Asynchronously sends an application message to the specified endpoint.
        /// </summary>
        /// <param name="point">The target IP endpoint.</param>
        /// <param name="message">The message to send.</param>
        /// <exception cref="InvalidDataException">Thrown when not all bytes are sent.</exception>
        public async Task SendAsync(IPEndPoint point, AppMessage message)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        int result = await Socket.SendAsync(bytes, bytes.Length, point);


        if (result != bytes.Length)
        {
            throw new InvalidDataException();
        }
    }

        /// <summary>
        /// Asynchronously sends an application message to the specified IP address and port.
        /// </summary>
        /// <param name="ip">The target IP address.</param>
        /// <param name="port">The target port.</param>
        /// <param name="message">The message to send.</param>
        public async Task SendAsync(IPAddress ip, int port, AppMessage message)
    {
        await SendAsync(new IPEndPoint(ip, port), message);
    }

        /// <summary>
        /// Asynchronously sends an application message to all ports in the specified range.
        /// </summary>
        /// <param name="ip">The target IP address.</param>
        /// <param name="range">The port range to send to.</param>
        /// <param name="message">The message to send.</param>
        public async Task SendAsync(IPAddress ip, PortRange range, AppMessage message)
    {
        for (int port = range.First; port <= range.Last; port++)
        {
            await SendAsync(ip, port, message);
        }
    }



        /// <summary>
        /// Asynchronously receives a message from any sender.
        /// </summary>
        /// <returns>The located message, or null if an error occurs.</returns>
        public async Task<LocatedMessage> ReceiveAsync()
    {
        var result = await Socket.ReceiveAsync();


        try
        {
            var message = JsonSerializer.Deserialize<AppMessage>(Encoding.UTF8.GetString(result.Buffer));

            return new LocatedMessage(result.RemoteEndPoint, message);
        }
        catch
        {
            return null;
        }
    }

        /// <summary>
        /// Asynchronously receives a message with a timeout.
        /// </summary>
        /// <param name="timeoutMilliseconds">The timeout in milliseconds.</param>
        /// <returns>The located message, or null if timeout occurs.</returns>
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
