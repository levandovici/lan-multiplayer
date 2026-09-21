//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#include <Michitai/Lan/Debugging/DebugConsole.hpp>


namespace Michitai::Lan::Debug
{
    Event<const std::string&> DebugConsole::OnLog;
    Event<const std::string&> DebugConsole::OnLogWarning;
    Event<const std::string&> DebugConsole::OnLogError;

    bool DebugConsole::Enabled = false;

    void DebugConsole::ClearEvents()
    {
        OnLog = nullptr;
        OnLogWarning = nullptr;
        OnLogError = nullptr;
    }

    void DebugConsole::Log(const std::string& message)
    {
        if (!Enabled)
            return;

        OnLog.Invoke(message);
    }

    void DebugConsole::LogWarning(const std::string& message)
    {
        if (!Enabled)
            return;

        OnLogWarning.Invoke(message);
    }

    void DebugConsole::LogError(const std::string& message)
    {
        if (!Enabled)
            return;

        OnLogError.Invoke(message);
    }
}
