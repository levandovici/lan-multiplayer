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

namespace Michitai.Lan.Net.Multiplayer.Chat
{
            public sealed class Letter
{
    private string _id;

    private string _message;


    private object _id_lock;

    private object _message_lock;



    /// <summary>
    /// 
    /// </summary>
    public string ID
    {
        get
        {
            lock (_id_lock)
            {
                return _id;
            }
        }

        set
        {
            lock (_id_lock)
            {
                _id = value;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string Message
    {
        get
        {
            lock (_message_lock)
            {
                return _message;
            }
        }

        set
        {
            lock (_message_lock)
            {
                _message = value;
            }
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="message"></param>
    public Letter(string id, string message)
    {
        _id = id;

        _message = message;


        _id_lock = new object();

        _message_lock = new object();
    }

    /// <summary>
    /// 
    /// </summary>
    public Letter()
    {
        _id_lock = new object();

        _message_lock = new object();
    }
}
}
