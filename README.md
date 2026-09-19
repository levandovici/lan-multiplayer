# Lan Multiplayer (Michitai.Lan)

A LAN multiplayer networking library for game developers, available as a .NET Framework class library and as a drop-in Unity source package. This repository also contains the PHP website used to distribute it.

## Repository Layout

```
lan-multiplayer/
├── dotnet/            # .NET Framework 4.8 class library (michitai-lan.sln)
├── unity/             # Unity source variant (same API + mobile helpers)
├── web/               # PHP/MySQL website for distributing the library
├── reorganize_cs.py   # Utility that splits C# files by namespace/type
└── LICENSE            # MIT No Attribution
```

## The Library

`Michitai.Lan` provides everything needed to host, discover, and join multiplayer sessions on a local network:

- **Automatic server discovery** — servers answer UDP broadcast probes (default ports `60000–60128`); clients scan the LAN to find running sessions.
- **TCP client/server channel** — reliable request/response messaging once connected (default ports `50000–50128`, picked from a configurable `PortRange`).
- **Command pattern** — fluent `Terminal`/`Command` API for batching named commands with arguments into a single request.
- **Game data models** — `ServerGameData`, `ClientGameData`, `PlayerGameData`, `Credentials`, `ServerInfo`, etc., with public/private views and JSON storage support.
- **Chat** — bounded, thread-safe `Chat`/`Letter` container.
- **Cross-platform** — `EPlatform` flags (`Windows`, `Linux`, `MacOS`, `Standalone`, `Android`, `IOS`, `Mobile`) select the right network code path per platform.

### Namespaces

| Namespace | Contents |
|---|---|
| `Michitai.Lan.Net` | `Message`, `AppMessage`, `IdentifiedMessage`, `LocatedMessage`, `TCPClient`, `TCPServer`, `TCPServerClient`, `UDPBroadcast`, `PortRange`, `Lan` (local IP helpers) |
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
  |  ------- Terminal commands ---->       |
  |  <------------- responses -----        |
```

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

### dotnet/ vs unity/

The two trees share the same API and file layout. The Unity variant adds:

- `MobileBroadcastClient` / `MobileBroadcastServer` — UdpClient-based discovery helpers for Android/iOS.
- `Data/JsonStorage.cs` — Unity-friendly JSON storage implementation (in place of `IJsonStorage`).
- `UnityEngine` references for engine integration.

Use the `unity/` sources directly inside a Unity project's `Assets` folder; use `dotnet/michitai-lan.sln` to build the .NET Framework 4.8 assembly.

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

- **Library:** .NET Framework 4.8 (dotnet) or Unity (unity)
- **Website:** PHP 7.4+, MySQL 5.7+/MariaDB 10.2+, Composer
- **Tooling:** Python 3 for `reorganize_cs.py`

## License

Released under **MIT No Attribution** — see [LICENSE](LICENSE).

Note: the distribution website (`web/`) displays its own licensing terms for downloaded packages (free with attribution to lan.michitai.com, or a paid license without attribution).

## Author

**Nichita Levandovici** — support@michitai.com — https://michitai.com
