//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include <Michitai/Lan/Net/Multiplayer/Server.hpp>
#include <Michitai/Lan/Debugging/DebugConsole.hpp>


namespace Michitai::Lan::Net::Multiplayer
{
    Server::Server(std::string name, Data::ServerGameDataPtr serverGameData,
                   const IPEndPoint& endPoint, int bufferSize)
        : _name(std::move(name))
        , _serverData(std::move(serverGameData))
    {
        _server = TCPServer::Create(endPoint, bufferSize);
    }

    Server::Server(std::string name, Data::ServerGameDataPtr serverGameData,
                   const IPAddress& ip, const PortRange& range, int bufferSize)
        : _name(std::move(name))
        , _serverData(std::move(serverGameData))
    {
        PortRange::Store store = range.RangeStore();

        while (true)
        {
            try
            {
                _server = TCPServer::Create(ip, store.RandomPort(), bufferSize);
                break;
            }
            catch (const std::out_of_range&)
            {
                throw std::runtime_error("Server: no free port in range");
            }
            catch (const std::exception& e)
            {
                Debug::DebugConsole::LogError(e.what());
            }
        }
    }

    Server::Ptr Server::Create(const std::string& name, const Data::ServerGameDataPtr& serverGameData,
                               const IPAddress& ip, const PortRange& range, int bufferSize)
    {
        Ptr server = Ptr(new Server(name, serverGameData, ip, range, bufferSize));
        server->Initialize();
        return server;
    }

    void Server::Initialize()
    {
        std::weak_ptr<Server> weak = shared_from_this();

        _server->OnClientConnected += [weak](const std::string& id)
        {
            if (Ptr self = weak.lock())
                self->OnClientConnected.Invoke(id);
        };

        _server->OnClientDisconnected += [weak](const std::string& id)
        {
            if (Ptr self = weak.lock())
            {
                self->Disconnect(id);
                self->OnClientDisconnected.Invoke(id);
            }
        };

        _server->OnRequest += [weak](const IdentifiedMessage& message)
        {
            if (Ptr self = weak.lock())
                self->OnRequest.Invoke(message);
        };

        _server->OnStop += [weak]
        {
            if (Ptr self = weak.lock())
                self->OnDisconnected.Invoke();
        };
    }

    Server::~Server()
    {
        Stop();
    }

    const IPEndPoint& Server::IpEndPoint() const
    {
        return _server->IpEndPoint();
    }

    void Server::Start()
    {
        _server->Start();

        try
        {
            _dataChannel = UDPChannel::Create(_server->IpEndPoint());

            std::weak_ptr<Server> weak = shared_from_this();

            _dataChannel->OnReceive += [weak](const IPEndPoint& point, const Message& message)
            {
                if (Ptr self = weak.lock())
                {
                    try
                    {
                        self->OnState.Invoke(point, message);
                    }
                    catch (const std::exception& e)
                    {
                        Debug::DebugConsole::LogError("[Michitai.Lan][STATE-HANDLER-ERROR][" +
                                                    std::string(e.what()) + "]");
                    }
                }
            };

            _dataChannel->Start();
        }
        catch (const std::exception& e)
        {
            Debug::DebugConsole::LogError("[Michitai.Lan][DATA-CHANNEL-START-ERROR][" +
                                        std::string(e.what()) + "]");
            _dataChannel = nullptr;
        }
    }

    void Server::Stop()
    {
        if (_dataChannel)
        {
            _dataChannel->Stop();
            _dataChannel = nullptr;
        }

        if (_server)
            _server->Stop();
    }

    void Server::SendState(const IPEndPoint& target, const Message& message)
    {
        if (_dataChannel)
            _dataChannel->Send(target, message);
    }

    void Server::BroadcastState(const Message& message)
    {
        if (_dataChannel)
            _dataChannel->Broadcast(message);
    }

    void Server::Response(const IdentifiedMessage& identifiedMessage)
    {
        _server->Response(identifiedMessage);
    }

    void Server::LogInPlayer(const std::string& id, const Data::Credentials& credentials)
    {
        _clients.LogIn(ServerClient(id, credentials));
    }

    void Server::Disconnect(const std::string& id)
    {
        ServerClient client("", Data::Credentials());

        if (_clients.TryGetPlayer(id, client))
        {
            _serverData->DeletePlayer(client.Credentials);
        }

        _clients.LogOut(id);

        _server->Disconnect(id);
    }

    Data::Credentials Server::RegisterNewPlayer(const Michitai::Lan::Data::PlayerGameData& data)
    {
        Data::Credentials credentials = Data::Credentials::New();

        _serverData->AddPlayer(Data::ServerClientGameData(data, credentials));

        return credentials;
    }

    bool Server::Contains(const Data::Credentials& player) const
    {
        return _serverData->Contains(player);
    }

    bool Server::TryGetLoggedInPlayerPublicData(const std::string& id,
                                              Data::ServerClientGameData& serverClientGameData)
    {
        ServerClient client("", Data::Credentials());

        if (_clients.TryGetPlayer(id, client))
        {
            return _serverData->TryGetPublicPlayerData(client.Credentials, serverClientGameData);
        }

        return false;
    }

    bool Server::TryGetLoggedInPlayerPrivateData(const std::string& id,
                                               Data::ServerClientGameData& serverClientGameData)
    {
        ServerClient client("", Data::Credentials());

        if (_clients.TryGetPlayer(id, client))
        {
            return _serverData->TryGetPrivatePlayerData(client.Credentials, serverClientGameData);
        }

        return false;
    }
}
