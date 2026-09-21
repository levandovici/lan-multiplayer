//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once


namespace Michitai::Lan
{
    /// <summary>
    /// Enumeration representing different operating platforms (flag combinable).
    /// </summary>
    enum class EPlatform
    {
        /// <summary>Windows platform</summary>
        Windows = 1,
        /// <summary>Linux platform</summary>
        Linux = 2,
        /// <summary>macOS platform</summary>
        MacOS = 4,
        /// <summary>Standalone desktop platform (Windows + Linux + macOS)</summary>
        Standalone = 7,
        /// <summary>Android platform</summary>
        Android = 8,
        /// <summary>iOS platform</summary>
        IOS = 16,
        /// <summary>Mobile platforms (Android + iOS)</summary>
        Mobile = 24
    };

    constexpr EPlatform operator|(EPlatform a, EPlatform b)
    {
        return static_cast<EPlatform>(static_cast<int>(a) | static_cast<int>(b));
    }

    constexpr EPlatform operator&(EPlatform a, EPlatform b)
    {
        return static_cast<EPlatform>(static_cast<int>(a) & static_cast<int>(b));
    }

    /// <summary>
    /// Equivalent of the C# flag check (platform & flag) == flag.
    /// </summary>
    constexpr bool HasFlag(EPlatform platform, EPlatform flag)
    {
        return (platform & flag) == flag;
    }
}
