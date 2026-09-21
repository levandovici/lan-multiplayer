# Lan Multiplayer (Michitai.Lan)

A LAN multiplayer networking library for game developers, available as a .NET Framework class library, as a drop-in Unity source package, and as a standalone C++20 library. This repository also contains the PHP website used to distribute it.

## Repository Layout

```
lan-multiplayer/
├── dotnet/            # .NET Framework 4.8 class library (michitai-lan.sln)
├── unity/             # Unity source variant (same API + mobile helpers)
├── cpp/               # C++20 port — standalone CMake static lib + demo (same wire protocol)
├── web/               # PHP/MySQL website for distributing the library
├── reorganize_cs.py   # Utility that splits C# files by namespace/type
└── LICENSE            # MIT No Attribution
```

## The Library

`Michitai.Lan` provides everything needed to host, discover, and join multiplayer sessions on a local network — optimized for 30–60 FPS state synchronization over Wi-Fi/LAN:

- **Automatic server discovery** — servers answer UDP broadcast probes (default ports `60000–60128`); clients scan the LAN to find running sessions.
- **Reliable TCP channel** — length-prefixed binary framing, `NoDelay`, `SocketAsyncEventArgs` I/O, and queued writes for commands, login, and request/response traffic (default ports `50000–50128`).
- **Unreliable UDP state channel** — `UDPChannel` carries per-frame state (positions, transforms, inputs) with no head-of-line blocking; bound to the same port as the TCP server so discovered endpoints just work.
- **GZip compression** — payloads ≥ 256 bytes are compressed transparently when it shrinks them (`Compressor.Enabled` / `Compressor.Threshold` to tune).
- **Command pattern** — fluent `Terminal`/`Command` API for batching named commands with arguments into a single request.
- **Game data models** — `ServerGameData`, `ClientGameData`, `PlayerGameData`, `Credentials`, `ServerInfo`, etc., with public/private views and JSON storage support.
- **Chat** — bounded, thread-safe `Chat`/`Letter` container.
- **Cross-platform** — `EPlatform` flags (`Windows`, `Linux`, `MacOS`, `Standalone`, `Android`, `IOS`, `Mobile`) select the right network code path per platform.

> **Breaking change:** the transport was rewritten from a `#end#<>#message#` string-delimiter protocol to a binary length-prefixed frame protocol. Old and new builds cannot talk to each other.

### Namespaces

| Namespace | Contents |
|---|---|
| `Michitai.Lan.Net` | `Message`, `AppMessage`, `IdentifiedMessage`, `LocatedMessage`, `TCPClient`, `TCPServer`, `TCPServerClient`, `UDPBroadcast`, `UDPChannel`, `Frame`/`FrameBuffer`, `Compressor`, `PortRange`, `Lan` (local IP helpers) |
| `Michitai.Lan.Net.Multiplayer` | `Multiplayer` (static facade), `Server`, `Client`, `BroadcastServer`, `BroadcastClient` |
| `Michitai.Lan.Net.Multiplayer.Commands` | `Command`, `Terminal` |
| `Michitai.Lan.Net.Multiplayer.Chat` | `Chat`, `Letter` |
| `Michitai.Lan.Net.Multiplayer.Data` | `ServerGameData`, `ClientGameData`, `ServerClientGameData`, `Credentials`, `ServerInfo`, `ServerInfoStack`, `LocatedServerInfo`, `LocatedServerInfoStack`, `MultiplayerGamesData` |
| `Michitai.Lan.Data` | `IJsonStorage` (dotnet) / `JsonStorage` (unity) |
| `Michitai.Lan.Debug` | `DebugConsole` |

### How It Works

```
Client                                  Server
  |  -- UDP broadcast "discover" -->       |   (BroadcastServer, ports 60000-60128)
  |  <-- ServerInfo (name, port) --        |
  |                                       |
  |  ------- TCP connect ----------->      |   (Server, ports 50000-50128)
  |  ------- Terminal commands ---->       |   reliable channel (frames, compressed)
  |  <------------- responses -----        |
  |                                       |
  |  ======= UDP state datagrams ==>       |   (UDPChannel, same port as TCP)
  |  <====== BroadcastState =======        |   unreliable, 30-60Hz, latest-wins
```

**Wire protocol.** Every TCP message is a frame: `[4-byte LE payload length][1-byte flags][UTF-8 payload]`. Flag `0x01` marks a GZip-compressed payload. The `FrameBuffer` decoder handles partial reads and multiple frames per read, and rejects frames announcing more than `Frame.MaxPayloadSize` (32 MB default). UDP state datagrams are `[1-byte flags][payload]`.

**Which channel for what:** use `Client.Request` / `Server.Response` (TCP) for anything that must arrive — commands, login, world data. Use `Client.SendState` / `Server.BroadcastState` (UDP) for transient per-frame values where a dropped packet is immediately superseded by the next one.

The static `Multiplayer` facade owns the active `Server`, `Client`, `BroadcastServer`, and `BroadcastClient` instances and exposes `StartServer`, `StartClient`, `StartBroadcastClient`, `Stop`, and lifecycle events (`OnServerStarted`, `OnClientStarted`, ...).

### Quick Start

Start a server and answer discovery probes:

