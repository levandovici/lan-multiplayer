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
    /// Represents a message that includes its network location (IP endpoint).
    /// </summary>
    public sealed class LocatedMessage
    {
        private IPEndPoint _point;

        private AppMessage _message;



        /// <summary>
        /// Gets or sets the IP endpoint where the message originated or is destined.
        /// </summary>
        public IPEndPoint IPEndPoint
        {
            get
            {
                return _point;
            }

            set
            {
                _point = value;
            }
        }

        /// <summary>
        /// Gets or sets the application message content.
        /// </summary>
        public AppMessage Message
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
        /// Initializes a new instance of LocatedMessage with the specified IP endpoint and message.
        /// </summary>
        /// <param name="point">The IP endpoint associated with the message.</param>
        /// <param name="message">The application message content.</param>
        public LocatedMessage(IPEndPoint point, AppMessage message)
        {
            _point = point;

            _message = message;
        }



        /// <summary>
        /// Returns a string representation of the located message.
        /// </summary>
        /// <returns>A string containing the IP endpoint and message content.</returns>
        public override string ToString()
        {
            return $"IP End Point: {IPEndPoint}\t App Message: {Message}";
        }
}
}
