using System.Collections;
using System.Collections.Generic;
using TaskSample;
using UnityEngine;

namespace TaskSample
{
    namespace Command
    {
        public class CommandManager : MonoBehaviour
        {
            private static CommandManager instance;

            private CommandDatabase _Database = new CommandDatabase();

            private void Awake()
            {
                if (instance == null)
                {
                    instance = this;

                    CommandExtension_Movement.Extend(_Database);
                }
                else
                {

                }
            }
            private void OnDestroy()
            {
                if (instance != this)
                {
                    instance = null;
                }
            }
        }
    }
}