//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

#include <random>
#include <stdexcept>
#include <vector>


namespace Michitai::Lan::Net
{
    /// <summary>
    /// Represents a range of network ports with predefined ranges and port management functionality.
    /// </summary>
    struct PortRange
    {
        /// <summary>System ports (0-1023).</summary>
        static PortRange System() { return PortRange(0, 1023); }

        /// <summary>Registered ports (1024-49151).</summary>
        static PortRange Registered() { return PortRange(1024, 49151); }

        /// <summary>Dynamic/private ports (49152-65535).</summary>
        static PortRange Dynamic() { return PortRange(49152, 65535); }

        /// <summary>All valid ports (Min-Max).</summary>
        static PortRange All() { return PortRange(Min, Max); }

        /// <summary>Broadcast ports (1024 ports, 64512-65535).</summary>
        static PortRange Broadcast() { return PortRange(64512, 65535); }

        /// <summary>Simplified broadcast ports (128 ports, 65408-65535).</summary>
        static PortRange BroadcastSimplified() { return PortRange(65408, 65535); }

        /// <summary>Minimum valid port number.</summary>
        static constexpr int Min = 1024;

        /// <summary>Maximum valid port number.</summary>
        static constexpr int Max = 65535;

        /// <summary>The first port in the range.</summary>
        int First;

        /// <summary>The last port in the range.</summary>
        int Last;

        /// <summary>Gets the total number of ports in the range.</summary>
        int Count() const { return Last - First + 1; }

        class Store;

        /// <summary>
        /// Initializes a new instance of PortRange with the specified first and last ports.
        /// Throws std::out_of_range when outside [Min, Max].
        /// </summary>
        PortRange(int first, int last) : First(first), Last(last)
        {
            if (first < Min)
                throw std::out_of_range("First can't be less than PortRange.Min");
            if (last > Max)
                throw std::out_of_range("Last can't be more than PortRange.Max");
        }


        /// <summary>
        /// Tracks and hands out random unused ports from a range.
        /// </summary>
        class Store
        {
        public:
            /// <summary>Initializes a new instance of Store with all ports from the specified range.</summary>
            explicit Store(const PortRange& range)
            {
                for (int port = range.First; port <= range.Last; port++)
                {
                    _ports.push_back(port);
                }
            }

            /// <summary>
            /// Removes and returns a random port. Throws std::out_of_range when the store is empty.
            /// </summary>
            int RandomPort()
            {
                if (_ports.empty())
                    throw std::out_of_range("PortRange.Store is Empty! All ports are in use!");

                std::uniform_int_distribution<std::size_t> pick(0, _ports.size() - 1);

                const std::size_t index = pick(_random);
                const int port = _ports[index];

                _ports.erase(_ports.begin() + index);

                return port;
            }

        private:
            std::mt19937 _random{std::random_device{}()};
            std::vector<int> _ports;
        };

        /// <summary>
        /// Gets a port store for managing available ports in this range.
        /// </summary>
        Store RangeStore() const { return Store(*this); }
    };
}
