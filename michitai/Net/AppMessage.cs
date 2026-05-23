//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Net.NetworkInformation;
using System.Text;
using System.Runtime;
using System.Runtime.Serialization;


namespace Michitai.Lan.Net
{
            public sealed class AppMessage
{
    private int _version;

    private string _name;

    private string _message;



    public int Version
    {
        get
        {
            return _version;
        }

        set
        {
            _version = value;
        }
    }

    public string Name
    {
        get
        {
            return _name;
        }

        set
        {
            _name = value;
        }
    }

    public string Message
    {
        get
        {
            return _message;
        }

        set
        {
            _message = value;
        }
    }



    public AppMessage()
    {

    }

    public AppMessage(int version, string name, string message)
    {
        Version = version;

        Name = name;

        Message = message;
    }
}
}
