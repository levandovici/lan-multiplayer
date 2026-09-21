//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <memory>
#include <mutex>
#include <optional>
#include <string>
#include <vector>

#include <Michitai/Lan/Guid.hpp>
#include <Michitai/Lan/Json.hpp>
#include <Michitai/Lan/Data/JsonStorage.hpp>
#include <Michitai/Lan/Data/PlayerData.hpp>
#include <Michitai/Lan/Net/EndPoint.hpp>


namespace Michitai::Lan::Net::Multiplayer::Data
{
    /// <summary>
    /// Represents authentication credentials with an ID and password.
    /// </summary>
    class Credentials
    {
    public:
        std::string ID;
        std::string Password;

        /// <summary>Creates a new instance of Credentials with randomly generated ID and password.</summary>
        static Credentials New()
        {
            return Credentials(Michitai::Lan::NewGuid(), Michitai::Lan::NewGuid());
        }

        Credentials() : ID("id"), Password("password") {}
        explicit Credentials(std::string id) : ID(std::move(id)), Password("") {}
        Credentials(std::string id, std::string password) : ID(std::move(id)), Password(std::move(password)) {}

        /// <summary>Gets a public view of the credentials (ID only).</summary>
        Credentials Public() const { return Credentials(ID); }

        std::string ToString() const
        {
            return "[CREDENTIALS][ID][" + ID + "][PASSWORD][" + Password + "]";
        }

        bool operator==(const Credentials& other) const
        {
            return ID == other.ID && Password == other.Password;
        }

        bool operator!=(const Credentials& other) const { return !(*this == other); }

        JsonValue ToJson() const
        {
            return JsonValue(JsonValue::Object{
                {"ID",       JsonValue(ID)},
                {"Password", JsonValue(Password)}
            });
        }

        static Credentials FromJson(const JsonValue& json)
        {
            return Credentials(json["ID"].AsString(), json["Password"].AsString());
        }
    };


    /// <summary>
    /// Represents information about a multiplayer server.
    /// </summary>
    class ServerInfo
    {
    public:
        int Port = 0;
        int ClientsCount = -1;
        std::string ServerID = "default";
        std::string Name = "default";

        ServerInfo() = default;

        ServerInfo(int port, std::string name, std::string serverID, int clientsCount)
            : Port(port), ClientsCount(clientsCount), ServerID(std::move(serverID)), Name(std::move(name))
        {
        }

        JsonValue ToJson() const
        {
            return JsonValue(JsonValue::Object{
                {"ClientsCount", JsonValue(ClientsCount)},
                {"Name",         JsonValue(Name)},
                {"Port",         JsonValue(Port)},
                {"ServerID",     JsonValue(ServerID)}
            });
        }

        static ServerInfo FromJson(const JsonValue& json)
        {
            ServerInfo info;
            info.Port         = static_cast<int>(json["Port"].AsInt());
            info.ClientsCount = static_cast<int>(json["ClientsCount"].AsInt());
            info.ServerID     = json["ServerID"].AsString();
            info.Name         = json["Name"].AsString();
            return info;
        }
    };


    /// <summary>
    /// Thread-safe stack for managing server information.
    /// </summary>
    class ServerInfoStack
    {
    public:
        ServerInfoStack() = default;

        /// <summary>Pushes a server info onto the stack. Thread-safe.</summary>
        void Push(const ServerInfo& serverInfo)
        {
            std::lock_guard lock(_mutex);
            _stack.push_back(serverInfo);
        }

        /// <summary>Pops a server info from the stack. Thread-safe.</summary>
        ServerInfo Pop()
        {
            std::lock_guard lock(_mutex);
            ServerInfo top = _stack.back();
            _stack.pop_back();
            return top;
        }

        /// <summary>Peeks at the top server info without removing it. Thread-safe.</summary>
        ServerInfo Peek() const
        {
            std::lock_guard lock(_mutex);
            return _stack.back();
        }

        /// <summary>Gets the number of server infos in the stack. Thread-safe.</summary>
        int Count() const
        {
            std::lock_guard lock(_mutex);
            return static_cast<int>(_stack.size());
        }

        /// <summary>Clears all server infos from the stack. Thread-safe.</summary>
        void Clear()
        {
            std::lock_guard lock(_mutex);
            _stack.clear();
        }

    private:
        std::vector<ServerInfo> _stack;
        mutable std::mutex _mutex;
    };


    /// <summary>
    /// Represents a discovered server with its information and network endpoint.
    /// </summary>
    class LocatedServerInfo
    {
    public:
        std::optional<Data::ServerInfo> ServerInfo;
        std::optional<Net::IPEndPoint> IPEndPoint;

        LocatedServerInfo() = default;

        LocatedServerInfo(Data::ServerInfo serverInfo, Net::IPEndPoint point)
            : ServerInfo(std::move(serverInfo)), IPEndPoint(std::move(point))
        {
        }
    };


