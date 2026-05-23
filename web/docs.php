<?php
require_once 'config.php';
?>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Documentation - Lan Multiplayer</title>
    <link rel="stylesheet" href="css/style.css?v=1.1">
</head>
<body>
    <header>
        <div class="container">
            <nav>
                <a href="index.php" class="logo"><img src="logo.png" alt="Lan Multiplayer"></a>
                <ul class="nav-links">
                    <li><a href="index.php">Home</a></li>
                    <li><a href="about.php">About</a></li>
                    <li><a href="docs.php">Docs</a></li>
                    <li><a href="privacy.php">Privacy</a></li>
                    <li><a href="terms.php">Terms</a></li>
                </ul>
            </nav>
        </div>
    </header>

    <main>
        <div class="container">
            <div class="content-page">
            <section class="doc-section">
                <h2>Overview</h2>
                <p>Michitai LAN Multiplayer is a comprehensive networking library for building local area network multiplayer games and applications. It provides both Unity and .NET implementations with support for multiple platforms including Windows, Linux, macOS, Android, and iOS.</p>
                
                <h3>Key Features</h3>
                <ul>
                    <li>Cross-platform LAN discovery and communication</li>
                    <li>UDP broadcast for server discovery</li>
                    <li>TCP client-server architecture for reliable data transfer</li>
                    <li>Thread-safe data synchronization</li>
                    <li>JSON serialization for game data</li>
                    <li>Player authentication and management</li>
                    <li>Command system for terminal-like operations</li>
                </ul>
            </section>

            <section class="doc-section">
                <h2>Architecture</h2>
                <p>The library is organized into several key namespaces:</p>
                
                <div class="namespace-block">
                    <h3>Michitai.Lan</h3>
                    <p>Root namespace containing platform enumeration and core types.</p>
                    <ul>
                        <li><code>EPlatform</code> - Platform enumeration (Windows, Linux, MacOS, Android, iOS, Standalone, Mobile)</li>
                    </ul>
                </div>

                <div class="namespace-block">
                    <h3>Michitai.Lan.Data</h3>
                    <p>Data structures and JSON storage interfaces for game data management.</p>
                    <ul>
                        <li><code>IJsonStorage</code> - Interface for JSON serialization/deserialization</li>
                        <li><code>PlayerGameData</code> - Player game data with JSON capabilities</li>
                        <li><code>PlayerCharacterData</code> - Player character-specific data</li>
                        <li><code>PlayerWorldData</code> - Player world-specific data</li>
                    </ul>
                </div>

                <div class="namespace-block">
                    <h3>Michitai.Lan.Net</h3>
                    <p>Core networking functionality for LAN operations.</p>
                    <ul>
                        <li><code>Lan</code> - Static class for IP address and broadcast mask operations</li>
                        <li><code>UDPBroadcast</code> - UDP broadcast functionality for network discovery</li>
                        <li><code>TCPClient</code> - TCP client for reliable connections</li>
                        <li><code>TCPServer</code> - TCP server for accepting client connections</li>
                        <li><code>TCPServerClient</code> - Server-side representation of connected clients</li>
                        <li><code>PortRange</code> - Port range management with predefined ranges</li>
                        <li><code>Message</code> - Base message class</li>
                        <li><code>AppMessage</code> - Application-level message</li>
                        <li><code>IdentifiedMessage</code> - Message with client identification</li>
                        <li><code>LocatedMessage</code> - Message with sender endpoint information</li>
                    </ul>
                </div>

                <div class="namespace-block">
                    <h3>Michitai.Lan.Net.Multiplayer</h3>
                    <p>High-level multiplayer game networking components.</p>
                    <ul>
                        <li><code>Client</code> - Multiplayer client for connecting to servers</li>
                        <li><code>Server</code> - Multiplayer server for managing game sessions</li>
                        <li><code>BroadcastClient</code> - Client-side broadcast discovery</li>
                        <li><code>BroadcastServer</code> - Server-side broadcast announcement</li>
                        <li><code>Multiplayer</code> - Main multiplayer coordination class</li>
                    </ul>
                </div>

                <div class="namespace-block">
                    <h3>Michitai.Lan.Net.Multiplayer.Data</h3>
                    <p>Data structures specific to multiplayer networking.</p>
                    <ul>
                        <li><code>ClientGameData</code> - Client-side game data</li>
                        <li><code>ServerGameData</code> - Server-side game data with player management</li>
                        <li><code>ServerClientGameData</code> - Combined server client data</li>
                        <li><code>Credentials</code> - Player authentication credentials</li>
                        <li><code>ServerInfo</code> - Server information for discovery</li>
                        <li><code>LocatedServerInfo</code> - Discovered server information</li>
                        <li><code>ServerInfoStack</code> - Stack of discovered servers</li>
                        <li><code>MultiplayerGamesData</code> - Multiplayer game metadata</li>
                    </ul>
                </div>

                <div class="namespace-block">
                    <h3>Michitai.Lan.Net.Multiplayer.Commands</h3>
                    <p>Command system for terminal-like operations.</p>
                    <ul>
                        <li><code>Command</code> - Command structure and execution</li>
                        <li><code>Terminal</code> - Terminal interface for command input</li>
                    </ul>
                </div>
            </section>

            <section class="doc-section">
                <h2>Directory Structure</h2>
                
                <h3>.NET Implementation (dotnet/)</h3>
                <pre class="code-block">dotnet/
