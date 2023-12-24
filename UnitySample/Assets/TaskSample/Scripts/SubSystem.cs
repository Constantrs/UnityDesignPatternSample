using DesignPatternSample;
using System.Collections;
using System.Collections.Generic;
using TaskSample.Command;
using UnityEngine;
using static UnityEditor.Searcher.Searcher.AnalyticsEvent;

namespace TaskSample
{
    public interface IObserver
    {
        public void OnNotify(EventType eventType);
    }


    public abstract class SubSystem : IObserver
    {
        public abstract void Initialize(MainSystem main);

        protected abstract void NotifyEvent(EventType eventType);

        public void OnNotify(EventType eventType)
        {
            NotifyEvent(eventType);
        }
    }
}