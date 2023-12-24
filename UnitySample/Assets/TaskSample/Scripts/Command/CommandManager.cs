using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TaskSample;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace TaskSample
{
    namespace Command
    {

        public abstract class CommandProcess
        {
            public abstract void Execute();
            public abstract bool IsCompleted();
        }

        public class UniTaskCommandProcess<T> : CommandProcess
        {
            private T _value;
            private bool _IsCompleted = false;
            private Func<T, UniTask> _Action;

            public UniTaskCommandProcess(Func<T, UniTask> action, T value) 
            {
                _value = value;
                _Action = action;
            }

            public async override void Execute()
            {
                await _Action(_value);
                _IsCompleted = true;
            }

            public override bool IsCompleted()
            {
                return _IsCompleted;
            }
        }


        public class CommandManager : MonoBehaviour
        {
            private static CommandManager _Instance;

            private CommandDatabase _Database = new CommandDatabase();

            private List<CommandProcess> _Commands = new List<CommandProcess>();

            private bool _IsExecuting = false;

            private void Awake()
            {
                if (_Instance == null)
                {
                    _Instance = this;

                    CommandExtension_Movement.Extend(_Database);
                    CommandExtension_Text.Extend(_Database);
                }
                else
                {

                }
            }

            private void OnDestroy()
            {
                if (_Instance == this)
                {
                    _Instance = null;
                }
            }

            public static CommandManager GetInstance()
            {
               return _Instance;
            }

            public bool IsExecuting()
            {
                return _IsExecuting;
            }

            public void PushProcess(string name, object[] parameters)
            {
                if (_IsExecuting)
                {
                    return;
                }

                var func = _Database.GetCommand(name);
                if (func != null)
                {
                    var process = func?.Invoke(parameters);
                    if (process != null)
                    {
                        _Commands.Add(process);
                    }
                }
            }

            public async UniTask Execute()
            {
                Debug.Log("Command Execute Start");

                if (_Commands.Count > 0)
                {
                    foreach (var command in _Commands)
                    {
                        command.Execute();
                    }
                    _IsExecuting = true;
                }

                await UniTask.WaitUntil( () => (CheckProcessExecuted() == true));

                _Commands.Clear();
                _IsExecuting = false;

                Debug.Log("Command Execute End");
            }

            private bool CheckProcessExecuted()
            {
                if (!_IsExecuting)
                {
                    return false;
                }
                else
                {
                    bool result = true;
                    foreach (var command in _Commands)
                    {
                        if (!command.IsCompleted())
                        {
                            result = false;
                            break;
                        }
                    }
                    return result;
                }
            }
        }
    }
}