├── Data/
│   ├── IJsonStorage.cs          # JSON storage interface
│   ├── PlayerGameData.cs        # Player game data
│   ├── PlayerCharacterData.cs   # Player character data
│   └── PlayerWorldData.cs       # Player world data
├── Net/
│   ├── Lan.cs                   # LAN IP operations
│   ├── UDPBroadcast.cs          # UDP broadcast
│   ├── TCPClient.cs             # TCP client
│   ├── TCPServer.cs             # TCP server
│   ├── TCPServerClient.cs       # Server client representation
│   ├── PortRange.cs             # Port range management
│   ├── Message.cs               # Base message
│   ├── AppMessage.cs            # Application message
│   ├── IdentifiedMessage.cs     # Identified message
│   ├── LocatedMessage.cs        # Located message
│   ├── Multiplayer/
│   │   ├── Client.cs            # Multiplayer client
│   │   ├── Server.cs            # Multiplayer server
│   │   ├── BroadcastClient.cs   # Broadcast discovery client
│   │   ├── BroadcastServer.cs   # Broadcast announcement server
│   │   ├── Multiplayer.cs       # Main multiplayer class
│   │   ├── Data/                # Multiplayer data structures
│   │   │   ├── ClientGameData.cs
│   │   │   ├── ServerGameData.cs
│   │   │   ├── ServerClientGameData.cs
│   │   │   ├── Credentials.cs
│   │   │   ├── ServerInfo.cs
│   │   │   ├── LocatedServerInfo.cs
│   │   │   ├── ServerInfoStack.cs
│   │   │   └── MultiplayerGamesData.cs
│   │   └── Commands/            # Command system
│   │       ├── Command.cs
│   │       └── Terminal.cs
│   └── Debugging/               # Debugging utilities
├── EPlatform.cs                 # Platform enumeration
├── michitai-lan.csproj          # Project file
└── michitai-lan.sln             # Solution file</pre>

                <h3>Unity Implementation (unity/)</h3>
                <pre class="code-block">unity/
