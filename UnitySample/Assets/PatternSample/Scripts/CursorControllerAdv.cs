using UnityEngine;


[System.Serializable]
public class RaycastDataAdv
{
    public Camera camera;
    public LayerMask layerMask;
    public float maxRayDistance;
    public Vector3 effectOffset;
}

public class RaycastResultAdv
{
    public bool hitted = false;
    public Vector3 targetPos = Vector3.zero;
}

[System.Serializable]
public class CursorControllerAdv
{
    public RaycastDataAdv raycastData = new RaycastDataAdv();
    public GameObject effect;
    public Material clickMat;
    public Material forceClickMat;

    private ParticleSystem _particle = null;
    private RaycastResultAdv _result = new RaycastResultAdv();

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if(effect != null) 
        {
           _particle = effect.GetComponent<ParticleSystem>();
        }
    }
    public void CalculateRaycast()
    {
        if (raycastData != null)
        {
            _result.hitted = false;
            Ray ray = raycastData.camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, raycastData.maxRayDistance, raycastData.layerMask))
            {
                _result.hitted = true;
                _result.targetPos = hit.point;
                effect.transform.position = hit.point + raycastData.effectOffset;
            }
        }
    }

    public void PlayClickEffct()
    {
        if (_particle)
        {
            var renderer = effect.GetComponent<ParticleSystemRenderer>();
            if(renderer != null) 
            {
                renderer.sharedMaterial = clickMat;
            }
           _particle.Play();
        }
    }

    public void PlayForceClickEffct()
    {
        if (_particle)
        {
            var renderer = effect.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = forceClickMat;
            }
            _particle.Play();
        }
    }

    public RaycastResultAdv GetRaycastResult()
    {
        return _result;
    }
}
