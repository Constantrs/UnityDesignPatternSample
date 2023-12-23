using Cysharp.Threading.Tasks;
using DesignPatternSample;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TaskSample
{

    public abstract class IObjectCommand
    {
        public abstract void Execute();

        public abstract string GetCommandName();
    }

    //public class ObjectCommandManager
    //{
    //    private Stack<IObjectCommand> _commandList;

    //    public ObjectCommandManager()
    //    {
    //        _commandList = new Stack<IObjectCommand>();
    //    }

    //    /// <summary>
    //    /// コマンド追加
    //    /// </summary>
    //    public void AddCommand(IObjectCommand command)
    //    {
    //        // 追加同時に実行
    //        command.Execute();
    //        _commandList.Push(command);
    //    }

    //    /// <summary>
    //    /// コマンド文字列取得
    //    /// </summary>
    //    public List<string> GetCommandNameList()
    //    {
    //        List<string> list = new List<string>();
    //        foreach (var command in _commandList)
    //        {
    //            list.Add(command.GetCommandName());
    //        }
    //        return list;
    //    }
    //}

    //public class WorldObjectController
    //{
    //    public GameObject go;
    //    private ObjectCommandManager _Manager;
    //}

    public class WorldManager : IObserver
    {
        private Dictionary<int, GameObject> _ObjectMap = new Dictionary<int, GameObject>();

        private List<IObjectCommand> _commandList = new List<IObjectCommand>();

        private bool _Running = false;

        ~WorldManager()
        {
            if (_Running)
            {
                Debug.Log("Release WorldManager");
            }
        }

        public void Initialize(MainManager main, GameObject root)
        {
            if (main != null && root != null)
            {
                var childrenTransform = root.GetComponentsInChildren<Transform>().Where(t => t != root.transform);
                foreach (var tarnsform in childrenTransform)
                {
                    _ObjectMap.Add(tarnsform.GetInstanceID(), tarnsform.gameObject);
                }
                Debug.Log(string.Format("Count : {0}", _ObjectMap.Count));
                _Running = true;
                main.AddObserver(this);
                Update().Forget();
            }
        }

        public async UniTask Update()
        {
            while (_Running)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            Debug.Log("WorldManager End");
        }

        public void OnNotify(EventType eventtype)
        {
            switch(eventtype) 
            {
                case EventType.End:
                    _Running = false; 
                    break;
                default:
                    break;
            }
        }
    }
}
