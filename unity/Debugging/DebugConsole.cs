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

namespace Michitai.Lan.Debug
{
    /// <summary>
    /// Provides static debug logging functionality with event-based notification system.
    /// </summary>
    public static class DebugConsole
    {
        /// <summary>
        /// Delegate for log action callbacks.
        /// </summary>
        /// <param name="log">The log message.</param>
        public delegate void LogActionDelegate(string log);

        private static bool _debug = false;

        /// <summary>
        /// Event raised when a log message is generated.
        /// </summary>
        public static event LogActionDelegate OnLog;

        /// <summary>
        /// Event raised when a warning message is generated.
        /// </summary>
        public static event LogActionDelegate OnLogWarning;

        /// <summary>
        /// Event raised when an error message is generated.
        /// </summary>
        public static event LogActionDelegate OnLogError;

        /// <summary>
        /// Gets or sets whether the debug console is enabled. Defaults to false.
        /// </summary>
        public static bool Enabled
    {
        get
        {
            return _debug;
        }

        set
        {
            _debug = value;
        }
    }



        /// <summary>
        /// Clears all event subscribers.
        /// </summary>
        public static void ClearEvents()
    {
        OnLog = null;

        OnLogWarning = null;

        OnLogError = null;
    }



        /// <summary>
        /// Logs a message if the debug console is enabled.
        /// </summary>
        /// <param name="message">The message to log.</param>
        internal static void Log(string message)
    {
        if (!Enabled)
            return;

        OnLog?.Invoke(message);
    }

        /// <summary>
        /// Logs a warning message if the debug console is enabled.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        internal static void LogWarning(string message)
    {
        if (!Enabled)
            return;

        OnLogWarning?.Invoke(message);
    }

        /// <summary>
        /// Logs an error message if the debug console is enabled.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        internal static void LogError(string message)
    {
        if (!Enabled)
            return;

        OnLogError?.Invoke(message);
    }
}
}
