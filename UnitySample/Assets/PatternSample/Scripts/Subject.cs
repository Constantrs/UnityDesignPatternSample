using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObserver
{
    public void OnNotify();
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

    protected void NotifyOvservers()
    {
        _observers.ForEach(observer => observer.OnNotify());
    }
}
