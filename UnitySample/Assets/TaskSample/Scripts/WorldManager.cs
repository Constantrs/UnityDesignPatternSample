using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TaskSample
{
    public class WorldManager
    {
        private Dictionary<int, Transform> _objectMap = new Dictionary<int, Transform>();

        public void Initialize(GameObject root)
        {
            if(root != null)
            {
                var childrenTransform = root.GetComponentsInChildren<Transform>().Where(t => t != root.transform);
                foreach (var tarnsform in childrenTransform)
                {
                    _objectMap.Add(tarnsform.GetInstanceID(), tarnsform);
                }
                Debug.Log(string.Format("Count : {0}", _objectMap.Count));
            }
        }
    }
}
