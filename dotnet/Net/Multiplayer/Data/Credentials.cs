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


namespace Michitai.Lan.Net.Multiplayer.Data
{
            public sealed class Credentials
{
    public static Credentials New()
    {
        return new Credentials(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
    }



    private string _id;

    private string _password;


    private readonly object _id_lock;

    private readonly object _password_lock;



    /// <summary>
    /// 
    /// </summary>
    public string ID
    {
        get
        {
            lock (_id_lock)
            {
                return _id;
            }
        }

        set
        {
            lock (_id_lock)
            {
                _id = value;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string Password
    {
        get
        {
            lock (_password_lock)
            {
                return _password;
            }
        }

        set
        {
            lock (_password_lock)
            {
                _password = value;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [JsonIgnore]
    public Credentials Public => new Credentials(ID);



    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="password"></param>
    public Credentials(string id, string password)
    {
        _id = id;

        _password = password;


        _id_lock = new object();

        _password_lock = new object();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public Credentials(string id) : this(id, "")
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public Credentials()
    {
        _id = "id";

        _password = "password";


        _id_lock = new object();

        _password_lock = new object();
    }



    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"[CREDENTIALS][ID][{_id}][PASSWORD][{_password}]";
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            Credentials credentials = (Credentials)obj;

            return ID == credentials.ID && Password == credentials.Password;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return $"{ID}{Password}".GetHashCode();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <returns></returns>
    public static bool operator ==(Credentials A, Credentials B)
    {
        return A.Equals(B);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <returns></returns>
    public static bool operator !=(Credentials A, Credentials B)
    {
        return !A.Equals(B);
    }
}
}
