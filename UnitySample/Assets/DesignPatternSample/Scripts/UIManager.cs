using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DesignPatternSample
{
    public class UIManager : MonoBehaviour
    {
        // 実績フェードイン時間
        private const float ACHIEVEMNT_FADEINTIME = 60.0f;
        // 実績通常持続時間
        private const float ACHIEVEMNT_ACTIVETIME = 120.0f;
        // 実績フェードアウト時間
        private const float ACHIEVEMNT_FADEOUTTIME = 60.0f;

        public Image _achievementsImage;
        public Text _achievementsInfoText;
        public Text _achievementsDetailText;

        public Text _pauseText;
        private SampleManager manager => SampleManager.GetInstance();

        /// <summary>
        /// 一時停止UI表示
        /// </summary>
        public void ShowPause(bool pause)
        {
            _pauseText.enabled = pause;
        }

        /// <summary>
        /// 実績UI表示
        /// </summary>
        private void ShowAchievement(string text)
        {
            _achievementsImage.enabled = true;
            _achievementsInfoText.enabled = true;
            _achievementsDetailText.enabled = true;
            _achievementsDetailText.text = text;
            StartCoroutine(CoShowAchievement());
        }

        /// <summary>
        /// 実績UI非表示
        /// </summary>
        private void HideAchievement()
        {
            _achievementsImage.enabled = false;
            _achievementsInfoText.enabled = false;
            _achievementsDetailText.enabled = false;
            _achievementsDetailText.text = "";
            StopCoroutine(CoShowAchievement());
        }

        /// <summary>
        /// (コルーチン)実績UIフェード表示
        /// </summary>
        IEnumerator CoShowAchievement(float startAlpha = 0.0f, float endAlpha = 1.0f)
        {
            float timer = 0.0f;
            Color imageColor = _achievementsImage.color;
            Color textColor = _achievementsDetailText.color;

            _achievementsImage.color = new Color(imageColor.r, imageColor.g, imageColor.b, 0.0f);
            _achievementsInfoText.color = new Color(textColor.r, textColor.g, textColor.b, 0.0f);
            _achievementsDetailText.color = new Color(textColor.r, textColor.g, textColor.b, 0.0f);

            while (timer < ACHIEVEMNT_FADEINTIME && manager)
            {
                float timeRate = timer / ACHIEVEMNT_FADEINTIME;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, timeRate);
                imageColor.a = alpha;
                textColor.a = alpha;
                _achievementsImage.color = imageColor;
                _achievementsInfoText.color = textColor;
                _achievementsDetailText.color = textColor;
                timer += manager.GetTimeScale();
                yield return null;
            }
            timer = 0.0f;

            while (timer < ACHIEVEMNT_ACTIVETIME && manager)
            {
                timer += manager.GetTimeScale();
                yield return null;
            }
            timer = 0.0f;

            while (timer < ACHIEVEMNT_FADEOUTTIME && manager)
            {
                float timeRate = timer / ACHIEVEMNT_FADEOUTTIME;
                float alpha = Mathf.Lerp(endAlpha, startAlpha, timeRate);
                imageColor.a = alpha;
                textColor.a = alpha;
                _achievementsImage.color = imageColor;
                _achievementsInfoText.color = textColor;
                _achievementsDetailText.color = textColor;
                timer += manager.GetTimeScale();
                yield return null;
            }

            HideAchievement();
        }
    }
}
