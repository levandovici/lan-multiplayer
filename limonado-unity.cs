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

using LimonadoEntertainment;
using LimonadoEntertainment.Data;
using LimonadoEntertainment.Net;
using LimonadoEntertainment.Net.Multiplayer;
using LimonadoEntertainment.Net.Multiplayer.Chat;
using LimonadoEntertainment.Net.Multiplayer.Commands;
using LimonadoEntertainment.Net.Multiplayer.Data;
using LimonadoEntertainment.Debug;

using UnityEngine;



namespace LimonadoEntertainment
{
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------
    //  Project :           Limonado Entertainment Library
    //  Author  :           NikitaLnc
    //  GitLab  :           03.11.2021 (first commit)
    //  Date    :           23.08.2022
    //  Email   :           nikitalnc.mailbox@gmail.com
    //  Twitter :           nikitalnc
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------



    namespace Debug
    {
        /// <summary>
        /// Library debugging
        /// </summary>
        public static class DebugConsole
        {
            public delegate void LogActionDelegate(string log);



            private static bool _debug = false;



            public static event LogActionDelegate OnLog;

            public static event LogActionDelegate OnLogWarning;

            public static event LogActionDelegate OnLogError;



            /// <summary>
            /// Enable or Disable DebugConsole, by Default = False
            /// </summary>
            public static bool Enabled
            {
                get
                {
                    return _debug;
                }

                set
                {
                    _debug = value;
                }
            }



            public static void ClearEvents()
            {
                OnLog = null;

                OnLogWarning = null;

                OnLogError = null;
            }



            internal static void Log(string message)
            {
                if (!Enabled)
                    return;

                OnLog?.Invoke(message);
            }

            internal static void LogWarning(string message)
            {
                if (!Enabled)
                    return;

                OnLogWarning?.Invoke(message);
            }

            internal static void LogError(string message)
            {
                if (!Enabled)
                    return;

                OnLogError?.Invoke(message);
            }
        }
    }

    namespace Data
    {
        /// <summary>
        /// Player World Data
        /// </summary>
        [Serializable]
        public sealed class JsonStorage : IJsonStorage
        {
            public string json;

            private object _json_lock;



            public string Json
            {
                get
                {
                    lock (_json_lock)
                    {
                        return json;
                    }
                }

                set
                {
                    lock (_json_lock)
                    {
                        json = value;
                    }
                }
            }



            public JsonStorage()
            {
                json = "";

                _json_lock = new object();
            }

            public JsonStorage(string json)
            {
                this.json = json;

                _json_lock = new object();
            }



            public T Get<T>()
            {
                return JsonUtility.FromJson<T>(Json);
            }

            public void Set<T>(T @object)
            {
                Json = JsonUtility.ToJson(@object);
            }
        }



        public interface IJsonStorage
        {
            string Json { get; set; }



            T Get<T>();

            void Set<T>(T @object);
        }
    }

    namespace Net
    {
        /// <summary>
        /// 
        /// </summary>
        public static class Lan
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="platform"></param>
            /// <returns>IPAddress Array of all local NetworkInterfaces</returns>
            public static IPAddress[] LocalIPv4Addresses(EPlatform platform)
            {
                var ip_addresses = new List<IPAddress>();

                int count = 0;


                if ((platform & EPlatform.Windows) == EPlatform.Windows ||
                    (platform & EPlatform.Linux) == EPlatform.Linux ||
                    (platform & EPlatform.MacOS) == EPlatform.MacOS)
                {
                    var ni = NetworkInterface.GetAllNetworkInterfaces();

                    foreach (NetworkInterface item in ni)
                    {
                        if (item.OperationalStatus == OperationalStatus.Up)
                        {
                            foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                            {
                                if (ip.Address.AddressFamily == AddressFamily.InterNetwork & !IPAddress.IsLoopback(ip.Address))
                                {
                                    ip_addresses.Add(ip.Address);

                                    count++;
                                }
                            }
                        }
                    }
                }
                else if ((platform & EPlatform.Android) == EPlatform.Android ||
                         (platform & EPlatform.IOS) == EPlatform.IOS)
                {
                    var host = Dns.GetHostEntry(Dns.GetHostName());

                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                        {
                            ip_addresses.Add(ip);

                            count++;
                        }
                    }
                }