├── Data/
│   ├── IJsonStorage.cs          # JSON storage interface
│   └── JsonStorage.cs           # JSON storage implementation
├── Net/
│   ├── Lan.cs                   # LAN IP operations
│   ├── UDPBroadcast.cs          # UDP broadcast
│   ├── TCPClient.cs             # TCP client
│   ├── TCPServer.cs             # TCP server
│   ├── TCPServerClient.cs       # Server client representation
│   ├── PortRange.cs             # Port range management
│   ├── Message.cs               # Base message
│   ├── AppMessage.cs            # Application message
│   ├── IdentifiedMessage.cs     # Identified message
│   ├── LocatedMessage.cs        # Located message
│   ├── Multiplayer/
│   │   ├── Client.cs            # Multiplayer client
│   │   ├── Server.cs            # Multiplayer server
│   │   ├── BroadcastClient.cs   # Broadcast discovery client
│   │   ├── BroadcastServer.cs   # Broadcast announcement server
│   │   ├── MobileBroadcastClient.cs   # Mobile broadcast client
│   │   ├── MobileBroadcastServer.cs   # Mobile broadcast server
│   │   ├── Multiplayer.cs       # Main multiplayer class
│   │   ├── Data/                # Multiplayer data structures
│   │   │   ├── ClientGameData.cs
│   │   │   ├── ServerGameData.cs
│   │   │   ├── ServerClientGameData.cs
│   │   │   ├── Credentials.cs
│   │   │   ├── ServerInfo.cs
│   │   │   ├── LocatedServerInfo.cs
│   │   │   ├── ServerInfoStack.cs
│   │   │   └── MultiplayerGamesData.cs
│   │   ├── Commands/            # Command system
│   │   │   ├── Command.cs
│   │   │   └── Terminal.cs
│   │   └── Chat/                # Chat functionality
│   │       ├── ChatClient.cs
│   │       └── ChatServer.cs
│   └── Debugging/               # Debugging utilities
└── EPlatform.cs                 # Platform enumeration</pre>
            </section>

            <section class="doc-section">
                <h2>Platform Support</h2>
                <p>The library supports multiple platforms through the <code>EPlatform</code> enumeration:</p>
                
                <table class="platform-table">
                    <thead>
                        <tr>
                            <th>Platform</th>
                            <th>Value</th>
                            <th>Description</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>Windows</td>
                            <td>1</td>
                            <td>Windows desktop platform</td>
                        </tr>
                        <tr>
                            <td>Linux</td>
                            <td>2</td>
                            <td>Linux desktop platform</td>
                        </tr>
                        <tr>
                            <td>MacOS</td>
                            <td>4</td>
                            <td>macOS desktop platform</td>
                        </tr>
                        <tr>
                            <td>Standalone</td>
                            <td>7</td>
                            <td>All desktop platforms (Windows + Linux + macOS)</td>
                        </tr>
                        <tr>
                            <td>Android</td>
                            <td>8</td>
                            <td>Android mobile platform</td>
                        </tr>
                        <tr>
                            <td>iOS</td>
                            <td>16</td>
                            <td>iOS mobile platform</td>
                        </tr>
                        <tr>
                            <td>Mobile</td>
                            <td>24</td>
                            <td>All mobile platforms (Android + iOS)</td>
                        </tr>
                    </tbody>
                </table>

                <p>Platform flags can be combined using bitwise operations for multi-platform support.</p>
            </section>

            <section class="doc-section">
                <h2>Key Components</h2>

                <h3>LAN Operations</h3>
                <p>The <code>Lan</code> static class provides utilities for working with local network addresses:</p>
                <pre class="code-block">// Get all local IPv4 addresses
IPAddress[] addresses = Lan.LocalIPv4Addresses(EPlatform.Standalone);

// Get broadcast masks for discovery
IPAddress[] masks = Lan.LocalIPv4Masks(EPlatform.Standalone);

// Try-get pattern with error handling
if (Lan.TryGetLocalIPv4Addresses(EPlatform.Standalone, out var ips)) {
    // Use addresses
}</pre>

                <h3>UDP Broadcast</h3>
                <p><code>UDPBroadcast</code> enables network discovery through UDP broadcasting:</p>
                <pre class="code-block">// Create broadcast client
var broadcast = new UDPBroadcast(ipAddress, port);

// Send discovery message
await broadcast.SendAsync(broadcastAddress, portRange, message);

// Receive responses
var response = await broadcast.ReceiveAsync(timeoutMs);

// Cleanup
broadcast.Stop();</pre>

                <h3>Multiplayer Server</h3>
                <p>The <code>Server</code> class manages multiplayer game sessions:</p>
                <pre class="code-block">// Create server
var server = new Server(
    name: "MyGameServer",
    serverGameData: gameData,
    ip: ipAddress,
    port: 7777
);

// Subscribe to events
server.OnClientConnected += (id) => Console.WriteLine($"Client {id} connected");
server.OnClientDisconnected += (id) => Console.WriteLine($"Client {id} disconnected");
server.OnRequest += (msg) => HandleRequest(msg);

// Start server
server.Start();

// Register new player
var credentials = server.RegisterNewPlayer(playerData);

// Log in player
server.LogInPlayer(clientId, credentials);

// Send response
server.Response(identifiedMessage);

// Stop server
server.Stop();</pre>

                <h3>Multiplayer Client</h3>
                <p>The <code>Client</code> class connects to multiplayer servers:</p>
                <pre class="code-block">// Create client
