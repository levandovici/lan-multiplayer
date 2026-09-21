//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include <Michitai/Lan/Net/Multiplayer/Commands.hpp>

#include <stdexcept>


namespace Michitai::Lan::Net::Multiplayer::Commands
{
    Command::Ptr Command::Arg(const std::string& argument)
    {
        {
            std::lock_guard lock(_mutex);
            _args.push_back(argument);
        }

        return shared_from_this();
    }

    std::string Command::ToString() const
    {
        const std::vector<std::string> args = Arguments();

        std::string result = "[ARGS][" + std::to_string(args.size()) + "]";

        for (std::size_t i = 0; i < args.size(); i++)
        {
            result += "[ARG][" + std::to_string(i) + "][" + args[i] + "]";
        }

        return result;
    }

    JsonValue Command::ToJson() const
    {
        JsonValue::Array args;
        for (const std::string& arg : Arguments())
        {
            args.push_back(JsonValue(arg));
        }

        return JsonValue(JsonValue::Object{{"Arguments", JsonValue(std::move(args))}});
    }

    Command Command::FromJson(const JsonValue& json)
    {
        Command command;

        std::vector<std::string> args;
        for (const JsonValue& arg : json["Arguments"].AsArray())
        {
            args.push_back(arg.AsString());
        }
        command.SetArguments(std::move(args));

        return command;
    }


    Terminal::Ptr Terminal::Arg(const std::string& argument)
    {
        {
            std::lock_guard lock(_mutex);
            if (_commands.empty())
                throw std::out_of_range("Terminal: no command to add an argument to");
            _commands.back()->Arg(argument);
        }

        return shared_from_this();
    }

    Terminal::Ptr Terminal::Next(const Command::Ptr& command)
    {
        {
            std::lock_guard lock(_mutex);
            _commands.push_back(command);
        }

        return shared_from_this();
    }

    JsonValue Terminal::ToJson() const
    {
        JsonValue::Array commands;
        for (const Command::Ptr& command : Commands())
        {
            commands.push_back(command ? command->ToJson() : JsonValue(nullptr));
        }

        return JsonValue(JsonValue::Object{{"Commands", JsonValue(std::move(commands))}});
    }

    Terminal Terminal::FromJson(const JsonValue& json)
    {
        Terminal terminal;

        std::vector<Command::Ptr> commands;
        for (const JsonValue& command : json["Commands"].AsArray())
        {
            Command value = Command::FromJson(command);
            Command::Ptr ptr = std::make_shared<Command>();
            ptr->SetArguments(value.Arguments());
            commands.push_back(std::move(ptr));
        }
        terminal.SetCommands(std::move(commands));

        return terminal;
    }
}
