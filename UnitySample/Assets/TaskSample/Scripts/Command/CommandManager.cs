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

            private CommandDatabase commandDatabase = new CommandDatabase();

            private void Awake()
            {
                if (instance == null)
                {
                    instance = this;
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