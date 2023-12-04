using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatternSample
{
    public abstract class ICommand
    {
        public abstract void Execute();
    }

    public class CommandManager
    {
        private Stack<ICommand> _commandList;

        public CommandManager()
        {
            _commandList = new Stack<ICommand>();
        }

        /// <summary>
        /// コマンド追加
        /// </summary>
        public void AddCommand(ICommand command)
        {
            // 追加同時に実行
            command.Execute();
            _commandList.Push(command);
        }
    }

}
