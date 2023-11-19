using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ICommand
{
    public abstract void Execute();

    public abstract string GetCommandStr();
}

public class MoveCommand : ICommand
{
    private const string CommandName = "MoveCommand";

    private PlayerController _Controller;
    private Vector3 _TartgetPos;

    public MoveCommand(PlayerController controller, Vector3 target)
    {
        _Controller = controller;
        _TartgetPos = target;
    }

    public override void Execute()
    {
        _Controller.StartMoveToTarget(_TartgetPos);
    }

    public override string GetCommandStr()
    {
        string str = CommandName;
        str += $" {_TartgetPos} ";
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
