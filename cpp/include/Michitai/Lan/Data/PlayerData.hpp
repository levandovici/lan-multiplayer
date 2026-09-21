//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <string>

#include <Michitai/Lan/Data/JsonStorage.hpp>
#include <Michitai/Lan/Json.hpp>


namespace Michitai::Lan::Data
{
    /// <summary>
    /// Represents player game data with JSON serialization capabilities.
    /// </summary>
    class PlayerGameData : public JsonStorage
    {
    public:
        PlayerGameData() = default;
        explicit PlayerGameData(std::string json) : JsonStorage(std::move(json)) {}

        JsonValue ToJson() const
        {
            return JsonValue(JsonValue::Object{{"Json", JsonValue(Json())}});
        }

        static PlayerGameData FromJson(const JsonValue& json)
        {
            return PlayerGameData(json["Json"].AsString());
        }
    };


    /// <summary>
    /// Represents player character data with JSON serialization capabilities.
    /// </summary>
    class PlayerCharacterData : public JsonStorage
    {
    public:
        PlayerCharacterData() = default;
        explicit PlayerCharacterData(std::string json) : JsonStorage(std::move(json)) {}

        JsonValue ToJson() const
        {
            return JsonValue(JsonValue::Object{{"Json", JsonValue(Json())}});
        }

        static PlayerCharacterData FromJson(const JsonValue& json)
        {
            return PlayerCharacterData(json["Json"].AsString());
        }
    };


    /// <summary>
    /// Represents player world data with JSON serialization capabilities.
    /// </summary>
    class PlayerWorldData : public JsonStorage
    {
    public:
        PlayerWorldData() = default;
        explicit PlayerWorldData(std::string json) : JsonStorage(std::move(json)) {}

        JsonValue ToJson() const
        {
            return JsonValue(JsonValue::Object{{"Json", JsonValue(Json())}});
        }

        static PlayerWorldData FromJson(const JsonValue& json)
        {
            return PlayerWorldData(json["Json"].AsString());
        }
    };
}
