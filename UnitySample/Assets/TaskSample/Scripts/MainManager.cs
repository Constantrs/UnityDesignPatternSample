using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditorInternal;
using DesignPatternSample;
using static UnityEditor.Searcher.Searcher.AnalyticsEvent;

namespace TaskSample
{
    public enum EventType
    {
        Input,
        End
    }

    public interface IObserver
    {
        public void OnNotify(EventType eventtype);
    }


    public class MainManager : MonoBehaviour
    {
        public enum State
        {
            Uninitialized,
            Running,
            Exit,
        }

        private static MainManager instance;

        public FramerateMode mode;
        public State state = State.Uninitialized;
        public bool pause;

        public GameObject root;

        private WorldManager _worldManager = new WorldManager();
        private List<IObserver> _observers = new List<IObserver>();
        private CancellationToken _cts;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                FrameManager.SetFramerateMode(mode);
                _worldManager.Initialize(this, root);

                _cts = this.GetCancellationTokenOnDestroy();
                state = State.Running;

                OnLastEarlyUpdate().Forget();
                OnPostLateUpdate().Forget();
            }
            else
            {

            }
        }
        private void OnDestroy()
        {
            NotifyOvservers(EventType.End);
            _observers.Clear();

            if (instance != this)
            {
                instance = null;
            }
        }

        public static MainManager GetInstance()
        {
            return instance;
        }

        public float GetTimeMultiplier()
        {
            if (pause)
            {
                return 0.0f;
            }
            else
            {
                return FrameManager.GetTimeMultiplier(mode);
            }
        }

        public void TestMoveObject()
        {
            Debug.Log("MoveCharacter");
        }

        public async UniTask OnLastEarlyUpdate()
        {
            while (state != State.Exit) 
            {
                await UniTask.Yield(PlayerLoopTiming.LastEarlyUpdate, _cts);
            }
        }

        public async UniTask OnPostLateUpdate()
        {
            while (state != State.Exit)
            {
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, _cts);
            }
        }

        public void AddObserver(IObserver observer)
        {
            _observers.Add(observer);
        }

        public void RemoveOvserver(IObserver observer)
        {
            _observers.Remove(observer);
        }

        protected void NotifyOvservers(EventType eventType)
        {
            _observers.ForEach(observer => observer.OnNotify(eventType));
        }
    }
}
