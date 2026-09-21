//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include <Michitai/Lan/Net/TCPServer.hpp>
#include <Michitai/Lan/Debugging/DebugConsole.hpp>
#include <Michitai/Lan/Guid.hpp>
#include "SocketInternal.hpp"


namespace Michitai::Lan::Net
{
    //===========================================================================================================================================================
    //  TCPServerClient
    //===========================================================================================================================================================

    TCPServerClient::TCPServerClient(std::unique_ptr<Detail::Socket> socket, RequestHandler onRequest,
                                     StopHandler onStop, int bufferSize)
        : _id(NewGuid())
        , _bufferSize(bufferSize)
        , _socket(std::move(socket))
        , _frameBuffer(bufferSize * 2)
        , _onRequest(std::move(onRequest))
        , _onStop(std::move(onStop))
    {
        // Disable Nagle's algorithm — required for 30-60Hz small-message latency.
        _socket->SetNoDelay(true);
        _socket->SetSendBufferSize(65536);
        _socket->SetReceiveBufferSize(65536);
    }

    TCPServerClient::~TCPServerClient()
    {
        Stop();
        JoinThreads();
    }

    int TCPServerClient::PendingWrites() const
    {
        std::lock_guard lock(_sendMutex);
        return static_cast<int>(_sendQueue.size());
    }

    void TCPServerClient::Start()
    {
        Ptr self = shared_from_this();

        _receiveThread = std::thread([self] { self->ReceiveLoop(); });
        _writeThread = std::thread([self] { self->WriteLoop(); });
    }

    void TCPServerClient::Stop()
    {
        {
            std::lock_guard lock(_stateMutex);
            if (_closed.load())
                return;
            _closed.store(true);
        }

        _onRequest = nullptr;

        {
            std::lock_guard lock(_sendMutex);
            while (!_sendQueue.empty())
                _sendQueue.pop();
        }
        _sendCv.notify_all();

        if (_socket)
        {
            _socket->Shutdown();
            _socket->Close();
        }

        if (_onStop)
            _onStop(_id);
    }

    void TCPServerClient::Response(const Message& message)
    {
        if (IsClosed())
            return;

        std::vector<std::uint8_t> frame = Frame::Pack(message.GetMessage());

        {
            std::lock_guard lock(_sendMutex);
            _sendQueue.push(std::move(frame));
        }
        _sendCv.notify_one();
    }

    void TCPServerClient::ReceiveLoop()
    {
        std::vector<std::uint8_t> buffer(static_cast<std::size_t>(_bufferSize));

        while (!IsClosed())
        {
            const int received = _socket->Receive(buffer.data(), static_cast<int>(buffer.size()));

            if (received <= 0)
            {
                Stop();
                return;
            }

            try
            {
                _frameBuffer.Write(buffer.data(), 0, received);

                std::vector<std::uint8_t> payload;
                std::uint8_t flags;

                while (_frameBuffer.TryRead(payload, flags))
                {
                    Message message(Frame::Unpack(payload, flags));

                    try
                    {
                        if (_onRequest)
                            _onRequest(IdentifiedMessage(message, _id));
                    }
                    catch (const std::exception& ex)
                    {
                        Debug::DebugConsole::LogError("[Michitai.Lan][S-REQUEST-HANDLER-ERROR][" +
                                                    std::string(ex.what()) + "]");
                    }
                }
            }
            catch (const std::exception& ex)
            {
                Debug::DebugConsole::LogError("[Michitai.Lan][S-FRAME-ERROR][" +
                                            std::string(typeid(ex).name()) + ":" + ex.what() + "]");
                Stop();
                return;
            }
        }
    }

    void TCPServerClient::WriteLoop()
    {
        std::unique_lock lock(_sendMutex);

        while (true)
        {
            _sendCv.wait(lock, [&] { return _closed.load() || !_sendQueue.empty(); });

            while (!_sendQueue.empty())
            {
                std::vector<std::uint8_t> frame = std::move(_sendQueue.front());
                _sendQueue.pop();

                lock.unlock();

                const bool sent = _socket->SendAll(frame.data(), static_cast<int>(frame.size()));

                lock.lock();

                if (!sent || _closed.load())
                {
                    lock.unlock();
                    Stop();
                    return;
                }
            }

            if (_closed.load())
                return;
        }
    }

    void TCPServerClient::JoinThreads()
    {
        const std::thread::id current = std::this_thread::get_id();

        if (_receiveThread.joinable())
        {
            if (_receiveThread.get_id() == current)
                _receiveThread.detach();
            else
                _receiveThread.join();
        }

        if (_writeThread.joinable())
        {
            if (_writeThread.get_id() == current)
                _writeThread.detach();
            else
                _writeThread.join();
        }
    }


