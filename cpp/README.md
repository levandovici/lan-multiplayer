# Michitai.Lan — C++ Port

Standalone C++20 port of the `Michitai.Lan` LAN multiplayer library (originally
C#/.NET in `dotnet/` and `unity/`). Same namespaces, same API shape, same wire
protocol — a C++ client can talk to a C# server and vice versa.

## Layout

```
cpp/
├── CMakeLists.txt          # standalone project (or add_subdirectory target)
├── include/Michitai/Lan/   # public headers — #include <Michitai/Lan.hpp> pulls everything
│   ├── Data/               # JsonStorage, PlayerGameData/PlayerCharacterData/PlayerWorldData
│   ├── Debugging/          # DebugConsole
│   └── Net/                # EndPoint, Messages, Frame, Compression, PortRange, Lan,
│       └── Multiplayer/    #   TCPClient, TCPServer, UDPChannel, UDPBroadcast
│           └── ...         #       Server, Client, BroadcastServer/Client, Multiplayer,
│                           #       Commands, Chat, Data
├── src/                    # library sources (src/Net/SocketInternal.* is private)
├── apps/demo.cpp           # loopback smoke test — exits non-zero on failure
└── build/                  # generated (git-ignored)
```

## Building

Requires a C++20 compiler (MSVC 19.3x / Clang 15+ / GCC 11+) and CMake ≥ 3.20.

```bash
cmake -S cpp -B cpp/build -G "Visual Studio 18 2026" -A x64   # or Ninja, etc.
cmake --build cpp/build --config Debug
./cpp/build/Debug/lan-demo                                  # smoke test
```

Options:

- `-DMICHITAI_LAN_BUILD_EXAMPLES=OFF` — library only.
- `-DMICHITAI_LAN_FETCH_MINIZ=OFF` — don't FetchContent miniz; provide a `miniz`
  or `miniz::miniz` target/package yourself (e.g. your engine already ships
  miniz 3.1.2 — define the target before `add_subdirectory(cpp)` and it is
  picked up automatically).

### Verifying C# ↔ C++ interop

`lan-demo` doubles as a wire-protocol peer, and `tests/interop.cs` is a tiny
harness built against the real .NET `michitai-lan.dll`:

```bash
# Build the .NET library, then the harness:
msbuild dotnet/michitai-lan.csproj -p:Configuration=Release
csc -nologo -reference:dotnet\bin\Release\michitai-lan.dll -out:cpp\build\interop.exe cpp\tests\interop.cs
copy dotnet\bin\Release\michitai-lan.dll cpp\build\

# C# client -> C++ server
./cpp/build/Debug/lan-demo.exe server 51234 15 &   # wait for "READY"
./cpp/build/interop.exe client 51234             # -> echo:ping-from-csharp / state-echo:...

# C++ client -> C# server
./cpp/build/interop.exe server 51235 15 &
./cpp/build/Debug/lan-demo.exe client 127.0.0.1 51235
```

Both directions verified: framed TCP request/response and flagged UDP state
datagrams cross the C#/C++ boundary identically (gzip flag `0x01`, LE32 length
prefix, UTF-8 payloads).

Used from a parent project:

```cmake
add_subdirectory(lan-multiplayer/cpp)
target_link_libraries(my-game PRIVATE Michitai::Lan)
```

Links `ws2_32` + `iphlpapi` on Windows, `pthread` on Linux.

## Porting notes (C# → C++)

| C# | C++ |
|---|---|
| `event Action<...>` | `Event<...>` — `+=` subscribe, `= nullptr` clears, `Invoke()` dispatches on a snapshot |
| `SocketAsyncEventArgs`/`Task` I/O | receive/write threads per connection; `*Async` methods return `std::future` |
| `CancellationTokenSource` | `std::stop_source`/`std::stop_token` |
| `Guid.NewGuid()` | `Michitai::Lan::NewGuid()` (UUID v4) |
| `IPAddress`/`IPEndPoint` | `Net::IPAddress`/`Net::IPEndPoint` (IPv4 only) |
| `DataContractJsonSerializer` | `JsonValue` + `ToJson()`/`FromJson()` per type |
| `GZipStream` | `Compressor` — miniz raw deflate in an RFC 1952 gzip container (wire-identical) |
| reference types | `std::shared_ptr` handles (`Server::Create`, `Client::Create`, `UDPChannel::Create`, ...); upward callbacks hold `weak_ptr` |

