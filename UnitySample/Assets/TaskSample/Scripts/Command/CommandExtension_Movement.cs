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
                //database.AddCommand("wait", new Func<string, IEnumerator>(CoWait));
            }
        }
    }
}