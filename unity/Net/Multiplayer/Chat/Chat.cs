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

using Michitai.Lan;
using Michitai.Lan.Data;
using Michitai.Lan.Net;
using Michitai.Lan.Net.Multiplayer;
using Michitai.Lan.Net.Multiplayer.Chat;
using Michitai.Lan.Net.Multiplayer.Commands;
using Michitai.Lan.Net.Multiplayer.Data;
using Michitai.Lan.Debug;
using UnityEngine;

namespace Michitai.Lan.Net.Multiplayer.Chat
{
            public sealed class Chat
{
    private Letter[] _letters;

    private int _max_length;


    private object _letters_lock;

    private object _max_length_lock;



    /// <summary>
    /// 
    /// </summary>
    public Letter[] Letters
    {
        get
        {
            lock (_letters_lock)
            {
                return _letters;
            }
        }

        set
        {
            lock (_letters_lock)
            {
                _letters = value;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public int MaxLength
    {
        get
        {
            lock (_max_length_lock)
            {
                return _max_length;
            }
        }

        set
        {
            lock (_max_length_lock)
            {
                MaxLength = value;
            }

            while (Letters.Length > MaxLength)
            {
                Delete(0);
            }
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="maxLength"></param>
    public Chat(int maxLength)
    {
        _letters = new Letter[0];

        _max_length = maxLength;


        _letters_lock = new object();

        _max_length_lock = new object();
    }

    /// <summary>
    /// 
    /// </summary>
    public Chat()
    {
        _letters = new Letter[0];

        _max_length = 128;


        _letters_lock = new object();

        _max_length_lock = new object();
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="letter"></param>
    public void Add(Letter letter)
    {
        if (Letters.Length >= MaxLength)
        {
            Delete(0);
        }

        Letter[] letters = _letters;

        _letters = new Letter[letters.Length + 1];

        for (int i = 0; i < letters.Length; i++)
        {
            _letters[i] = letters[i];
        }

        _letters[letters.Length] = letter;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    public void Delete(int index)
    {
        Letter[] letters = _letters;

        _letters = new Letter[letters.Length - 1];

        for (int i = 0; i < index; i++)
        {
            _letters[i] = letters[i];
        }

        for (int i = index; i < _letters.Length; i++)
        {
            _letters[i] = letters[i + 1];
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Clear()
    {
        _letters = new Letter[0];
    }
}
}