Semantics preserved: 5-byte TCP frame `[LE32 length][flags][payload]`; flag `0x01`
= gzip; compression only when it shrinks the payload (`Compressor::Threshold` 256,
`Compressor::Enabled`); `Frame::MaxPayloadSize` 32 MB; UDP datagrams
`[flags][payload]`, `MaxDatagramSize` 65507 / `SafeDatagramSize` 1472; one
in-flight TCP request per client with a pending queue (`Client::PendingRequests`);
facade port ranges `ServerPortRange` 50000–50128, `BroadcastPortRange` 60000–60128.

### Namespace gotcha

`Multiplayer` and `Data` exist as both namespace names and class/namespace
members, so `using namespace Michitai::Lan::Net::Multiplayer` together with the
parent namespaces creates ambiguous lookups. Prefer aliases:

```cpp
namespace Mp = Michitai::Lan::Net::Multiplayer;
Mp::Multiplayer::StartServer(...);
Mp::Data::Credentials credentials;
```

### Windows header gotcha

`<windows.h>` defines `GetMessage` as a macro — include it before
`<Michitai/Lan.hpp>` is fine (the library `#undef`s it internally), but if your
own TU includes windows headers *after* ours and calls `Message::GetMessage()`,
add `#undef GetMessage` or define `NOUSER` before `<windows.h>`.

## C++20 features in use

Required (the library does not compile as C++17):

- `std::stop_source` / `std::stop_token` — cooperative cancellation of the
  broadcast server/client loops (`Multiplayer`, `BroadcastClient::CollectResponses`).
- `std::erase_if` — member removal on `std::vector` (`ServerGameData::DeletePlayer`,
  `Server::ServerClients::LogOut`).
- Defaulted `operator==` (`= default`) — `IPAddress`, `IPEndPoint`.
- `static inline` members on the `Multiplayer` facade (C++17, relied on heavily).

Not used (kept portable/simple): coroutines, `std::jthread`, ranges, concepts,
`<=>`, `std::span`, `std::format`, `std::string::starts_with`.

Everything else is C++17-and-earlier: `std::optional`, `std::variant`
(`JsonValue`), `std::atomic`, `std::shared_ptr`/`weak_ptr`, `std::scoped_lock`
(C++17), `std::async`/`std::future`, `thread_local`, `std::mt19937`.

## Quick start (facade)

```cpp
#include <Michitai/Lan.hpp>
namespace Mp = Michitai::Lan::Net::Multiplayer;
namespace Net = Michitai::Lan::Net;

// server
Mp::Multiplayer::IpAddress = Net::IPAddress::Any();
Mp::Multiplayer::StartServer(
    Michitai::Lan::EPlatform::Standalone,
    std::make_shared<Mp::Data::ServerGameData>(Michitai::Lan::NewGuid()),
    [](const Net::LocatedMessage& probe) {
        return Net::AppMessage(1, "my-game", "{\"Name\":\"My Server\"}");
    });

// client (same process for a loopback test)
Mp::Multiplayer::IpAddress = Net::IPAddress::Loopback();
Mp::Multiplayer::StartClient();

Mp::Multiplayer::Server->OnRequest += [](const Net::IdentifiedMessage& m) {
    Mp::Multiplayer::Server->Response(m);   // echo
};
Mp::Multiplayer::Client->OnResponse += [](const Net::Message& m) { /* ... */ };
Mp::Multiplayer::Client->Request(Net::Message("ping"));

// unreliable per-frame state (UDP)
Mp::Multiplayer::OnState += [](const Net::IPEndPoint& from, const Net::Message& m) { /* ... */ };
Mp::Multiplayer::SendState(Net::Message("pos:1,2,3"));        // client -> server
Mp::Multiplayer::BroadcastState(Net::Message("snapshot"));    // server -> all clients

Mp::Multiplayer::Stop();
```
