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
    /// Represents a command with arguments for terminal/command-line operations.
    /// </summary>
    [Serializable]
    public sealed class Command
    {
        public string[] args;

        private object _args_lock;

        /// <summary>
        /// Gets or sets the command arguments.
        /// </summary>
        public string[] Arguments
    {
        get
        {
            lock (_args_lock)
            {
                return args;
            }
        }

        set
        {
            lock (_args_lock)
            {
                args = value;
            }
        }
    }



        /// <summary>
        /// Initializes a new instance of Command with the specified arguments.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        private Command(params string[] args)
    {
        this.args = args;

        _args_lock = new object();
    }

        /// <summary>
        /// Initializes a new instance of Command with no arguments.
        /// </summary>
        public Command()
    {
        args = new string[0];

        _args_lock = new object();
    }



        /// <summary>
        /// Adds an argument to the command.
        /// </summary>
        /// <param name="argument">The argument to add.</param>
        /// <returns>This command instance for method chaining.</returns>
        public Command Arg(string argument)
    {
        lock (_args_lock)
        {
            string[] args = this.args;

            this.args = new string[args.Length + 1];

            for (int i = 0; i < args.Length; i++)
            {
                this.args[i] = args[i];
            }

            this.args[args.Length] = argument;
        }

        return this;
    }



        /// <summary>
        /// Creates a new command from a string, ensuring it starts with '/'.
        /// </summary>
        /// <param name="command">The command string.</param>
        /// <returns>A new Command instance.</returns>
        public static Command New(string command)
    {
        if (command[0] != '/')
        {
            return new Command($"/{command}");
        }

        return new Command(command);
    }



        /// <summary>
        /// Returns a string representation of the command.
        /// </summary>
        /// <returns>A string containing all arguments.</returns>
        public override string ToString()
    {
        lock (_args_lock)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"[ARGS][{Arguments.Length}]");

            for (int i = 0; i < Arguments.Length; i++)
            {
                sb.Append($"[ARG][{i}][{Arguments[i]}]");
            }

            return sb.ToString();
        }
    }

        /// <summary>
        /// Determines whether the specified object is equal to the current command by comparing the first argument.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            Command cmds = (Command)obj;

            lock (_args_lock)
            {
                return Arguments[0] == cmds.Arguments[0];
            }
        }
    }

        /// <summary>
        /// Returns a hash code for the current command.
        /// </summary>
        /// <returns>A hash code based on the arguments.</returns>
        public override int GetHashCode()
    {
        lock (_args_lock)
        {
            return args.GetHashCode();
        }
    }



        /// <summary>
        /// Determines whether two commands are equal.
        /// </summary>
        /// <param name="A">The first command.</param>
        /// <param name="B">The second command.</param>
        /// <returns>True if the commands are equal; otherwise, false.</returns>
        public static bool operator ==(Command A, Command B)
    {
        return A.Equals(B);
    }

        /// <summary>
        /// Determines whether two commands are not equal.
        /// </summary>
        /// <param name="A">The first command.</param>
        /// <param name="B">The second command.</param>
        /// <returns>True if the commands are not equal; otherwise, false.</returns>
        public static bool operator !=(Command A, Command B)
    {
        return !A.Equals(B);
    }
}
}
