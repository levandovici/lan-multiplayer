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
    /// <summary>
    /// Represents an application-level message containing version, name, and message content.
    /// </summary>
    public sealed class AppMessage
    {
        /// <summary>The version of the application message protocol.</summary>
        public int _version;

        /// <summary>The name associated with the message.</summary>
        public string _name;

        /// <summary>The message content.</summary>
        public string _message;



        /// <summary>
        /// Gets or sets the version of the application message protocol.
        /// </summary>
        public int Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = value;
            }
        }

        /// <summary>
        /// Gets or sets the name associated with the message.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Gets or sets the message content.
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }

            set
            {
                _message = value;
            }
        }



        /// <summary>
        /// Initializes a new instance of AppMessage with default values.
        /// </summary>
        public AppMessage()
        {

        }

        /// <summary>
        /// Initializes a new instance of AppMessage with the specified version, name, and message content.
        /// </summary>
        /// <param name="version">The version of the application message protocol.</param>
        /// <param name="name">The name associated with the message.</param>
        /// <param name="message">The message content.</param>
        public AppMessage(int version, string name, string message)
        {
            Version = version;

            Name = name;

            Message = message;
        }
}
}
