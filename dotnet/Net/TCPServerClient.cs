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

namespace Michitai.Lan.Net
{
    /// <summary>
    /// Represents a connected TCP client on the server side, using the
    /// length-prefixed frame protocol with NoDelay, SocketAsyncEventArgs I/O,
    /// queued writes, and GZip payload compression.
    /// </summary>
    public sealed class TCPServerClient
    {
        private string _id;

        private TcpClient _client;

        private Socket _socket;

        private SocketAsyncEventArgs _read_args;

        private SocketAsyncEventArgs _write_args;

        private byte[] _read_buffer;

        private int _buffer_size;

        private FrameBuffer _frame_buffer;

        private readonly Queue<byte[]> _send_queue;

        private readonly object _send_lock;

        private bool _writing;

        private bool _closed;

        private readonly object _state_lock;

        /// <summary>
        /// Event raised when a request message is received.
        /// </summary>
        private event Action<IdentifiedMessage> OnRequest;

        /// <summary>
        /// Event raised when the client stops.
        /// </summary>
        private event Action<string> OnStop;

        /// <summary>
        /// Gets the unique client ID.
        /// </summary>
        public string ID => _id;

        /// <summary>
        /// Gets whether the client is closed.
        /// </summary>
        public bool IsClosed
    {
        get
        {
            return _closed;
        }

        private set
        {
            _closed = value;
        }
    }

        /// <summary>
        /// Gets the number of frames queued for writing (backpressure indicator).
        /// </summary>
        public int PendingWrites
    {
        get
        {
            lock (_send_lock)
            {
                return _send_queue.Count;
            }
        }
    }


        /// <summary>
        /// Initializes a new instance of TCPServerClient with the specified parameters.
        /// </summary>
        /// <param name="client">The accepted TCP client connection.</param>
        /// <param name="on_request">Callback for request messages.</param>
        /// <param name="on_stop">Callback for client disconnection.</param>
        /// <param name="buffer_size">The read buffer size in bytes.</param>
        public TCPServerClient(TcpClient client, Action<IdentifiedMessage> on_request, Action<string> on_stop, int buffer_size = 8192)
    {
        _id = Guid.NewGuid().ToString();

        _client = client;

        _socket = client.Client;

        // Disable Nagle's algorithm — required for 30-60Hz small-message latency.
        _socket.NoDelay = true;

        _socket.SendBufferSize = 65536;

        _socket.ReceiveBufferSize = 65536;

        _buffer_size = buffer_size;

        _read_buffer = new byte[buffer_size];

        _frame_buffer = new FrameBuffer(buffer_size * 2);

        _send_queue = new Queue<byte[]>();

        _send_lock = new object();

        _state_lock = new object();

        _writing = false;

        IsClosed = false;

        OnRequest += on_request;

        OnStop += on_stop;
    }



        /// <summary>
        /// Starts the receive loop. Must be called only after the client is
        /// registered with the server (TCPServer.AddClient), otherwise requests
        /// arriving during construction would have their responses dropped.
        /// </summary>
        public void Start()
    {
        _read_args = new SocketAsyncEventArgs();

        _read_args.SetBuffer(_read_buffer, 0, _buffer_size);

        _read_args.Completed += OnReadCompleted;

        _write_args = new SocketAsyncEventArgs();

        _write_args.Completed += OnWriteCompleted;



        IssueRead();
    }



        /// <summary>
        /// Stops the client and cleans up resources.
        /// </summary>
        public void Stop()
    {
        lock (_state_lock)
        {
            if (IsClosed)
                return;

            IsClosed = true;
        }

        OnRequest = null;

        lock (_send_lock)
        {
            _send_queue.Clear();

            _writing = false;
        }

        try
        {
            if (_socket.Connected)
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
        }
        catch
        {
        }

        try
        {
            _read_args?.Dispose();

            _write_args?.Dispose();
        }
        catch
        {
        }

        try
        {
            _client.Close();

            _client.Dispose();
        }
        catch
        {
        }


        OnStop?.Invoke(ID);
    }



        /// <summary>
        /// Sends a response message to the client. Messages are framed, optionally
        /// compressed, and queued so concurrent calls never interleave or get lost.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void Response(Message message)
    {
        if (IsClosed)
            return;

        byte[] frame = Frame.Pack(message.GetMessage);

        lock (_send_lock)
        {
            _send_queue.Enqueue(frame);

            if (_writing)
                return;

            _writing = true;
        }

        PumpWrite();
    }



        /// <summary>
        /// Issues the next socket read.
        /// </summary>
        private void IssueRead()
    {
        bool pending;

        try
        {
            pending = _socket.ReceiveAsync(_read_args);
        }
        catch
        {
            Stop();
            return;
        }

        if (!pending)
        {
            ProcessRead(_read_args);
        }
    }

        /// <summary>
        /// Handles a completed socket read (event or synchronous completion),
        /// drains all complete frames, and reissues the read.
        /// </summary>
        /// <param name="e">The completed async event args.</param>
        private void ProcessRead(SocketAsyncEventArgs e)
    {
        while (true)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred <= 0)
            {
                Stop();
                return;
            }

            try
            {
                _frame_buffer.Write(e.Buffer, e.Offset, e.BytesTransferred);

                byte[] payload;

                byte flags;

                while (_frame_buffer.TryRead(out payload, out flags))
                {
                    Message message = new Message(Frame.Unpack(payload, flags));

                    try
                    {
                        OnRequest?.Invoke(new IdentifiedMessage(message, _id));
                    }
                    catch (Exception ex)
                    {
                        DebugConsole.LogError($"[Michitai.Lan][S-REQUEST-HANDLER-ERROR][{ex.Message}]");
                    }
                }
            }
            catch (Exception ex)
            {
                DebugConsole.LogError($"[Michitai.Lan][S-FRAME-ERROR][{ex.GetType().Name}:{ex.Message}]");

                Stop();
                return;
            }

            // Reissue the read; loop when the socket completes synchronously.
            bool pending;

            try
            {
                pending = _socket.ReceiveAsync(_read_args);
            }
            catch
            {
                Stop();
                return;
            }

            if (pending)
                return;
        }
    }

        /// <summary>
        /// SocketAsyncEventArgs completion handler for reads.
        /// </summary>
        private void OnReadCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (IsClosed)
            return;

        try
        {
            ProcessRead(e);
        }
        catch
        {
            // Args disposed or socket torn down during Stop().
        }
    }



        /// <summary>
        /// Dequeues the next frame and writes it to the socket, looping
        /// while sends complete synchronously.
        /// </summary>
        private void PumpWrite()
    {
        while (true)
        {
            byte[] frame;

            lock (_send_lock)
            {
                if (_send_queue.Count == 0 || IsClosed)
                {
                    _writing = false;

                    return;
                }

                frame = _send_queue.Dequeue();
            }

            _write_args.SetBuffer(frame, 0, frame.Length);

            bool pending;

            try
            {
                pending = _socket.SendAsync(_write_args);
            }
            catch
            {
                Stop();
                return;
            }

            if (pending)
                return;

            if (_write_args.SocketError != SocketError.Success)
            {
                Stop();
                return;
            }

            // Completed synchronously — loop to send the next queued frame.
        }
    }

        /// <summary>
        /// SocketAsyncEventArgs completion handler for writes.
        /// </summary>
        private void OnWriteCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (IsClosed)
            return;

        try
        {
            if (e.SocketError != SocketError.Success)
            {
                Stop();
                return;
            }

            PumpWrite();
        }
        catch
        {
            // Args disposed or socket torn down during Stop().
        }
    }
}
}
