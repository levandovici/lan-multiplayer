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

namespace Michitai.Lan.Data
{
    /// <summary>
    /// Thread-safe implementation of JSON storage using Unity's JsonUtility for serialization.
    /// </summary>
    [Serializable]
    public sealed class JsonStorage : IJsonStorage
    {
        /// <summary>
        /// The raw JSON string representation of the stored data.
        /// </summary>
        public string json;

        /// <summary>
        /// Lock object for thread-safe access to the JSON data.
        /// </summary>
        private object _json_lock;

        /// <summary>
        /// Gets or sets the JSON string in a thread-safe manner.
        /// </summary>
        public string Json
        {
            get
            {
                lock (_json_lock)
                {
                    return json;
                }
            }

            set
            {
                lock (_json_lock)
                {
                    json = value;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of JsonStorage with an empty JSON string.
        /// </summary>
        public JsonStorage()
        {
            json = "";

            _json_lock = new object();
        }

        /// <summary>
        /// Initializes a new instance of JsonStorage with the specified JSON string.
        /// </summary>
        /// <param name="json">The initial JSON string.</param>
        public JsonStorage(string json)
        {
            this.json = json;

            _json_lock = new object();
        }

        /// <summary>
        /// Deserializes the JSON string into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize into.</typeparam>
        /// <returns>The deserialized object of type T.</returns>
        public T Get<T>()
        {
            return JsonUtility.FromJson<T>(Json);
        }

        /// <summary>
        /// Serializes the specified object to JSON and stores it in a thread-safe manner.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="@object">The object to serialize to JSON.</param>
        public void Set<T>(T @object)
        {
            Json = JsonUtility.ToJson(@object);
        }
    }
}