                if (count > 0)
                {
                    return ip_addresses.ToArray();
                }
                else
                {
                    return new IPAddress[0];
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="platform"></param>
            /// <returns>IPAddress Masks Array of all local NetworkInterfaces. Example 192.168.0.255</returns>
            public static IPAddress[] LocalIPv4Masks(EPlatform platform)
            {
                var ips = LocalIPv4Addresses(platform);

                return LocalIPv4Masks(ips);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="iPv4Addresses">IPAddresses Array. Example 192.168.0.1</param>
            /// <returns>IPAddresses Masks Array. Example 192.168.0.255</returns>
            public static IPAddress[] LocalIPv4Masks(IPAddress[] iPv4Addresses)
            {
                return iPv4Addresses.Select(ip =>
                {
                    var bytes = ip.GetAddressBytes();

                    bytes[bytes.Length - 1] = 255;

                    return new IPAddress(bytes);

                }).ToArray();
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="platform"></param>
            /// <param name="ipAddresses"></param>
            /// <returns></returns>
            public static bool TryGetLocalIPv4Addresses(EPlatform platform, out IPAddress[] ipAddresses)
            {
                ipAddresses = LocalIPv4Addresses(platform);

                return ipAddresses.Length > 0;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="platform"></param>
            /// <param name="masks"></param>
            /// <returns></returns>
            public static bool TryGetLocalIPv4Masks(EPlatform platform, out IPAddress[] masks)
            {
                masks = LocalIPv4Masks(platform);

                return masks.Length > 0;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="platform"></param>
            /// <param name="masks"></param>
            /// <returns></returns>
            public static bool TryGetLocalIPv4Masks(EPlatform platform, out string[] masks)
            {
                masks = LocalIPv4Masks(platform).Select(ip => ip.ToString()).ToArray();

                return masks.Length > 0;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public struct PortRange
        {
            /// <summary>
            /// 
            /// </summary>
            public static PortRange System => new PortRange(0, 1023);


            /// <summary>
            /// 
            /// </summary>
            public static PortRange Registered => new PortRange(1024, 49151);

            /// <summary>
            /// 
            /// </summary>
            public static PortRange Dynamic => new PortRange(49152, 65535);


            /// <summary>
            /// 
            /// </summary>
            public static PortRange All => new PortRange(Min, Max);


            /// <summary>
            /// 1024 ports - inclusive between 64512 - 65535
            /// </summary>
            public static PortRange Broadcast => new PortRange(64512, 65535);

            /// <summary>
            /// 128 ports - inclusive between 65408 - 65535
            /// </summary>
            public static PortRange BroadcastSimplified => new PortRange(65408, 65535);



            /// <summary>
            ///
            /// </summary>
            public const int Min = 1024;

            /// <summary>
            /// 
            /// </summary>
            public const int Max = 65535;



            /// <summary>
            /// 
            /// </summary>
            public int First;

            /// <summary>
            /// 
            /// </summary>
            public int Last;



            /// <summary>
            /// 
            /// </summary>
            public int Count => Last - First + 1;

            /// <summary>
            /// 
            /// </summary>
            public Store RangeStore => new Store(this);



            /// <summary>
            /// </summary>
            /// <param name="first"></param>
            /// <param name="last"></param>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public PortRange(int first, int last)
            {
                if (first < Min)
                    throw new ArgumentOutOfRangeException("First can't be less than ServerPortRange.Min");

                if (last > Max)
                    throw new ArgumentOutOfRangeException("Last can't be more than ServerPortRange.Max");


                First = first;

                Last = last;
            }



            public class Store
            {
                private System.Random _random;

                private List<int> _ports;

                private int _count;



                /// <summary>
                /// 
                /// </summary>
                public int RandomPort
                {
                    get
                    {
                        if (_count > 0)
                        {
                            int id = _random.Next(0, _count);


                            int item = _ports[id];

                            _ports.Remove(item);

                            _count--;


                            return item;
                        }
                        else
                        {
                            throw new IndexOutOfRangeException("ServerPortRange.Store is Empty! All ports are in use!");
                        }
                    }
                }



                public Store(PortRange range)
                {
                    _random = new System.Random();

                    _ports = new List<int>();

                    for (int port = range.First; port <= range.Last; port++)
                    {
                        _ports.Add(port);

                        _count++;
                    }
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public sealed class TCPServer
        {
            private TcpListener _listner;

            private IPEndPoint _IPEndPoint;

            private int _buffer_size;

            private TCPServerClient[] _clients;

            private bool _closed;


            private readonly object _clients_lock;



            /// <summary>
            /// 
            /// </summary>
            public event Action<string> OnClientConnected;

            /// <summary>
            /// 
            /// </summary>
            public event Action<string> OnClientDisconnected;

            /// <summary>
            /// 
            /// </summary>
            public event Action<IdentifiedMessage> OnRequest;

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
            public IPEndPoint IpEndPoint => _IPEndPoint;



            /// <summary>
            /// 
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            /// <param name="buffer_size"></param>
            public TCPServer(IPAddress ip, int port, int buffer_size = 4096) :
                this(new IPEndPoint(ip, port), buffer_size)
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="ip_end_point"></param>
            /// <param name="buffer_size"></param>
            public TCPServer(IPEndPoint ip_end_point, int buffer_size = 4096)
            {
                _IPEndPoint = ip_end_point;

                _buffer_size = buffer_size;

                _listner = new TcpListener(_IPEndPoint);

                _clients = new TCPServerClient[0];

                IsClosed = false;


                _clients_lock = new object();
            }



            /// <summary>
            /// 
            /// </summary>
            public void Start()
            {
                _listner.Start();

                BeginAccept();
            }

            /// <summary>
            /// 
            /// </summary>
            public void Stop()
            {
                if (IsClosed)
                    return;

                IsClosed = true;

                _listner.Stop();

                Socket socket = _listner.Server;

                socket.Close();

                socket.Dispose();

                lock (_clients_lock)
                {
                    for (int i = 0; i < _clients.Length; i++)
                    {
                        _clients[i].Stop();
                    }
                }

                OnStop?.Invoke();
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="identifiedMessage"></param>
            public void Response(IdentifiedMessage identifiedMessage)
            {
                lock (_clients_lock)
                {
                    for (int i = 0; i < _clients.Length; i++)
                    {
                        if (_clients[i].ID == identifiedMessage.ID)
                        {
                            _clients[i].Response(identifiedMessage.Message);

                            return;
                        }
                    }
                }
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            public void Disconnect(string id)
            {
                lock (_clients_lock)
                {
                    for (int i = 0; i < _clients.Length; i++)
                    {
                        if (_clients[i].ID == id)
                        {
                            _clients[i].Stop();
                            break;
                        }
                    }
                }

                DeleteClient(id);
            }



            /// <summary>
            /// 
            /// </summary>
            private void BeginAccept()
            {
                try
                {
                    _listner.BeginAcceptTcpClient(BeginAcceptTcpClientCallback, null);
                }
                catch
                {
                    DebugConsole.LogError("[TCP-Server] Begin Accept Error!");
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="result"></param>
            private void BeginAcceptTcpClientCallback(IAsyncResult result)
            {
                TcpClient client = null;

                try
                {
                    client = _listner.EndAcceptTcpClient(result);

                    AddClient(new TCPServerClient(client, (im) => OnRequest?.Invoke(im), (id) => Disconnect(id), _buffer_size));
                }
                catch
                {
                    DebugConsole.LogError("[TCP-Server] End Accept Error!");
                }

                BeginAccept();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="server_client"></param>
            private void AddClient(TCPServerClient server_client)
            {
                lock (_clients_lock)
                {
                    TCPServerClient[] clients = _clients;

                    _clients = new TCPServerClient[clients.Length + 1];


                    for (int i = 0; i < clients.Length; i++)
                    {
                        _clients[i] = clients[i];
                    }

                    _clients[clients.Length] = server_client;


                    OnClientConnected?.Invoke(server_client.ID);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            private void DeleteClient(string id)
            {
                lock (_clients_lock)
                {
                    int index = -1;

                    for (int i = 0; i < _clients.Length; i++)
                    {
                        if (_clients[i].ID == id)
                        {
                            index = i;

                            break;
                        }
                    }

                    if (index != -1)
                    {
                        TCPServerClient[] clients = _clients;

                        _clients = new TCPServerClient[clients.Length - 1];

                        for (int i = 0; i < index; i++)
                        {
                            _clients[i] = clients[i];
                        }

                        for (int i = index; i < _clients.Length; i++)
                        {
                            _clients[i] = clients[i + 1];
                        }

                        OnClientDisconnected?.Invoke(id);
                    }
                }
            }
        }

        /// <summary>
        /// 
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
            /// 
            /// </summary>
            private event Action<IdentifiedMessage> OnRequest;

            /// <summary>
            /// 
            /// </summary>
            private event Action<string> OnStop;



            /// <summary>
            /// 
            /// </summary>
            public string ID => _id;

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
            /// <param name="client"></param>
            /// <param name="on_request"></param>
            /// <param name="on_stop"></param>
            /// <param name="buffer_size"></param>
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
                    DebugConsole.Log("[LimonadoEntertainmnetLibrary][START-READ]");

                    _stream.BeginRead(_read_buffer, 0, _buffer_size, BeginReadCallback, null);
                }
                catch
                {
                    Stop();
                    return;
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
            /// 
            /// </summary>
            /// <param name="message"></param>
            public void Response(Message message)
            {
                _write_message = Encoding.UTF8.GetBytes($"{message.GetMessage}#end#<>#message#");

                int count = TransferWriteMessageBytes();

                try
                {
                    DebugConsole.Log("[LimonadoEntertainmnetLibrary][START-WRITE]");

                    _stream?.BeginWrite(_write_buffer, 0, count, BeginWriteCallback, null);
                }
                catch
                {
                    Stop();
                    return;
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

                DebugConsole.Log($"[LimonadoEntertainmentLibrary][READ-BYTES][{count}]");

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
                            DebugConsole.Log("[LimonadoEntertainmnetLibrary][END-READ]");

                            OnRequest?.Invoke(new IdentifiedMessage(new Message(msg.Substring(0, msg.Length - 16)), _id));

                            _read_message = new byte[0];

                            _read_buffer = new byte[_buffer_size];

                            try
                            {
                                DebugConsole.Log("[LimonadoEntertainmnetLibrary][START-READ]");

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
                        DebugConsole.Log("[LimonadoEntertainmnetLibrary][CONTINUE-READ]");

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
                        DebugConsole.Log("[LimonadoEntertainmnetLibrary][CONTINUE-WRITE]");

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

                    DebugConsole.Log("[LimonadoEntertainmnetLibrary][END-WRITE]");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
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
                    DebugConsole.Log("[LimonadoEntertainmnetLibrary][START-READ]");

                    _stream.BeginRead(_read_buffer, 0, _buffer_size, BeginReadCallback, null);
                }
                catch
                {
                    DebugConsole.Log("[LimonadoEntertainmentLibrary][CLIENT-START-READ-ERROR]");

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
                    DebugConsole.Log("[LimonadoEntertainmnetLibrary][START-WRITE]");

                    _stream?.BeginWrite(_write_buffer, 0, count, BeginWriteCallback, null);
                }
                catch
                {
                    DebugConsole.Log("[LimonadoEntertainmentLibrary][CLIENT-START-WRITE-ERROR]");

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

                DebugConsole.Log($"[LimonadoEntertainmentLibrary][READ-BYTES][{count}]");

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
                        DebugConsole.Log("[LimonadoEntertainmnetLibrary][END-READ]");

                        OnResponse?.Invoke(new Message(msg.Substring(0, msg.Length - 16)));

                        _read_message = new byte[0];

                        _read_buffer = new byte[_buffer_size];

                        try
                        {
                            DebugConsole.Log("[LimonadoEntertainmnetLibrary][START-READ]");

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
                        DebugConsole.Log("[LimonadoEntertainmnetLibrary][CONTINUE-READ]");

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
                        DebugConsole.Log("[LimonadoEntertainmnetLibrary][CONTINUE-WRITE]");

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

                    DebugConsole.Log("[LimonadoEntertainmnetLibrary][END-WRITE]");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public sealed class Message
        {
            private string _message;



            /// <summary>
            /// 
            /// </summary>
            public string GetMessage => _message;



            /// <summary>
            /// 
            /// </summary>
            /// <param name="message"></param>
            public Message(string message)
            {
                _message = message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public sealed class IdentifiedMessage
        {
            private string _id;

            private Message _message;



            /// <summary>
            /// 
            /// </summary>
            public string ID => _id;

            /// <summary>
            /// 
            /// </summary>
            public Message Message => _message;



            /// <summary>
            /// 
            /// </summary>
            /// <param name="message"></param>
            /// <param name="id"></param>
            public IdentifiedMessage(Message message, string id)
            {
                _message = message;

                _id = id;
            }
        }



        /// <summary>
        /// 
        /// </summary>
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
                var bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(message));

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
                    var message = JsonUtility.FromJson<AppMessage>(Encoding.UTF8.GetString(bytes));

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
                    var message = JsonUtility.FromJson<AppMessage>(Encoding.UTF8.GetString(result.Buffer));

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



        /// <summary>
        /// 
        /// </summary>
        public sealed class AppMessage
        {
            public int _version;

            public string _name;

            public string _message;



            public int Version
            {
                get
                {
                    return _version;
                }

                set
                {
                    _version = value;
                }
            }

            public string Name
            {
                get
                {
                    return _name;
                }

                set
                {
                    _name = value;
                }
            }

            public string Message
            {
                get
                {
                    return _message;
                }

                set
                {
                    _message = value;
                }
            }



            public AppMessage()
            {

            }

            public AppMessage(int version, string name, string message)
            {
                Version = version;

                Name = name;

                Message = message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public sealed class LocatedMessage
        {
            private IPEndPoint _point;

            private AppMessage _message;



            /// <summary>
            /// 
            /// </summary>
            public IPEndPoint IPEndPoint
            {
                get
                {
                    return _point;
                }

                set
                {
                    _point = value;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public AppMessage Message
            {
                get
                {
                    return _message;
                }

                set
                {
                    _message = value;
                }
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="point"></param>
            /// <param name="message"></param>
            public LocatedMessage(IPEndPoint point, AppMessage message)
            {
                _point = point;

                _message = message;
            }



            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return $"IP End Point: {IPEndPoint}\t App Message: {Message}";
            }
        }
    }

    namespace Net.Multiplayer
    {
        /// <summary>
        /// 
        /// </summary>
        public static class Multiplayer
        {
            /// <summary>
            /// 
            /// </summary>
            public static string Name = "New Multiplayer Game";

            /// <summary>
            /// The IP address to which the server or/and client will connect
            /// </summary>
            public static IPAddress IpAddress = IPAddress.Any;

            /// <summary>
            /// The port to which the client will connect
            /// </summary>
            public static int Port = 50000;


            /// <summary>
            /// Port range. One of them will be used by the server. Default 50000-50128
            /// </summary>
            public static PortRange ServerPortRange = new PortRange(50000, 50128);

            /// <summary>
            /// Port range. Used by Server and Client to Response and Request UDP Messages. Default 60000-60128
            /// </summary>
            public static PortRange BroadcastPortRange = new PortRange(60000, 60128);

            public static int MobileBroadcastServerPort = 49001;

            public static int MobileBroadcastClientPort = 49002;


            /// <summary>
            /// 
            /// </summary>
            public static Server Server = null;

            /// <summary>
            /// 
            /// </summary>
            public static Client Client = null;

            /// <summary>
            /// 
            /// </summary>
            public static BroadcastServer BroadcastServer = null;

            /// <summary>
            /// 
            /// </summary>
            private static Task _BroadcastServerTask = null;

            /// <summary>
            /// 
            /// </summary>
            private static CancellationTokenSource _BroadcastServerTaskTokenSource = null;


            /// <summary>
            /// 
            /// </summary>
            public static BroadcastClient BroadcastClient = null;

            /// <summary>
            /// 
            /// </summary>
            private static Task _BroadcastClientTask = null;

            /// <summary>
            /// 
            /// </summary>
            private static CancellationTokenSource _BroadcastClientTaskTokenSource = null;

            /// <summary>
            /// 
            /// </summary>
            private static MobileBroadcastClient _MobileBroadcastClient = null;

            /// <summary>
            /// 
            /// </summary>
            private static MobileBroadcastServer _MobileBroadcastServer = null;


            /// <summary>
            /// 
            /// </summary>
            public static Queue<Command> ClientCommands = new Queue<Command>();

            /// <summary>
            /// Number of requests that can be sent at the same time.
            /// </summary>
            public static int ClientOnceMaxCommands = 4;



            /// <summary>
            /// 
            /// </summary>
            public static bool IsServer => Server != null;

            /// <summary>
            /// 
            /// </summary>
            public static bool IsClient => Client != null;



            /// <summary>
            /// 
            /// </summary>
            public static event Action OnStartServer;

            /// <summary>
            /// 
            /// </summary>
            public static event Action OnStartClient;

            /// <summary>
            /// 
            /// </summary>
            public static event Action OnClientStarted;

            /// <summary>
            /// 
            /// </summary>
            public static event Action OnServerStarted;



            /// <summary>
            /// 
            /// </summary>
            /// <param name="serverGameData"></param>
            public static void StartServer(EPlatform platform, ServerGameData serverGameData, BroadcastServer.ProcessMessageDelegate processMessage, int receiveRequestsDelayMilliseconds = 500)
            {
                if (IsServer)
                    return;


                Server = new Server(Name, serverGameData, IpAddress, ServerPortRange);

                IpAddress = Server.IPEndPoint.Address;

                Port = Server.IPEndPoint.Port;


                if ((platform & EPlatform.Windows) == EPlatform.Windows || (platform & EPlatform.Linux) == EPlatform.Linux || (platform & EPlatform.MacOS) == EPlatform.MacOS)
                {
                    _BroadcastServerTaskTokenSource = new CancellationTokenSource();

                    var token = _BroadcastServerTaskTokenSource.Token;

                    _BroadcastServerTask = Task.Run(async () =>
                    {
                        token.ThrowIfCancellationRequested();

                        BroadcastServer = new BroadcastServer(IpAddress, BroadcastPortRange);

                        while (true)
                        {
                            token.ThrowIfCancellationRequested();

                            await BroadcastServer.BroadcastAsync(processMessage);

                            token.ThrowIfCancellationRequested();

                            await Task.Delay(receiveRequestsDelayMilliseconds, _BroadcastServerTaskTokenSource.Token);
                        }

                    }, _BroadcastServerTaskTokenSource.Token);
                }
                else if ((platform & EPlatform.Android) == EPlatform.Android || (platform & EPlatform.IOS) == EPlatform.IOS)
                {
                    _MobileBroadcastServer = new MobileBroadcastServer();



                    _MobileBroadcastServer.Start(MobileBroadcastClientPort, MobileBroadcastServerPort, receiveRequestsDelayMilliseconds, (ip, msg) =>
                    {
                        try
                        {
                            DebugConsole.LogWarning($"{ip} - {msg.Length}");

                            AppMessage appMessage = JsonUtility.FromJson<AppMessage>(Encoding.UTF8.GetString(msg));

                            LocatedMessage locatedMessage = new LocatedMessage(ip, appMessage);


                            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(processMessage.Invoke(locatedMessage)));
                        }
                        catch
                        {
                            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(processMessage(new LocatedMessage(ip, null))));
                        }
                    });
                }
                else
                {
                    throw new NotImplementedException();
                }


                OnStartServer?.Invoke();

                Server.Start();

                OnServerStarted?.Invoke();
            }

            /// <summary>
            /// 
            /// </summary>
            public static void StartClient()
            {
                if (IsClient)
                    return;


                Client = new Client(IpAddress, Port);


                ClientCommands = new Queue<Command>();


                OnStartClient?.Invoke();

                Client.Start();

                OnClientStarted?.Invoke();
            }


            /// <summary>
            /// 
            /// </summary>
            public static void StopServer()
            {
                DebugConsole.LogWarning("[MULTIPLAYER] Cancelling BroadcastServerTask...");

                _BroadcastServerTaskTokenSource?.Cancel();

                _BroadcastClientTaskTokenSource = null;

                _BroadcastServerTask = null;

                DebugConsole.Log("[MULTIPLAYER] BroadcastServerTask canceled.");


                DebugConsole.LogWarning("[MULTIPLAYER] Stopping Server...");

                Server?.Stop();

                Server = null;

                DebugConsole.Log("[MULTIPLAYER] Server stopped.");


                DebugConsole.LogWarning("[MULTIPLAYER] Stopping BroadcastServer...");

                BroadcastServer?.Stop();

                BroadcastServer = null;

                DebugConsole.Log("[MULTIPLAYER] BroadcastServer stopped.");


                _MobileBroadcastServer?.Stop();

                _MobileBroadcastServer = null;
            }

            /// <summary>
            /// 
            /// </summary>
            public static void StopClient()
            {
                Client?.Stop();

                Client = null;
            }

            /// <summary>
            /// 
            /// </summary>
            public static void Stop()
            {
                StopClient();

                StopServer();
            }

            /// <summary>
            /// 
            /// </summary>
            public static void ClearEvents()
            {
                OnStartServer = null;

                OnServerStarted = null;

                OnStartClient = null;

                OnClientStarted = null;
            }



            /// <summary>
            /// 
            /// </summary>
            public static void StartBroadcastClient(EPlatform platform, AppMessage request, BroadcastClient.OnReceiveResponseDelegate onReceiveResponse, int receiveResponsesMilliseconds = 5000, int repeatAfterMilliseconds = 5000)
            {
                if (_BroadcastClientTask != null)
                    return;

                if ((platform & EPlatform.Windows) == EPlatform.Windows || (platform & EPlatform.Linux) == EPlatform.Linux || (platform & EPlatform.MacOS) == EPlatform.MacOS)
                {
                    _BroadcastClientTaskTokenSource = new CancellationTokenSource();

                    var token = _BroadcastClientTaskTokenSource.Token;

                    _BroadcastClientTask = Task.Run(async () =>
                    {
                        token.ThrowIfCancellationRequested();

                        BroadcastClient = new BroadcastClient(IpAddress, BroadcastPortRange);

                        while (true)
                        {
                            token.ThrowIfCancellationRequested();

                            IPAddress[] masks = null;

                            bool success = Lan.TryGetLocalIPv4Masks(platform, out masks);

                            if (success)
                            {
                                await BroadcastClient.BroadcastRequestAsync(masks, BroadcastPortRange, request, onReceiveResponse, receiveResponsesMilliseconds);
                            }
                            
                            await BroadcastClient.BroadcastRequestAsync(IPAddress.Broadcast, BroadcastPortRange, request, onReceiveResponse, receiveResponsesMilliseconds);

                            token.ThrowIfCancellationRequested();

                            await Task.Delay(repeatAfterMilliseconds, _BroadcastClientTaskTokenSource.Token);
                        }

                    }, _BroadcastClientTaskTokenSource.Token);
                }
                else if ((platform & EPlatform.Android) == EPlatform.Android || (platform & EPlatform.IOS) == EPlatform.IOS)
                {
                    _MobileBroadcastClient = new MobileBroadcastClient();

                    _MobileBroadcastClient.Start(MobileBroadcastClientPort, 
                        MobileBroadcastServerPort,Encoding.UTF8.GetBytes(JsonUtility.ToJson(request)), repeatAfterMilliseconds, 500, 
                        (ip, msg) =>
                    {
                        try
                        {
                            DebugConsole.LogWarning($"{ip} - {Encoding.UTF8.GetString(msg)}");

                            AppMessage appMessage = JsonUtility.FromJson<AppMessage>(Encoding.UTF8.GetString(msg));

                            LocatedMessage locatedMessage = new LocatedMessage(ip, appMessage);

                            onReceiveResponse.Invoke(locatedMessage);
                        }
                        catch
                        {
                            onReceiveResponse.Invoke(new LocatedMessage(ip, null));
                        }
                    });
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public static void StopBroadcastClient()
            {
                _BroadcastClientTaskTokenSource?.Cancel();

                _BroadcastClientTaskTokenSource = null;

                _BroadcastClientTask = null;


                BroadcastClient?.Stop();

                BroadcastClient = null;


                _MobileBroadcastClient?.Stop();

                _MobileBroadcastClient = null;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public sealed class Server
        {
            private string _name;

            private TCPServer _server;

            private ServerClients _clients;

            private ServerGameData _server_data;



            /// <summary>
            /// 
            /// </summary>
            public event Action<string> OnClientConnected;

            /// <summary>
            /// 
            /// </summary>
            public event Action<string> OnClientDisconnected;

            /// <summary>
            /// 
            /// </summary>
            public event Action<IdentifiedMessage> OnRequest;

            /// <summary>
            /// 
            /// </summary>
            public event Action OnDisconnected;



            /// <summary>
            /// 
            /// </summary>
            public string Name => _name;

            /// <summary>
            /// 
            /// </summary>
            public ServerGameData PublicServerData => _server_data.Public;

            /// <summary>
            /// 
            /// </summary>
            public ServerGameData PrivateServerData => _server_data;

            /// <summary>
            /// 
            /// </summary>
            public IPEndPoint IPEndPoint => _server.IpEndPoint;



            /// <summary>
            /// 
            /// </summary>
            /// <param name="name"></param>
            /// <param name="serverGameData"></param>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            /// <param name="bufferSize"></param>
            public Server(string name, ServerGameData serverGameData, IPAddress ip, int port, int bufferSize = 4096) : this(name, serverGameData, new IPEndPoint(ip, port), bufferSize)
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="name"></param>
            /// <param name="serverGameData"></param>
            /// <param name="ipEndPoint"></param>
            /// <param name="bufferSize"></param>
            public Server(string name, ServerGameData serverGameData, IPEndPoint ipEndPoint, int bufferSize = 4096)
            {
                _name = name;

                _server_data = serverGameData;

                _clients = new ServerClients();

                _server = new TCPServer(ipEndPoint, bufferSize);

                _server.OnClientConnected += (id) => OnClientConnected?.Invoke(id);

                _server.OnClientDisconnected += (id) =>
                {
                    Disconnect(id);

                    OnClientDisconnected?.Invoke(id);
                };

                _server.OnRequest += (im) => OnRequest?.Invoke(im);

                _server.OnStop += () => OnDisconnected?.Invoke();
            }

            public Server(string name, ServerGameData serverGameData, IPAddress ip, PortRange range, int bufferSize = 4096)
            {
                _name = name;

                _server_data = serverGameData;

                _clients = new ServerClients();


                PortRange.Store store = range.RangeStore;

                while (true)
                {
                    try
                    {
                        _server = new TCPServer(ip, store.RandomPort, bufferSize);

                        break;
                    }
                    catch (SocketException e)
                    {
                        DebugConsole.LogError(e.Message);
                    }
                }


                _server.OnClientConnected += (id) => OnClientConnected?.Invoke(id);

                _server.OnClientDisconnected += (id) =>
                {
                    Disconnect(id);

                    OnClientDisconnected?.Invoke(id);
                };

                _server.OnRequest += (im) => OnRequest?.Invoke(im);

                _server.OnStop += () => OnDisconnected?.Invoke();
            }



            /// <summary>
            /// 
            /// </summary>
            public void Start()
            {
                _server.Start();
            }

            /// <summary>
            /// 
            /// </summary>
            public void Stop()
            {
                _server.Stop();
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="identified_message"></param>
            public void Response(IdentifiedMessage identified_message)
            {
                _server.Response(identified_message);
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            /// <param name="credentials"></param>
            public void LogInPlayer(string id, Credentials credentials)
            {
                _clients.LogIn(new ServerClient(id, credentials));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            public void Disconnect(string id)
            {
                ServerClient client;

                bool exists = _clients.TryGetPlayer(id, out client);

                if (exists)
                {
                    _server_data.DeletePlayer(client.Credentials);
                }

                _clients.LogOut(id);

                _server.Disconnect(id);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            public Credentials RegisterNewPlayer(JsonStorage data)
            {
                Credentials credentials = Credentials.New();

                _server_data.AddPlayer(new ServerClientGameData(data, credentials));

                return credentials;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="player"></param>
            /// <returns></returns>
            public bool Contains(Credentials player)
            {
                return PrivateServerData.Contains(player);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            /// <param name="serverClientGameData"></param>
            /// <returns></returns>
            public bool TryGetLoggedInPlayerPublicData(string id, out ServerClientGameData serverClientGameData)
            {
                ServerClient serverClient;

                bool success = _clients.TryGetPlayer(id, out serverClient);


                if (success)
                {
                    return PrivateServerData.TryGetPublicPlayerData(serverClient.Credentials, out serverClientGameData);
                }


                serverClientGameData = null;

                return false;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            /// <param name="serverClientGameData"></param>
            /// <returns></returns>
            public bool TryGetLoggedInPlayerPrivateData(string id, out ServerClientGameData serverClientGameData)
            {
                ServerClient serverClient;

                bool success = _clients.TryGetPlayer(id, out serverClient);


                if (success)
                {
                    return PrivateServerData.TryGetPrivatePlayerData(serverClient.Credentials, out serverClientGameData);
                }


                serverClientGameData = null;

                return false;
            }



            /// <summary>
            /// 
            /// </summary>
            [Serializable]
            public class ServerClients
            {
                private ServerClient[] _clients;

                private readonly object _clients_lock;



                /// <summary>
                /// 
                /// </summary>
                public ServerClients()
                {
                    _clients = new ServerClient[0];

                    _clients_lock = new object();
                }



                /// <summary>
                /// 
                /// </summary>
                /// <param name="player"></param>
                public void LogIn(ServerClient player)
                {
                    lock (_clients_lock)
                    {
                        ServerClient[] clients = _clients;

                        _clients = new ServerClient[clients.Length + 1];


                        for (int i = 0; i < clients.Length; i++)
                        {
                            _clients[i] = clients[i];
                        }


                        _clients[clients.Length] = player;
                    }
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="id"></param>
                public void LogOut(string id)
                {
                    if (!Contains(id))
                        return;

                    lock (_clients_lock)
                    {
                        ServerClient[] clients = _clients;

                        _clients = new ServerClient[_clients.Length - 1];


                        for (int i = 0, n = 0; i < clients.Length; i++)
                        {
                            if (clients[i].ID != id)
                            {
                                _clients[n++] = clients[i];
                            }
                        }
                    }
                }



                /// <summary>
                /// 
                /// </summary>
                /// <param name="id"></param>
                /// <param name="serverClient"></param>
                /// <returns></returns>
                public bool TryGetPlayer(string id, out ServerClient serverClient)
                {
                    lock (_clients_lock)
                    {
                        for (int i = 0; i < _clients.Length; i++)
                        {
                            if (_clients[i].ID == id)
                            {
                                serverClient = _clients[i];

                                return true;
                            }
                        }
                    }

                    serverClient = null;

                    return false;
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                private bool Contains(string id)
                {
                    lock (_clients_lock)
                    {
                        for (int i = 0; i < _clients.Length; i++)
                        {
                            if (_clients[i].ID == id)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
            }

            [Serializable]
            public class ServerClient
            {
                private string _id;

                private Credentials _credentials;


                private readonly object _id_lock;

                private readonly object _credentials_lock;



                public string ID
                {
                    get
                    {
                        lock (_id_lock)
                        {
                            return _id;
                        }
                    }
                }

                public Credentials Credentials
                {
                    get
                    {
                        lock (_credentials_lock)
                        {
                            return _credentials;
                        }
                    }
                }



                public ServerClient(string id, Credentials credentials)
                {
                    _id = id;

                    _credentials = credentials;


                    _id_lock = new object();

                    _credentials_lock = new object();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public sealed class Client
        {
            private TCPClient _client;

            private ClientGameData _client_data;

            private JsonStorage _game_data;

            private ServerGameData _server_data;

            private bool _is_responsed = true;



            /// <summary>
            /// 
            /// </summary>
            public event Action<Message> OnResponse;

            /// <summary>
            /// 
            /// </summary>
            public event Action OnDisconnected;



            /// <summary>
            /// 
            /// </summary>
            public bool IsClosed => _client.IsClosed;

            /// <summary>
            /// 
            /// </summary>
            public bool IsInitialized
            {
                get
                {
                    return ClientData != null && GameData != null && ServerData != null;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public bool IsResponsed
            {
                get
                {
                    return _is_responsed;
                }

                private set
                {
                    _is_responsed = value;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public bool CanRequest
            {
                get
                {
                    return IsInitialized && IsResponsed;
                }
            }


            /// <summary>
            /// 
            /// </summary>
            public ClientGameData ClientData
            {
                get
                {
                    return _client_data;
                }

                set
                {
                    _client_data = value;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public JsonStorage GameData
            {
                get
                {
                    return _game_data;
                }

                set
                {
                    _game_data = value;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public ServerGameData ServerData
            {
                get
                {
                    return _server_data;
                }

                set
                {
                    _server_data = value;
                }
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="clientGameData"></param>
            /// <param name="ipEndPoint"></param>
            /// <param name="bufferSize"></param>
            public Client(ClientGameData clientGameData, JsonStorage gameData, IPEndPoint ipEndPoint, int bufferSize = 4096)
            {
                _client_data = clientGameData;

                _game_data = gameData;

                _client = new TCPClient(ipEndPoint, bufferSize);

                IsResponsed = true;

                _client.OnResponse += (m) =>
                {
                    IsResponsed = true;

                    OnResponse?.Invoke(m);
                };

                _client.OnStop += () => OnDisconnected?.Invoke();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="clientGameData"></param>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            /// <param name="bufferSize"></param>
            public Client(ClientGameData clientGameData, JsonStorage gameData, IPAddress ip, int port, int bufferSize = 4096) :
                this(clientGameData, gameData, new IPEndPoint(ip, port), bufferSize)
            {
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="clientGameData"></param>
            /// <param name="ipEndPoint"></param>
            /// <param name="bufferSize"></param>
            public Client(ClientGameData clientGameData, IPEndPoint ipEndPoint, int bufferSize = 4096) :
                this(clientGameData, null, ipEndPoint, bufferSize)
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="clientGameData"></param>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            /// <param name="bufferSize"></param>
            public Client(ClientGameData clientGameData, IPAddress ip, int port, int bufferSize = 4096) :
                this(clientGameData, null, new IPEndPoint(ip, port), bufferSize)
            {
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="ipEndPoint"></param>
            /// <param name="bufferSize"></param>
            public Client(IPEndPoint ipEndPoint, int bufferSize = 4096) :
                this(null, null, ipEndPoint, bufferSize)
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            /// <param name="bufferSize"></param>
            public Client(IPAddress ip, int port, int bufferSize = 4096) :
                this(null, null, new IPEndPoint(ip, port), bufferSize)
            {
            }



            /// <summary>
            /// 
            /// </summary>
            public void Start()
            {
                _client.Start();
            }

            /// <summary>
            /// 
            /// </summary>
            public void Stop()
            {
                _client.Stop();
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="message"></param>
            public void Request(Message message)
            {
                if (IsResponsed)
                {
                    IsResponsed = false;

                    _client.Request(message);
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public sealed class BroadcastServer
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="incoming"></param>
            /// <returns></returns>
            public delegate AppMessage ProcessMessageDelegate(LocatedMessage incoming);



            private UDPBroadcast _socket;



            /// <summary>
            /// 
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
            /// 
            /// </summary>
            /// <param name="point"></param>
            public BroadcastServer(IPEndPoint point)
            {
                _socket = new UDPBroadcast(point);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            public BroadcastServer(IPAddress ip, int port)
            {
                _socket = new UDPBroadcast(ip, port);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="range"></param>
            public BroadcastServer(IPAddress ip, PortRange range)
            {
                _socket = new UDPBroadcast(ip, range);
            }



            /// <summary>
            /// 
            /// </summary>
            public void Stop()
            {
                Socket.Stop();
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="process"></param>
            public void Broadcast(ProcessMessageDelegate process)
            {
                LocatedMessage message = Socket.Receive();

                Socket.Send(message.IPEndPoint, process.Invoke(message));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="process"></param>
            /// <param name="timeoutMilliseconds"></param>
            /// <returns></returns>
            public bool Broadcast(ProcessMessageDelegate process, int timeoutMilliseconds)
            {
                Task broadcast = Task.Run(() => Broadcast(process));

                return broadcast.Wait(new TimeSpan(timeoutMilliseconds * 10000));
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="process"></param>
            /// <returns></returns>
            public async Task BroadcastAsync(ProcessMessageDelegate process)
            {
                LocatedMessage incoming = await Socket.ReceiveAsync();

                if (incoming == null)
                    return;

                await Socket.SendAsync(incoming.IPEndPoint, process.Invoke(incoming));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="process"></param>
            /// <param name="timeoutMilliseconds"></param>
            /// <returns></returns>
            public async Task<bool> BroadcastAsync(ProcessMessageDelegate process, int timeoutMilliseconds)
            {
                Task timeout = Task.Run(async () => await Task.Delay(timeoutMilliseconds));

                Task broadcast = Task.Run(async () => await BroadcastAsync(process));


                Task task = await Task.WhenAny(broadcast, timeout);


                return broadcast.IsCompleted;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public sealed class BroadcastClient
        {
            public delegate void OnReceiveResponseDelegate(LocatedMessage response);



            private UDPBroadcast _socket;



            /// <summary>
            /// 
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
            /// 
            /// </summary>
            /// <param name="point"></param>
            public BroadcastClient(IPEndPoint point)
            {
                _socket = new UDPBroadcast(point);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            public BroadcastClient(IPAddress ip, int port)
            {
                _socket = new UDPBroadcast(ip, port);
            }

            public BroadcastClient(IPAddress ip, PortRange range)
            {
                _socket = new UDPBroadcast(ip, range);
            }



            /// <summary>
            /// 
            /// </summary>
            public void Stop()
            {
                Socket.Stop();
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            /// <param name="message"></param>
            /// <param name="receiveTimeoutMilliseconds"></param>
            /// <returns></returns>
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
                for (int port = range.First; port <= range.Last; port++)
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



        /// <summary>
        /// 
        /// </summary>
        public class MobileBroadcastClient
        {
            private UdpClient _client = null;

            private CancellationTokenSource _source = null;

            private Task _sender = null;

            private Task _receiver = null;



            public void Start(int clientPort, int serverPort, byte[] request, int sendDelayMilliseconds, int receiveDelayMilliseconds, Action<IPEndPoint, byte[]> onReceive)
            {
                if (_client != null || _source != null || _sender != null || _receiver != null)
                    return;


                _source = new CancellationTokenSource();

                CancellationToken token = _source.Token;


                _client = new UdpClient(clientPort);

                _client.EnableBroadcast = true;


                _sender = Task.Run(() =>
                {
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();

                        var count = _client.Send(request, request.Length, new IPEndPoint(IPAddress.Broadcast, serverPort));

                        token.ThrowIfCancellationRequested();

                        Task.Delay(sendDelayMilliseconds).Wait();
                    }

                }, token);

                _receiver = Task.Run(() =>
                {
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();

                        var point = new IPEndPoint(IPAddress.Any, serverPort);

                        var bytes = _client.Receive(ref point);

                        token.ThrowIfCancellationRequested();

                        onReceive(point, bytes);

                        Task.Delay(receiveDelayMilliseconds).Wait();
                    }

                }, token);
            }

            public void Stop()
            {
                _source?.Cancel();

                _client?.Close();


                _source = null;

                _sender = null;

                _receiver = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class MobileBroadcastServer
        {
            private UdpClient _server = null;

            private CancellationTokenSource _source = null;

            private Task _task = null;



            public void Start(int clientPort, int serverPort, int delayMilliseconds, Func<IPEndPoint, byte[], byte[]> onReceive)
            {
                if (_server != null || _source != null || _task != null)
                    return;


                _source = new CancellationTokenSource();

                CancellationToken token = _source.Token;


                _server = new UdpClient(serverPort);

                _server.EnableBroadcast = true;
                

                _task = Task.Run(() =>
                {
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();

                        IPEndPoint point = null;

                        byte[] bytes = null;

                        try
                        {
                            point = new IPEndPoint(IPAddress.Any, clientPort);

                            bytes = _server.Receive(ref point);
                        }
                        catch(Exception e)
                        {
                            DebugConsole.LogError(e.Message);
                        }

                        token.ThrowIfCancellationRequested();

                        try
                        {
                            bytes = onReceive(point, bytes);

                            if (bytes != null && bytes.Length > 0)
                            {
                                var count = _server.Send(bytes, bytes.Length, point);
                            }
                        }
                        catch(Exception e)
                        {
                            DebugConsole.LogError(e.Message);
                        }

                        Task.Delay(delayMilliseconds).Wait();
                    }

                }, token);
            }

            public void Stop()
            {
                _source?.Cancel();

                _server?.Close();


                _server = null;

                _source = null;

                _task = null;
            }
        }
    }

    namespace Net.Multiplayer.Data
    {
        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public sealed class MultiplayerGamesData
        {
            public ServerGameData[] Servers;

            public ClientGameData[] Clients;



            public MultiplayerGamesData()
            {
                Servers = new ServerGameData[0];

                Clients = new ClientGameData[0];
            }
        }



        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public sealed class ServerGameData : IJsonStorage
        {
            public string serverID;

            public ServerClientGameData[] clients;

            public string json;


            private readonly object _server_id_lock;

            private readonly object _clients_lock;

            private readonly object _json_lock;



            /// <summary>
            /// 
            /// </summary>
            public string ServerID
            {
                get
                {
                    lock (_server_id_lock)
                    {
                        return serverID;
                    }
                }

                set
                {
                    lock (_server_id_lock)
                    {
                        serverID = value;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public ServerClientGameData[] Clients
            {
                get
                {
                    lock (_clients_lock)
                    {
                        return clients;
                    }
                }

                set
                {
                    lock (_clients_lock)
                    {
                        clients = value;
                    }
                }
            }

            public string Json
            {
                get
                {
                    lock (_json_lock)
                    {
                        return json;
                    }
                }

                set
                {
                    lock (_json_lock)
                    {
                        json = value;
                    }
                }
            }



            /// <summary>
            /// 
            /// </summary>
            public ServerGameData Public
            {
                get
                {
                    ServerClientGameData[] clients;


                    lock (_clients_lock)
                    {
                        clients = new ServerClientGameData[this.clients.Length];

                        for (int i = 0; i < this.clients.Length; i++)
                        {
                            clients[i] = this.clients[i].Public;
                        }
                    }


                    return new ServerGameData(ServerID, clients);
                }
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="server_id"></param>
            /// <param name="clients"></param>
            public ServerGameData(string server_id, ServerClientGameData[] clients)
            {
                serverID = server_id;

                this.clients = clients;

                json = "";


                _server_id_lock = new object();

                _clients_lock = new object();

                _json_lock = new object();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="server_id"></param>
            public ServerGameData(string server_id) : this(server_id, new ServerClientGameData[0])
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public ServerGameData()
            {
                serverID = "default";

                clients = new ServerClientGameData[0];

                json = "";


                _server_id_lock = new object();

                _clients_lock = new object();

                _json_lock = new object();
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="player"></param>
            public void AddPlayer(ServerClientGameData player)
            {
                lock (_clients_lock)
                {
                    ServerClientGameData[] players = new ServerClientGameData[clients.Length + 1];

                    for (int i = 0; i < clients.Length; i++)
                    {
                        players[i] = clients[i];
                    }

                    players[clients.Length] = player;

                    clients = players;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="player"></param>
            public void DeletePlayer(Credentials player)
            {
                lock (_clients_lock)
                {
                    ServerClientGameData[] clients = new ServerClientGameData[this.clients.Length - 1];

                    for (int i = 0, id = 0; i < this.clients.Length; i++)
                    {
                        if (this.clients[i].Credentials != player)
                        {
                            clients[id++] = this.clients[i];
                        }
                    }

                    this.clients = clients;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="player"></param>
            /// <returns></returns>
            public bool Contains(Credentials player)
            {
                lock (_clients_lock)
                {
                    for (int i = 0; i < clients.Length; i++)
                    {
                        if (clients[i].Credentials == player)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="player"></param>
            /// <param name="client"></param>
            /// <returns></returns>
            public bool TryGetPublicPlayerData(Credentials player, out ServerClientGameData client)
            {
                lock (_clients_lock)
                {
                    for (int i = 0; i < clients.Length; i++)
                    {
                        if (clients[i].Credentials == player)
                        {
                            client = clients[i].Public;

                            return true;
                        }
                    }
                }


                client = null;

                return false;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="player"></param>
            /// <param name="client"></param>
            /// <returns></returns>
            public bool TryGetPrivatePlayerData(Credentials player, out ServerClientGameData client)
            {
                lock (_clients_lock)
                {
                    for (int i = 0; i < clients.Length; i++)
                    {
                        if (clients[i].Credentials == player)
                        {
                            client = clients[i];

                            return true;
                        }
                    }
                }


                client = null;

                return false;
            }



            public T Get<T>()
            {
                return JsonUtility.FromJson<T>(Json);
            }

            public void Set<T>(T @object)
            {
                Json = JsonUtility.ToJson(@object);
            }



            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return $"[SERVER-GAME-DATA][SERVER-ID][{serverID}][PLAYERS][{clients.Length}]";
            }
        }


        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public sealed class ServerClientGameData
        {
            public JsonStorage data;

            public Credentials credentials;


            private readonly object _data_lock;

            private readonly object _credentials_lock;



            /// <summary>
            /// 
            /// </summary>
            public JsonStorage Data
            {
                get
                {
                    lock (_data_lock)
                    {
                        return data;
                    }
                }

                set
                {
                    lock (_data_lock)
                    {
                        data = value;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public Credentials Credentials
            {
                get
                {
                    lock (_credentials_lock)
                    {
                        return credentials;
                    }
                }

                set
                {
                    lock (_credentials_lock)
                    {
                        credentials = value;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public ServerClientGameData Public => new ServerClientGameData(Data, Credentials.Public);



            /// <summary>
            /// 
            /// </summary>
            /// <param name="gameData"></param>
            /// <param name="credentials"></param>
            public ServerClientGameData(JsonStorage gameData, Credentials credentials)
            {
                data = gameData;

                this.credentials = credentials;


                _data_lock = new object();

                _credentials_lock = new object();
            }

            /// <summary>
            /// 
            /// </summary>
            public ServerClientGameData()
            {
                credentials = new Credentials("id", "password");


                _data_lock = new object();

                _credentials_lock = new object();
            }



            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return $"[SERVER-CLIENT-GAME-DATA]{credentials}{data?.ToString()}";
            }
        }



        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public sealed class ClientGameData
        {
            public string server_id;

            public Credentials credentials;


            private readonly object _server_id_lock;

            private readonly object _credentials_lock;



            /// <summary>
            /// 
            /// </summary>
            public string Server_ID
            {
                get
                {
                    lock (_server_id_lock)
                    {
                        return server_id;
                    }
                }

                set
                {
                    lock (_server_id_lock)
                    {
                        server_id = value;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public Credentials Credentials
            {
                get
                {
                    lock (_credentials_lock)
                    {
                        return credentials;
                    }
                }

                set
                {
                    lock (_credentials_lock)
                    {
                        credentials = value;
                    }
                }
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="server_id"></param>
            /// <param name="credentials"></param>
            public ClientGameData(string server_id, Credentials credentials)
            {
                this.server_id = server_id;

                this.credentials = credentials;


                _server_id_lock = new object();

                _credentials_lock = new object();
            }

            /// <summary>
            /// 
            /// </summary>
            public ClientGameData()
            {
                server_id = "default";

                credentials = new Credentials("id", "password");


                _server_id_lock = new object();

                _credentials_lock = new object();
            }
        }



        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class ServerInfo
        {
            public int _port;

            public int _clients_count;

            public string _server_id;

            public string _name;


            private readonly object _port_lock;

            private readonly object _clients_count_lock;

            private readonly object _server_id_lock;

            private readonly object _name_lock;



            public string Name
            {
                get
                {
                    lock (_name_lock)
                    {
                        return _name;
                    }
                }

                set
                {
                    lock (_name_lock)
                    {
                        _name = value;
                    }
                }
            }

            public string ServerID
            {
                get
                {
                    lock (_server_id_lock)
                    {
                        return _server_id;
                    }
                }

                set
                {
                    lock (_server_id_lock)
                    {
                        _server_id = value;
                    }
                }
            }

            public int Port
            {
                get
                {
                    lock (_port_lock)
                    {
                        return _port;
                    }
                }

                set
                {
                    lock (_port_lock)
                    {
                        _port = value;
                    }
                }
            }

            public int ClientsCount
            {
                get
                {
                    lock (_clients_count_lock)
                    {
                        return _clients_count;
                    }
                }

                set
                {
                    lock (_clients_count_lock)
                    {
                        _clients_count = value;
                    }
                }
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="port"></param>
            /// <param name="name"></param>
            /// <param name="serverID"></param>
            /// <param name="clientsCount"></param>
            public ServerInfo(int port, string name, string serverID, int clientsCount)
            {
                _port = port;

                _name = name;

                _server_id = serverID;

                _clients_count = clientsCount;


                _port_lock = new object();

                _name_lock = new object();

                _server_id_lock = new object();

                _clients_count_lock = new object();
            }

            /// <summary>
            /// 
            /// </summary>
            public ServerInfo()
            {
                _port = 0;

                _name = "default";

                _server_id = "default";

                _clients_count = -1;


                _port_lock = new object();

                _name_lock = new object();

                _server_id_lock = new object();

                _clients_count_lock = new object();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ServerInfoStack
        {
            public Stack<ServerInfo> _stack;

            public int _count;


            private object _stack_lock;

            private object _count_lock;



            public ServerInfoStack()
            {
                _stack = new Stack<ServerInfo>();

                _count = 0;


                _stack_lock = new object();

                _count_lock = new object();
            }



            public void Push(ServerInfo serverInfo)
            {
                lock (_stack_lock)
                {
                    _stack.Push(serverInfo);

                    lock (_count_lock)
                    {
                        _count++;
                    }
                }
            }

            public ServerInfo Pop()
            {
                lock (_stack_lock)
                {
                    lock (_count_lock)
                    {
                        _count--;
                    }

                    return _stack.Pop();
                }
            }

            public ServerInfo Peek()
            {
                lock (_stack_lock)
                {
                    return _stack.Peek();
                }
            }

            public int Count()
            {
                lock (_count_lock)
                {
                    return _count;
                }
            }

            public void Clear()
            {
                lock (_stack_lock)
                {
                    lock (_count_lock)
                    {
                        _count = 0;
                    }

                    _stack.Clear();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class LocatedServerInfo
        {
            public ServerInfo _server_info;

            public IPEndPoint _point;


            private readonly object _server_info_lock;

            private readonly object _point_lock;



            public ServerInfo ServerInfo
            {
                get
                {
                    lock (_server_info_lock)
                    {
                        return _server_info;
                    }
                }

                set
                {
                    lock (_server_info_lock)
                    {
                        _server_info = value;
                    }
                }
            }

            public IPEndPoint IPEndPoint
            {
                get
                {
                    lock (_point_lock)
                    {
                        return _point;
                    }
                }

                set
                {
                    lock (_point_lock)
                    {
                        _point = value;
                    }
                }
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="serverInfo"></param>
            /// <param name="point"></param>
            public LocatedServerInfo(ServerInfo serverInfo, IPEndPoint point)
            {
                _server_info = serverInfo;

                _point = point;


                _server_info_lock = new object();

                _point_lock = new object();
            }

            /// <summary>
            /// 
            /// </summary>
            public LocatedServerInfo()
            {
                _server_info = null;

                _point = null;


                _server_info_lock = new object();

                _point_lock = new object();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class LocatedServerInfoStack
        {
            public Stack<LocatedServerInfo> _stack;

            public int _count;


            private object _stack_lock;

            private object _count_lock;



            public LocatedServerInfoStack()
            {
                _stack = new Stack<LocatedServerInfo>();

                _count = 0;


                _stack_lock = new object();

                _count_lock = new object();
            }



            public void Push(LocatedServerInfo locatedServerInfo)
            {
                lock (_stack_lock)
                {
                    _stack.Push(locatedServerInfo);

                    lock (_count_lock)
                    {
                        _count++;
                    }
                }
            }

            public LocatedServerInfo Pop()
            {
                lock (_stack_lock)
                {
                    lock (_count_lock)
                    {
                        _count--;
                    }

                    return _stack.Pop();
                }
            }

            public LocatedServerInfo Peek()
            {
                lock (_stack_lock)
                {
                    return _stack.Peek();
                }
            }

            public int Count()
            {
                lock (_count_lock)
                {
                    return _count;
                }
            }

            public void Clear()
            {
                lock (_stack_lock)
                {
                    lock (_count_lock)
                    {
                        _count = 0;
                    }

                    _stack.Clear();
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public sealed class Credentials
        {
            public static Credentials New()
            {
                return new Credentials(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            }



            public string _id;

            public string _password;


            private readonly object _id_lock;

            private readonly object _password_lock;



            /// <summary>
            /// 
            /// </summary>
            public string ID
            {
                get
                {
                    lock (_id_lock)
                    {
                        return _id;
                    }
                }

                set
                {
                    lock (_id_lock)
                    {
                        _id = value;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public string Password
            {
                get
                {
                    lock (_password_lock)
                    {
                        return _password;
                    }
                }

                set
                {
                    lock (_password_lock)
                    {
                        _password = value;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public Credentials Public => new Credentials(ID);



            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            /// <param name="password"></param>
            public Credentials(string id, string password)
            {
                _id = id;

                _password = password;


                _id_lock = new object();

                _password_lock = new object();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            public Credentials(string id) : this(id, "")
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public Credentials()
            {
                _id = "id";

                _password = "password";


                _id_lock = new object();

                _password_lock = new object();
            }



            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return $"[CREDENTIALS][ID][{_id}][PASSWORD][{_password}]";
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                else
                {
                    Credentials credentials = (Credentials)obj;

                    return ID == credentials.ID && Password == credentials.Password;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return $"{ID}{Password}".GetHashCode();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="A"></param>
            /// <param name="B"></param>
            /// <returns></returns>
            public static bool operator ==(Credentials A, Credentials B)
            {
                return A.Equals(B);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="A"></param>
            /// <param name="B"></param>
            /// <returns></returns>
            public static bool operator !=(Credentials A, Credentials B)
            {
                return !A.Equals(B);
            }
        }
    }

    namespace Net.Multiplayer.Chat
    {
        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public sealed class Chat
        {
            private Letter[] _letters;

            private int _max_length;


            private object _letters_lock;

            private object _max_length_lock;



            /// <summary>
            /// 
            /// </summary>
            public Letter[] Letters
            {
                get
                {
                    lock (_letters_lock)
                    {
                        return _letters;
                    }
                }

                set
                {
                    lock (_letters_lock)
                    {
                        _letters = value;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public int MaxLength
            {
                get
                {
                    lock (_max_length_lock)
                    {
                        return _max_length;
                    }
                }

                set
                {
                    lock (_max_length_lock)
                    {
                        MaxLength = value;
                    }

                    while (Letters.Length > MaxLength)
                    {
                        Delete(0);
                    }
                }
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="maxLength"></param>
            public Chat(int maxLength)
            {
                _letters = new Letter[0];

                _max_length = maxLength;


                _letters_lock = new object();

                _max_length_lock = new object();
            }

            /// <summary>
            /// 
            /// </summary>
            public Chat()
            {
                _letters = new Letter[0];

                _max_length = 128;


                _letters_lock = new object();

                _max_length_lock = new object();
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="letter"></param>
            public void Add(Letter letter)
            {
                if (Letters.Length >= MaxLength)
                {
                    Delete(0);
                }

                Letter[] letters = _letters;

                _letters = new Letter[letters.Length + 1];

                for (int i = 0; i < letters.Length; i++)
                {
                    _letters[i] = letters[i];
                }

                _letters[letters.Length] = letter;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="index"></param>
            public void Delete(int index)
            {
                Letter[] letters = _letters;

                _letters = new Letter[letters.Length - 1];

                for (int i = 0; i < index; i++)
                {
                    _letters[i] = letters[i];
                }

                for (int i = index; i < _letters.Length; i++)
                {
                    _letters[i] = letters[i + 1];
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void Clear()
            {
                _letters = new Letter[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public sealed class Letter
        {
            private string _id;

            private string _message;


            private object _id_lock;

            private object _message_lock;



            /// <summary>
            /// 
            /// </summary>
            public string ID
            {
                get
                {
                    lock (_id_lock)
                    {
                        return _id;
                    }
                }

                set
                {
                    lock (_id_lock)
                    {
                        _id = value;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public string Message
            {
                get
                {
                    lock (_message_lock)
                    {
                        return _message;
                    }
                }

                set
                {
                    lock (_message_lock)
                    {
                        _message = value;
                    }
                }
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            /// <param name="message"></param>
            public Letter(string id, string message)
            {
                _id = id;

                _message = message;


                _id_lock = new object();

                _message_lock = new object();
            }

            /// <summary>
            /// 
            /// </summary>
            public Letter()
            {
                _id_lock = new object();

                _message_lock = new object();
            }
        }
    }

    namespace Net.Multiplayer.Commands
    {
        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public sealed class Terminal
        {
            public Command[] commands;

            private object _commands_lock;



            /// <summary>
            /// 
            /// </summary>
            public Command[] Commands
            {
                get
                {
                    lock (_commands_lock)
                    {
                        return commands;
                    }
                }

                set
                {
                    lock (_commands_lock)
                    {
                        commands = value;
                    }
                }
            }



            /// <summary>
            /// 
            /// </summary>
            public Terminal()
            {
                commands = new Command[0];

                _commands_lock = new object();
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="argument"></param>
            /// <returns></returns>
            public Terminal Arg(string argument)
            {
                lock (_commands_lock)
                {
                    commands[commands.Length - 1].Arg(argument);
                }

                return this;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="command"></param>
            /// <returns></returns>
            public Terminal Next(string command)
            {
                return Next(Command.New(command));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="command"></param>
            /// <returns></returns>
            public Terminal Next(Command command)
            {
                lock (_commands_lock)
                {
                    Command[] commands = this.commands;

                    this.commands = new Command[commands.Length + 1];

                    for (int i = 0; i < commands.Length; i++)
                    {
                        this.commands[i] = commands[i];
                    }

                    this.commands[commands.Length] = command;
                }

                return this;
            }



            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static Terminal New()
            {
                return new Terminal();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="command"></param>
            /// <returns></returns>
            public static Terminal New(string command)
            {
                return New().Next(command);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public sealed class Command
        {
            public string[] args;

            private object _args_lock;



            /// <summary>
            /// 
            /// </summary>
            public string[] Arguments
            {
                get
                {
                    lock (_args_lock)
                    {
                        return args;
                    }
                }

                set
                {
                    lock (_args_lock)
                    {
                        args = value;
                    }
                }
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="args"></param>
            private Command(params string[] args)
            {
                this.args = args;

                _args_lock = new object();
            }

            /// <summary>
            /// 
            /// </summary>
            public Command()
            {
                args = new string[0];

                _args_lock = new object();
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="argument"></param>
            /// <returns></returns>
            public Command Arg(string argument)
            {
                lock (_args_lock)
                {
                    string[] args = this.args;

                    this.args = new string[args.Length + 1];

                    for (int i = 0; i < args.Length; i++)
                    {
                        this.args[i] = args[i];
                    }

                    this.args[args.Length] = argument;
                }

                return this;
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="command"></param>
            /// <returns></returns>
            public static Command New(string command)
            {
                if (command[0] != '/')
                {
                    return new Command($"/{command}");
                }

                return new Command(command);
            }



            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                lock (_args_lock)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append($"[ARGS][{Arguments.Length}]");

                    for (int i = 0; i < Arguments.Length; i++)
                    {
                        sb.Append($"[ARG][{i}][{Arguments[i]}]");
                    }

                    return sb.ToString();
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                else
                {
                    Command cmds = (Command)obj;

                    lock (_args_lock)
                    {
                        return Arguments[0] == cmds.Arguments[0];
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                lock (_args_lock)
                {
                    return args.GetHashCode();
                }
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="A"></param>
            /// <param name="B"></param>
            /// <returns></returns>
            public static bool operator ==(Command A, Command B)
            {
                return A.Equals(B);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="A"></param>
            /// <param name="B"></param>
            /// <returns></returns>
            public static bool operator !=(Command A, Command B)
            {
                return !A.Equals(B);
            }
        }
    }



    /// <summary>
    /// Platform
    /// </summary>
    [Flags]
    public enum EPlatform
    {
        Windows = 1, Linux = 2, MacOS = 4, Standalone = 7, Android = 8, IOS = 16, Mobile = 24,
    }
}