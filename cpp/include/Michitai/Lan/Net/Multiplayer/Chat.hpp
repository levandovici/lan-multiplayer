//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <mutex>
#include <string>
#include <vector>

#include <Michitai/Lan/Json.hpp>


namespace Michitai::Lan::Net::Multiplayer::Chat
{
    /// <summary>
    /// Represents a single chat message with an ID and message content.
    /// </summary>
    class Letter
    {
    public:
        std::string ID;
        std::string Message;

        Letter() = default;

        Letter(std::string id, std::string message)
            : ID(std::move(id)), Message(std::move(message))
        {
        }

        JsonValue ToJson() const
        {
            return JsonValue(JsonValue::Object{
                {"ID",      JsonValue(ID)},
                {"Message", JsonValue(Message)}
            });
        }

        static Letter FromJson(const JsonValue& json)
        {
            return Letter(json["ID"].AsString(), json["Message"].AsString());
        }
    };


    /// <summary>
    /// Thread-safe chat container for managing chat messages with a maximum length limit.
    /// </summary>
    class Chat
    {
    public:
        /// <summary>Initializes a new instance of Chat with the specified maximum length.</summary>
        explicit Chat(int maxLength) : _maxLength(maxLength) {}

        /// <summary>Initializes a new instance of Chat with a default maximum length of 128.</summary>
        Chat() = default;

        /// <summary>Copies letters and max length (the mutex is not copied).</summary>
        Chat(const Chat& other)
        {
            std::lock_guard lock(other._mutex);
            _letters = other._letters;
            _maxLength = other._maxLength;
        }

        Chat& operator=(const Chat& other)
        {
            if (this != &other)
            {
                std::scoped_lock lock(_mutex, other._mutex);
                _letters = other._letters;
                _maxLength = other._maxLength;
            }
            return *this;
        }

        /// <summary>Gets the chat letters. Thread-safe snapshot.</summary>
        std::vector<Letter> Letters() const
        {
            std::lock_guard lock(_mutex);
            return _letters;
        }

        /// <summary>Replaces the letters. Thread-safe.</summary>
        void SetLetters(std::vector<Letter> letters)
        {
            std::lock_guard lock(_mutex);
            _letters = std::move(letters);
        }

        /// <summary>
        /// Gets or sets the maximum number of letters the chat can hold. Thread-safe.
        /// When set, removes oldest letters if exceeding the limit.
        /// </summary>
        int MaxLength() const
        {
            std::lock_guard lock(_mutex);
            return _maxLength;
        }

        void SetMaxLength(int maxLength)
        {
            std::lock_guard lock(_mutex);
            _maxLength = maxLength;
            while (static_cast<int>(_letters.size()) > _maxLength)
            {
                _letters.erase(_letters.begin());
            }
        }

        /// <summary>Adds a letter to the chat. Removes the oldest letter if at maximum capacity.</summary>
        void Add(const Letter& letter)
        {
            std::lock_guard lock(_mutex);

            if (static_cast<int>(_letters.size()) >= _maxLength)
            {
                _letters.erase(_letters.begin());
            }

            _letters.push_back(letter);
        }

        /// <summary>Deletes a letter at the specified index.</summary>
        void Delete(int index)
        {
            std::lock_guard lock(_mutex);
            if (index >= 0 && index < static_cast<int>(_letters.size()))
            {
                _letters.erase(_letters.begin() + index);
            }
        }

        /// <summary>Clears all letters from the chat.</summary>
        void Clear()
        {
            std::lock_guard lock(_mutex);
            _letters.clear();
        }

        JsonValue ToJson() const
        {
            const std::vector<Letter> letters = Letters();

            JsonValue::Array array;
            for (const Letter& letter : letters)
            {
                array.push_back(letter.ToJson());
            }

            return JsonValue(JsonValue::Object{
                {"Letters",   JsonValue(std::move(array))},
                {"MaxLength", JsonValue(MaxLength())}
            });
        }

        static Chat FromJson(const JsonValue& json)
        {
            Chat chat(static_cast<int>(json["MaxLength"].AsInt()));

            std::vector<Letter> letters;
            for (const JsonValue& item : json["Letters"].AsArray())
            {
                letters.push_back(Letter::FromJson(item));
            }
            chat.SetLetters(std::move(letters));

            return chat;
        }

    private:
        std::vector<Letter> _letters;
        int _maxLength = 128;
        mutable std::mutex _mutex;
    };
}
