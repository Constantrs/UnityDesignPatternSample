using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaskSample
{
    namespace Command
    {
        public class CommandExtension_Movement : CommandExtension
        {
            new public static void Extend(CommandDatabase database)
            {
                // Add action with no parameters
                database.AddCommand("waitsecond", new Func<object[], CommandProcess>(WaitSecond));
            }

            public static CommandProcess WaitSecond(object[] objects)
            {
                int second = UnityEngine.Random.Range(1, 11);
                var manager = MainSystem.GetInstance();
                if (manager != null)
                {
                    UniTaskCommandProcess<int> commandProcess = new UniTaskCommandProcess<int>(manager.GetWorldManager().WaitSecond, second);
                    return commandProcess;
                }

                return null;
            }
        }
    }
}