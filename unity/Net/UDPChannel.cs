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
    /// Unreliable UDP datagram channel for high-frequency state synchronization
    /// (positions, transforms, inputs) at 30-60+ Hz without TCP head-of-line blocking.
    /// Datagram format: [1-byte flags][payload], payload compressed when beneficial.
    /// </summary>
    public sealed class UDPChannel
    {
        /// <summary>
        /// Maximum UDP datagram payload (IPv4). Datagrams larger than this are rejected.
        /// </summary>
        public const int MaxDatagramSize = 65507;

        /// <summary>
        /// Datagrams larger than this may be fragmented by IP and lose parts on congested networks.
        /// Keep per-frame state under this size for reliability (Ethernet MTU 1500 minus headers).
        /// </summary>
        public const int SafeDatagramSize = 1472;

        private UdpClient _socket;

        private bool _closed;

        private readonly object _send_lock;

        private readonly List<IPEndPoint> _peers;

        private readonly object _peers_lock;

        /// <summary>
        /// Event raised when a datagram is received. First argument is the sender endpoint.
        /// </summary>
        public event Action<IPEndPoint, Message> OnReceive;

        /// <summary>
        /// Gets the local endpoint the channel is bound to, or null when not started.
        /// </summary>
        public IPEndPoint IPEndPoint
    {
        get
        {
            try
            {
                return _socket?.Client?.LocalEndPoint as IPEndPoint;
            }
            catch
            {
                return null;
            }
        }
    }

        /// <summary>
        /// Gets the endpoints this channel has received datagrams from. Thread-safe snapshot.
        /// Used by Broadcast to reach all known peers.
        /// </summary>
        public IPEndPoint[] Peers
    {
        get
        {
            lock (_peers_lock)
            {
                return _peers.ToArray();
            }
        }
    }

        /// <summary>
        /// Gets whether the channel is closed.
        /// </summary>
        public bool IsClosed => _closed;



        /// <summary>
        /// Initializes a new instance of UDPChannel bound to the specified endpoint.
        /// </summary>
        /// <param name="point">The local IP endpoint to bind to. Use port 0 for an ephemeral port.</param>
        public UDPChannel(IPEndPoint point)
    {
        _socket = new UdpClient(point);

        _peers = new List<IPEndPoint>();

        _peers_lock = new object();

        _send_lock = new object();

        _closed = false;
    }

        /// <summary>
        /// Initializes a new instance of UDPChannel bound to the specified IP and port.
        /// </summary>
        /// <param name="ip">The local IP address to bind to.</param>
        /// <param name="port">The local port to bind to. Use 0 for an ephemeral port.</param>
        public UDPChannel(IPAddress ip, int port) : this(new IPEndPoint(ip, port))
    {
    }

        /// <summary>
        /// Initializes a new instance of UDPChannel bound to a free port from the specified range.
        /// </summary>
        /// <param name="ip">The local IP address to bind to.</param>
        /// <param name="range">The port range to select a free port from.</param>
        public UDPChannel(IPAddress ip, PortRange range)
    {
        PortRange.Store store = range.RangeStore;

        while (true)
        {
            try
            {
                _socket = new UdpClient(new IPEndPoint(ip, store.RandomPort));

                break;
            }
            catch (SocketException)
            {
            }
        }

        _peers = new List<IPEndPoint>();

        _peers_lock = new object();

        _send_lock = new object();

        _closed = false;
    }



        /// <summary>
        /// Starts the asynchronous receive loop.
        /// </summary>
        public void Start()
    {
        try
        {
            _socket.BeginReceive(ReceiveCallback, null);
        }
        catch
        {
            Stop();
        }
    }

        /// <summary>
        /// Stops the channel and releases the socket.
        /// </summary>
        public void Stop()
    {
        if (_closed)
            return;

        _closed = true;

        OnReceive = null;

        try
        {
            _socket?.Close();

            _socket?.Dispose();
        }
        catch
        {
        }
    }



        /// <summary>
        /// Sends a message to the specified endpoint as a single datagram.
        /// </summary>
        /// <param name="target">The destination endpoint.</param>
        /// <param name="message">The message to send.</param>
        /// <exception cref="InvalidDataException">Thrown when the datagram exceeds MaxDatagramSize.</exception>
        public void Send(IPEndPoint target, Message message)
    {
        Send(target, message?.GetMessage ?? string.Empty);
    }

        /// <summary>
        /// Sends a string to the specified endpoint as a single datagram.
        /// </summary>
        /// <param name="target">The destination endpoint.</param>
        /// <param name="text">The string to send.</param>
        /// <exception cref="InvalidDataException">Thrown when the datagram exceeds MaxDatagramSize.</exception>
        public void Send(IPEndPoint target, string text)
    {
        byte[] payload = Encoding.UTF8.GetBytes(text ?? string.Empty);

        bool compressed = Compressor.TryCompress(payload, out payload);

        byte[] datagram = new byte[payload.Length + 1];

        datagram[0] = compressed ? Frame.FlagCompressed : Frame.FlagNone;

        Buffer.BlockCopy(payload, 0, datagram, 1, payload.Length);

        if (datagram.Length > MaxDatagramSize)
        {
            throw new InvalidDataException($"Datagram exceeds maximum size ({datagram.Length} > {MaxDatagramSize})");
        }

        if (datagram.Length > SafeDatagramSize)
        {
            DebugConsole.LogWarning($"[Michitai.Lan][UDP-CHANNEL][DATAGRAM-FRAGMENTATION-RISK][{datagram.Length}]");
        }

        lock (_send_lock)
        {
            if (_closed)
                return;

            _socket.Send(datagram, datagram.Length, target);
        }
    }

        /// <summary>
        /// Sends a message to all endpoints that have sent datagrams to this channel.
        /// </summary>
        /// <param name="message">The message to broadcast.</param>
        public void Broadcast(Message message)
    {
        IPEndPoint[] peers = Peers;

        for (int i = 0; i < peers.Length; i++)
        {
            try
            {
                Send(peers[i], message);
            }
            catch
            {
            }
        }
    }

        /// <summary>
        /// Removes an endpoint from the known peers list (e.g., when a client disconnects).
        /// </summary>
        /// <param name="point">The endpoint to forget.</param>
        public void Forget(IPEndPoint point)
    {
        lock (_peers_lock)
        {
            _peers.Remove(point);
        }
    }



        /// <summary>
        /// Callback for asynchronous datagram reception.
        /// </summary>
        /// <param name="result">The asynchronous result.</param>
        private void ReceiveCallback(IAsyncResult result)
    {
        IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);

        byte[] datagram;

        try
        {
            datagram = _socket.EndReceive(result, ref remote);
        }
        catch
        {
            if (!_closed)
            {
                Stop();
            }

            return;
        }

        if (datagram != null && datagram.Length > 1)
        {
            try
            {
                byte flags = datagram[0];

                int length = datagram.Length - 1;

                byte[] payload = new byte[length];

                Buffer.BlockCopy(datagram, 1, payload, 0, length);

                if ((flags & Frame.FlagCompressed) == Frame.FlagCompressed)
                {
                    payload = Compressor.Decompress(payload);
                }

                TrackPeer(remote);

                OnReceive?.Invoke(remote, new Message(Encoding.UTF8.GetString(payload)));
            }
            catch
            {
                DebugConsole.LogError("[Michitai.Lan][UDP-CHANNEL][RECEIVE-ERROR]");
            }
        }

        try
        {
            _socket.BeginReceive(ReceiveCallback, null);
        }
        catch
        {
            if (!_closed)
            {
                Stop();
            }
        }
    }

        /// <summary>
        /// Tracks a sender endpoint for later Broadcast use.
        /// </summary>
        /// <param name="point">The endpoint to track.</param>
        private void TrackPeer(IPEndPoint point)
    {
        lock (_peers_lock)
        {
            if (!_peers.Contains(point))
            {
                _peers.Add(point);
            }
        }
    }
}
}
