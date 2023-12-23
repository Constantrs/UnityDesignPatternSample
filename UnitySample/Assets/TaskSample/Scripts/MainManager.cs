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

        private void Awake()
        {
            if(instance == null )
            {
                instance = this;
                FrameManager.SetFramerateMode(mode);
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
