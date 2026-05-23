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
    /// Static class providing centralized multiplayer management for servers, clients, and broadcast discovery.
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
        /// Mobile broadcast server port.
        /// </summary>
        public static int MobileBroadcastServerPort = 49001;

        /// <summary>
        /// Mobile broadcast client port.
        /// </summary>
        public static int MobileBroadcastClientPort = 49002;

        /// <summary>
        /// The active server instance.
        /// </summary>
        public static Server Server = null;

        /// <summary>
        /// The active client instance.
        /// </summary>
        public static Client Client = null;

        /// <summary>
        /// The active broadcast server instance.
        /// </summary>
        public static BroadcastServer BroadcastServer = null;

        /// <summary>
        /// The broadcast server task for asynchronous operations.
        /// </summary>
        private static Task _BroadcastServerTask = null;

        /// <summary>
        /// Cancellation token source for the broadcast server task.
        /// </summary>
        private static CancellationTokenSource _BroadcastServerTaskTokenSource = null;

        /// <summary>
        /// The active broadcast client instance.
        /// </summary>
        public static BroadcastClient BroadcastClient = null;

        /// <summary>
        /// The broadcast client task for asynchronous operations.
        /// </summary>
        private static Task _BroadcastClientTask = null;

        /// <summary>
        /// Cancellation token source for the broadcast client task.
        /// </summary>
        private static CancellationTokenSource _BroadcastClientTaskTokenSource = null;

        /// <summary>
        /// The mobile broadcast client instance.
        /// </summary>
        private static MobileBroadcastClient _MobileBroadcastClient = null;

        /// <summary>
        /// The mobile broadcast server instance.
        /// </summary>
        private static MobileBroadcastServer _MobileBroadcastServer = null;

        /// <summary>
        /// Queue of client commands to be processed.
        /// </summary>
        public static Queue<Command> ClientCommands = new Queue<Command>();

        /// <summary>
        /// Number of requests that can be sent at the same time.
        /// </summary>
        public static int ClientOnceMaxCommands = 4;

        /// <summary>
        /// Gets whether this instance is running as a server.
        /// </summary>
        public static bool IsServer => Server != null;

        /// <summary>
        /// Gets whether this instance is running as a client.
        /// </summary>
        public static bool IsClient => Client != null;

        /// <summary>
        /// Event raised when starting the server.
        /// </summary>
        public static event Action OnStartServer;

        /// <summary>
        /// Event raised when starting the client.
        /// </summary>
        public static event Action OnStartClient;

        /// <summary>
        /// Event raised when the client has started.
        /// </summary>
        public static event Action OnClientStarted;

        /// <summary>
        /// Event raised when the server has started.
        /// </summary>
        public static event Action OnServerStarted;

        /// <summary>
        /// Starts the multiplayer server with the specified game data and broadcast message processor.
        /// </summary>
        /// <param name="platform">The platform the server is running on.</param>
        /// <param name="serverGameData">The server's game data.</param>
        /// <param name="processMessage">Delegate for processing broadcast messages.</param>
        /// <param name="receiveRequestsDelayMilliseconds">Delay between receiving broadcast requests.</param>
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
        /// Starts the multiplayer client and connects to the server.
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
        /// Stops the multiplayer server and all associated services.
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
        /// Stops the multiplayer client.
        /// </summary>
        public static void StopClient()
    {
        Client?.Stop();

        Client = null;
    }

        /// <summary>
        /// Stops all multiplayer services (both client and server).
        /// </summary>
        public static void Stop()
    {
        StopClient();

        StopServer();
    }

        /// <summary>
        /// Clears all event handlers.
        /// </summary>
        public static void ClearEvents()
    {
        OnStartServer = null;

        OnServerStarted = null;

        OnStartClient = null;

        OnClientStarted = null;
    }



        /// <summary>
        /// Starts the broadcast client for discovering servers on the network.
        /// </summary>
        /// <param name="platform">The platform the client is running on.</param>
        /// <param name="request">The broadcast request message to send.</param>
        /// <param name="onReceiveResponse">Callback for handling received responses.</param>
        /// <param name="receiveResponsesMilliseconds">Duration to receive responses.</param>
        /// <param name="repeatAfterMilliseconds">Interval between broadcast requests.</param>
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
        /// Stops the broadcast client.
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
}
