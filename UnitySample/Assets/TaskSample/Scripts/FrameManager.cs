using DesignPatternSample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.ContentSizeFitter;

namespace TaskSample
{
    public enum FramerateMode
    {
        UNLIMITED = 0,
        FORCE_30,
        FORCE_60,
        FORCE_120
    }

    public class FrameManager
    {
        public static void SetFramerateMode(FramerateMode mode)
        {
            switch (mode)
            {
                case FramerateMode.UNLIMITED:
                    QualitySettings.vSyncCount = 1;
                    Application.targetFrameRate = -1;
                    break;
                case FramerateMode.FORCE_30:
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = 30;
                    break;
                case FramerateMode.FORCE_60:
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = 60;
                    break;
                case FramerateMode.FORCE_120:
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = 120;
                    break;
            }
        }

        public static float GetTimeMultiplier(FramerateMode mode)
        {
            if (mode == FramerateMode.FORCE_30)
            {
                return 2.0f;
            }
            else if (mode == FramerateMode.FORCE_60)
            {
                return 1.0f;
            }
            else if (mode == FramerateMode.FORCE_120)
            {
                return 0.5f;
            }
            else
            {
                return Time.deltaTime;
            }
        }
    }
}