    //===========================================================================================================================================================
    //  TCPServer
    //===========================================================================================================================================================

    TCPServer::TCPServer(const IPEndPoint& endPoint, int bufferSize)
        : _endPoint(endPoint)
        , _bufferSize(bufferSize)
    {
        _listener = std::make_unique<Detail::Socket>(Detail::Socket::TcpV4());
        _listener->SetReuseAddress(true);

        if (!_listener->Bind(_endPoint))
            Detail::ThrowSocketError("bind");

        _listener->Listen();
    }

    TCPServer::~TCPServer()
    {
        Stop();
        JoinThreads();
    }

    int TCPServer::PendingWrites() const
    {
        std::lock_guard lock(_clientsMutex);

        int sum = 0;
        for (const TCPServerClient::Ptr& client : _clients)
        {
            sum += client->PendingWrites();
        }
        return sum;
    }

    void TCPServer::Start()
    {
        // Reflect the actual bound endpoint (matters when port 0 was requested).
        try
        {
            if (std::optional<IPEndPoint> bound = _listener->LocalEndPoint())
                _endPoint = *bound;
        }
        catch (...)
        {
        }

        Ptr self = shared_from_this();
        _acceptThread = std::thread([self] { self->AcceptLoop(); });
    }

    void TCPServer::Stop()
    {
        if (_closed.exchange(true))
            return;

        if (_listener)
        {
            _listener->Shutdown();
            _listener->Close();
        }

        {
            std::lock_guard lock(_clientsMutex);
            for (const TCPServerClient::Ptr& client : _clients)
            {
                client->Stop();
            }
        }

        OnStop.Invoke();
    }

    void TCPServer::Response(const IdentifiedMessage& identifiedMessage)
    {
        {
            std::lock_guard lock(_clientsMutex);
            for (const TCPServerClient::Ptr& client : _clients)
            {
                if (client->ID() == identifiedMessage.ID)
                {
                    client->Response(identifiedMessage.Message);
                    return;
                }
            }
        }

        Debug::DebugConsole::LogWarning("[TCP-Server] Response dropped: no client with ID '" +
                                      identifiedMessage.ID + "'.");
    }

    void TCPServer::Disconnect(const std::string& id)
    {
        {
            std::lock_guard lock(_clientsMutex);
            for (const TCPServerClient::Ptr& client : _clients)
            {
                if (client->ID() == id)
                {
                    client->Stop();
                    break;
                }
            }
        }

        DeleteClient(id);
    }

    void TCPServer::AcceptLoop()
    {
        while (!IsClosed())
        {
            IPEndPoint remote;
            Detail::Socket socket = _listener->Accept(&remote);

            if (!socket.Valid())
            {
                if (IsClosed())
                    return;

                Debug::DebugConsole::LogError("[TCP-Server] Accept Error!");
                continue;
            }

            try
            {
                std::weak_ptr<TCPServer> weak = shared_from_this();

                TCPServerClient::Ptr client = TCPServerClient::Ptr(new TCPServerClient(
                    std::make_unique<Detail::Socket>(std::move(socket)),
                    [weak](const IdentifiedMessage& message)
                    {
                        if (Ptr server = weak.lock())
                            server->OnRequest.Invoke(message);
                    },
                    [weak](const std::string& id)
                    {
                        if (Ptr server = weak.lock())
                            server->Disconnect(id);
                    },
                    _bufferSize));

                // Register BEFORE starting reads — a read completing during
                // construction could raise OnRequest before the client is routable,
                // and its response would be silently dropped.
                AddClient(client);

                client->Start();
            }
            catch (...)
            {
                Debug::DebugConsole::LogError("[TCP-Server] Accept Error!");
            }
        }
    }

    void TCPServer::AddClient(const TCPServerClient::Ptr& client)
    {
        {
            std::lock_guard lock(_clientsMutex);
            _clients.push_back(client);
        }

        OnClientConnected.Invoke(client->ID());
    }

    void TCPServer::DeleteClient(const std::string& id)
    {
        bool removed = false;

        {
            std::lock_guard lock(_clientsMutex);
            for (auto it = _clients.begin(); it != _clients.end(); ++it)
            {
                if ((*it)->ID() == id)
                {
                    _clients.erase(it);
                    removed = true;
                    break;
                }
            }
        }

        if (removed)
            OnClientDisconnected.Invoke(id);
    }

    void TCPServer::JoinThreads()
    {
        if (_acceptThread.joinable())
        {
            if (_acceptThread.get_id() == std::this_thread::get_id())
                _acceptThread.detach();
            else
                _acceptThread.join();
        }
    }
}
