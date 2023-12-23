using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaskSample
{
    public class MainManager : MonoBehaviour
    {
        private static MainManager instance;

        public FramerateMode mode;
        public bool pause;

        public GameObject root;

        private WorldManager _worldManager = new WorldManager();

        private void Awake()
        {
            if(instance == null )
            {
                instance = this;
                FrameManager.SetFramerateMode(mode);
                _worldManager.Initialize(root);
            }
            else
            {

            }
        }
        private void OnDestroy()
        {
            if( instance != this )
            {
                instance = null;
            }
        }

        public float GetTimeMultiplier()
        {
            if(pause)
            {
                return 0.0f;
            }
            else
            {
                return FrameManager.GetTimeMultiplier(mode);
            }
        }
    }
}
