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
            private Dictionary<string, Func<object[], CommandProcess>> _Database = new Dictionary<string, Func<object[], CommandProcess>>();

            public bool HasCommand(string commandName) => _Database.ContainsKey(commandName);

            public void AddCommand(string commandName, Func<object[], CommandProcess> func)
            {
                commandName = commandName.ToLower();

                if (!_Database.ContainsKey(commandName))
                {
                    _Database.Add(commandName, func);
                }
                else
                {
                    Debug.LogError($"Command already exists in database! '{commandName}'");
                }
            }

            public Func<object[], CommandProcess> GetCommand(string commandName)
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