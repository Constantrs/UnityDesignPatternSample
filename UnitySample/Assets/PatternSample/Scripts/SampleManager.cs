using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class SampleManager : MonoBehaviour
{
    // Start is called before the first frame update
    public enum Framerate
    {
        UNLIMITED = 0,
        FORCE_30,
        FORCE_60,
    }

    private static SampleManager _Instance;

    public Framerate fpsmode = Framerate.FORCE_60;
    public bool pause = false;

    private InputManager _Input = null;

    private void Awake()
    {
        if (_Instance != null && _Instance != this)
        {
            Destroy(this);
        }
        else
        {
            if(Initialize())
            {
                _Instance = this;
            }
        }
    }

    public static SampleManager GetInstance()
    {
        return _Instance;
    }

    public bool Initialize()
    {
        switch (fpsmode)
        {
            case Framerate.UNLIMITED:
                QualitySettings.vSyncCount = 1;
                break;
            case Framerate.FORCE_30:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 30;
                break;
            case Framerate.FORCE_60:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
                break;
        }

        _Input = new InputManager();
        return true;
    }

    public InputManager GetPlayerInput()
    {
        return _Input;
    }

    public float GetTimeScale()
    {
        if (pause)
        {
            return 0.0f;
        }
        else
        {
            if (fpsmode == Framerate.FORCE_60)
            {
                return 1.0f;
            }
            else if (fpsmode == Framerate.FORCE_30)
            {
                return 2.0f;
            }
            else
            {
                return Time.deltaTime;
            }
        }
    }


}
