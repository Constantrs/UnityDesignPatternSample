using Cysharp.Threading.Tasks;
using DesignPatternSample;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Searcher.Searcher.AnalyticsEvent;

namespace TaskSample
{
    
    public class WorldManager : SubSystem
    {
        private Dictionary<int, GameObject> _ObjectMap = new Dictionary<int, GameObject>();

        private bool _Running = false;

        public override void Initialize(MainSystem main)
        {
            if (main != null)
            {
                var root = main.worldRoot;
                if (root != null) 
                {
                    var childrenTransform = root.GetComponentsInChildren<Transform>().Where(t => t != root.transform);
                    foreach (var tarnsform in childrenTransform)
                    {
                        _ObjectMap.Add(tarnsform.GetInstanceID(), tarnsform.gameObject);
                    }
                    Debug.Log(string.Format("Count : {0}", _ObjectMap.Count));
                    _Running = true;
                    //Update().Forget();
                }
                main.AddObserver(this);
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

        public async UniTask WaitSecond(int second)
        {
            int ms = second * 1000;
            string waitStartMessage = string.Format("Wait Start : {0} ms", ms);
            Debug.Log(waitStartMessage);
            await UniTask.Delay(ms);
            Debug.Log("Wait End");
        }

        protected override void NotifyEvent(EventType eventType)
        {
            switch (eventType)
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
