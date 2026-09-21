//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <optional>
#include <string>

#include <Michitai/Lan/Json.hpp>
#include <Michitai/Lan/Net/EndPoint.hpp>


namespace Michitai::Lan::Net
{
    /// <summary>
    /// Represents a simple string message.
    /// </summary>
    class Message
    {
    public:
        explicit Message(std::string message = {}) : _message(std::move(message)) {}

        /// <summary>Gets the message content.</summary>
        const std::string& GetMessage() const { return _message; }

    private:
        std::string _message;
    };


    /// <summary>
    /// Represents an application message with version, name, and content.
    /// JSON member names match the C# DataContractJsonSerializer output for wire compatibility.
    /// </summary>
    class AppMessage
    {
    public:
        int Version = 0;
        std::string Name;
        std::string Message;

        AppMessage() = default;

        AppMessage(int version, std::string name, std::string message)
            : Version(version), Name(std::move(name)), Message(std::move(message))
        {
        }

        JsonValue ToJson() const
        {
            return JsonValue(JsonValue::Object{
                {"Message", JsonValue(Message)},
                {"Name",    JsonValue(Name)},
                {"Version", JsonValue(Version)}
            });
        }

        static AppMessage FromJson(const JsonValue& json)
        {
            AppMessage message;
            message.Version = static_cast<int>(json["Version"].AsInt());
            message.Name    = json["Name"].AsString();
            message.Message = json["Message"].AsString();
            return message;
        }
    };


    /// <summary>
    /// Represents a message with an associated identifier.
    /// </summary>
    class IdentifiedMessage
    {
    public:
        std::string ID;
        Net::Message Message;

        IdentifiedMessage() = default;

        IdentifiedMessage(const Net::Message& message, std::string id)
            : ID(std::move(id)), Message(message)
        {
        }
    };


    /// <summary>
    /// Represents a message with an associated network endpoint (IP and port).
    /// Members are std::optional because the C# reference-type members can be null.
    /// </summary>
    class LocatedMessage
    {
    public:
        std::optional<Net::IPEndPoint> IPEndPoint;
        std::optional<Net::AppMessage> Message;

        LocatedMessage() = default;

        LocatedMessage(std::optional<Net::IPEndPoint> point, std::optional<Net::AppMessage> message)
            : IPEndPoint(std::move(point)), Message(std::move(message))
        {
        }

        /// <summary>Returns a string representation of the located message.</summary>
        std::string ToString() const
        {
            return "IP End Point: " + (IPEndPoint ? IPEndPoint->ToString() : "null") +
                   "\t App Message: " + (Message ? Message->ToJson().Dump() : "null");
        }
    };
}
