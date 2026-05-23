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
    /// Represents a connected TCP client on the server side.
    /// </summary>
    public sealed class TCPServerClient
    {
        private string _id;

        private TcpClient _client;

        private NetworkStream _stream;

        private byte[] _read_buffer;

        private byte[] _write_buffer;

        private int _buffer_size;

        private byte[] _read_message;

        private byte[] _write_message;

        private bool _closed;

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
        /// Initializes a new instance of TCPServerClient with the specified parameters.
        /// </summary>
        /// <param name="client">The TCP client connection.</param>
        /// <param name="on_request">Callback for request messages.</param>
        /// <param name="on_stop">Callback for client disconnection.</param>
        /// <param name="buffer_size">The buffer size for network operations.</param>
        public TCPServerClient(TcpClient client, Action<IdentifiedMessage> on_request, Action<string> on_stop, int buffer_size = 4096)
    {
        _id = Guid.NewGuid().ToString();

        _client = client;

        _stream = _client.GetStream();

        _buffer_size = buffer_size;

        _read_buffer = new byte[buffer_size];

        _write_buffer = new byte[buffer_size];

        _read_message = new byte[0];

        _write_message = new byte[0];

        IsClosed = false;

        OnRequest += on_request;

        OnStop += on_stop;



        try
        {
            DebugConsole.Log("[Michitai.Lan][START-READ]");

            _stream.BeginRead(_read_buffer, 0, _buffer_size, BeginReadCallback, null);
        }
        catch
        {
            Stop();
            return;
        }
    }



        /// <summary>
        /// Stops the client and cleans up resources.
        /// </summary>
        public void Stop()
    {
        if (IsClosed)
            return;

        IsClosed = true;

        OnRequest = null;

        Socket socket = _client.Client;

        socket.Disconnect(false);

        socket.Close();

        socket.Dispose();

        _stream?.Close();

        _stream?.Dispose();

        _client.Close();

        _client.Dispose();


        OnStop?.Invoke(ID);
    }



        /// <summary>
        /// Sends a response message to the client.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void Response(Message message)
    {
        _write_message = Encoding.UTF8.GetBytes($"{message.GetMessage}#end#<>#message#");

        int count = TransferWriteMessageBytes();

        try
        {
            DebugConsole.Log("[Michitai.Lan][START-WRITE]");

            _stream?.BeginWrite(_write_buffer, 0, count, BeginWriteCallback, null);
        }
        catch
        {
            Stop();
            return;
        }
    }



        /// <summary>
        /// Transfers bytes from the write message to the write buffer.
        /// </summary>
        /// <returns>The number of bytes transferred.</returns>
        private int TransferWriteMessageBytes()
    {
        int count = 0;


        for (int i = 0; i < _buffer_size && i < _write_message.Length; i++)
        {
            _write_buffer[i] = _write_message[i];

            count++;
        }

        byte[] write_message = _write_message;

        _write_message = new byte[Math.Max(write_message.Length - _buffer_size, 0)];

        for (int i = _buffer_size; i < write_message.Length; i++)
        {
            _write_message[i - _buffer_size] = write_message[i];
        }


        return count;
    }



        /// <summary>
        /// Callback for asynchronous read operations.
        /// </summary>
        /// <param name="result">The asynchronous result.</param>
        private void BeginReadCallback(IAsyncResult result)
    {
        int count = -1;

        try
        {
            count = _stream.EndRead(result);
        }
        catch
        {
            Stop();
            return;
        }

        DebugConsole.Log($"[Michitai.Lan][READ-BYTES][{count}]");

        if (count > 0)
        {
            try
            {
                byte[] message = _read_message;

                _read_message = new byte[message.Length + count];

                for (int i = 0; i < message.Length; i++)
                {
                    _read_message[i] = message[i];
                }

                for (int i = 0; i < count; i++)
                {
                    _read_message[message.Length + i] = _read_buffer[i];
                }



                string msg = Encoding.UTF8.GetString(_read_message);

                if (msg.Contains("#end#<>#message#"))
                {
                    DebugConsole.Log("[Michitai.Lan][END-READ]");

                    OnRequest?.Invoke(new IdentifiedMessage(new Message(msg.Substring(0, msg.Length - 16)), _id));

                    _read_message = new byte[0];

                    _read_buffer = new byte[_buffer_size];

                    try
                    {
                        DebugConsole.Log("[Michitai.Lan][START-READ]");

                        _stream?.BeginRead(_read_buffer, 0, _buffer_size, BeginReadCallback, null);
                    }
                    catch
                    {
                        Stop();
                        return;
                    }

                    return;
                }
            }
            catch
            {
                DebugConsole.LogError("[TCP-Server-Client][Error]");
            }



            try
            {
                DebugConsole.Log("[Michitai.Lan][CONTINUE-READ]");

                _stream.BeginRead(_read_buffer, 0, _buffer_size, BeginReadCallback, null);
            }
            catch
            {
                Stop();
                return;
            }
        }
        else
        {
            Stop();
        }
    }

        /// <summary>
        /// Callback for asynchronous write operations.
        /// </summary>
        /// <param name="result">The asynchronous result.</param>
        private void BeginWriteCallback(IAsyncResult result)
    {
        try
        {
            _stream.EndWrite(result);
        }
        catch
        {
            Stop();
            return;
        }

        if (_write_message.Length > 0)
        {
            int count = TransferWriteMessageBytes();

            try
            {
                DebugConsole.Log("[Michitai.Lan][CONTINUE-WRITE]");

                _stream.BeginWrite(_write_buffer, 0, count, BeginWriteCallback, null);
            }
            catch
            {
                Stop();
                return;
            }
        }
        else
        {
            _write_buffer = new byte[_buffer_size];

            DebugConsole.Log("[Michitai.Lan][END-WRITE]");
        }
    }
}
}
