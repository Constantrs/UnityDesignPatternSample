using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatternSample
{
    public abstract class ICommand
    {
        public abstract void Execute();

        public abstract void Undo();

        public abstract string GetCommandName();
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

        /// <summary>
        /// コマンド元に戻す
        /// </summary>
        public void UndoCommand()
        {
            if (_commandList.Count > 0)
            {
                ICommand lastCommand = _commandList.Pop();
                lastCommand.Undo();
            }
        }

        /// <summary>
        /// コマンド文字列取得
        /// </summary>
        public List<string> GetCommandNameList()
        {
            List<string> list = new List<string>();
            foreach (var command in _commandList)
            {
                list.Add(command.GetCommandName());
            }
            return list;
        }
    }

}
