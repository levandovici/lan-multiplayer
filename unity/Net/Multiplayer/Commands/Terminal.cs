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

using Michitai.Lan;
using Michitai.Lan.Data;
using Michitai.Lan.Net;
using Michitai.Lan.Net.Multiplayer;
using Michitai.Lan.Net.Multiplayer.Chat;
using Michitai.Lan.Net.Multiplayer.Commands;
using Michitai.Lan.Net.Multiplayer.Data;
using Michitai.Lan.Debug;
using UnityEngine;

namespace Michitai.Lan.Net.Multiplayer.Commands
{
    /// <summary>
    /// Represents a terminal for chaining multiple commands together.
    /// </summary>
    public sealed class Terminal
    {
        public Command[] commands;

        private object _commands_lock;

        /// <summary>
        /// Gets or sets the array of commands in the terminal.
        /// </summary>
        public Command[] Commands
    {
        get
        {
            lock (_commands_lock)
            {
                return commands;
            }
        }

        set
        {
            lock (_commands_lock)
            {
                commands = value;
            }
        }
    }



        /// <summary>
        /// Initializes a new instance of Terminal with no commands.
        /// </summary>
        public Terminal()
    {
        commands = new Command[0];

        _commands_lock = new object();
    }



        /// <summary>
        /// Adds an argument to the last command in the terminal.
        /// </summary>
        /// <param name="argument">The argument to add.</param>
        /// <returns>This terminal instance for method chaining.</returns>
        public Terminal Arg(string argument)
    {
        lock (_commands_lock)
        {
            commands[commands.Length - 1].Arg(argument);
        }

        return this;
    }

        /// <summary>
        /// Adds a new command to the terminal from a string.
        /// </summary>
        /// <param name="command">The command string to add.</param>
        /// <returns>This terminal instance for method chaining.</returns>
        public Terminal Next(string command)
    {
        return Next(Command.New(command));
    }

        /// <summary>
        /// Adds a new command to the terminal.
        /// </summary>
        /// <param name="command">The command to add.</param>
        /// <returns>This terminal instance for method chaining.</returns>
        public Terminal Next(Command command)
    {
        lock (_commands_lock)
        {
            Command[] commands = this.commands;

            this.commands = new Command[commands.Length + 1];

            for (int i = 0; i < commands.Length; i++)
            {
                this.commands[i] = commands[i];
            }

            this.commands[commands.Length] = command;
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
        /// <param name="command">The command to add to the terminal.</param>
        /// <returns>A new Terminal instance with the command.</returns>
        public static Terminal New(string command)
    {
        return New().Next(command);
    }
}
}
