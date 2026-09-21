//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <Michitai/Lan/Json.hpp>


namespace Michitai::Lan
{
    /// <summary>
    /// Serialization hook used by JsonSerializer. The primary template forwards
    /// to a `JsonValue ToJson() const` member and a `static T FromJson(const JsonValue&amp;)`
    /// member on T. Specialize this template for types that cannot carry members.
    /// </summary>
    template<typename T>
    struct JsonCodec
    {
        static JsonValue ToJson(const T& value) { return value.ToJson(); }
        static T FromJson(const JsonValue& json) { return T::FromJson(json); }
    };

    /// <summary>
    /// Minimal self-contained JSON serializer. C++ has no runtime reflection, so
    /// serializable types provide ToJson()/FromJson() (or a JsonCodec specialization)
    /// instead of the DataContractJsonSerializer's reflection path.
    /// </summary>
    class JsonSerializer
    {
    public:
        template<typename T>
        static std::string Serialize(const T& value)
        {
            return JsonCodec<T>::ToJson(value).Dump();
        }

        template<typename T>
        static T Deserialize(const std::string& json)
        {
            return JsonCodec<T>::FromJson(JsonValue::Parse(json));
        }
    };
}
