//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <cstdint>
#include <map>
#include <memory>
#include <stdexcept>
#include <string>
#include <variant>
#include <vector>


namespace Michitai::Lan
{
    /// <summary>
    /// Minimal self-contained JSON value: null, bool, number (int64/double),
    /// string, array, object. Member order of objects is alphabetical (std::map),
    /// matching DataContractJsonSerializer output.
    /// </summary>
    class JsonValue
    {
    public:
        using Array = std::vector<JsonValue>;
        using Object = std::map<std::string, JsonValue>;

    private:
        std::variant<std::nullptr_t, bool, std::int64_t, double, std::string, Array, Object> _value;

    public:
        JsonValue() : _value(nullptr) {}
        JsonValue(std::nullptr_t) : _value(nullptr) {}
        JsonValue(bool value) : _value(value) {}
        JsonValue(int value) : _value(static_cast<std::int64_t>(value)) {}
        JsonValue(std::int64_t value) : _value(value) {}
        JsonValue(double value) : _value(value) {}
        JsonValue(const char* value) : _value(std::string(value ? value : "")) {}
        JsonValue(const std::string& value) : _value(value) {}
        JsonValue(std::string&& value) : _value(std::move(value)) {}
        JsonValue(const Array& value) : _value(value) {}
        JsonValue(Array&& value) : _value(std::move(value)) {}
        JsonValue(const Object& value) : _value(value) {}
        JsonValue(Object&& value) : _value(std::move(value)) {}

        bool IsNull() const { return std::holds_alternative<std::nullptr_t>(_value); }
        bool IsBool() const { return std::holds_alternative<bool>(_value); }
        bool IsInt() const { return std::holds_alternative<std::int64_t>(_value); }
        bool IsDouble() const { return std::holds_alternative<double>(_value); }
        bool IsNumber() const { return IsInt() || IsDouble(); }
        bool IsString() const { return std::holds_alternative<std::string>(_value); }
        bool IsArray() const { return std::holds_alternative<Array>(_value); }
        bool IsObject() const { return std::holds_alternative<Object>(_value); }

        bool AsBool() const { return IsBool() ? std::get<bool>(_value) : false; }

        std::int64_t AsInt() const
        {
            if (IsInt()) return std::get<std::int64_t>(_value);
            if (IsDouble()) return static_cast<std::int64_t>(std::get<double>(_value));
            return 0;
        }

        double AsDouble() const
        {
            if (IsDouble()) return std::get<double>(_value);
            if (IsInt()) return static_cast<double>(std::get<std::int64_t>(_value));
            return 0.0;
        }

        const std::string& AsString() const
        {
            static const std::string empty;
            return IsString() ? std::get<std::string>(_value) : empty;
        }

        const Array& AsArray() const
        {
            static const Array empty;
            return IsArray() ? std::get<Array>(_value) : empty;
        }

        const Object& AsObject() const
        {
            static const Object empty;
            return IsObject() ? std::get<Object>(_value) : empty;
        }

        /// <summary>Returns the object member or a null JsonValue when absent.</summary>
        const JsonValue& operator[](const std::string& key) const
        {
            static const JsonValue null;
            if (!IsObject())
                return null;
            const Object& object = std::get<Object>(_value);
            auto it = object.find(key);
            return it != object.end() ? it->second : null;
        }

        /// <summary>Parses a JSON document. Throws std::runtime_error on malformed input.</summary>
        static JsonValue Parse(const std::string& text);

        /// <summary>Serializes to compact JSON text.</summary>
        std::string Dump() const;
    };
}
