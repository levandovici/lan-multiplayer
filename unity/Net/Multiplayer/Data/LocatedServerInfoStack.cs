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
            public class LocatedServerInfoStack
{
    public Stack<LocatedServerInfo> _stack;

    public int _count;


    private object _stack_lock;

    private object _count_lock;



    public LocatedServerInfoStack()
    {
        _stack = new Stack<LocatedServerInfo>();

        _count = 0;


        _stack_lock = new object();

        _count_lock = new object();
    }



    public void Push(LocatedServerInfo locatedServerInfo)
    {
        lock (_stack_lock)
        {
            _stack.Push(locatedServerInfo);

            lock (_count_lock)
            {
                _count++;
            }
        }
    }

    public LocatedServerInfo Pop()
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

    public LocatedServerInfo Peek()
    {
        lock (_stack_lock)
        {
            return _stack.Peek();
        }
    }

    public int Count()
    {
        lock (_count_lock)
        {
            return _count;
        }
    }

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
