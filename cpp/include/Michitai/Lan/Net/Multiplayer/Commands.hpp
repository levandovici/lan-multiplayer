//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <memory>
#include <mutex>
#include <string>
#include <vector>

#include <Michitai/Lan/Json.hpp>


namespace Michitai::Lan::Net::Multiplayer::Commands
{
    /// <summary>
    /// Represents a command with its arguments. Thread-safe.
    /// Owned via std::shared_ptr — create with Command::New().
    /// </summary>
    class Command : public std::enable_shared_from_this<Command>
    {
    public:
        using Ptr = std::shared_ptr<Command>;

        /// <summary>Initializes a new instance of Command with no arguments.</summary>
        Command() = default;

        /// <summary>Copies the arguments (the mutex is not copied).</summary>
        Command(const Command& other) : _args(other.Arguments()) {}

        Command& operator=(const Command& other)
        {
            if (this != &other)
                SetArguments(other.Arguments());
            return *this;
        }

        /// <summary>Gets or sets the command arguments. Thread-safe.</summary>
        std::vector<std::string> Arguments() const
        {
            std::lock_guard lock(_mutex);
            return _args;
        }

        void SetArguments(std::vector<std::string> args)
        {
            std::lock_guard lock(_mutex);
            _args = std::move(args);
        }

        /// <summary>Adds an argument to the command. Returns this command for chaining.</summary>
        Ptr Arg(const std::string& argument);

        /// <summary>Creates a new command from a string. Prepends '/' if not present.</summary>
        static Ptr New(const std::string& command)
        {
            Ptr result = std::make_shared<Command>();
            if (!command.empty() && command[0] == '/')
                result->SetArguments({command});
            else
                result->SetArguments({"/" + command});
            return result;
        }

        /// <summary>Returns a string representation of the command and its arguments.</summary>
        std::string ToString() const;

        /// <summary>Commands are equal when their first argument (the command name) matches.</summary>
        bool Equals(const Command& other) const
        {
            const std::vector<std::string> a = Arguments();
            const std::vector<std::string> b = other.Arguments();
            if (a.empty() || b.empty())
                return a.empty() && b.empty();
            return a[0] == b[0];
        }

        bool operator==(const Command& other) const { return Equals(other); }
        bool operator!=(const Command& other) const { return !Equals(other); }

        JsonValue ToJson() const;
        static Command FromJson(const JsonValue& json);

    private:
        std::vector<std::string> _args;
        mutable std::mutex _mutex;
    };


    /// <summary>
    /// Terminal for chaining and managing multiple commands. Thread-safe.
    /// Owned via std::shared_ptr — create with Terminal::New().
    /// </summary>
    class Terminal : public std::enable_shared_from_this<Terminal>
    {
    public:
        using Ptr = std::shared_ptr<Terminal>;

        /// <summary>Initializes a new instance of Terminal with no commands.</summary>
        Terminal() = default;

        /// <summary>Copies the command list (the mutex is not copied).</summary>
        Terminal(const Terminal& other) : _commands(other.Commands()) {}

        Terminal& operator=(const Terminal& other)
        {
            if (this != &other)
                SetCommands(other.Commands());
            return *this;
        }

        /// <summary>Gets or sets the array of commands. Thread-safe.</summary>
        std::vector<Command::Ptr> Commands() const
        {
            std::lock_guard lock(_mutex);
            return _commands;
        }

        void SetCommands(std::vector<Command::Ptr> commands)
        {
            std::lock_guard lock(_mutex);
            _commands = std::move(commands);
        }

        /// <summary>Adds an argument to the last command. Returns this terminal for chaining.</summary>
        Ptr Arg(const std::string& argument);

        /// <summary>Adds a new command to the terminal from a string. Returns this terminal.</summary>
        Ptr Next(const std::string& command)
        {
            return Next(Command::New(command));
        }

        /// <summary>Adds a new command to the terminal. Returns this terminal.</summary>
        Ptr Next(const Command::Ptr& command);

        /// <summary>Creates a new empty terminal.</summary>
        static Ptr New()
        {
            return std::make_shared<Terminal>();
        }

        /// <summary>Creates a new terminal with the specified command.</summary>
        static Ptr New(const std::string& command)
        {
            return New()->Next(command);
        }

        JsonValue ToJson() const;
        static Terminal FromJson(const JsonValue& json);

    private:
        std::vector<Command::Ptr> _commands;
        mutable std::mutex _mutex;
    };
}
