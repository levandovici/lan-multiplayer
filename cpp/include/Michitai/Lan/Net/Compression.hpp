//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <cstdint>
#include <vector>


namespace Michitai::Lan::Net
{
    /// <summary>
    /// Provides GZip compression for message payloads. Compression is applied
    /// transparently by the transport when it reduces the payload size.
    /// Implemented with miniz raw deflate inside an RFC 1952 gzip container,
    /// wire-compatible with the .NET GZipStream payloads of the C# build.
    /// </summary>
    class Compressor
    {
    public:
        /// <summary>Enables or disables compression globally. Default is true.</summary>
        static bool Enabled;

        /// <summary>
        /// Payloads smaller than this many bytes are never compressed,
        /// since compression overhead would exceed the savings.
        /// </summary>
        static int Threshold;

        /// <summary>Compresses the specified data using GZip.</summary>
        static std::vector<std::uint8_t> Compress(const std::vector<std::uint8_t>& data);

        /// <summary>Decompresses GZip-compressed data (zlib-wrapped input is also accepted).</summary>
        static std::vector<std::uint8_t> Decompress(const std::vector<std::uint8_t>& data);

        /// <summary>
        /// Compresses the data only when compression is enabled, the data exceeds
        /// the threshold, and the result is smaller than the input.
        /// </summary>
        static bool TryCompress(const std::vector<std::uint8_t>& data, std::vector<std::uint8_t>& result);
    };
}
