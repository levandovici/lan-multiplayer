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
            public sealed class TCPClient
{
    private IPEndPoint _ip_end_point;

    private TcpClient _client;

    private NetworkStream _stream;

    private byte[] _read_buffer;

    private byte[] _write_buffer;

    private int _buffer_size;

    private byte[] _read_message;

    private byte[] _write_message;

    private bool _closed;



    /// <summary>
    /// 
    /// </summary>
    public event Action<Message> OnResponse;

    /// <summary>
    /// 
    /// </summary>
    public event Action OnStop;



    /// <summary>
    /// 
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
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="buffer_size"></param>
    public TCPClient(IPAddress ip, int port, int buffer_size = 4096) :
        this(new IPEndPoint(ip, port), buffer_size)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip_end_point"></param>
    /// <param name="buffer_size"></param>
    public TCPClient(IPEndPoint ip_end_point, int buffer_size = 4096)
    {
        _ip_end_point = ip_end_point;

        _buffer_size = buffer_size;

        Initialize();
    }



    /// <summary>
    /// 
    /// </summary>
    private void Initialize()
    {
        _client = new TcpClient();

        _read_buffer = new byte[_buffer_size];

        _write_buffer = new byte[_buffer_size];

        _read_message = new byte[0];

        _write_message = new byte[0];

        IsClosed = false;
    }



    /// <summary>
    /// 
    /// </summary>
    public void Start()
    {
        _client.Connect(_ip_end_point);

        _stream = _client.GetStream();

        try
        {
            DebugConsole.Log("[Michitai.Lan][START-READ]");
            
            _stream.BeginRead(_read_buffer, 0, _buffer_size, BeginReadCallback, null);
        }
        catch
        {
            DebugConsole.Log("[Michitai.Lan][CLIENT-START-READ-ERROR]");

            Stop();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Stop()
    {
        if (IsClosed)
            return;

        IsClosed = true;

        OnResponse = null;

        if (_client.Connected)
        {
            try
            {
                Socket socket = _client.Client;

                socket.Disconnect(false);

                socket.Close();

                socket.Dispose();
            }
            catch
            {

            }
        }

        if (_stream != null)
        {
            _stream.Close();

            _stream.Dispose();
        }

        _client.Close();

        _client.Dispose();


        OnStop?.Invoke();
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public void Request(Message message)
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
            DebugConsole.Log("[Michitai.Lan][CLIENT-START-WRITE-ERROR]");

            Stop();
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="result"></param>
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

                OnResponse?.Invoke(new Message(msg.Substring(0, msg.Length - 16)));

                _read_message = new byte[0];

                _read_buffer = new byte[_buffer_size];

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

                return;
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
    /// 
    /// </summary>
    /// <param name="result"></param>
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
