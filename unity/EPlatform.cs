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

namespace Michitai.Lan
{
    /// <summary>
    /// Enumeration of supported platforms for the LAN multiplayer system.
    /// </summary>
    public enum EPlatform
    {
        /// <summary>Windows desktop platform.</summary>
        Windows = 1,
        /// <summary>Linux desktop platform.</summary>
        Linux = 2,
        /// <summary>macOS desktop platform.</summary>
        MacOS = 4,
        /// <summary>All desktop platforms combined (Windows, Linux, macOS).</summary>
        Standalone = 7,
        /// <summary>Android mobile platform.</summary>
        Android = 8,
        /// <summary>iOS mobile platform.</summary>
        IOS = 16,
        /// <summary>All mobile platforms combined (Android, iOS).</summary>
        Mobile = 24,
    }
}
