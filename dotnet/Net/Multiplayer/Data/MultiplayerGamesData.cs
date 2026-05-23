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


namespace Michitai.Lan.Net.Multiplayer.Data
{
    /// <summary>
    /// Container for multiplayer game data including servers and clients.
    /// </summary>
    [Serializable]
    public sealed class MultiplayerGamesData
    {
        private ServerGameData[] _servers;

        private ClientGameData[] _clients;

        /// <summary>
        /// Gets or sets the array of server game data.
        /// </summary>
        public ServerGameData[] ServersData
    {
        get
        {
            return _servers;
        }

        set
        {
            _servers = value;
        }
    }

        /// <summary>
        /// Gets or sets the array of client game data.
        /// </summary>
        public ClientGameData[] ClientsData
    {
        get
        {
            return _clients;
        }

        set
        {
            _clients = value;
        }
    }



        /// <summary>
        /// Initializes a new instance of MultiplayerGamesData with empty arrays.
        /// </summary>
        public MultiplayerGamesData()
    {
        _servers = new ServerGameData[0];

        _clients = new ClientGameData[0];
    }
}
}
