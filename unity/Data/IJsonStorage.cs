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
    /// Interface for JSON storage operations, providing serialization and deserialization capabilities.
    /// </summary>
    public interface IJsonStorage
    {
        /// <summary>
        /// Gets or sets the JSON string representation of the stored data.
        /// </summary>
        string Json { get; set; }

        /// <summary>
        /// Deserializes the JSON string into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize into.</typeparam>
        /// <returns>The deserialized object of type T.</returns>
        T Get<T>();

        /// <summary>
        /// Serializes the specified object to JSON and stores it.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="@object">The object to serialize to JSON.</param>
        void Set<T>(T @object);
    }
}
