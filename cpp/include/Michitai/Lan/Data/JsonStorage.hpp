//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <string>

#include <Michitai/Lan/JsonSerializer.hpp>


namespace Michitai::Lan::Data
{
    /// <summary>
    /// JSON storage with serialization and deserialization capabilities.
    /// Equivalent of the C# IJsonStorage interface; the generic Get&lt;T&gt;/Set&lt;T&gt;
    /// resolve through JsonCodec&lt;T&gt; — specialize it (or add ToJson/FromJson members)
    /// for user payload types.
    /// </summary>
    class JsonStorage
    {
    public:
        virtual ~JsonStorage() = default;

        /// <summary>Gets or sets the JSON string representation of the data.</summary>
        const std::string& Json() const { return _json; }
        void SetJson(std::string json) { _json = std::move(json); }

        /// <summary>Deserializes the JSON data to the specified type.</summary>
        template<typename T>
        T Get() const
        {
            return JsonSerializer::Deserialize<T>(_json);
        }

        /// <summary>Serializes the specified object to JSON.</summary>
        template<typename T>
        void Set(const T& object)
        {
            _json = JsonSerializer::Serialize(object);
        }

    protected:
        JsonStorage() = default;
        explicit JsonStorage(std::string json) : _json(std::move(json)) {}

    private:
        std::string _json;
    };
}
