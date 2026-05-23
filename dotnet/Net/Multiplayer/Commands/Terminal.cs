//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Net.NetworkInformation;
using System.Text;
using System.Runtime;
using System.Runtime.Serialization;


namespace Michitai.Lan.Net.Multiplayer.Commands
{
    /// <summary>
    /// Terminal for chaining and managing multiple commands.
    /// </summary>
    [Serializable]
    public sealed class Terminal
    {
        private Command[] _commands;

        private object _commands_lock;

        /// <summary>
        /// Gets or sets the array of commands. Thread-safe.
        /// </summary>
        public Command[] Commands
    {
        get
        {
            lock (_commands_lock)
            {
                return _commands;
            }
        }

        set
        {
            lock (_commands_lock)
            {
                _commands = value;
            }
        }
    }



        /// <summary>
        /// Initializes a new instance of Terminal with no commands.
        /// </summary>
        public Terminal()
    {
        _commands = new Command[0];

        _commands_lock = new object();
    }



        /// <summary>
        /// Adds an argument to the last command. Thread-safe.
        /// </summary>
        /// <param name="argument">The argument to add.</param>
        /// <returns>This terminal instance for chaining.</returns>
        public Terminal Arg(string argument)
    {
        lock (_commands_lock)
        {
            _commands[_commands.Length - 1].Arg(argument);
        }

        return this;
    }

        /// <summary>
        /// Adds a new command to the terminal from a string.
        /// </summary>
        /// <param name="command">The command string.</param>
        /// <returns>This terminal instance for chaining.</returns>
        public Terminal Next(string command)
    {
        return Next(Command.New(command));
    }

        /// <summary>
        /// Adds a new command to the terminal.
        /// </summary>
        /// <param name="command">The command to add.</param>
        /// <returns>This terminal instance for chaining.</returns>
        public Terminal Next(Command command)
    {
        lock (_commands_lock)
        {
            Command[] commands = _commands;

            _commands = new Command[commands.Length + 1];

            for (int i = 0; i < commands.Length; i++)
            {
                _commands[i] = commands[i];
            }

            _commands[commands.Length] = command;
        }

        return this;
    }



        /// <summary>
        /// Creates a new empty terminal.
        /// </summary>
        /// <returns>A new Terminal instance.</returns>
        public static Terminal New()
    {
        return new Terminal();
    }

        /// <summary>
        /// Creates a new terminal with the specified command.
        /// </summary>
        /// <param name="command">The initial command string.</param>
        /// <returns>A new Terminal instance with the command.</returns>
        public static Terminal New(string command)
    {
        return New().Next(command);
    }
}
}
