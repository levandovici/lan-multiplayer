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


namespace Michitai.Lan.Net.Multiplayer.Chat
{
    /// <summary>
    /// Represents a single chat message with an ID and message content.
    /// </summary>
    public sealed class Letter
    {
        private string _id;

        private string _message;

        private object _id_lock;

        private object _message_lock;

        /// <summary>
        /// Gets or sets the letter ID. Thread-safe.
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
        /// Gets or sets the message content. Thread-safe.
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
        /// Initializes a new instance of Letter with the specified ID and message.
        /// </summary>
        /// <param name="id">The letter ID.</param>
        /// <param name="message">The message content.</param>
        public Letter(string id, string message)
    {
        _id = id;

        _message = message;


        _id_lock = new object();

        _message_lock = new object();
    }

        /// <summary>
        /// Initializes a new instance of Letter with empty values.
        /// </summary>
        public Letter()
    {
        _id_lock = new object();

        _message_lock = new object();
    }
}
}
