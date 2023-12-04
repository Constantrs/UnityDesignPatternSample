using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatternSample
{
    public abstract class NotifyMessage
    {

    }

    public interface IObserver
    {
        public void OnNotify(NotifyMessage message);
    }

    public class Subject : MonoBehaviour
    {
        private List<IObserver> _observers = new List<IObserver>();

        /// <summary>
        /// 観測者を追加
        /// </summary>
        public void AddObserver(IObserver observer)
        {
            _observers.Add(observer);
        }

        /// <summary>
        /// 観測者を削除
        /// </summary>
        public void RemoveOvserver(IObserver observer)
        {
            _observers.Remove(observer);
        }

        /// <summary>
        /// 観測者に通知メッセージを送信
        /// </summary>
        protected void NotifyOvservers(NotifyMessage message)
        {
            _observers.ForEach(observer => observer.OnNotify(message));
        }
    }

}
