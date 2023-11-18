using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[System.Serializable]
public class RaycastData
{
    public Camera camera;
    public LayerMask layerMask;
    public float maxRayDistance;
    public Vector3 effectOffset;
}

public class RaycastResult
{
    public bool hitted = false;
    public Vector3 targetPos = Vector3.zero;
}

[System.Serializable]
public class CursorController
{
    public RaycastData raycastData = new RaycastData();
    public GameObject effect;

    private ParticleSystem _particle = null;
    private RaycastResult _result = new RaycastResult();

    public void Initialize()
    {
        if(effect != null) 
        {
           _particle = effect.GetComponent<ParticleSystem>();
        }
    }
    
    public void UpdateCursor()
    {
        if (raycastData != null)
        {
            _result.hitted = false;
            Ray ray = raycastData.camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, raycastData.maxRayDistance, raycastData.layerMask))
            {
                if (_particle)
                {
                    _particle.Play();
                }
                _result.hitted = true;
                _result.targetPos = hit.point;
                effect.transform.position = hit.point + raycastData.effectOffset;
            }
        }
    }

    public RaycastResult GetRaycastResult()
    {
        return _result;
    }
}
