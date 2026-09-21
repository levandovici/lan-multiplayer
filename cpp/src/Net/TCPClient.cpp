//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include <Michitai/Lan/Net/TCPClient.hpp>
#include <Michitai/Lan/Debugging/DebugConsole.hpp>
#include "SocketInternal.hpp"


namespace Michitai::Lan::Net
{
    TCPClient::TCPClient(const IPEndPoint& endPoint, int bufferSize)
        : _endPoint(endPoint)
        , _bufferSize(bufferSize)
        , _frameBuffer(bufferSize * 2)
    {
        _socket = std::make_unique<Detail::Socket>(Detail::Socket::TcpV4());

        // Disable Nagle's algorithm — required for 30-60Hz small-message latency.
        _socket->SetNoDelay(true);
        _socket->SetSendBufferSize(65536);
        _socket->SetReceiveBufferSize(65536);
    }

    TCPClient::~TCPClient()
    {
        Stop();
        JoinThreads();
    }

    int TCPClient::PendingWrites() const
    {
        std::lock_guard lock(_sendMutex);
        return static_cast<int>(_sendQueue.size());
    }

    void TCPClient::Start()
    {
        _socket->Connect(_endPoint);

        Ptr self = shared_from_this();

        _receiveThread = std::thread([self] { self->ReceiveLoop(); });
        _writeThread = std::thread([self] { self->WriteLoop(); });
    }

    void TCPClient::Stop()
    {
        {
            std::lock_guard lock(_stateMutex);
            if (_closed.load())
                return;
            _closed.store(true);
        }

        OnResponse = nullptr;

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

        OnStop.Invoke();
    }

    void TCPClient::Request(const Message& message)
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

    void TCPClient::ReceiveLoop()
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
                        OnResponse.Invoke(message);
                    }
                    catch (const std::exception& ex)
                    {
                        Debug::DebugConsole::LogError("[Michitai.Lan][C-RESPONSE-HANDLER-ERROR][" +
                                                    std::string(ex.what()) + "]");
                    }
                }
            }
            catch (const std::exception& ex)
            {
                Debug::DebugConsole::LogError("[Michitai.Lan][C-FRAME-ERROR][" +
                                            std::string(typeid(ex).name()) + ":" + ex.what() + "]");
                Stop();
                return;
            }
        }
    }

    void TCPClient::WriteLoop()
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

    void TCPClient::JoinThreads()
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
}
