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
    /// <summary>
    /// Represents authentication credentials with an ID and password.
    /// </summary>
    public sealed class Credentials
    {
        /// <summary>
        /// Creates a new instance of Credentials with randomly generated ID and password.
        /// </summary>
        /// <returns>A new Credentials instance.</returns>
        public static Credentials New()
    {
        return new Credentials(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
    }



    private string _id;

    private string _password;


    private readonly object _id_lock;

    private readonly object _password_lock;



        /// <summary>
        /// Gets or sets the credential ID. Thread-safe.
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
        /// Gets or sets the credential password. Thread-safe.
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
        /// Gets a public view of the credentials (ID only).
        /// </summary>
        [JsonIgnore]
        public Credentials Public => new Credentials(ID);



        /// <summary>
        /// Initializes a new instance of Credentials with the specified ID and password.
        /// </summary>
        /// <param name="id">The credential ID.</param>
        /// <param name="password">The credential password.</param>
        public Credentials(string id, string password)
    {
        _id = id;

        _password = password;


        _id_lock = new object();

        _password_lock = new object();
    }

        /// <summary>
        /// Initializes a new instance of Credentials with the specified ID only.
        /// </summary>
        /// <param name="id">The credential ID.</param>
        public Credentials(string id) : this(id, "")
    {
    }

        /// <summary>
        /// Initializes a new instance of Credentials with default values.
        /// </summary>
        public Credentials()
    {
        _id = "id";

        _password = "password";


        _id_lock = new object();

        _password_lock = new object();
    }



        /// <summary>
        /// Returns a string representation of the credentials.
        /// </summary>
        /// <returns>A string containing ID and password.</returns>
        public override string ToString()
    {
        return $"[CREDENTIALS][ID][{_id}][PASSWORD][{_password}]";
    }



        /// <summary>
        /// Determines whether the specified object is equal to the current credentials.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if equal; otherwise, false.</returns>
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
        /// Returns a hash code for the credentials.
        /// </summary>
        /// <returns>A hash code based on ID and password.</returns>
        public override int GetHashCode()
    {
        return $"{ID}{Password}".GetHashCode();
    }

        /// <summary>
        /// Determines whether two credentials are equal.
        /// </summary>
        /// <param name="A">The first credentials.</param>
        /// <param name="B">The second credentials.</param>
        /// <returns>True if equal; otherwise, false.</returns>
        public static bool operator ==(Credentials A, Credentials B)
    {
        return A.Equals(B);
    }

        /// <summary>
        /// Determines whether two credentials are not equal.
        /// </summary>
        /// <param name="A">The first credentials.</param>
        /// <param name="B">The second credentials.</param>
        /// <returns>True if not equal; otherwise, false.</returns>
        public static bool operator !=(Credentials A, Credentials B)
    {
        return !A.Equals(B);
    }
}
}
