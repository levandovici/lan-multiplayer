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
            public sealed class UDPBroadcast
{
    private UdpClient _socket;



    /// <summary>
    /// 
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
    /// 
    /// </summary>
    /// <param name="point"></param>
    public UDPBroadcast(IPEndPoint point)
    {
        _socket = new UdpClient(point);

        _socket.EnableBroadcast = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public UDPBroadcast(IPAddress ip, int port) : this(new IPEndPoint(ip, port))
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="range"></param>
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



    public void Stop()
    {
        Socket.Close();

        Socket.Dispose();
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="point"></param>
    /// <param name="message"></param>
    /// <exception cref="InvalidDataException"></exception>
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
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="message"></param>
    public void Send(IPAddress ip, int port, AppMessage message)
    {
        Send(new IPEndPoint(ip, port), message);
    }



    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="timeoutMilliseconds"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="point"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
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
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendAsync(IPAddress ip, int port, AppMessage message)
    {
        await SendAsync(new IPEndPoint(ip, port), message);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="range"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendAsync(IPAddress ip, PortRange range, AppMessage message)
    {
        for (int port = range.First; port <= range.Last; port++)
        {
            await SendAsync(ip, port, message);
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="timeoutMilliseconds"></param>
    /// <returns></returns>
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