var client = new Client(
    clientGameData: myData,
    gameData: playerData,
    ip: serverIp,
    port: 7777
);

// Subscribe to events
client.OnResponse += (msg) => HandleResponse(msg);
client.OnDisconnected += () => Console.WriteLine("Disconnected");

// Start client
client.Start();

// Send request
if (client.CanRequest) {
    client.Request(message);
}

// Stop client
client.Stop();</pre>

                <h3>Port Management</h3>
                <p><code>PortRange</code> provides predefined port ranges and port management:</p>
                <pre class="code-block">// Use predefined ranges
var broadcastPorts = PortRange.Broadcast;        // 64512-65535
var dynamicPorts = PortRange.Dynamic;            // 49152-65535
var registeredPorts = PortRange.Registered;      // 1024-49151

// Create custom range
var customRange = new PortRange(8000, 9000);

// Get port store for random port selection
var store = customRange.RangeStore;
int randomPort = store.RandomPort;  // Gets and removes a random port</pre>
            </section>

            <section class="doc-section">
                <h2>Data Serialization</h2>
                <p>All game data implements <code>IJsonStorage</code> for JSON serialization:</p>
                <pre class="code-block">// Create player data
var playerData = new PlayerGameData();

// Serialize object to JSON
playerData.Set(myPlayerObject);

// Get JSON string
string json = playerData.Json;

// Deserialize JSON to object
var player = playerData.Get&lt;MyPlayerType&gt;();

// Thread-safe access
lock (playerData) {
    // Access data safely
}</pre>
            </section>

            <section class="doc-section">
                <h2>Thread Safety</h2>
                <p>The library implements thread-safe operations using locks:</p>
                <ul>
                    <li>All data structures use private lock objects for synchronization</li>
                    <li>JSON storage properties are thread-safe with lock guards</li>
                    <li>Server client collections are protected with locks</li>
                    <li>Credentials and ID properties are thread-safe</li>
                </ul>
            </section>

            <section class="doc-section">
                <h2>Message Flow</h2>
                <p>The library uses a message-based communication pattern:</p>
                <ol>
                    <li><strong>Discovery Phase</strong>: Servers broadcast their presence via UDP</li>
                    <li><strong>Connection Phase</strong>: Clients connect via TCP to discovered servers</li>
                    <li><strong>Authentication Phase</strong>: Players authenticate with credentials</li>
                    <li><strong>Game Phase</strong>: Request/response pattern for game data synchronization</li>
                    <li><strong>Disconnection Phase</strong>: Clean disconnect with resource cleanup</li>
                </ol>
            </section>

            <section class="doc-section">
                <h2>Usage Examples</h2>

                <h3>Example 1: Complete Server Setup</h3>
                <p>This example shows how to set up a complete multiplayer server with discovery, player registration, and game state synchronization:</p>
                <pre class="code-block">// Initialize platform
#if UNITY_STANDALONE
    EPlatform platform = EPlatform.Standalone;
#elif UNITY_ANDROID
    EPlatform platform = EPlatform.Android;
#endif

// Configure server
Multiplayer.Name = "Car Driving Multiplayer";
IPAddress[] ips = null;
bool success = Lan.TryGetLocalIPv4Addresses(platform, out ips);
Multiplayer.IpAddress = success ? ips[0] : IPAddress.Any;

// Create server data
ServerGameData serverData = new ServerGameData(Guid.NewGuid().ToString());

// Start server with broadcast discovery
Multiplayer.StartServer(
    platform: platform,
    serverGameData: serverData,
    processMessage: (LocatedMessage msg) => {
        // Process broadcast discovery requests
        Command command = JsonUtility.FromJson<Command>(msg.Message.Message);
        if (msg.Message.Name == "car-driving-multiplayer" && 
            command == Command.New("get-server-info"))
        {
            ServerInfo info = new ServerInfo(
                Multiplayer.Server.IPEndPoint.Port,
                Multiplayer.Name,
                serverData.ServerID,
                serverData.Clients.Length
            );
            return new AppMessage(1, "car-driving-multiplayer", 
                JsonUtility.ToJson(Command.New("server-info").Arg(JsonUtility.ToJson(info))));
        }
        return new AppMessage(1, "car-driving-multiplayer", "denied");
    },
    receiveRequestsDelayMilliseconds: 500
);

