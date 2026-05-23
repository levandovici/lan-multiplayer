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


namespace Michitai.Lan
{
    /// <summary>
    /// Enumeration representing different operating platforms.
    /// </summary>
    public enum EPlatform
    {
        /// <summary>Windows platform</summary>
        Windows = 1,
        /// <summary>Linux platform</summary>
        Linux = 2,
        /// <summary>macOS platform</summary>
        MacOS = 4,
        /// <summary>Standalone desktop platform (Windows + Linux + macOS)</summary>
        Standalone = 7,
        /// <summary>Android platform</summary>
        Android = 8,
        /// <summary>iOS platform</summary>
        IOS = 16,
        /// <summary>Mobile platforms (Android + iOS)</summary>
        Mobile = 24
    }
}
