//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include <Michitai/Lan/Json.hpp>

#include <charconv>
#include <cmath>


namespace Michitai::Lan
{
    namespace
    {
        class Parser
        {
        public:
            explicit Parser(const std::string& text) : _text(text) {}

            JsonValue Parse()
            {
                SkipWhitespace();

                JsonValue value = ParseValue();

                SkipWhitespace();

                if (_position != _text.size())
                    throw std::runtime_error("JSON: unexpected trailing characters");

                return value;
            }

        private:
            const std::string& _text;
            std::size_t _position = 0;

            [[noreturn]] void Fail(const char* message) const
            {
                throw std::runtime_error(std::string("JSON parse error: ") + message);
            }

            void SkipWhitespace()
            {
                while (_position < _text.size() &&
                       (_text[_position] == ' ' || _text[_position] == '\t' ||
                        _text[_position] == '\n' || _text[_position] == '\r'))
                {
                    _position++;
                }
            }

            char Peek() const
            {
                return _position < _text.size() ? _text[_position] : '\0';
            }

            bool Consume(char expected)
            {
                if (Peek() == expected)
                {
                    _position++;
                    return true;
                }
                return false;
            }

            void Expect(char expected)
            {
                if (!Consume(expected))
                    Fail("unexpected character");
            }

            JsonValue ParseValue()
            {
                SkipWhitespace();

                switch (Peek())
                {
                    case '{': return ParseObject();
                    case '[': return ParseArray();
                    case '"': return ParseString();
                    case 't': case 'f': return ParseBool();
                    case 'n': return ParseNull();
                    default:  return ParseNumber();
                }
            }

            JsonValue ParseObject()
            {
                Expect('{');

                JsonValue::Object object;

                SkipWhitespace();

                if (Consume('}'))
                    return JsonValue(std::move(object));

                while (true)
                {
                    SkipWhitespace();
                    std::string key = ParseString().AsString();

                    SkipWhitespace();
                    Expect(':');

                    object.emplace(std::move(key), ParseValue());

                    SkipWhitespace();

                    if (Consume('}'))
                        break;

                    Expect(',');
                }

                return JsonValue(std::move(object));
            }

            JsonValue ParseArray()
            {
                Expect('[');

                JsonValue::Array array;

                SkipWhitespace();

                if (Consume(']'))
                    return JsonValue(std::move(array));

                while (true)
                {
                    array.push_back(ParseValue());

                    SkipWhitespace();

                    if (Consume(']'))
                        break;

                    Expect(',');
                }

                return JsonValue(std::move(array));
            }

            JsonValue ParseString()
            {
                Expect('"');

                std::string result;

                while (true)
                {
                    if (_position >= _text.size())
                        Fail("unterminated string");

                    char c = _text[_position++];

                    if (c == '"')
                        return JsonValue(std::move(result));

                    if (c != '\\')
                    {
                        result += c;
                        continue;
                    }

                    if (_position >= _text.size())
                        Fail("unterminated escape");

                    char escape = _text[_position++];

                    switch (escape)
                    {
                        case '"':  result += '"';  break;
                        case '\\': result += '\\'; break;
                        case '/':  result += '/';  break;
                        case 'b':  result += '\b'; break;
                        case 'f':  result += '\f'; break;
                        case 'n':  result += '\n'; break;
                        case 'r':  result += '\r'; break;
                        case 't':  result += '\t'; break;
                        case 'u':  result += ParseUnicodeEscape(); break;
                        default:   Fail("invalid escape");
                    }
                }
            }

            std::string ParseUnicodeEscape()
            {
                std::uint32_t code = ParseHex4();

                // Combine UTF-16 surrogate pairs.
                if (code >= 0xD800 && code <= 0xDBFF)
                {
                    if (_position + 1 < _text.size() && _text[_position] == '\\' && _text[_position + 1] == 'u')
                    {
                        _position += 2;
                        std::uint32_t low = ParseHex4();
                        if (low >= 0xDC00 && low <= 0xDFFF)
                        {
                            code = 0x10000 + ((code - 0xD800) << 10) + (low - 0xDC00);
                        }
                        else
                        {
                            Fail("invalid surrogate pair");
                        }
                    }
                    else
                    {
                        Fail("lone surrogate");
                    }
                }

                return EncodeUtf8(code);
            }

            std::uint32_t ParseHex4()
            {
                if (_position + 4 > _text.size())
                    Fail("truncated \\u escape");

                std::uint32_t value = 0;

                for (int i = 0; i < 4; i++)
                {
                    char c = _text[_position++];
                    value <<= 4;
                    if (c >= '0' && c <= '9')      value |= static_cast<std::uint32_t>(c - '0');
                    else if (c >= 'a' && c <= 'f') value |= static_cast<std::uint32_t>(c - 'a' + 10);
                    else if (c >= 'A' && c <= 'F') value |= static_cast<std::uint32_t>(c - 'A' + 10);
                    else Fail("invalid hex digit");
                }

                return value;
            }