// Handle server requests
Multiplayer.Server.OnRequest += (identifiedMessage) => {
    Terminal terminal = JsonUtility.FromJson<Terminal>(identifiedMessage.Message.GetMessage);
    Command[] commands = terminal.Commands;
    Terminal response_terminal = Terminal.New();
    
    foreach (Command cmd in commands)
    {
        if (cmd == Command.New("get-server-id"))
        {
            response_terminal.Next("server-id")
                .Arg($"{Multiplayer.Server.PublicServerData.ServerID}");
        }
        else if (cmd == Command.New("register"))
        {
            JsonStorage data = new JsonStorage(cmd.Arguments[1]);
            Credentials credentials = Multiplayer.Server.RegisterNewPlayer(data);
            response_terminal.Next("credentials")
                .Arg($"{JsonUtility.ToJson(credentials)}");
        }
        else if (cmd == Command.New("log-in"))
        {
            Credentials credentials = JsonUtility.FromJson<Credentials>(cmd.Arguments[1]);
            if (Multiplayer.Server.Contains(credentials))
            {
                Multiplayer.Server.LogInPlayer(identifiedMessage.ID, credentials);
                response_terminal.Next("log-in-successful");
            }
            else
            {
                response_terminal.Next("log-in-error");
            }
        }
        else if (cmd == Command.New("set-game-data"))
        {
            bool success = Multiplayer.Server.TryGetLoggedInPlayerPrivateData(
                identifiedMessage.ID, out ServerClientGameData data);
            if (success)
            {
                data.Data.Json = cmd.Arguments[1];
            }
        }
        else if (cmd == Command.New("get-server-data"))
        {
            response_terminal.Next("server-data")
                .Arg($"{JsonUtility.ToJson(Multiplayer.Server.PublicServerData)}");
        }
    }
    
    Multiplayer.Server.Response(new IdentifiedMessage(
        new Message(JsonUtility.ToJson(response_terminal)), 
        identifiedMessage.ID
    ));
};</pre>

                <h3>Example 2: Complete Client Setup</h3>
                <p>This example shows how to set up a client that discovers servers, connects, and synchronizes game state:</p>
                <pre class="code-block">// Initialize platform
#if UNITY_STANDALONE
    EPlatform platform = EPlatform.Standalone;
#elif UNITY_ANDROID
    EPlatform platform = EPlatform.Android;
#endif

// Start broadcast discovery in main menu
Multiplayer.StartBroadcastClient(
    platform: platform,
    request: new AppMessage(1, "car-driving-multiplayer", 
        JsonUtility.ToJson(Command.New("get-server-info"))),
    onReceiveResponse: (LocatedMessage response) => {
        if (response.Message.Name == "car-driving-multiplayer")
        {
            Command command = JsonUtility.FromJson<Command>(response.Message.Message);
            if (command == Command.New("server-info"))
            {
                ServerInfo serverInfo = JsonUtility.FromJson<ServerInfo>(command.Arguments[1]);
                // Store server info for user selection
                discoveredServers.Add(new LocatedServerInfo(serverInfo, response.IPEndPoint));
            }
        }
    },
    receiveResponsesMilliseconds: 5000,
    repeatAfterMilliseconds: 5000
);

// User selects server - connect to it
Multiplayer.IpAddress = selectedServer.IPEndPoint.Address;
Multiplayer.Port = selectedServer.ServerInfo.Port;

// Start client
Multiplayer.StartClient();