    /// <summary>
    /// Thread-safe stack for managing discovered server information.
    /// </summary>
    class LocatedServerInfoStack
    {
    public:
        LocatedServerInfoStack() = default;

        void Push(const LocatedServerInfo& locatedServerInfo)
        {
            std::lock_guard lock(_mutex);
            _stack.push_back(locatedServerInfo);
        }

        LocatedServerInfo Pop()
        {
            std::lock_guard lock(_mutex);
            LocatedServerInfo top = _stack.back();
            _stack.pop_back();
            return top;
        }

        LocatedServerInfo Peek() const
        {
            std::lock_guard lock(_mutex);
            return _stack.back();
        }

        int Count() const
        {
            std::lock_guard lock(_mutex);
            return static_cast<int>(_stack.size());
        }

        void Clear()
        {
            std::lock_guard lock(_mutex);
            _stack.clear();
        }

    private:
        std::vector<LocatedServerInfo> _stack;
        mutable std::mutex _mutex;
    };


    /// <summary>
    /// Represents client game data including server ID and credentials.
    /// </summary>
    class ClientGameData
    {
    public:
        std::string Server_ID = "default";
        Data::Credentials Credentials{std::string("id"), std::string("password")};

        ClientGameData() = default;

        ClientGameData(std::string serverId, Data::Credentials credentials)
            : Server_ID(std::move(serverId)), Credentials(std::move(credentials))
        {
        }

        JsonValue ToJson() const
        {
            return JsonValue(JsonValue::Object{
                {"Credentials", Credentials.ToJson()},
                {"Server_ID",   JsonValue(Server_ID)}
            });
        }

        static ClientGameData FromJson(const JsonValue& json)
        {
            ClientGameData data;
            data.Server_ID   = json["Server_ID"].AsString();
            data.Credentials = Data::Credentials::FromJson(json["Credentials"]);
            return data;
        }
    };


    /// <summary>
    /// Represents client game data on the server side, including player data and credentials.
    /// </summary>
    class ServerClientGameData
    {
    public:
        Michitai::Lan::Data::PlayerGameData Data;
        Michitai::Lan::Net::Multiplayer::Data::Credentials Credentials{std::string("id"), std::string("password")};

        ServerClientGameData() = default;

        ServerClientGameData(Michitai::Lan::Data::PlayerGameData gameData,
                             Michitai::Lan::Net::Multiplayer::Data::Credentials credentials)
            : Data(std::move(gameData)), Credentials(std::move(credentials))
        {
        }

        /// <summary>Gets a public view of the client data (with public credentials only).</summary>
        ServerClientGameData Public() const
        {
            return ServerClientGameData(Data, Credentials.Public());
        }

        std::string ToString() const
        {
            return "[SERVER-CLIENT-GAME-DATA]" + Credentials.ToString() + Data.Json();
        }

        JsonValue ToJson() const
        {
            return JsonValue(JsonValue::Object{
                {"Credentials", Credentials.ToJson()},
                {"Data",        Data.ToJson()}
            });
        }

        static ServerClientGameData FromJson(const JsonValue& json)
        {
            ServerClientGameData data;
            data.Credentials = Michitai::Lan::Net::Multiplayer::Data::Credentials::FromJson(json["Credentials"]);
            data.Data        = Michitai::Lan::Data::PlayerGameData::FromJson(json["Data"]);
            return data;
        }
    };


    /// <summary>
    /// Represents server game data including server ID and connected client data.
    /// The client list is guarded internally like the C# original.
    /// Owned via std::shared_ptr so mutations are visible to the caller that
    /// passed it to Server/Multiplayer.
    /// </summary>
    class ServerGameData : public Michitai::Lan::Data::JsonStorage
    {
    public:
        using Ptr = std::shared_ptr<ServerGameData>;

        std::string ServerID = "default";

        ServerGameData() = default;
        explicit ServerGameData(std::string serverId) : ServerID(std::move(serverId)) {}
        ServerGameData(std::string serverId, std::vector<ServerClientGameData> clients)
            : ServerID(std::move(serverId)), _clients(std::move(clients))
        {
        }

        ServerGameData(const ServerGameData& other) : Michitai::Lan::Data::JsonStorage(other)
        {
            std::lock_guard lock(other._clientsMutex);
            ServerID = other.ServerID;
            _clients = other._clients;
        }

        ServerGameData& operator=(const ServerGameData& other)
        {
            if (this != &other)
            {
                Michitai::Lan::Data::JsonStorage::operator=(other);
                std::scoped_lock lock(_clientsMutex, other._clientsMutex);
                ServerID = other.ServerID;
                _clients = other._clients;
            }
            return *this;
        }

        /// <summary>Gets or sets the connected client data. Thread-safe snapshot.</summary>
        std::vector<ServerClientGameData> Clients() const
        {
            std::lock_guard lock(_clientsMutex);
            return _clients;
        }

