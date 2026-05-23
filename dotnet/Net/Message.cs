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
    /// Represents a simple string message.
    /// </summary>
    public sealed class Message
    {
        private string _message;

        /// <summary>
        /// Gets the message content.
        /// </summary>
        public string GetMessage => _message;

        /// <summary>
        /// Initializes a new instance of Message with the specified content.
        /// </summary>
        /// <param name="message">The message content.</param>
        public Message(string message)
    {
        _message = message;
    }
}
}
