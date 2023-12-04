using System;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatternSample
{
    public class AchievementManager : Subject, IObserver
    {
        [SerializeField] private List<bool> _achievements = new List<bool>();

        private int _ItemsCount = 0;

        private List<Subject> _ItemList = new List<Subject>();

        private void Awake()
        {
            var objests = GameObject.FindGameObjectsWithTag("Item");
            if (objests.Length > 0)
            {
                for (int i = 0; i < objests.Length; i++)
                {
                    var itemController = objests[i].GetComponent<ItemController>();
                    if (itemController != null)
                    {
                        _ItemList.Add(itemController);
                    }
                }
            }
        }

        private void OnEnable()
        {
            foreach (var item in _ItemList)
            {
                if (item != null)
                {
                    item.AddObserver(this);
                }
            }
        }

        private void OnDisable()
        {
            foreach (var item in _ItemList)
            {
                if (item != null)
                {
                    item.RemoveOvserver(this);
                }
            }
        }

        /// <summary>
        /// 観測者に通知メッセージを送信
        /// </summary>
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

                    UpdateData(unlockAchievementMessage.achievementId);
                }
            }
        }

        /// <summary>
        /// 実績データ更新
        /// </summary>
        private void UpdateData(int id)
        {
            if (id < 0 && id > _achievements.Count)
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
}
