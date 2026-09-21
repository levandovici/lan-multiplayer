//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <functional>
#include <string>

#include <Michitai/Lan/Events.hpp>


namespace Michitai::Lan::Debug
{
    /// <summary>
    /// Library debugging.
    /// </summary>
    class DebugConsole
    {
    public:
        using LogActionDelegate = std::function<void(const std::string&)>;

        static Event<const std::string&> OnLog;
        static Event<const std::string&> OnLogWarning;
        static Event<const std::string&> OnLogError;

        /// <summary>
        /// Enable or Disable DebugConsole, by Default = False
        /// </summary>
        static bool Enabled;

        static void ClearEvents();

        static void Log(const std::string& message);
        static void LogWarning(const std::string& message);
        static void LogError(const std::string& message);
    };
}
