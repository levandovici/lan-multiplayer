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

namespace Michitai.Lan.Net
{
            public sealed class IdentifiedMessage
{
    private string _id;

    private Message _message;



    /// <summary>
    /// 
    /// </summary>
    public string ID => _id;

    /// <summary>
    /// 
    /// </summary>
    public Message Message => _message;



    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="id"></param>
    public IdentifiedMessage(Message message, string id)
    {
        _message = message;

        _id = id;
    }
}
}