        void SetClients(std::vector<ServerClientGameData> clients)
        {
            std::lock_guard lock(_clientsMutex);
            _clients = std::move(clients);
        }

        /// <summary>Gets a public view of the server data (with public client data).</summary>
        Ptr Public() const
        {
            std::vector<ServerClientGameData> clients;
            {
                std::lock_guard lock(_clientsMutex);
                clients.reserve(_clients.size());
                for (const ServerClientGameData& client : _clients)
                {
                    clients.push_back(client.Public());
                }
            }

            auto result = std::make_shared<ServerGameData>(ServerID, std::move(clients));
            result->SetJson(Json());
            return result;
        }

        /// <summary>Adds a player to the server data. Thread-safe.</summary>
        void AddPlayer(const ServerClientGameData& player)
        {
            std::lock_guard lock(_clientsMutex);
            _clients.push_back(player);
        }

        /// <summary>Deletes a player from the server data by credentials. Thread-safe.</summary>
        void DeletePlayer(const Credentials& player)
        {
            std::lock_guard lock(_clientsMutex);
            std::erase_if(_clients, [&player](const ServerClientGameData& client)
            {
                return client.Credentials == player;
            });
        }

        /// <summary>Checks if a player with the specified credentials exists. Thread-safe.</summary>
        bool Contains(const Credentials& player) const
        {
            std::lock_guard lock(_clientsMutex);
            for (const ServerClientGameData& client : _clients)
            {
                if (client.Credentials == player)
                    return true;
            }
            return false;
        }

        /// <summary>Tries to get the public player data for the specified credentials. Thread-safe.</summary>
        bool TryGetPublicPlayerData(const Credentials& player, ServerClientGameData& client) const
        {
            std::lock_guard lock(_clientsMutex);
            for (const ServerClientGameData& item : _clients)
            {
                if (item.Credentials == player)
                {
                    client = item.Public();
                    return true;
                }
            }
            return false;
        }

        /// <summary>Tries to get the private player data for the specified credentials. Thread-safe.</summary>
        bool TryGetPrivatePlayerData(const Credentials& player, ServerClientGameData& client) const
        {
            std::lock_guard lock(_clientsMutex);
            for (const ServerClientGameData& item : _clients)
            {
                if (item.Credentials == player)
                {
                    client = item;
                    return true;
                }
            }
            return false;
        }

        std::string ToString() const
        {
            return "[SERVER-GAME-DATA][SERVER-ID][" + ServerID + "][PLAYERS][" +
                   std::to_string(Clients().size()) + "]";
        }

        JsonValue ToJson() const
        {
            JsonValue::Array clients;
            for (const ServerClientGameData& client : Clients())
            {
                clients.push_back(client.ToJson());
            }

            return JsonValue(JsonValue::Object{
                {"Clients",  JsonValue(std::move(clients))},
                {"Json",     JsonValue(Json())},
                {"ServerID", JsonValue(ServerID)}
            });
        }

        static ServerGameData FromJson(const JsonValue& json)
        {
            ServerGameData data;
            data.ServerID = json["ServerID"].AsString();
            data.SetJson(json["Json"].AsString());

            std::vector<ServerClientGameData> clients;
            for (const JsonValue& item : json["Clients"].AsArray())
            {
                clients.push_back(ServerClientGameData::FromJson(item));
            }
            data.SetClients(std::move(clients));

            return data;
        }

    private:
        std::vector<ServerClientGameData> _clients;
        mutable std::mutex _clientsMutex;
    };

    using ServerGameDataPtr = ServerGameData::Ptr;


    /// <summary>
    /// Container for multiplayer game data including servers and clients.
    /// </summary>
    class MultiplayerGamesData
    {
    public:
        std::vector<ServerGameData> ServersData;
        std::vector<ClientGameData> ClientsData;

        MultiplayerGamesData() = default;

        JsonValue ToJson() const
        {
            JsonValue::Array servers;
            for (const ServerGameData& server : ServersData)
            {
                servers.push_back(server.ToJson());
            }

            JsonValue::Array clients;
            for (const ClientGameData& client : ClientsData)
            {
                clients.push_back(client.ToJson());
            }

            return JsonValue(JsonValue::Object{
                {"ClientsData", JsonValue(std::move(clients))},
                {"ServersData", JsonValue(std::move(servers))}
            });
        }

        static MultiplayerGamesData FromJson(const JsonValue& json)
        {
            MultiplayerGamesData data;
            for (const JsonValue& item : json["ServersData"].AsArray())
            {
                data.ServersData.push_back(ServerGameData::FromJson(item));
            }
            for (const JsonValue& item : json["ClientsData"].AsArray())
            {
                data.ClientsData.push_back(ClientGameData::FromJson(item));
            }
            return data;
        }
    };
}
