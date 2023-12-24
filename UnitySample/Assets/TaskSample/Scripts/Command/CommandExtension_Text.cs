using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaskSample
{
    namespace Command
    {
        public class CommandExtension_Text : CommandExtension
        {
            new public static void Extend(CommandDatabase database)
            {
                database.AddCommand("setmessage", new Func<object[], CommandProcess>(SetMessage));
            }

            public static CommandProcess SetMessage(object[] objects)
            {
                if (objects.Length > 0)
                {
                    string text = objects[0].ToString();
                    var manager = MainSystem.GetInstance();
                    if (manager != null && !string.IsNullOrEmpty(text))
                    {
                        var textManager = manager.GetTextManager();
                        if (textManager != null && !textManager.IsRunning())
                        {
                            UniTaskCommandProcess<string> commandProcess = new UniTaskCommandProcess<string>(manager.GetTextManager().SetMessage, text);
                            return commandProcess;
                        }
                    }
                }
                return null;
            }
        }
    }
}