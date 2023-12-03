using UnityEngine;

namespace DesignPatternSample
{

    [System.Serializable]
    public class RaycastData
    {
        public Camera camera;
        public LayerMask layerMask;
        public float maxRayDistance;
    }

    public class RaycastResult
    {
        public bool hitted = false;
        public Vector3 hitPosition = Vector3.zero;
    }
}