// Handle responses
Multiplayer.Client.OnResponse += (message) => {
    Terminal terminal = JsonUtility.FromJson<Terminal>(message.GetMessage);
    Command[] commands = terminal.Commands;
    
    foreach (Command cmd in commands)
    {
        if (cmd == Command.New("server-id"))
        {
            server_id = cmd.Arguments[1];
            // Register with game data
            GameData data = new GameData(new CharacterData(...));
            Multiplayer.Client.GameData = new JsonStorage(JsonUtility.ToJson(data));
            Multiplayer.Client.Request(new Message(
                JsonUtility.ToJson(Terminal.New("register").Arg(JsonUtility.ToJson(data)))
            ));
        }
        else if (cmd == Command.New("credentials"))
        {
            Credentials credentials = JsonUtility.FromJson<Credentials>(cmd.Arguments[1]);
            Multiplayer.Client.ClientData = new ClientGameData(server_id, credentials);
            Multiplayer.Client.Request(new Message(
                JsonUtility.ToJson(Terminal.New("log-in").Arg(JsonUtility.ToJson(credentials)))
            ));
        }
        else if (cmd == Command.New("log-in-successful"))
        {
            Multiplayer.Client.Request(new Message(
                JsonUtility.ToJson(Terminal.New("get-server-data"))
            ));
        }
        else if (cmd == Command.New("server-data"))
        {
            ServerGameData server_data = JsonUtility.FromJson<ServerGameData>(cmd.Arguments[1]);
            Multiplayer.Client.ServerData = server_data;
            // Update remote players
            UpdateRemotePlayers(server_data.Clients);
        }
    }
};

// Game loop - send player state
private void Update()
{
    if (Multiplayer.IsClient && Multiplayer.Client.CanRequest)
    {
        if (Time.time >= updateRate + lastUpdate)
        {
            lastUpdate = Time.time;
            
            Terminal commands = Terminal.New()
                .Next("set-game-data").Arg(JsonUtility.ToJson(new GameData(characterData)))
                .Next("get-server-data");
            
            Multiplayer.Client.Request(new Message(JsonUtility.ToJson(commands)));
        }
    }
}</pre>

                <h3>Example 3: Game Data Serialization</h3>
                <p>This example shows how to create serializable game data structures:</p>
                <pre class="code-block">// Define serializable game data classes
[Serializable]
public class GameData
{
    public CharacterData character_data;
    public WorldData world_data;
}

[Serializable]
public class CharacterData
{
    public float position_x;
    public float position_y;
    public float position_z;
    public float rotation_x;
    public float rotation_y;
    public float rotation_z;
    public bool lights_on;
    public int car_index;
    // Add more fields as needed
}

[Serializable]
public class WorldData
{
    public string world_name;
    public int level_index;
    // Add world-specific data
}

// Use JSON storage for serialization
PlayerGameData playerData = new PlayerGameData();
playerData.Set(new GameData(characterData, worldData));

// Get JSON string
string json = playerData.Json;

// Deserialize back
GameData data = playerData.Get&lt;GameData&gt;();</pre>

                <h3>Example 4: Command Processing Pattern</h3>
                <p>This example shows the recommended pattern for processing commands on the server:</p>
                <pre class="code-block">// Server-side command processing
Multiplayer.Server.OnRequest += (identifiedMessage) => {
    Terminal terminal = JsonUtility.FromJson&lt;Terminal&gt;(identifiedMessage.Message.GetMessage);
    Command[] commands = terminal.Commands;
    Terminal response_terminal = Terminal.New();
    
    foreach (Command cmd in commands)
    {
        switch (cmd.Arguments[0])
        {
            case "/get-server-id":
                // Return server ID
                response_terminal.Next("server-id")
                    .Arg($"{Multiplayer.Server.PublicServerData.ServerID}");
                break;
                
            case "/register":
                // Register new player
                JsonStorage data = new JsonStorage(cmd.Arguments[1]);
                Credentials credentials = Multiplayer.Server.RegisterNewPlayer(data);
                response_terminal.Next("credentials")
                    .Arg($"{JsonUtility.ToJson(credentials)}");
                break;
                
            case "/log-in":
                // Authenticate player
                Credentials credentials = JsonUtility.FromJson&lt;Credentials&gt;(cmd.Arguments[1]);
                if (Multiplayer.Server.Contains(credentials))
                {
                    Multiplayer.Server.LogInPlayer(identifiedMessage.ID, credentials);
                    response_terminal.Next("log-in-successful");
                }
                else
                {
                    response_terminal.Next("log-in-error");
                }
                break;
                
            case "/set-game-data":
                // Update player game state
                bool success = Multiplayer.Server.TryGetLoggedInPlayerPrivateData(
                    identifiedMessage.ID, out ServerClientGameData data);
                if (success)
                {
                    data.Data.Json = cmd.Arguments[1];
                }
                break;
                
            case "/get-server-data":
                // Return all players' public data
                response_terminal.Next("server-data")
                    .Arg($"{JsonUtility.ToJson(Multiplayer.Server.PublicServerData)}");
                break;
                
            default:
                // Unknown command
                response_terminal.Next("error").Arg("Unknown command");
                break;
        }
    }
    
    // Send response
    Multiplayer.Server.Response(new IdentifiedMessage(
        new Message(JsonUtility.ToJson(response_terminal)), 
        identifiedMessage.ID
    ));
};</pre>

                <h3>Example 5: Thread-Safe Data Access</h3>
                <p>This example shows how to safely access data in a multi-threaded environment:</p>
                <pre class="code-block">// All data structures use locks for thread safety
