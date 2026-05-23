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

namespace Michitai.Lan.Debug
{
    /// <summary>
    /// Library debugging
    /// </summary>
    public static class DebugConsole
    {
        public delegate void LogActionDelegate(string log);



        private static bool _debug = false;



        public static event LogActionDelegate OnLog;

        public static event LogActionDelegate OnLogWarning;

        public static event LogActionDelegate OnLogError;



        /// <summary>
        /// Enable or Disable DebugConsole, by Default = False
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



        public static void ClearEvents()
        {
            OnLog = null;

            OnLogWarning = null;

            OnLogError = null;
        }



        internal static void Log(string message)
        {
            if (!Enabled)
                return;

            OnLog?.Invoke(message);
        }

        internal static void LogWarning(string message)
        {
            if (!Enabled)
                return;

            OnLogWarning?.Invoke(message);
        }

        internal static void LogError(string message)
        {
            if (!Enabled)
                return;

            OnLogError?.Invoke(message);
        }
    }
}