            static std::string EncodeUtf8(std::uint32_t code)
            {
                std::string out;

                if (code < 0x80)
                {
                    out += static_cast<char>(code);
                }
                else if (code < 0x800)
                {
                    out += static_cast<char>(0xC0 | (code >> 6));
                    out += static_cast<char>(0x80 | (code & 0x3F));
                }
                else if (code < 0x10000)
                {
                    out += static_cast<char>(0xE0 | (code >> 12));
                    out += static_cast<char>(0x80 | ((code >> 6) & 0x3F));
                    out += static_cast<char>(0x80 | (code & 0x3F));
                }
                else
                {
                    out += static_cast<char>(0xF0 | (code >> 18));
                    out += static_cast<char>(0x80 | ((code >> 12) & 0x3F));
                    out += static_cast<char>(0x80 | ((code >> 6) & 0x3F));
                    out += static_cast<char>(0x80 | (code & 0x3F));
                }

                return out;
            }

            JsonValue ParseBool()
            {
                if (_text.compare(_position, 4, "true") == 0)
                {
                    _position += 4;
                    return JsonValue(true);
                }
                if (_text.compare(_position, 5, "false") == 0)
                {
                    _position += 5;
                    return JsonValue(false);
                }
                Fail("invalid literal");
            }

            JsonValue ParseNull()
            {
                if (_text.compare(_position, 4, "null") == 0)
                {
                    _position += 4;
                    return JsonValue(nullptr);
                }
                Fail("invalid literal");
            }

            JsonValue ParseNumber()
            {
                const std::size_t start = _position;

                if (Peek() == '-')
                    _position++;

                if (Peek() < '0' || Peek() > '9')
                    Fail("invalid value");

                bool isDouble = false;

                while (_position < _text.size())
                {
                    char c = _text[_position];
                    if ((c >= '0' && c <= '9') || c == '-' || c == '+')
                    {
                        _position++;
                    }
                    else if (c == '.' || c == 'e' || c == 'E')
                    {
                        isDouble = true;
                        _position++;
                    }
                    else
                    {
                        break;
                    }
                }

                std::string token = _text.substr(start, _position - start);

                if (isDouble)
                {
                    double value = 0;
                    auto result = std::from_chars(token.data(), token.data() + token.size(), value);
                    if (result.ec != std::errc())
                        Fail("invalid number");
                    return JsonValue(value);
                }

                std::int64_t value = 0;
                auto result = std::from_chars(token.data(), token.data() + token.size(), value);
                if (result.ec != std::errc())
                    Fail("invalid number");
                return JsonValue(value);
            }
        };

        void DumpEscaped(const std::string& text, std::string& out)
        {
            out += '"';

            for (char c : text)
            {
                switch (c)
                {
                    case '"':  out += "\\\""; break;
                    case '\\': out += "\\\\"; break;
                    case '\b': out += "\\b";  break;
                    case '\f': out += "\\f";  break;
                    case '\n': out += "\\n";  break;
                    case '\r': out += "\\r";  break;
                    case '\t': out += "\\t";  break;
                    default:
                    {
                        const auto byte = static_cast<unsigned char>(c);
                        if (byte < 0x20)
                        {
                            static constexpr char hex[] = "0123456789abcdef";
                            out += "\\u00";
                            out += hex[byte >> 4];
                            out += hex[byte & 0x0F];
                        }
                        else
                        {
                            out += c;
                        }
                    }
                }
            }

            out += '"';
        }
    }

    JsonValue JsonValue::Parse(const std::string& text)
    {
        return Parser(text).Parse();
    }

    std::string JsonValue::Dump() const
    {
        std::string out;

        std::visit([&out](const auto& value)
        {
            using T = std::decay_t<decltype(value)>;

            if constexpr (std::is_same_v<T, std::nullptr_t>)
            {
                out += "null";
            }
            else if constexpr (std::is_same_v<T, bool>)
            {
                out += value ? "true" : "false";
            }
            else if constexpr (std::is_same_v<T, std::int64_t>)
            {
                out += std::to_string(value);
            }
            else if constexpr (std::is_same_v<T, double>)
            {
                if (std::isfinite(value))
                {
                    std::string text = std::to_string(value);
                    while (text.size() > 1 && text.back() == '0' && text.find('.') != std::string::npos)
                        text.pop_back();
                    if (!text.empty() && text.back() == '.')
                        text += '0';
                    out += text;
                }
                else
                {
                    out += "0";
                }
            }
            else if constexpr (std::is_same_v<T, std::string>)
            {
                DumpEscaped(value, out);
            }
            else if constexpr (std::is_same_v<T, Array>)
            {
                out += '[';
                for (std::size_t i = 0; i < value.size(); i++)
                {
                    if (i > 0) out += ',';
                    out += value[i].Dump();
                }
                out += ']';
            }
            else
            {
                out += '{';
                bool first = true;
                for (const auto& [key, member] : value)
                {
                    if (!first) out += ',';
                    first = false;
                    DumpEscaped(key, out);
                    out += ':';
                    out += member.Dump();
                }
                out += '}';
            }
        }, _value);

        return out;
    }
}