// Example from ServerGameData
private readonly object _clients_lock;
private ServerClientGameData[] _clients;

public ServerClientGameData[] Clients
{
    get
    {
        lock (_clients_lock)
        {
            return _clients;
        }
    }
    set
    {
        lock (_clients_lock)
        {
            _clients = value;
        }
    }
}

// Safe access pattern
public void AddPlayer(ServerClientGameData player)
{
    lock (_clients_lock)
    {
        ServerClientGameData[] players = new ServerClientGameData[_clients.Length + 1];
        
        for (int i = 0; i < _clients.Length; i++)
        {
            players[i] = _clients[i];
        }
        
        players[_clients.Length] = player;
        _clients = players;
    }
}

// Usage in game code
lock (serverData)
{
    // Access data safely
    var players = serverData.Clients;
    foreach (var player in players)
    {
        // Process player data
    }
}</pre>

                <h3>Example 6: Platform-Specific Initialization</h3>
                <p>This example shows how to handle different platforms:</p>
                <pre class="code-block">// Platform detection
private EPlatform GetPlatform()
{
#if UNITY_STANDALONE_WIN
    return EPlatform.Windows;
#elif UNITY_STANDALONE_LINUX
    return EPlatform.Linux;
#elif UNITY_STANDALONE_OSX
    return EPlatform.MacOS;
#elif UNITY_ANDROID
    return EPlatform.Android;
#elif UNITY_IOS
    return EPlatform.IOS;
#else
    return EPlatform.Standalone;
#endif
}

// Platform-specific IP retrieval
EPlatform platform = GetPlatform();
IPAddress[] addresses = Lan.LocalIPv4Addresses(platform);

// Platform-specific broadcast
if ((platform & EPlatform.Mobile) != 0)
{
    // Use mobile broadcast implementation
    // MobileBroadcastClient / MobileBroadcastServer
}
else
{
    // Use desktop broadcast implementation
    // BroadcastClient / BroadcastServer
}</pre>
            </section>

            <section class="doc-section">
                <h2>Building</h2>

                <h3>.NET Project</h3>
                <pre class="code-block"># Build the project
dotnet build dotnet/michitai-lan.csproj

# Run tests (if available)
dotnet test</pre>

                <h3>Unity Project</h3>
                <p>Copy the Unity folder contents to your Unity project's Assets folder. The Unity version uses Unity's JSON serialization instead of System.Text.Json.</p>
            </section>

            <section class="doc-section">
                <h2>Dependencies</h2>

                <h3>.NET Version</h3>
                <ul>
                    <li>.NET Framework 4.8 or .NET 6.0+</li>
                    <li>System.Text.Json (for .NET Framework 4.8)</li>
                    <li>System.Net.Sockets</li>
                    <li>System.Threading</li>
                </ul>

                <h3>Unity Version</h3>
                <ul>
                    <li>Unity 2019.4 or later</li>
                    <li>Unity's built-in JSON serialization</li>
                </ul>
            </section>

            <section class="doc-section">
                <h2>License</h2>
                <p>This project is licensed under the terms specified in the <a href="terms.php">Terms and Conditions</a>.</p>
            </section>

            <section class="doc-section">
                <h2>Support</h2>
                <p>For support and inquiries, contact: <a href="mailto:support@michitai.com">support@michitai.com</a></p>
                <p>Website: <a href="https://michitai.com">https://michitai.com</a></p>
            </section>
            </div>
        </div>
    </main>

    <footer>
        <div class="container">
            <p>&copy; 2026 Nichita Levandovici. All rights reserved.</p>
            <p>
                <a href="privacy.php">Privacy Policy</a> | 
                <a href="terms.php">Terms and Conditions</a> | 
                <a href="about.php">About Us</a>
            </p>
        </div>
    </footer>
</body>
</html>
