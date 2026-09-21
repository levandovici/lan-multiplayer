//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include <Michitai/Lan/Net/Multiplayer/Broadcast.hpp>


namespace Michitai::Lan::Net::Multiplayer
{
    void BroadcastServer::Broadcast(const ProcessMessageDelegate& process)
    {
        LocatedMessage message = _socket.Receive();

        if (!message.IPEndPoint.has_value())
            return;

        _socket.Send(*message.IPEndPoint, process(message));
    }

    bool BroadcastServer::Broadcast(const ProcessMessageDelegate& process, int timeoutMilliseconds)
    {
        LocatedMessage message = _socket.Receive(timeoutMilliseconds);

        if (!message.IPEndPoint.has_value())
            return false;

        _socket.Send(*message.IPEndPoint, process(message));
        return true;
    }


    void BroadcastClient::CollectResponses(const OnReceiveResponseDelegate& onReceiveResponse,
                                           int receiveResponsesMilliseconds,
                                           const std::stop_token& stopToken)
    {
        const auto deadline = std::chrono::steady_clock::now() +
                              std::chrono::milliseconds(receiveResponsesMilliseconds);

        while (!stopToken.stop_requested())
        {
            const auto remaining = std::chrono::duration_cast<std::chrono::milliseconds>(
                deadline - std::chrono::steady_clock::now()).count();

            if (remaining <= 0)
                break;

            LocatedMessage response = _socket.Receive(static_cast<int>(remaining > 50 ? 50 : remaining));

            if (!response.IPEndPoint.has_value())
                continue;

            if (onReceiveResponse)
                onReceiveResponse(response);
        }
    }

    void BroadcastClient::CollectResponses(const OnReceiveResponseDelegate& onReceiveResponse,
                                           int receiveResponsesMilliseconds)
    {
        CollectResponses(onReceiveResponse, receiveResponsesMilliseconds, std::stop_token{});
    }
}
