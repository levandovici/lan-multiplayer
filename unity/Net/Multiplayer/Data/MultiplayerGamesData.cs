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

namespace Michitai.Lan.Net.Multiplayer.Data
{
    /// <summary>
    /// Container for storing multiplayer game data including servers and clients.
    /// </summary>
    [Serializable]
    public sealed class MultiplayerGamesData
    {
        /// <summary>
        /// Array of server game data.
        /// </summary>
        public ServerGameData[] Servers;

        /// <summary>
        /// Array of client game data.
        /// </summary>
        public ClientGameData[] Clients;

        /// <summary>
        /// Initializes a new instance of MultiplayerGamesData with empty arrays.
        /// </summary>
        public MultiplayerGamesData()
    {
        Servers = new ServerGameData[0];

        Clients = new ClientGameData[0];
    }
}
}
