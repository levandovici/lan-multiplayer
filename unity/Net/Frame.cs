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

namespace Michitai.Lan.Net
{
    /// <summary>
    /// Binary wire frame: [4-byte little-endian payload length][1-byte flags][payload].
    /// Replaces the legacy string delimiter framing and supports multiple messages per TCP read.
    /// </summary>
    public static class Frame
    {
        /// <summary>
        /// Frame header size in bytes (4-byte length + 1-byte flags).
        /// </summary>
        public const int HeaderSize = 5;

        /// <summary>
        /// No flags set.
        /// </summary>
        public const byte FlagNone = 0x00;

        /// <summary>
        /// Payload is GZip-compressed.
        /// </summary>
        public const byte FlagCompressed = 0x01;

        /// <summary>
        /// Maximum accepted payload size in bytes. Frames announcing more are rejected as corrupt.
        /// </summary>
        public static int MaxPayloadSize = 32 * 1024 * 1024;

        /// <summary>
        /// Encodes a payload into a wire frame with the specified flags.
        /// </summary>
        /// <param name="payload">The payload bytes.</param>
        /// <param name="flags">The frame flags.</param>
        /// <returns>The encoded frame bytes.</returns>
        public static byte[] Encode(byte[] payload, byte flags)
    {
        byte[] frame = new byte[HeaderSize + payload.Length];

        frame[0] = (byte)(payload.Length);
        frame[1] = (byte)(payload.Length >> 8);
        frame[2] = (byte)(payload.Length >> 16);
        frame[3] = (byte)(payload.Length >> 24);
        frame[4] = flags;

        Buffer.BlockCopy(payload, 0, frame, HeaderSize, payload.Length);

        return frame;
    }

        /// <summary>
        /// Packs a string message into a wire frame, compressing the payload when beneficial.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <returns>The encoded frame bytes.</returns>
        public static byte[] Pack(string message)
    {
        byte[] payload = Encoding.UTF8.GetBytes(message ?? string.Empty);

        bool compressed = Compressor.TryCompress(payload, out payload);

        return Encode(payload, compressed ? FlagCompressed : FlagNone);
    }

        /// <summary>
        /// Unpacks a frame payload back into a string message, decompressing when flagged.
        /// </summary>
        /// <param name="payload">The frame payload bytes.</param>
        /// <param name="flags">The frame flags.</param>
        /// <returns>The decoded message.</returns>
        public static string Unpack(byte[] payload, byte flags)
    {
        if ((flags & FlagCompressed) == FlagCompressed)
        {
            payload = Compressor.Decompress(payload);
        }

        return Encoding.UTF8.GetString(payload);
    }
}

    /// <summary>
    /// Accumulates bytes read from a stream and extracts complete frames,
    /// handling partial reads and multiple frames per read.
    /// </summary>
    public sealed class FrameBuffer
    {
        private byte[] _data;

        private int _start;

        private int _end;

        private readonly object _lock;

        /// <summary>
        /// Initializes a new instance of FrameBuffer with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The initial buffer capacity in bytes.</param>
        public FrameBuffer(int capacity = 8192)
    {
        _data = new byte[capacity];

        _start = 0;

        _end = 0;

        _lock = new object();
    }

        /// <summary>
        /// Gets the number of buffered bytes not yet consumed.
        /// </summary>
        public int BufferedCount
    {
        get
        {
            lock (_lock)
            {
                return _end - _start;
            }
        }
    }

        /// <summary>
        /// Appends bytes to the buffer.
        /// </summary>
        /// <param name="source">The source byte array.</param>
        /// <param name="offset">The offset into the source array.</param>
        /// <param name="count">The number of bytes to append.</param>
        public void Write(byte[] source, int offset, int count)
    {
        if (count <= 0)
            return;

        lock (_lock)
        {
            Ensure(count);

            Buffer.BlockCopy(source, offset, _data, _end, count);

            _end += count;
        }
    }

        /// <summary>
        /// Tries to extract one complete frame from the buffer.
        /// </summary>
        /// <param name="payload">When this method returns, contains the frame payload if a frame was read.</param>
        /// <param name="flags">When this method returns, contains the frame flags if a frame was read.</param>
        /// <returns>True if a complete frame was extracted; otherwise, false.</returns>
        /// <exception cref="InvalidDataException">Thrown when a frame announces an invalid payload size.</exception>
        public bool TryRead(out byte[] payload, out byte flags)
    {
        payload = null;

        flags = Frame.FlagNone;

        lock (_lock)
        {
            int available = _end - _start;

            if (available < Frame.HeaderSize)
                return false;

            int length = _data[_start]
                       | (_data[_start + 1] << 8)
                       | (_data[_start + 2] << 16)
                       | (_data[_start + 3] << 24);

            if (length < 0 || length > Frame.MaxPayloadSize)
            {
                throw new InvalidDataException($"Invalid frame payload size: {length}");
            }

            if (available < Frame.HeaderSize + length)
                return false;

            flags = _data[_start + 4];

            payload = new byte[length];

            Buffer.BlockCopy(_data, _start + Frame.HeaderSize, payload, 0, length);

            _start += Frame.HeaderSize + length;

            if (_start == _end)
            {
                _start = 0;

                _end = 0;
            }

            return true;
        }
    }

        /// <summary>
        /// Clears all buffered data.
        /// </summary>
        public void Clear()
    {
        lock (_lock)
        {
            _start = 0;

            _end = 0;
        }
    }

        /// <summary>
        /// Ensures the buffer can accept the specified number of additional bytes,
        /// compacting or growing the internal storage when needed.
        /// </summary>
        /// <param name="incoming">The number of bytes to make room for.</param>
        private void Ensure(int incoming)
    {
        if (_data.Length - _end >= incoming)
            return;

        int buffered = _end - _start;

        int needed = buffered + incoming;

        if (_start > 0 && _data.Length >= needed)
        {
            Buffer.BlockCopy(_data, _start, _data, 0, buffered);

            _start = 0;

            _end = buffered;

            return;
        }

        int capacity = _data.Length;

        while (capacity < needed)
        {
            capacity *= 2;
        }

        byte[] grown = new byte[capacity];

        Buffer.BlockCopy(_data, _start, grown, 0, buffered);

        _data = grown;

        _start = 0;

        _end = buffered;
    }
}
}
