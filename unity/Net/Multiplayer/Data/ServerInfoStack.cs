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

namespace Michitai.Lan.Net.Multiplayer.Data
{
    /// <summary>
    /// Thread-safe stack for managing server information.
    /// </summary>
    public class ServerInfoStack
    {
        public Stack<ServerInfo> _stack;

        public int _count;

        private object _stack_lock;

        private object _count_lock;

        /// <summary>
        /// Initializes a new instance of ServerInfoStack.
        /// </summary>
        public ServerInfoStack()
    {
        _stack = new Stack<ServerInfo>();

        _count = 0;


        _stack_lock = new object();

        _count_lock = new object();
    }



        /// <summary>
        /// Pushes a server info onto the stack.
        /// </summary>
        /// <param name="serverInfo">The server info to push.</param>
        public void Push(ServerInfo serverInfo)
    {
        lock (_stack_lock)
        {
            _stack.Push(serverInfo);

            lock (_count_lock)
            {
                _count++;
            }
        }
    }

        /// <summary>
        /// Pops a server info from the stack.
        /// </summary>
        /// <returns>The server info at the top of the stack.</returns>
        public ServerInfo Pop()
    {
        lock (_stack_lock)
        {
            lock (_count_lock)
            {
                _count--;
            }

            return _stack.Pop();
        }
    }

        /// <summary>
        /// Peeks at the server info at the top of the stack without removing it.
        /// </summary>
        /// <returns>The server info at the top of the stack.</returns>
        public ServerInfo Peek()
    {
        lock (_stack_lock)
        {
            return _stack.Peek();
        }
    }

        /// <summary>
        /// Gets the number of items in the stack.
        /// </summary>
        /// <returns>The count of items in the stack.</returns>
        public int Count()
    {
        lock (_count_lock)
        {
            return _count;
        }
    }

        /// <summary>
        /// Clears all items from the stack.
        /// </summary>
        public void Clear()
    {
        lock (_stack_lock)
        {
            lock (_count_lock)
            {
                _count = 0;
            }

            _stack.Clear();
        }
    }
}
}
