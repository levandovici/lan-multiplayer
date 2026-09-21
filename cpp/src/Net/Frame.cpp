//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include <Michitai/Lan/Net/Frame.hpp>
#include <Michitai/Lan/Net/Compression.hpp>

#include <cstring>
#include <stdexcept>


namespace Michitai::Lan::Net
{
    int Frame::MaxPayloadSize = 32 * 1024 * 1024;

    std::vector<std::uint8_t> Frame::Encode(const std::vector<std::uint8_t>& payload, std::uint8_t flags)
    {
        std::vector<std::uint8_t> frame(HeaderSize + payload.size());

        const std::size_t length = payload.size();

        frame[0] = static_cast<std::uint8_t>(length);
        frame[1] = static_cast<std::uint8_t>(length >> 8);
        frame[2] = static_cast<std::uint8_t>(length >> 16);
        frame[3] = static_cast<std::uint8_t>(length >> 24);
        frame[4] = flags;

        std::memcpy(frame.data() + HeaderSize, payload.data(), payload.size());

        return frame;
    }

    std::vector<std::uint8_t> Frame::Pack(const std::string& message)
    {
        std::vector<std::uint8_t> payload(message.begin(), message.end());

        const bool compressed = Compressor::TryCompress(payload, payload);

        return Encode(payload, compressed ? FlagCompressed : FlagNone);
    }

    std::string Frame::Unpack(const std::vector<std::uint8_t>& payload, std::uint8_t flags)
    {
        std::vector<std::uint8_t> data = payload;

        if ((flags & FlagCompressed) == FlagCompressed)
        {
            data = Compressor::Decompress(data);
        }

        return std::string(data.begin(), data.end());
    }


    FrameBuffer::FrameBuffer(int capacity)
        : _data(static_cast<std::size_t>(capacity))
    {
    }

    int FrameBuffer::BufferedCount() const
    {
        std::lock_guard lock(_lock);
        return _end - _start;
    }

    void FrameBuffer::Write(const std::uint8_t* source, int offset, int count)
    {
        if (count <= 0)
            return;

        std::lock_guard lock(_lock);

        Ensure(count);

        std::memcpy(_data.data() + _end, source + offset, static_cast<std::size_t>(count));

        _end += count;
    }

    bool FrameBuffer::TryRead(std::vector<std::uint8_t>& payload, std::uint8_t& flags)
    {
        payload.clear();
        flags = Frame::FlagNone;

        std::lock_guard lock(_lock);

        const int available = _end - _start;

        if (available < Frame::HeaderSize)
            return false;

        const int length = _data[_start]
                         | (_data[_start + 1] << 8)
                         | (_data[_start + 2] << 16)
                         | (_data[_start + 3] << 24);

        if (length < 0 || length > Frame::MaxPayloadSize)
        {
            throw std::runtime_error("Invalid frame payload size: " + std::to_string(length));
        }

        if (available < Frame::HeaderSize + length)
            return false;

        flags = _data[_start + 4];

        payload.assign(_data.begin() + _start + Frame::HeaderSize,
                       _data.begin() + _start + Frame::HeaderSize + length);

        _start += Frame::HeaderSize + length;

        if (_start == _end)
        {
            _start = 0;
            _end = 0;
        }

        return true;
    }

    void FrameBuffer::Clear()
    {
        std::lock_guard lock(_lock);
        _start = 0;
        _end = 0;
    }

    void FrameBuffer::Ensure(int incoming)
    {
        if (static_cast<int>(_data.size()) - _end >= incoming)
            return;

        const int buffered = _end - _start;
        const int needed = buffered + incoming;

        if (_start > 0 && static_cast<int>(_data.size()) >= needed)
        {
            std::memmove(_data.data(), _data.data() + _start, static_cast<std::size_t>(buffered));
            _start = 0;
            _end = buffered;
            return;
        }

        std::size_t capacity = _data.size();
        while (static_cast<int>(capacity) < needed)
        {
            capacity *= 2;
        }

        std::vector<std::uint8_t> grown(capacity);
        std::memcpy(grown.data(), _data.data() + _start, static_cast<std::size_t>(buffered));

        _data = std::move(grown);
        _start = 0;
        _end = buffered;
    }
}
