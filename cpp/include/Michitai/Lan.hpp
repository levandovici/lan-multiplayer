//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

#pragma once

// Foundation
#include <Michitai/Lan/EPlatform.hpp>
#include <Michitai/Lan/Events.hpp>
#include <Michitai/Lan/Guid.hpp>
#include <Michitai/Lan/Json.hpp>
#include <Michitai/Lan/JsonSerializer.hpp>

// Data
#include <Michitai/Lan/Data/JsonStorage.hpp>
#include <Michitai/Lan/Data/PlayerData.hpp>

// Debugging
#include <Michitai/Lan/Debugging/DebugConsole.hpp>

// Net core
#include <Michitai/Lan/Net/Compression.hpp>
#include <Michitai/Lan/Net/EndPoint.hpp>
#include <Michitai/Lan/Net/Frame.hpp>
#include <Michitai/Lan/Net/Lan.hpp>
#include <Michitai/Lan/Net/Messages.hpp>
#include <Michitai/Lan/Net/PortRange.hpp>

// Transports
#include <Michitai/Lan/Net/TCPClient.hpp>
#include <Michitai/Lan/Net/TCPServer.hpp>
#include <Michitai/Lan/Net/UDPBroadcast.hpp>
#include <Michitai/Lan/Net/UDPChannel.hpp>

// Multiplayer
#include <Michitai/Lan/Net/Multiplayer/Broadcast.hpp>
#include <Michitai/Lan/Net/Multiplayer/Chat.hpp>
#include <Michitai/Lan/Net/Multiplayer/Client.hpp>
#include <Michitai/Lan/Net/Multiplayer/Commands.hpp>
#include <Michitai/Lan/Net/Multiplayer/Data.hpp>
#include <Michitai/Lan/Net/Multiplayer/Multiplayer.hpp>
#include <Michitai/Lan/Net/Multiplayer/Server.hpp>
