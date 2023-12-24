using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;


namespace TaskSample
{
    public class TextManager : SubSystem
    {
        private TMP_Text _Text;

        private int _MaxTextLength = 60;

        private int _TextNormalSpeed = 1;
        private int _TextFastSpeed = 3;

        private int _CurrentTextSpeed = 0;
        private float _TextInterval = 1.0f;

        private bool _Running = false;

        public override void Initialize(MainSystem main)
        {
            if (main != null)
            {
                if(main.text != null) 
                {
                    _Text = main.UIText;
                    _Text.text = string.Empty;
                }
                main.AddObserver(this);
            }
        }

        public bool IsRunning()
        {
            return _Running;
        }

        public async UniTask SetMessage(string text)
        {
            Debug.Log("Text Start");

            _Text.maxVisibleCharacters = 0;
            _Text.text = text;
            _CurrentTextSpeed = _TextNormalSpeed;
            _Running = true;

            float timer = 0.0f;
            int textLength = _Text.text.Length;
            float currentLength = 0;
            while (_Running)
            {
                var mainSystem = MainSystem.GetInstance();

                if (mainSystem == null ||
                    (currentLength >= textLength || currentLength >= _MaxTextLength))
                {
                    _Running = false;
                    break;
                }

                if (timer >= _TextInterval)
                {
                    float t = timer / _TextInterval;
                    currentLength += _CurrentTextSpeed * t;
                    _Text.maxVisibleCharacters = (int)currentLength;
                    timer = 0.0f;
                }
                else
                {
                    timer += mainSystem.GetTimeMultiplier();
                }

                await UniTask.Yield(PlayerLoopTiming.LastPreLateUpdate);
            }

            Debug.Log("Text End");
        }


        protected override void NotifyEvent(EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Input:
                    if(_Running)
                    {
                        _CurrentTextSpeed = (_CurrentTextSpeed <= _TextNormalSpeed) ? _TextFastSpeed : _TextNormalSpeed;
                    }
                    break;
                case EventType.End:
                    _Running = false;
                    break;
                default:
                    break;
            }
        }
    }
}