```csharp
Multiplayer.Name = "My Game Server";
Multiplayer.IpAddress = Lan.LocalIPv4Addresses(EPlatform.Standalone)[0];

Multiplayer.StartServer(
    platform: EPlatform.Standalone,
    serverGameData: new ServerGameData(Guid.NewGuid().ToString()),
    processMessage: (LocatedMessage msg) =>
        new AppMessage(1, "my-game", /* serialized ServerInfo */ "..."),
    receiveRequestsDelayMilliseconds: 500);
```

Discover servers on the LAN:

```csharp
Multiplayer.StartBroadcastClient(
    platform: EPlatform.Standalone,
    request: new AppMessage(1, "my-game", "discover"),
    onReceiveResponse: (LocatedMessage response) => {
        // response.IPEndPoint + payload identify the server
    },
    receiveResponsesMilliseconds: 5000,
    repeatAfterMilliseconds: 5000);
```

Connect and send commands:

```csharp
Multiplayer.IpAddress = serverIp;
Multiplayer.Port = serverPort;
Multiplayer.StartClient();

Multiplayer.Client.OnResponse += (message) => { /* handle reply */ };

if (Multiplayer.Client.CanRequest)
{
    Multiplayer.Client.Request(new Message(
        /* serialized */ Terminal.New()
            .Next("login").Arg("user").Arg("pass")
            .Next("get-data")));
}
```

Send per-frame state over the unreliable channel (call each frame, ~30–60 Hz):

```csharp
// client -> server
Multiplayer.SendState(new Message(JsonUtility.ToJson(myTransform)));

// server -> all clients that have sent state
Multiplayer.OnState += (IPEndPoint from, Message msg) => { /* apply state */ };
Multiplayer.BroadcastState(new Message(worldSnapshot));

// server -> one client
Multiplayer.SendState(clientEndPoint, new Message(playerSnapshot));
```

Useful knobs and diagnostics:

- `Compressor.Enabled`, `Compressor.Threshold` (default 256 bytes) — compression tuning.
- `Frame.MaxPayloadSize` (default 32 MB) — corrupt-frame guard.
- `UDPChannel.SafeDatagramSize` (1472) — keep state datagrams under this to avoid IP fragmentation.
- `Client.PendingRequests`, `TCPClient.PendingWrites`, `TCPServer.PendingWrites` — backpressure indicators.
- `Client.Send(message)` — fire-and-forget TCP send that skips the request/response queue.

### dotnet/ vs unity/

The two trees share the same API and file layout. The Unity variant adds:

- `MobileBroadcastClient` / `MobileBroadcastServer` — UdpClient-based discovery helpers for Android/iOS.
- `Data/JsonStorage.cs` — Unity-friendly JSON storage implementation (in place of `IJsonStorage`).
- `UnityEngine` references for engine integration.

Use the `unity/` sources directly inside a Unity project's `Assets` folder; use `dotnet/michitai-lan.sln` to build the .NET Framework 4.8 assembly.

### cpp/

The `cpp/` tree is a full C++20 port of the library as a standalone CMake static
library (`Michitai::Lan`), wire-compatible with the C# builds (same frame format,
gzip flag, port ranges). It replaces `SocketAsyncEventArgs`/`Task` with
thread-per-connection I/O plus `std::future` async methods, and C# events with a
multicast `Event<>` type. Only external dependency: **miniz** (gzip codec —
reused from an existing `miniz` target if your engine provides one, otherwise
FetchContent'd at 3.1.2).

```bash
cmake -S cpp -B cpp/build -A x64
cmake --build cpp/build --config Debug
./cpp/build/Debug/lan-demo        # loopback smoke test
```

See `cpp/README.md` for porting notes, C++20 feature usage, and API examples.

## The Website (`web/`)

A PHP 7.4+/MySQL site that gates library downloads behind email verification:

- `index.php`, `about.php`, `docs.php`, `privacy.php`, `terms.php` — public pages.
- `download.php` → `send_code.php` → `verify.php`/`verify_code.php` → `downloads.php`/`serve_download.php` — the email-verified download flow for the .NET and Unity packages.
- `database.sql` — schema (`users`, `downloads`, `failed_attempts` tables).
- Config via `.env` (see `.env.example`); Composer dependencies: `vlucas/phpdotenv`, `phpmailer/phpmailer`.
- Includes rate limiting on code requests and download tracking. See `web/README.md` and `web/HOSTINGER_SETUP.md` for deployment notes.

## Tools

`reorganize_cs.py` — Python utility that parses C# files and splits them into one file per namespace/type, adding the standard project header and `using` block. Useful when refactoring the library's file layout.

```bash
python reorganize_cs.py
```

## Requirements

- **Library:** .NET Framework 4.8 (dotnet), Unity (unity), or C++20 + CMake ≥ 3.20 (cpp)
- **Website:** PHP 7.4+, MySQL 5.7+/MariaDB 10.2+, Composer
- **Tooling:** Python 3 for `reorganize_cs.py`

## License

Released under **MIT No Attribution** — see [LICENSE](https://github.com/levandovici/lan-multiplayer/blob/master/LICENSE).

Note: the distribution website (`web/`) reflects the same MIT-0 terms — downloaded packages are free to use without attribution or payment.

## Author

**Nichita Levandovici** — support@michitai.com — https://michitai.com
