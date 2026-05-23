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
    /// Represents a message with an associated network endpoint (IP and port).
    /// </summary>
    public sealed class LocatedMessage
    {
        private IPEndPoint _point;

        private AppMessage _message;

        /// <summary>
        /// Gets or sets the IP endpoint of the message source/destination.
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
        /// Gets or sets the application message.
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
        /// Initializes a new instance of LocatedMessage with the specified endpoint and message.
        /// </summary>
        /// <param name="point">The IP endpoint.</param>
        /// <param name="message">The application message.</param>
        public LocatedMessage(IPEndPoint point, AppMessage message)
    {
        _point = point;

        _message = message;
    }



        /// <summary>
        /// Returns a string representation of the located message.
        /// </summary>
        /// <returns>A string containing the IP endpoint and message.</returns>
        public override string ToString()
    {
        return $"IP End Point: {IPEndPoint}\t App Message: {Message}";
    }
}
}
