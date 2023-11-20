using System.Collections.Generic;
using UnityEngine;

public abstract class ICommand
{
    public abstract void Execute();

    public abstract string GetCommandStr();
}

public class MoveCommand : ICommand
{
    private const string CommandName = "MoveCommand";

    private PlayerController _Controller;
    private Vector3 _StartPos;
    private Vector3 _TartgetPos;

    public MoveCommand(PlayerController controller, Vector3 startPos, Vector3 targetPos)
    {
        _Controller = controller;
        _StartPos = startPos;
        _TartgetPos = targetPos;
    }

    public override void Execute()
    {
        _Controller.StartMoveToTarget(_TartgetPos);
    }

    public override string GetCommandStr()
    {
        string str = CommandName;
        str += $" From : {_StartPos} ";
        str += $" To : {_TartgetPos} ";
        return str; 
    }
}


public class CommandManager
{
    private Stack<ICommand> _commandList;

    public CommandManager()
    {
        _commandList = new Stack<ICommand>();
    }
    public void AddCommand(ICommand command)
    {
        command.Execute();
        _commandList.Push(command);
    }

    public List<string> GetCommandListStr()
    {
        List<string> list = new List<string>();
        foreach (var command in _commandList)
        {
            list.Add(command.GetCommandStr());
        }
        return list;
    }
}
