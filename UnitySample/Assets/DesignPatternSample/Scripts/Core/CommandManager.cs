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
        /// �R�}���h�ǉ�
        /// </summary>
        public void AddCommand(ICommand command)
        {
            // �ǉ������Ɏ��s
            command.Execute();
            _commandList.Push(command);
        }
    }

}
