//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  C# interop harness — uses the real .NET michitai-lan.dll to verify wire
//  compatibility with the C++ port (cpp/build/Debug/lan-demo.exe).
//
//  Build:  csc -nologo -reference:dotnet\bin\Release\michitai-lan.dll -out:cpp\build\interop.exe cpp\tests\interop.cs
//          copy dotnet\bin\Release\michitai-lan.dll cpp\build\
//
//  Modes:  interop.exe server <port> [seconds]   (TCP echo + UDP state echo)
//          interop.exe client <port>             (one TCP request + one UDP state)
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Threading;
using Michitai.Lan.Net;


public static class Interop
{
    public static int Main(string[] args)
    {
        if (args.Length >= 2 && args[0] == "server")
        {
            return Server(int.Parse(args[1]), args.Length > 2 ? int.Parse(args[2]) : 15);
        }
        if (args.Length >= 2 && args[0] == "client")
        {
            return Client(int.Parse(args[1]));
        }
        Console.WriteLine("usage: interop server <port> [seconds] | client <port>");
        return 2;
    }

    private static int Server(int port, int seconds)
    {
        var server  = new TCPServer(IPAddress.Loopback, port);
        var channel = new UDPChannel(IPAddress.Loopback, port);

        server.OnRequest += (IdentifiedMessage request) =>
        {
            server.Response(new IdentifiedMessage(
                new Message("echo:" + request.Message.GetMessage), request.ID));
        };

        channel.OnReceive += (IPEndPoint from, Message message) =>
        {
            channel.Send(from, new Message("state-echo:" + message.GetMessage));
        };

        server.Start();
        channel.Start();

        Console.WriteLine("READY " + port);
        Thread.Sleep(seconds * 1000);

        channel.Stop();
        server.Stop();
        return 0;
    }

    private static int Client(int port)
    {
        var responseSignal = new ManualResetEventSlim();
        var stateSignal    = new ManualResetEventSlim();
        string responseText = null;
        string stateText = null;

        var client  = new TCPClient(IPAddress.Loopback, port);
        var channel = new UDPChannel(IPAddress.Loopback, 0);

        client.OnResponse += (Message message) =>
        {
            responseText = message.GetMessage;
            responseSignal.Set();
        };

        channel.OnReceive += (IPEndPoint from, Message message) =>
        {
            stateText = message.GetMessage;
            stateSignal.Set();
        };

        client.Start();
        channel.Start();

        client.Request(new Message("ping-from-csharp"));

        if (!responseSignal.Wait(5000))
        {
            Console.WriteLine("FAIL tcp timeout");
            return 1;
        }
        Console.WriteLine("tcp-response: " + responseText);

        channel.Send(new IPEndPoint(IPAddress.Loopback, port), new Message("state-from-csharp"));

        if (!stateSignal.Wait(5000))
        {
            Console.WriteLine("FAIL udp timeout");
            return 1;
        }
        Console.WriteLine("udp-response: " + stateText);

        channel.Stop();
        client.Stop();

        Console.WriteLine("CLIENT-DONE");
        return 0;
    }
}
