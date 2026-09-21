//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include <Michitai/Lan/Net/Compression.hpp>

#include <miniz.h>

#include <stdexcept>
#include <string>


namespace Michitai::Lan::Net
{
    bool Compressor::Enabled = true;
    int Compressor::Threshold = 256;

    namespace
    {
        void AppendLE32(std::vector<std::uint8_t>& out, std::uint32_t value)
        {
            out.push_back(static_cast<std::uint8_t>(value));
            out.push_back(static_cast<std::uint8_t>(value >> 8));
            out.push_back(static_cast<std::uint8_t>(value >> 16));
            out.push_back(static_cast<std::uint8_t>(value >> 24));
        }

        std::uint32_t ReadLE32(const std::uint8_t* data)
        {
            return static_cast<std::uint32_t>(data[0])
                 | (static_cast<std::uint32_t>(data[1]) << 8)
                 | (static_cast<std::uint32_t>(data[2]) << 16)
                 | (static_cast<std::uint32_t>(data[3]) << 24);
        }

        std::vector<std::uint8_t> RawDeflate(const std::vector<std::uint8_t>& data)
        {
            mz_stream stream{};
            stream.next_in = data.data();
            stream.avail_in = static_cast<unsigned int>(data.size());

            if (mz_deflateInit2(&stream, MZ_DEFAULT_COMPRESSION, MZ_DEFLATED,
                                -MZ_DEFAULT_WINDOW_BITS, 8, MZ_DEFAULT_STRATEGY) != MZ_OK)
            {
                throw std::runtime_error("Compressor: deflate init failed");
            }

            std::vector<std::uint8_t> output(mz_compressBound(static_cast<mz_ulong>(data.size())));

            stream.next_out = output.data();
            stream.avail_out = static_cast<unsigned int>(output.size());

            const int status = mz_deflate(&stream, MZ_FINISH);
            mz_deflateEnd(&stream);

            if (status != MZ_STREAM_END)
                throw std::runtime_error("Compressor: deflate failed");

            output.resize(stream.total_out);
            return output;
        }

        std::vector<std::uint8_t> RawInflate(const std::uint8_t* data, std::size_t size,
                                             std::size_t expectedSize)
        {
            std::vector<std::uint8_t> output(expectedSize > 0 ? expectedSize : 64);

            mz_stream stream{};
            stream.next_in = data;
            stream.avail_in = static_cast<unsigned int>(size);

            if (mz_inflateInit2(&stream, -MZ_DEFAULT_WINDOW_BITS) != MZ_OK)
                throw std::runtime_error("Compressor: inflate init failed");

            int status = MZ_OK;
            std::size_t produced = 0;

            while (status == MZ_OK)
            {
                if (produced == output.size())
                {
                    output.resize(output.size() * 2);
                }

                stream.next_out = output.data() + produced;
                stream.avail_out = static_cast<unsigned int>(output.size() - produced);

                status = mz_inflate(&stream, MZ_NO_FLUSH);
                produced = stream.total_out;
            }

            mz_inflateEnd(&stream);

            if (status != MZ_STREAM_END)
                throw std::runtime_error("Compressor: corrupt compressed payload");

            output.resize(produced);
            return output;
        }
    }

    std::vector<std::uint8_t> Compressor::Compress(const std::vector<std::uint8_t>& data)
    {
        std::vector<std::uint8_t> result;

        // RFC 1952 header: magic, deflate method, no flags, zero mtime, no extra, OS unknown.
        const std::uint8_t header[] = {0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF};
        result.assign(std::begin(header), std::end(header));

        std::vector<std::uint8_t> deflate = RawDeflate(data);
        result.insert(result.end(), deflate.begin(), deflate.end());

        AppendLE32(result, mz_crc32(MZ_CRC32_INIT, data.data(), data.size()));
        AppendLE32(result, static_cast<std::uint32_t>(data.size()));

        return result;
    }

    std::vector<std::uint8_t> Compressor::Decompress(const std::vector<std::uint8_t>& data)
    {
        if (data.size() < 18)
            throw std::runtime_error("Compressor: payload too small for gzip");

        const bool isGzip = data[0] == 0x1F && data[1] == 0x8B && data[2] == 0x08;
        const bool isZlib = data[0] == 0x78;

        if (!isGzip && !isZlib)
            throw std::runtime_error("Compressor: not a gzip payload");

        if (isZlib)
        {
            // zlib-wrapped deflate (accepted for robustness; the wire format is gzip).
            return RawInflate(data.data() + 2, data.size() - 2, 0);
        }

        // Skip gzip header + optional fields.
        std::size_t offset = 10;
        const std::uint8_t flags = data[3];

        if (flags & 0x04) // FEXTRA
        {
            if (offset + 2 > data.size()) throw std::runtime_error("Compressor: truncated gzip");
            const std::size_t extra = data[offset] | (static_cast<std::size_t>(data[offset + 1]) << 8);
            offset += 2 + extra;
        }
        if (flags & 0x08) // FNAME — zero-terminated
        {
            while (offset < data.size() && data[offset] != 0) offset++;
            offset++;
        }
        if (flags & 0x10) // FCOMMENT — zero-terminated
        {
            while (offset < data.size() && data[offset] != 0) offset++;
            offset++;
        }
        if (flags & 0x02) // FHCRC
        {
            offset += 2;
        }

        if (offset + 8 > data.size())
            throw std::runtime_error("Compressor: truncated gzip");

        const std::uint32_t expected = ReadLE32(data.data() + data.size() - 4);

        std::vector<std::uint8_t> output = RawInflate(data.data() + offset, data.size() - offset - 8, expected);

        if (output.size() != expected)
            throw std::runtime_error("Compressor: gzip size mismatch");

        if (mz_crc32(MZ_CRC32_INIT, output.data(), output.size()) !=
            ReadLE32(data.data() + data.size() - 8))
        {
            throw std::runtime_error("Compressor: gzip CRC mismatch");
        }

        return output;
    }

    bool Compressor::TryCompress(const std::vector<std::uint8_t>& data, std::vector<std::uint8_t>& result)
    {
        if (!Enabled || data.size() < static_cast<std::size_t>(Threshold))
        {
            result = data;
            return false;
        }

        std::vector<std::uint8_t> compressed = Compress(data);

        if (compressed.size() < data.size())
        {
            result = std::move(compressed);
            return true;
        }

        result = data;
        return false;
    }
}
