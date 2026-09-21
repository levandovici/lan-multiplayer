//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <cstdint>
#include <mutex>
#include <string>
#include <vector>


namespace Michitai::Lan::Net
{
    /// <summary>
    /// Binary wire frame: [4-byte little-endian payload length][1-byte flags][payload].
    /// Replaces the legacy string delimiter framing and supports multiple messages per TCP read.
    /// </summary>
    class Frame
    {
    public:
        /// <summary>Frame header size in bytes (4-byte length + 1-byte flags).</summary>
        static constexpr int HeaderSize = 5;

        /// <summary>No flags set.</summary>
        static constexpr std::uint8_t FlagNone = 0x00;

        /// <summary>Payload is GZip-compressed.</summary>
        static constexpr std::uint8_t FlagCompressed = 0x01;

        /// <summary>
        /// Maximum accepted payload size in bytes. Frames announcing more are rejected as corrupt.
        /// </summary>
        static int MaxPayloadSize;

        /// <summary>Encodes a payload into a wire frame with the specified flags.</summary>
        static std::vector<std::uint8_t> Encode(const std::vector<std::uint8_t>& payload, std::uint8_t flags);

        /// <summary>Packs a string message into a wire frame, compressing the payload when beneficial.</summary>
        static std::vector<std::uint8_t> Pack(const std::string& message);

        /// <summary>Unpacks a frame payload back into a string message, decompressing when flagged.</summary>
        static std::string Unpack(const std::vector<std::uint8_t>& payload, std::uint8_t flags);
    };


    /// <summary>
    /// Accumulates bytes read from a stream and extracts complete frames,
    /// handling partial reads and multiple frames per read.
    /// </summary>
    class FrameBuffer
    {
    public:
        /// <summary>Initializes a new instance of FrameBuffer with the specified initial capacity.</summary>
        explicit FrameBuffer(int capacity = 8192);

        /// <summary>Gets the number of buffered bytes not yet consumed.</summary>
        int BufferedCount() const;

        /// <summary>Appends bytes to the buffer.</summary>
        void Write(const std::uint8_t* source, int offset, int count);

        /// <summary>
        /// Tries to extract one complete frame from the buffer.
        /// Throws std::runtime_error when a frame announces an invalid payload size.
        /// </summary>
        bool TryRead(std::vector<std::uint8_t>& payload, std::uint8_t& flags);

        /// <summary>Clears all buffered data.</summary>
        void Clear();

    private:
        /// <summary>
        /// Ensures the buffer can accept the specified number of additional bytes,
        /// compacting or growing the internal storage when needed.
        /// </summary>
        void Ensure(int incoming);

        std::vector<std::uint8_t> _data;
        int _start = 0;
        int _end = 0;
        mutable std::mutex _lock;
    };
}
