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
