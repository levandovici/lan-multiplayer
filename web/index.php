<?php
define('LAN_PUBLIC_PAGE', true);
require_once 'config.php';
require_once 'seo.php';

$howTo = [
    '@type' => 'HowTo',
    'name' => 'How to add LAN multiplayer to a game with Lan Multiplayer',
    'description' => 'Download the free MIT-0 library, drop the sources into a .NET, Unity or C++ project, then discover servers and sync state on the local network.',
    'image' => seoUrl('logo.png'),
    'totalTime' => 'PT15M',
    'estimatedCost' => ['@type' => 'MonetaryAmount', 'currency' => 'USD', 'value' => '0'],
    'step' => [
        ['@type' => 'HowToStep', 'position' => 1, 'name' => 'Download the library', 'url' => seoUrl('/download.php')],
        ['@type' => 'HowToStep', 'position' => 2, 'name' => 'Add the sources to your .NET, Unity or C++ project', 'url' => seoUrl('/docs.php')],
        ['@type' => 'HowToStep', 'position' => 3, 'name' => 'Start a server and answer UDP discovery probes', 'url' => seoUrl('/docs.php')],
        ['@type' => 'HowToStep', 'position' => 4, 'name' => 'Connect clients and sync state at 30-60 Hz', 'url' => seoUrl('/docs.php')],
    ],
];
?>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=5.0, viewport-fit=cover">
    <?php seoHead([
        'title' => 'Lan Multiplayer - Free LAN Networking Library for .NET, Unity & C++',
        'description' => 'Free MIT-0 LAN multiplayer library for game developers: UDP server discovery, reliable TCP commands and 30-60 FPS UDP state sync. Drop-in sources for .NET, Unity and C++.',
        'path' => '/',
        'keywords' => 'LAN multiplayer library, local multiplayer, Unity LAN networking, .NET game networking, C++ multiplayer library, UDP broadcast discovery, TCP UDP game server, MIT-0 open source, Michitai Lan',
        'jsonld' => [$howTo],
    ]); ?>
    <link rel="stylesheet" href="css/style.css?v=<?php echo CSS_VERSION; ?>">
</head>
<body>
    <header>
        <div class="container">
            <nav>
                <a href="index.php" class="logo"><img src="logo.png" alt="Lan Multiplayer"><span>Lan Multiplayer</span></a>
                <ul class="nav-links">
                    <li><a href="index.php">Home</a></li>
                    <li><a href="about.php">About</a></li>
                    <li><a href="docs.php">Docs</a></li>
                    <li><a href="faq.php">FAQ</a></li>
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

            <section class="what-is">
                <div class="section-header">
                    <h2>What is Lan Multiplayer?</h2>
                    <p>Lan Multiplayer (Michitai.Lan) is a free, open-source networking library that adds local-area-network multiplayer to games. It handles server discovery, reliable commands and fast per-frame state sync — so game developers can ship couch-coop, LAN-party and same-Wi-Fi multiplayer without running any servers.</p>
                </div>
                <div class="features-grid">
                    <div class="feature-card">
                        <h3>Discover</h3>
                        <p>Servers answer UDP broadcast probes on ports 60000-60128; clients scan the LAN and list running sessions automatically.</p>
                    </div>
                    <div class="feature-card">
                        <h3>Command</h3>
                        <p>A reliable, length-prefixed TCP channel with queued writes carries login, commands and request/response traffic on ports 50000-50128.</p>
                    </div>
                    <div class="feature-card">
                        <h3>Synchronize</h3>
                        <p>An unreliable UDP channel streams positions and inputs at 30-60 Hz with no head-of-line blocking — latest state always wins.</p>
                    </div>
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

            <section class="license-info">
                <h3>Built for AI Coding Assistants</h3>
                <p>The library is machine-readable. Point an AI assistant or coding agent at <a href="llms-full.txt">llms-full.txt</a> — the complete API reference in a single text file — and it can generate, edit and debug LAN multiplayer code for your engine. A shorter index lives at <a href="llms.txt">llms.txt</a>.</p>
            </section>
        </div>
    </main>

    <footer>
        <div class="container">
            <p>&copy; 2026 Nichita Levandovici. Released under MIT-0.</p>
            <p>
                <a href="about.php">About</a> |
                <a href="docs.php">Documentation</a> |
                <a href="faq.php">FAQ</a> |
                <a href="download.php">Download</a> |
                <a href="privacy.php">Privacy Policy</a> |
                <a href="terms.php">Terms and Conditions</a>
            </p>
        </div>
    </footer>
</body>
</html>
