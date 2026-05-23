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


namespace Michitai.Lan.Net.Multiplayer.Chat
{
    /// <summary>
    /// Thread-safe chat container for managing chat messages with a maximum length limit.
    /// </summary>
    public sealed class Chat
    {
        private Letter[] _letters;

        private int _max_length;

        private object _letters_lock;

        private object _max_length_lock;

        /// <summary>
        /// Gets or sets the array of chat letters. Thread-safe.
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
        /// Gets or sets the maximum number of letters the chat can hold. Thread-safe.
        /// When set, removes oldest letters if exceeding the limit.
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
        /// Initializes a new instance of Chat with the specified maximum length.
        /// </summary>
        /// <param name="maxLength">The maximum number of letters the chat can hold.</param>
        public Chat(int maxLength)
    {
        _letters = new Letter[0];

        _max_length = maxLength;


        _letters_lock = new object();

        _max_length_lock = new object();
    }

        /// <summary>
        /// Initializes a new instance of Chat with a default maximum length of 128.
        /// </summary>
        public Chat()
    {
        _letters = new Letter[0];

        _max_length = 128;


        _letters_lock = new object();

        _max_length_lock = new object();
    }



        /// <summary>
        /// Adds a letter to the chat. Removes the oldest letter if at maximum capacity.
        /// </summary>
        /// <param name="letter">The letter to add.</param>
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
        /// Deletes a letter at the specified index.
        /// </summary>
        /// <param name="index">The index of the letter to delete.</param>
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
        /// Clears all letters from the chat.
        /// </summary>
        public void Clear()
    {
        _letters = new Letter[0];
    }
}
}
