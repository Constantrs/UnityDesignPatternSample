using UnityEngine;

namespace DesignPatternSample
{
    public class CursorController : MonoBehaviour
    {
        public RaycastData raycastData = new RaycastData();
        public GameObject clickEffect;
        public Material clickMat;
        public Material forceClickMat;
        public Vector3 effectOffset;

        private bool _initialized = false;
        private ParticleSystem _particle = null;
        private ParticleSystemRenderer _particleRenderer = null;
        private RaycastResult _result = new RaycastResult();

        // Start is called before the first frame update
        private void Awake()
        {
            if (clickEffect != null)
            {
                _particle = clickEffect.GetComponent<ParticleSystem>();
                _particleRenderer = clickEffect.GetComponent<ParticleSystemRenderer>();

                if( _particle != null && _particleRenderer != null  ) 
                {
                    _initialized = true;
                }
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if(!_initialized)
            {
                return;
            }
        }

        /// <summary>
        /// クリックエフェクト再生
        /// </summary>
        public void PlayClickEffct(bool force)
        {
            if (!_initialized)
            {
                return;
            }

            if (_particle)
            {
                if (_particleRenderer != null)
                {
                    _particleRenderer.sharedMaterial = force ? forceClickMat : clickMat;
                }
                _particle.Play();
            }
        }

        /// <summary>
        /// レイキャスト結果取得
        /// </summary>
        public RaycastResult GetRaycastResult()
        {
            CalculateRaycast();
            return _result;
        }

        /// <summary>
        /// レイキャスト結果計算
        /// </summary>
        private void CalculateRaycast()
        {
            if (raycastData != null)
            {
                _result.hitted = false;
                Ray ray = raycastData.camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, raycastData.maxRayDistance, raycastData.layerMask))
                {
                    _result.hitted = true;
                    _result.hitPosition = hit.point;
                    clickEffect.transform.position = hit.point + effectOffset;
                }
            }
        }
    }
}
