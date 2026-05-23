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
            public sealed class Terminal
{
    public Command[] commands;

    private object _commands_lock;



    /// <summary>
    /// 
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
    /// 
    /// </summary>
    public Terminal()
    {
        commands = new Command[0];

        _commands_lock = new object();
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    public Terminal Arg(string argument)
    {
        lock (_commands_lock)
        {
            commands[commands.Length - 1].Arg(argument);
        }

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public Terminal Next(string command)
    {
        return Next(Command.New(command));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <returns></returns>
    public static Terminal New()
    {
        return new Terminal();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public static Terminal New(string command)
    {
        return New().Next(command);
    }
}
}
