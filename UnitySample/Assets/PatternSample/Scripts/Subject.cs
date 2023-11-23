using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void AddObserver(IObserver observer)
    {
        _observers.Add(observer);
    }

    public void RemoveOvserver(IObserver observer)
    {
        _observers.Remove(observer);
    }

    protected void NotifyOvservers(NotifyMessage message)
    {
        _observers.ForEach(observer => observer.OnNotify(message));
    }
}
