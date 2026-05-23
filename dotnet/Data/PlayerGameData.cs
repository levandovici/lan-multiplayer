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


namespace Michitai.Lan.Data
{
    /// <summary>
    /// Represents player game data with JSON serialization capabilities.
    /// </summary>
    public sealed class PlayerGameData : IJsonStorage
    {
        private string _json;

        private readonly object _json_lock;

        /// <summary>
        /// Gets or sets the JSON string representation of the player game data.
        /// </summary>
        public string Json
    {
        get
        {
            lock (_json_lock)
            {
                return _json;
            }
        }

        set
        {
            lock (_json_lock)
            {
                _json = value;
            }
        }
    }



        /// <summary>
        /// Initializes a new instance of PlayerGameData with the specified JSON string.
        /// </summary>
        /// <param name="json">The JSON string to initialize with.</param>
        public PlayerGameData(string json)
    {
        _json = json;

        _json_lock = new object();
    }

        /// <summary>
        /// Initializes a new instance of PlayerGameData with an empty JSON string.
        /// </summary>
        public PlayerGameData()
    {
        _json = "";

        _json_lock = new object();
    }



        /// <summary>
        /// Deserializes the JSON data to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <returns>The deserialized object.</returns>
        public T Get<T>()
    {
        return JsonSerializer.Deserialize<T>(Json);
    }

        /// <summary>
        /// Serializes the specified object to JSON.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="@object">The object to serialize.</param>
        public void Set<T>(T @object)
    {
        Json = JsonSerializer.Serialize(@object);
    }
}
}
