using System.Collections;
using System.Collections.Generic;
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

    public Framerate fpsmode = Framerate.FORCE_60;

    private static SampleManager _Instance;

    private InputManager _Input = null;

    private void Awake()
    {
        if (_Instance == null)
        {
            Initialize();
            _Instance = this;
        }
    }

    public static SampleManager GetInstance()
    {
        return _Instance;
    }

    public void Initialize()
    {
        _Input = new InputManager();

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
    }

    public InputManager GetPlayerInput()
    {
        return _Input;
    }

    public float GetTimeScale()
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
