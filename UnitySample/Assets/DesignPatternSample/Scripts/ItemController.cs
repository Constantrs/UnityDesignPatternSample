using System.Collections;
using UnityEngine;

namespace DesignPatternSample
{
    public class ItemController : MonoBehaviour
    {
        public float rotateSpeed = 1.0f;
        public float fadeOutTime = 30.0f;

        private float _EulerY = 0.0f;
        private bool _Destory = false;
        private SampleSceneManager manager => SampleSceneManager.GetInstance();

        private void Update()
        {
            if (manager == null)
            {
                return;
            }

            _EulerY += rotateSpeed * manager.GetTimeMultiplier();
            if (_EulerY > 360.0f)
            {
                _EulerY -= 360.0f;
            }

            // 自動回転
            Vector3 euler = transform.rotation.eulerAngles;
            euler.y = _EulerY;
            transform.rotation = Quaternion.Euler(euler);
        }

        /// <summary>
        /// プレイヤーとの当たり判定チェック
        /// </summary>
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (!_Destory)
                {
                    _Destory = true;
                    StartCoroutine(CoDestory());
                }
            }
        }

        /// <summary>
        /// (コルーチン)アイテム消失
        /// </summary>
        IEnumerator CoDestory()
        {
            bool fadeOutEnd = false;
            float timer = 0.0f;
            Vector3 originalScale = transform.localScale;

            while (!fadeOutEnd)
            {
                if (manager == null)
                {
                    fadeOutEnd = true;
                }

                if (timer >= fadeOutTime)
                {
                    transform.localScale = Vector3.zero;
                    fadeOutEnd = true;
                }
                else
                {
                    float timeRate = Mathf.Clamp(timer / fadeOutTime, 0.0f, 1.0f);
                    transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, timeRate);
                    timer += manager.GetTimeMultiplier();
                }
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
