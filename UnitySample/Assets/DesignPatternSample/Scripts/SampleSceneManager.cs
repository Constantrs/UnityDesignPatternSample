using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatternSample
{
    public class SampleSceneManager : MonoBehaviour
    {
        public enum FramerateMode
        {
            UNLIMITED = 0,
            FORCE_30,
            FORCE_60,
            FORCE_120
        }

        private static SampleSceneManager _Instance;

        public FramerateMode fpsmode = FramerateMode.FORCE_60;

        private bool _Pause = false;
        private InputManager _Input = null;

        private void Awake()
        {
            if (_Instance != null && _Instance != this)
            {
                Destroy(this);
            }
            else
            {
                if (Initialize())
                {
                    _Instance = this;
                }
            }
        }

        private void OnDestory()
        {
            if(_Instance == this)
            {
                _Instance = null;
            }
        }

        /// <summary>
        /// インスタンス取得
        /// </summary>
        public static SampleSceneManager GetInstance()
        {
            return _Instance;
        }

        /// <summary>
        /// 入力マネージャー取得
        /// </summary>
        public InputManager GetInput()
        {
            return _Input;
        }

        /// <summary>
        /// シーンマネージャー初期化
        /// </summary>
        public bool Initialize()
        {
            switch (fpsmode)
            {
                case FramerateMode.UNLIMITED:
                    QualitySettings.vSyncCount = 1;
                    break;
                case FramerateMode.FORCE_30:
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = 30;
                    break;
                case FramerateMode.FORCE_60:
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = 60;
                    break;
            }

            _Input = new InputManager();
            return true;
        }

        /// <summary>
        /// 一時停止フラグ設定
        /// </summary>
        public void SetPauseFlag(bool pause)
        {
            _Pause = pause;
        }

        /// <summary>
        /// 一時停止フラグ取得
        /// </summary>
        public bool GetPauseFlag()
        {
            return _Pause;
        }

        /// <summary>
        /// タイム倍率取得
        /// </summary>
        public float GetTimeMultiplier()
        {
            if (_Pause)
            {
                return 0.0f;
            }
            else
            {
                if (fpsmode == FramerateMode.FORCE_30)
                {
                    return 2.0f;
                }
                else if (fpsmode == FramerateMode.FORCE_60)
                {
                    return 1.0f;
                }
                else if (fpsmode == FramerateMode.FORCE_120)
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
}
