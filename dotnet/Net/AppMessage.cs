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


namespace Michitai.Lan.Net
{
    /// <summary>
    /// Represents an application message with version, name, and content.
    /// </summary>
    public sealed class AppMessage
    {
        private int _version;

        private string _name;

        private string _message;

        /// <summary>
        /// Gets or sets the message version.
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
        /// Gets or sets the application name.
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
        /// Initializes a new instance of AppMessage with the specified version, name, and message.
        /// </summary>
        /// <param name="version">The message version.</param>
        /// <param name="name">The application name.</param>
        /// <param name="message">The message content.</param>
        public AppMessage(int version, string name, string message)
    {
        Version = version;

        Name = name;

        Message = message;
    }
}
}
