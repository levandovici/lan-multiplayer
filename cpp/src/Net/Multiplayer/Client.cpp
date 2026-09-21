//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include <Michitai/Lan/Net/Multiplayer/Client.hpp>
#include <Michitai/Lan/Debugging/DebugConsole.hpp>


namespace Michitai::Lan::Net::Multiplayer
{
    Client::Client(std::optional<Data::ClientGameData> clientData,
                   std::optional<Michitai::Lan::Data::PlayerGameData> gameData,
                   const IPEndPoint& endPoint, int bufferSize)
        : _clientData(std::move(clientData))
        , _gameData(std::move(gameData))
        , _endPoint(endPoint)
    {
        _client = TCPClient::Create(endPoint, bufferSize);
    }

    void Client::Initialize()
    {
        std::weak_ptr<Client> weak = shared_from_this();

        _client->OnResponse += [weak](const Message& message)
        {
            if (Ptr self = weak.lock())
            {
                try
                {
                    self->OnResponse.Invoke(message);
                }
                catch (...)
                {
                }

                self->FlushPending();
            }
        };

        _client->OnStop += [weak]
        {
            if (Ptr self = weak.lock())
                self->OnDisconnected.Invoke();
        };
    }

    Client::~Client()
    {
        Stop();
    }

    void Client::Start()
    {
        _client->Start();

        try
        {
            _dataChannel = UDPChannel::Create(IPAddress::Any(), 0);

            std::weak_ptr<Client> weak = shared_from_this();

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

    void Client::Stop()
    {
        {
            std::lock_guard lock(_pendingMutex);
            while (!_pendingRequests.empty())
                _pendingRequests.pop();
        }

        if (_dataChannel)
        {
            _dataChannel->Stop();
            _dataChannel = nullptr;
        }

        if (_client)
            _client->Stop();
    }

    void Client::Request(const Message& message)
    {
        bool sendNow;

        {
            std::lock_guard lock(_pendingMutex);
            sendNow = _isResponsed.load();

            if (sendNow)
            {
                _isResponsed.store(false);
            }
            else
            {
                _pendingRequests.push(message);
            }
        }

        if (sendNow)
        {
            _client->Request(message);
        }
    }

    void Client::SendState(const Message& message)
    {
        if (_dataChannel)
            _dataChannel->Send(_endPoint, message);
    }

    void Client::SendState(const std::string& state)
    {
        if (_dataChannel)
            _dataChannel->Send(_endPoint, state);
    }

    void Client::FlushPending()
    {
        std::optional<Message> next;

        {
            std::lock_guard lock(_pendingMutex);
            if (!_pendingRequests.empty())
            {
                next = _pendingRequests.front();
                _pendingRequests.pop();
            }
            else
            {
                _isResponsed.store(true);
                return;
            }
        }

        _client->Request(*next);
    }
}
