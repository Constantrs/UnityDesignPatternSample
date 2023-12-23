using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaskSample
{
    namespace Command
    {
        public class CommandDatabase
        {
            //private Dictionary<string, Func<object[], CommandExtension>> commandDatabase = new Dictionary<string, Func<object[], CommandExtension>>();
            private Dictionary<string, Delegate> _Database = new Dictionary<string, Delegate>();

            public bool HasCommand(string commandName) => _Database.ContainsKey(commandName);

            public void AddCommand(string commandName, Delegate command)
            {
                commandName = commandName.ToLower();

                if (!_Database.ContainsKey(commandName))
                {
                    _Database.Add(commandName, command);
                }
                else
                {
                    Debug.LogError($"Command already exists in database! '{commandName}'");
                }
            }

            public Delegate GetCommand(string commandName)
            {
                commandName = commandName.ToLower();

                if (!_Database.ContainsKey(commandName))
                {
                    Debug.LogError($"Command '{commandName}' does not exists in the database!");
                    return null;
                }
                else
                {
                    return _Database[commandName];
                }
            }
        }
    }
}