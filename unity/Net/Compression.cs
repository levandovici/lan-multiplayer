//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.IO.Compression;
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

namespace Michitai.Lan.Net
{
    /// <summary>
    /// Provides GZip compression for message payloads. Compression is applied
    /// transparently by the transport when it reduces the payload size.
    /// </summary>
    public static class Compressor
    {
        /// <summary>
        /// Enables or disables compression globally. Default is true.
        /// </summary>
        public static bool Enabled = true;

        /// <summary>
        /// Payloads smaller than this many bytes are never compressed,
        /// since compression overhead would exceed the savings.
        /// </summary>
        public static int Threshold = 256;

        /// <summary>
        /// Compresses the specified data using GZip.
        /// </summary>
        /// <param name="data">The data to compress.</param>
        /// <returns>The compressed data.</returns>
        public static byte[] Compress(byte[] data)
    {
        using (MemoryStream output = new MemoryStream())
        {
            using (GZipStream gzip = new GZipStream(output, CompressionMode.Compress, true))
            {
                gzip.Write(data, 0, data.Length);
            }

            return output.ToArray();
        }
    }

        /// <summary>
        /// Decompresses GZip-compressed data.
        /// </summary>
        /// <param name="data">The compressed data.</param>
        /// <returns>The decompressed data.</returns>
        public static byte[] Decompress(byte[] data)
    {
        using (MemoryStream input = new MemoryStream(data))
        using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress))
        using (MemoryStream output = new MemoryStream())
        {
            gzip.CopyTo(output);

            return output.ToArray();
        }
    }

        /// <summary>
        /// Compresses the data only when compression is enabled, the data exceeds
        /// the threshold, and the result is smaller than the input.
        /// </summary>
        /// <param name="data">The data to compress.</param>
        /// <param name="result">When this method returns, contains either the compressed or the original data.</param>
        /// <returns>True if the data was compressed; otherwise, false.</returns>
        public static bool TryCompress(byte[] data, out byte[] result)
    {
        if (!Enabled || data == null || data.Length < Threshold)
        {
            result = data;

            return false;
        }

        byte[] compressed = Compress(data);

        if (compressed.Length < data.Length)
        {
            result = compressed;

            return true;
        }

        result = data;

        return false;
    }
}
}
