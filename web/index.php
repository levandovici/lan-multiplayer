<?php
require_once 'config.php';
?>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta http-equiv="Cache-Control" content="no-store, no-cache, must-revalidate, max-age=0">
    <meta http-equiv="Pragma" content="no-cache">
    <meta http-equiv="Expires" content="Wed, 11 Jan 1984 05:00:00 GMT">
    <title>Lan Multiplayer - Connect Games, Connect People</title>
    <link rel="stylesheet" href="css/style.css?v=<?php echo CSS_VERSION; ?>">
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
            <section class="hero">
                <div class="badge">MIT-0 License — Free for any use</div>
                <h1>LAN Multiplayer <span>Made Simple</span></h1>
                <p>Drop-in networking library for .NET and Unity. Discover servers over UDP, sync state over UDP, and reliably command over TCP — all on your local network.</p>
                <div class="hero-buttons">
                    <a href="download.php" class="btn btn-large">Download Now</a>
                    <a href="docs.php" class="btn btn-large btn-outline">View Docs</a>
                </div>
            </section>

            <section class="features">
                <div class="section-header">
                    <h2>Why Choose Lan Multiplayer?</h2>
                    <p>Everything you need to add local multiplayer to your game, without the complexity.</p>
                </div>
                <div class="features-grid">
                    <div class="feature-card">
                        <div class="feature-icon">API</div>
                        <h3>Easy Integration</h3>
                        <p>Simple API that works with both .NET and Unity projects. Get started in minutes.</p>
                    </div>
                    <div class="feature-card">
                        <div class="feature-icon">LAN</div>
                        <h3>LAN-First</h3>
                        <p>Optimized for local area networks — Wi-Fi and wired — with low latency discovery.</p>
                    </div>
                    <div class="feature-card">
                        <div class="feature-icon">60</div>
                        <h3>Real-Time State Sync</h3>
                        <p>Unreliable UDP channel for 30-60 FPS state updates with no head-of-line blocking.</p>
                    </div>
                    <div class="feature-card">
                        <div class="feature-icon">IO</div>
                        <h3>High Performance</h3>
                        <p>Binary framed protocol, NoDelay, SocketAsyncEventArgs I/O, and GZip compression.</p>
                    </div>
                    <div class="feature-card">
                        <div class="feature-icon">CX</div>
                        <h3>Cross-Platform</h3>
                        <p>Windows, Linux, macOS, Android, and iOS from a single codebase.</p>
                    </div>
                    <div class="feature-card">
                        <div class="feature-icon">DOCS</div>
                        <h3>Well Documented</h3>
                        <p>Comprehensive docs and copy-paste examples for common multiplayer patterns.</p>
                    </div>
                </div>
            </section>

            <section class="license-banner">
                <h3>100% Free. No Attribution Required.</h3>
                <p>Lan Multiplayer is released under the MIT No Attribution (MIT-0) license. Use it in personal, commercial, or proprietary projects without paying or giving credit.</p>
            </section>

            <section class="examples">
                <div class="section-header">
                    <h2>Quick Examples</h2>
                    <p>Copy, paste, and adapt these snippets to your project.</p>
                </div>
                
                <div class="example-card">
                    <h3>1. Server Discovery</h3>
                    <p>Automatically discover LAN servers using UDP broadcast:</p>
                    <pre class="code-block">// Start broadcast discovery
Multiplayer.StartBroadcastClient(
    platform: EPlatform.Standalone,
    request: new AppMessage(1, "my-game", JsonUtility.ToJson(Command.New("discover"))),
    onReceiveResponse: (LocatedMessage response) => {
        ServerInfo serverInfo = JsonUtility.FromJson<ServerInfo>(response.Message.Message);
        Console.WriteLine($"Found server: {serverInfo.Name} at {response.IPEndPoint}");
    },
    receiveResponsesMilliseconds: 5000,
    repeatAfterMilliseconds: 5000
);</pre>
                </div>

                <div class="example-card">
                    <h3>2. Start a Server</h3>
                    <p>Create and start a multiplayer server:</p>
                    <pre class="code-block">// Configure server
Multiplayer.Name = "My Game Server";
Multiplayer.IpAddress = Lan.LocalIPv4Addresses(EPlatform.Standalone)[0];

// Start server with game data
Multiplayer.StartServer(
    platform: EPlatform.Standalone,
    serverGameData: new ServerGameData(Guid.NewGuid().ToString()),
    processMessage: (LocatedMessage msg) => {
        // Handle discovery requests
        return new AppMessage(1, "my-game", JsonUtility.ToJson(Command.New("server-info")));
    },
    receiveRequestsDelayMilliseconds: 500
);</pre>
                </div>

                <div class="example-card">
                    <h3>3. Connect as Client</h3>
                    <p>Connect to a discovered server:</p>
                    <pre class="code-block">// Set connection parameters
Multiplayer.IpAddress = serverIpAddress;
Multiplayer.Port = serverPort;

// Start client
Multiplayer.StartClient();

// Handle responses
Multiplayer.Client.OnResponse += (message) => {
    Terminal terminal = JsonUtility.FromJson<Terminal>(message.GetMessage);
    // Process server response
};</pre>
                </div>

                <div class="example-card">
                    <h3>4. Send Game Data</h3>
                    <p>Synchronize game state with server over reliable TCP (queued automatically):</p>
                    <pre class="code-block">// Create command with game data
Terminal commands = Terminal.New()
    .Next("set-game-data").Arg(JsonUtility.ToJson(playerData))
    .Next("get-server-data");

// Send to server — queued if a response is still pending
Multiplayer.Client.Request(new Message(JsonUtility.ToJson(commands)));</pre>
                </div>

                <div class="example-card">
                    <h3>5. Per-Frame State Sync</h3>
                    <p>Send positions/transforms at 30-60 Hz over the unreliable UDP channel:</p>
                    <pre class="code-block">// Client: each frame
Multiplayer.SendState(new Message(JsonUtility.ToJson(playerTransform)));

// Server: apply + rebroadcast to all known clients
Multiplayer.OnState += (IPEndPoint from, Message msg) => ApplyState(from, msg);
Multiplayer.BroadcastState(new Message(JsonUtility.ToJson(worldSnapshot)));</pre>
                </div>

                <div class="example-card">
                    <h3>6. Command Pattern</h3>
                    <p>Use the terminal command system:</p>
                    <pre class="code-block">// Create commands with arguments
Command loginCmd = Command.New("login")
    .Arg("username")
    .Arg("password");

// Chain multiple commands
Terminal terminal = Terminal.New()
    .Next("login").Arg("user").Arg("pass")
    .Next("get-data")
    .Next("set-data").Arg(jsonData);</pre>
                </div>
            </section>

            <section class="license-info">
                <h3>Open License</h3>
                <p><strong>Lan Multiplayer is released under the MIT No Attribution (MIT-0) license.</strong></p>
                <p>Use it freely for personal, educational, or commercial projects. No payment and no attribution are required.</p>
            </section>
        </div>
    </main>

    <footer>
        <div class="container">
            <p>&copy; 2026 Nichita Levandovici. Released under MIT-0.</p>
            <p>
                <a href="privacy.php">Privacy Policy</a> | 
                <a href="terms.php">Terms and Conditions</a> | 
                <a href="about.php">About Us</a>
            </p>
        </div>
    </footer>
</body>
</html>
