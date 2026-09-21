#-----------------------------------------------------------------------------------------------------------------------------
#  Project :           Michitai.Lan
#  Author  :           Nichita Levandovici
#  Date    :           22.05.2026
#  Email   :           support@michitai.com
#  Website :           https://michitai.com
#-----------------------------------------------------------------------------------------------------------------------------

cmake_minimum_required(VERSION 3.20)

project(michitai-lan VERSION 1.0.0 LANGUAGES CXX)

option(MICHITAI_LAN_BUILD_EXAMPLES "Build the lan-demo example application" ON)
option(MICHITAI_LAN_FETCH_MINIZ "Fetch miniz from GitHub when no miniz target/package is available" ON)

find_package(Threads REQUIRED)


#-----------------------------------------------------------------------------------------------------------------------------
#  miniz (deflate codec used by the GZip-compatible Compressor).
#  Resolution order:
#    1. an existing `miniz` / `miniz::miniz` CMake target (e.g. provided by the host engine build),
#    2. an installed miniz package (find_package CONFIG),
#    3. FetchContent of the pinned 3.1.2 release (disable with -DMICHITAI_LAN_FETCH_MINIZ=OFF).
#-----------------------------------------------------------------------------------------------------------------------------

set(MICHITAI_LAN_MINIZ_TARGET "")

if(TARGET miniz::miniz)
    set(MICHITAI_LAN_MINIZ_TARGET miniz::miniz)
elseif(TARGET miniz)
    set(MICHITAI_LAN_MINIZ_TARGET miniz)
else()
    find_package(miniz CONFIG QUIET)
    if(TARGET miniz::miniz)
        set(MICHITAI_LAN_MINIZ_TARGET miniz::miniz)
    elseif(TARGET miniz)
        set(MICHITAI_LAN_MINIZ_TARGET miniz)
    elseif(MICHITAI_LAN_FETCH_MINIZ)
        include(FetchContent)
        FetchContent_Declare(miniz
            GIT_REPOSITORY https://github.com/richgel999/miniz.git
            GIT_TAG        3.1.2
            GIT_SHALLOW    TRUE)
        FetchContent_MakeAvailable(miniz)
        set(MICHITAI_LAN_MINIZ_TARGET miniz)
    else()
        message(FATAL_ERROR
            "miniz was not found. Provide a `miniz` target before add_subdirectory(cpp), "
            "install a miniz package, or configure with -DMICHITAI_LAN_FETCH_MINIZ=ON.")
    endif()
endif()


#-----------------------------------------------------------------------------------------------------------------------------
#  michitai-lan — static library, C++20.
#-----------------------------------------------------------------------------------------------------------------------------

add_library(michitai-lan STATIC
    src/Debugging/DebugConsole.cpp
    src/Json.cpp
    src/Net/EndPoint.cpp
    src/Net/SocketInternal.cpp
    src/Net/Frame.cpp
    src/Net/Compression.cpp
    src/Net/PortRange.cpp
    src/Net/Lan.cpp
    src/Net/TCPClient.cpp
    src/Net/TCPServer.cpp
    src/Net/UDPChannel.cpp
    src/Net/UDPBroadcast.cpp
    src/Net/Multiplayer/Commands.cpp
    src/Net/Multiplayer/Data.cpp
    src/Net/Multiplayer/Server.cpp
    src/Net/Multiplayer/Client.cpp
    src/Net/Multiplayer/Broadcast.cpp
    src/Net/Multiplayer/Multiplayer.cpp
)

add_library(Michitai::Lan ALIAS michitai-lan)

target_compile_features(michitai-lan PUBLIC cxx_std_20)

target_include_directories(michitai-lan
    PUBLIC  $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
            $<INSTALL_INTERFACE:include>
    PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/src)

target_link_libraries(michitai-lan
    PUBLIC  Threads::Threads
    PRIVATE ${MICHITAI_LAN_MINIZ_TARGET})

if(MSVC)
    target_compile_options(michitai-lan PRIVATE /W3 /utf-8 /Zc:__cplusplus)
endif()

if(WIN32)
    target_link_libraries(michitai-lan PRIVATE ws2_32 iphlpapi)
elseif(UNIX AND NOT APPLE)
    target_link_libraries(michitai-lan PRIVATE pthread)
endif()


#-----------------------------------------------------------------------------------------------------------------------------
#  Examples.
#-----------------------------------------------------------------------------------------------------------------------------

if(MICHITAI_LAN_BUILD_EXAMPLES)
    add_executable(lan-demo apps/demo.cpp)
    target_link_libraries(lan-demo PRIVATE michitai-lan)
endif()
