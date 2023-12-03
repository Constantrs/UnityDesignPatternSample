using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AchievementManager : Subject, IObserver
{
    [SerializeField] public List<bool> _achievements = new List<bool>();

    private int _ItemsCount = 0;

    private List<Subject> _ItemList = new List<Subject>();

    // Start is called before the first frame update
    private void Awake()
    {
        var objests = GameObject.FindGameObjectsWithTag("Item");
        if(objests.Length > 0 )
        {
            for(int i = 0; i < objests.Length; i++)
            {
                var itemController = objests[i].GetComponent<ItemController>();
                if(itemController != null)
                {
                    _ItemList.Add(itemController);
                }
            }
        }
    }
    
    // Start is called before the first frame update
    private void OnEnable()
    {
        foreach (var item in _ItemList)
        {
            item?.AddObserver(this);
        }
    }

    private void OnDisable()
    {
        foreach (var item in _ItemList)
        {
            item?.RemoveOvserver(this);
        }
    }

    public void OnNotify(NotifyMessage message)
    {
        if (message != null)
        {
            Type messageType = message.GetType();
            if (messageType == typeof(UnlockAchievementMessage))
            {
                var unlockAchievementMessage = message as UnlockAchievementMessage;
                if (unlockAchievementMessage == null)
                {
                    return;
                }

                UnlockAchievement(unlockAchievementMessage.achievementId);
            }
        }
    }

    public void UnlockAchievement(int id)
    {
        if(id < 0 && id > _achievements.Count)
        {
            return;
        }

        if (!_achievements[id])
        {
            switch (id) 
            {
                case 0:
                    _ItemsCount++;

                    if (_ItemsCount > 2)
                    {
                        NotifyOvservers(new UnlockAchievementMessage(0));
                        _achievements[id] = true;
                    }
                    break;
            }
        }
    }
}
