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
    /// Represents a message with an associated identifier.
    /// </summary>
    public sealed class IdentifiedMessage
    {
        private string _id;

        private Message _message;

        /// <summary>
        /// Gets the message identifier.
        /// </summary>
        public string ID => _id;

        /// <summary>
        /// Gets the message content.
        /// </summary>
        public Message Message => _message;

        /// <summary>
        /// Initializes a new instance of IdentifiedMessage with the specified message and ID.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <param name="id">The message identifier.</param>
        public IdentifiedMessage(Message message, string id)
    {
        _message = message;

        _id = id;
    }
}
}
