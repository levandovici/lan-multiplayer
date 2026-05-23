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
            public sealed class Command
{
    public string[] args;

    private object _args_lock;



    /// <summary>
    /// 
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
    /// 
    /// </summary>
    /// <param name="args"></param>
    private Command(params string[] args)
    {
        this.args = args;

        _args_lock = new object();
    }

    /// <summary>
    /// 
    /// </summary>
    public Command()
    {
        args = new string[0];

        _args_lock = new object();
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public static Command New(string command)
    {
        if (command[0] != '/')
        {
            return new Command($"/{command}");
        }

        return new Command(command);
    }



    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        lock (_args_lock)
        {
            return args.GetHashCode();
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <returns></returns>
    public static bool operator ==(Command A, Command B)
    {
        return A.Equals(B);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <returns></returns>
    public static bool operator !=(Command A, Command B)
    {
        return !A.Equals(B);
    }
}
}
