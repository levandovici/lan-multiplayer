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
    /// Interface for JSON storage with serialization and deserialization capabilities.
    /// </summary>
    public interface IJsonStorage
    {
        /// <summary>
        /// Gets or sets the JSON string representation of the data.
        /// </summary>
        string Json { get; set; }

        /// <summary>
        /// Deserializes the JSON data to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <returns>The deserialized object.</returns>
        T Get<T>();

        /// <summary>
        /// Serializes the specified object to JSON.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="@object">The object to serialize.</param>
        void Set<T>(T @object);
    }
}
