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
    /// Represents a command with its arguments.
    /// </summary>
    [Serializable]
    public sealed class Command
    {
        private string[] _args;

        private object _args_lock;

        /// <summary>
        /// Gets or sets the command arguments. Thread-safe.
        /// </summary>
        public string[] Arguments
    {
        get
        {
            lock (_args_lock)
            {
                return _args;
            }
        }

        set
        {
            lock (_args_lock)
            {
                _args = value;
            }
        }
    }



        /// <summary>
        /// Initializes a new instance of Command with the specified arguments.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        private Command(params string[] args)
    {
        _args = args;

        _args_lock = new object();
    }

        /// <summary>
        /// Initializes a new instance of Command with no arguments.
        /// </summary>
        public Command()
    {
        _args = new string[0];

        _args_lock = new object();
    }



        /// <summary>
        /// Adds an argument to the command. Thread-safe.
        /// </summary>
        /// <param name="argument">The argument to add.</param>
        /// <returns>This command instance for chaining.</returns>
        public Command Arg(string argument)
    {
        lock (_args_lock)
        {
            string[] args = _args;

            _args = new string[args.Length + 1];

            for (int i = 0; i < args.Length; i++)
            {
                _args[i] = args[i];
            }

            _args[args.Length] = argument;
        }

        return this;
    }



        /// <summary>
        /// Creates a new command from a string. Prepends '/' if not present.
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
        /// Returns a string representation of the command and its arguments.
        /// </summary>
        /// <returns>A string containing the command arguments.</returns>
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
        /// Determines whether the specified object is equal to the current command based on the first argument.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if equal; otherwise, false.</returns>
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
        /// Returns a hash code for the command.
        /// </summary>
        /// <returns>A hash code based on the arguments.</returns>
        public override int GetHashCode()
    {
        lock (_args_lock)
        {
            return _args.GetHashCode();
        }
    }



        /// <summary>
        /// Determines whether two commands are equal.
        /// </summary>
        /// <param name="A">The first command.</param>
        /// <param name="B">The second command.</param>
        /// <returns>True if equal; otherwise, false.</returns>
        public static bool operator ==(Command A, Command B)
    {
        return A.Equals(B);
    }

        /// <summary>
        /// Determines whether two commands are not equal.
        /// </summary>
        /// <param name="A">The first command.</param>
        /// <param name="B">The second command.</param>
        /// <returns>True if not equal; otherwise, false.</returns>
        public static bool operator !=(Command A, Command B)
    {
        return !A.Equals(B);
    }
}
}
