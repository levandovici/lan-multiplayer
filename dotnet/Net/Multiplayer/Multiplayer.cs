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


namespace Michitai.Lan.Net.Multiplayer
{
    /// <summary>
    /// Static class providing centralized multiplayer game management and configuration.
    /// </summary>
    public static class Multiplayer
    {
        /// <summary>
        /// The name of the multiplayer game.
        /// </summary>
        public static string Name = "New Multiplayer Game";

        /// <summary>
        /// The IP address to which the server or/and client will connect.
        /// </summary>
        public static IPAddress IpAddress = IPAddress.Any;

        /// <summary>
        /// The port to which the client will connect.
        /// </summary>
        public static int Port = 50000;

        /// <summary>
        /// Port range. One of them will be used by the server. Default 50000-50128.
        /// </summary>
        public static PortRange ServerPortRange = new PortRange(50000, 50128);

        /// <summary>
        /// Port range. Used by Server and Client to Response and Request UDP Messages. Default 60000-60128.
        /// </summary>
        public static PortRange BroadcastPortRange = new PortRange(60000, 60128);

        /// <summary>
        /// The multiplayer server instance.
        /// </summary>
        public static Server Server = null;

        /// <summary>
        /// The multiplayer client instance.
        /// </summary>
        public static Client Client = null;

        /// <summary>
        /// The broadcast server instance.
        /// </summary>
        public static BroadcastServer BroadcastServer = null;

        /// <summary>
        /// Task for running the broadcast server.
        /// </summary>
        private static Task _BroadcastServerTask = null;

        /// <summary>
        /// Cancellation token source for the broadcast server task.
        /// </summary>
        private static CancellationTokenSource _BroadcastServerTaskTokenSource = null;

        /// <summary>
        /// The broadcast client instance.
        /// </summary>
        public static BroadcastClient BroadcastClient = null;

        /// <summary>
        /// Gets whether the broadcast client is locating responses from the server.
        /// </summary>
        /// <returns>True if BroadcastClient locating Response from Server, otherwise False.</returns>
        public static bool BroadcastClientLocating
    {
        get
        {
            lock (_BroadcastClientLocatingLock)
            {
                return _BroadcastClientLocating;
            }
        }

        private set
        {
            lock (_BroadcastClientLocatingLock)
            {
                _BroadcastClientLocating = value;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private static bool _BroadcastClientLocating = false;

    /// <summary>
    /// 
    /// </summary>
    private static readonly object _BroadcastClientLocatingLock = new object();

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
    public static void StartServer(EPlatform platform, ServerGameData serverGameData, BroadcastServer.ProcessMessageDelegate processMessage, int receiveRequestsDelayMilliseconds = 100)
    {
        if (IsServer)
            return;


        Server = new Server(Name, serverGameData, IpAddress, ServerPortRange);

        IpAddress = Server.IPEndPoint.Address;

        Port = Server.IPEndPoint.Port;


        _BroadcastServerTaskTokenSource = new CancellationTokenSource();

        var token = _BroadcastServerTaskTokenSource.Token;


        _BroadcastServerTask = Task.Run(async () =>
        {
            token.ThrowIfCancellationRequested();

            BroadcastServer = new BroadcastServer(IpAddress, BroadcastPortRange);

            while (true)
            {
                token.ThrowIfCancellationRequested();

                if ((platform & EPlatform.Android) == EPlatform.Android)
                {
                    BroadcastServer.Broadcast(processMessage);
                }
                else if((platform & EPlatform.Windows) == EPlatform.Windows)
                {
                    await BroadcastServer.BroadcastAsync(processMessage);
                }
                else
                {
                    throw new NotImplementedException($"Platform: {platform}");
                }

                token.ThrowIfCancellationRequested();

                await Task.Delay(receiveRequestsDelayMilliseconds, _BroadcastServerTaskTokenSource.Token);
            }

        }, _BroadcastServerTaskTokenSource.Token);


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

        _BroadcastClientTaskTokenSource = new CancellationTokenSource();

        var token = _BroadcastClientTaskTokenSource.Token;

        if ((platform & EPlatform.Windows) == EPlatform.Windows || (platform & EPlatform.Linux) == EPlatform.Linux || (platform & EPlatform.MacOS) == EPlatform.MacOS)
        {
            _BroadcastClientTask = Task.Run(async () =>
            {
                token.ThrowIfCancellationRequested();

                BroadcastClient = new BroadcastClient(IpAddress, BroadcastPortRange);

                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    BroadcastClientLocating = true;

                    IPAddress[] masks = null;

                    bool success = Lan.TryGetLocalIPv4Masks(platform, out masks);

                    if (success)
                    {
                        await BroadcastClient.BroadcastRequestAsync(masks, BroadcastPortRange, request, onReceiveResponse, receiveResponsesMilliseconds);
                    }
                    else
                    {
                        await BroadcastClient.BroadcastRequestAsync(IPAddress.Broadcast, BroadcastPortRange, request, onReceiveResponse, receiveResponsesMilliseconds);
                    }

                    BroadcastClientLocating = false;

                    token.ThrowIfCancellationRequested();

                    await Task.Delay(repeatAfterMilliseconds, _BroadcastClientTaskTokenSource.Token);
                }

            }, _BroadcastClientTaskTokenSource.Token);
        }
        else if((platform & EPlatform.Android) == EPlatform.Android || (platform & EPlatform.IOS) == EPlatform.IOS)
        {
            Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();

                BroadcastClient = new BroadcastClient(IpAddress, BroadcastPortRange);

                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    BroadcastClientLocating = true;

                    IPAddress[] masks = null;

                    bool success = Lan.TryGetLocalIPv4Masks(platform, out masks);

                    if (success)
                    {
                        BroadcastClient.BeginBroadcastRequest(masks, BroadcastPortRange, request, onReceiveResponse, receiveResponsesMilliseconds);
                    }
                    else
                    {
                        BroadcastClient.BeginBroadcastRequest(IPAddress.Broadcast, BroadcastPortRange, request, onReceiveResponse, receiveResponsesMilliseconds);
                    }

                    BroadcastClientLocating = false;

                    token.ThrowIfCancellationRequested();

                    Task.Delay(repeatAfterMilliseconds, _BroadcastClientTaskTokenSource.Token).Wait();
                }

            }, _BroadcastClientTaskTokenSource.Token);
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


        BroadcastClientLocating = false;
    }
}
}
