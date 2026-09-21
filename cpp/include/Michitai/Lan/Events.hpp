//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <functional>
#include <mutex>
#include <vector>


namespace Michitai::Lan
{
    /// <summary>
    /// Multicast event replacing C# `event Action&lt;...&gt;`.
    /// Handlers are added with +=, removed all at once by assigning nullptr (like `Event = null`),
    /// and invoked through Invoke(). A snapshot is taken before dispatch so handlers may
    /// safely subscribe, unsubscribe or destroy the owner from inside a callback.
    /// </summary>
    template<typename... Args>
    class Event
    {
    public:
        using Handler = std::function<void(Args...)>;

        Event() = default;

        Event(const Event&) = delete;
        Event& operator=(const Event&) = delete;

        /// <summary>Subscribes a handler.</summary>
        Event& operator+=(Handler handler)
        {
            std::lock_guard lock(_mutex);
            _handlers.push_back(std::move(handler));
            return *this;
        }

        /// <summary>Clears all handlers (equivalent of C# `event = null`).</summary>
        Event& operator=(std::nullptr_t)
        {
            Clear();
            return *this;
        }

        /// <summary>Gets whether any handler is subscribed.</summary>
        bool HasHandlers() const
        {
            std::lock_guard lock(_mutex);
            return !_handlers.empty();
        }

        /// <summary>Removes every subscribed handler.</summary>
        void Clear()
        {
            std::lock_guard lock(_mutex);
            _handlers.clear();
        }

        /// <summary>Invokes all subscribed handlers on a snapshot.</summary>
        void Invoke(Args... args) const
        {
            std::vector<Handler> handlers;
            {
                std::lock_guard lock(_mutex);
                handlers = _handlers;
            }
            for (const Handler& handler : handlers)
            {
                handler(args...);
            }
        }

    private:
        mutable std::mutex _mutex;
        std::vector<Handler> _handlers;
    };
}
