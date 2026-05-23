<?php
require_once 'config.php';
?>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Lan Multiplayer - Connect Games, Connect People</title>
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
            <section class="hero">
                <h1>Lan Multiplayer</h1>
                <p>Powerful LAN multiplayer networking solution for game developers</p>
                <div class="hero-buttons">
                    <a href="download.php" class="btn btn-large">Download Now</a>
                    <a href="docs.php" class="btn btn-large">View Docs</a>
                </div>
            </section>

            <section class="features">
                <h2>Why Choose Lan Multiplayer?</h2>
                <div class="features-grid">
                    <div class="feature-card">
                        <h3>🚀 Easy Integration</h3>
                        <p>Simple API that works with both .NET and Unity projects</p>
                    </div>
                    <div class="feature-card">
                        <h3>🌐 LAN-First</h3>
                        <p>Optimized for local area networks with low latency</p>
                    </div>
                    <div class="feature-card">
                        <h3>🔒 Secure</h3>
                        <p>Built-in security features for safe multiplayer sessions</p>
                    </div>
                    <div class="feature-card">
                        <h3>⚡ High Performance</h3>
                        <p>Efficient networking stack for smooth gameplay</p>
                    </div>
                    <div class="feature-card">
                        <h3>🎮 Cross-Platform</h3>
                        <p>Works on Windows, Linux, and macOS</p>
                    </div>
                    <div class="feature-card">
                        <h3>📚 Well Documented</h3>
                        <p>Comprehensive documentation and examples</p>
                    </div>
                </div>
            </section>

            <section class="examples">
                <h2>📝 Quick Examples</h2>
                
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
                    <p>Synchronize game state with server:</p>
                    <pre class="code-block">// Create command with game data
Terminal commands = Terminal.New()
    .Next("set-game-data").Arg(JsonUtility.ToJson(playerData))
    .Next("get-server-data");

// Send to server
if (Multiplayer.Client.CanRequest) {
    Multiplayer.Client.Request(new Message(JsonUtility.ToJson(commands)));
}</pre>
                </div>

                <div class="example-card">
                    <h3>5. Command Pattern</h3>
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
                <h3>📜 Licensing</h3>
                <p><strong>Free for commercial use with attribution</strong> to lan.michitai.com</p>
                <p>Need to use without attribution? <strong>€20 one-time payment per project</strong> for commercial license</p>
            </section>
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
