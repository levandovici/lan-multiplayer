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
using System.Runtime.Serialization.Json;


using Michitai.Lan;
using Michitai.Lan.Data;
using Michitai.Lan.Net;
using Michitai.Lan.Net.Multiplayer;
using Michitai.Lan.Net.Multiplayer.Chat;
using Michitai.Lan.Net.Multiplayer.Commands;
using Michitai.Lan.Net.Multiplayer.Data;
using Michitai.Lan.Debug;

namespace Michitai.Lan
{
    /// <summary>
    /// Minimal self-contained JSON serializer built on DataContractJsonSerializer.
    /// Keeps the library dependency-free; serializes public fields and read/write properties.
    /// </summary>
    public static class JsonSerializer
    {
        /// <summary>
        /// Serializes the specified object to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="value">The object to serialize.</param>
        /// <returns>The JSON string representation.</returns>
        public static string Serialize<T>(T value)
    {
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

        using (MemoryStream stream = new MemoryStream())
        {
            serializer.WriteObject(stream, value);

            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }

        /// <summary>
        /// Deserializes a JSON string into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="json">The JSON string.</param>
        /// <returns>The deserialized object.</returns>
        public static T Deserialize<T>(string json)
    {
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

        using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
        {
            return (T)serializer.ReadObject(stream);
        }
    }
}
}
