using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;


public struct ObjectAABBInfo
{
    public Bounds bound;
    public Color color;
}

public class RenderMeshInstancedTest : MonoBehaviour
{
    private static readonly int INSTANCE_BATCH_MAX_COUNT = 511;

    public bool frustumCulling;

    [SerializeField] private Camera _camera;
    [SerializeField] private Mesh _mesh = null;
    [SerializeField] private Material _material = null;
    [SerializeField] private int row = 0;
    [SerializeField] private int column = 0;

    private PositionBuffer _buffer;
    private List<ObjectAABBInfo> _AABBInfo = new List<ObjectAABBInfo>();
    // レンダラーパラメータ
    private RenderParams _RenderParams;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 offset = new Vector3(0.0f, 0.0f, (float)column / 2.0f);
        _buffer = new PositionBuffer(row, column, offset);
        if(frustumCulling ) 
        {
            foreach (var pos in _buffer.Positions)
            {
                _AABBInfo.Add(new ObjectAABBInfo { bound = new Bounds(), color = Color.blue });
            }
        }

        _RenderParams = new RenderParams(_material);
    }

    void OnDestroy()
    {
        _buffer.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        _buffer.Update(Time.time);

        if (frustumCulling)
        {
            var position = _buffer.Positions;

            var planes = GeometryUtility.CalculateFrustumPlanes(_camera);

            for (int i = 0; i < position.Length; i++)
            {
                Vector3 center = position[i];
                ObjectAABBInfo info = new ObjectAABBInfo();
                info.bound = new Bounds(center, Vector3.one);

                if (GeometryUtility.TestPlanesAABB(planes, info.bound))
                {
                    info.color = Color.red;
                }
                else
                {
                    info.color = Color.green;
                }
                _AABBInfo[i] = info;
            }
        }

        var matrices = _buffer.Matrices;

        Profiler.BeginSample("Mass Mesh Update");

        for (var offs = 0; offs < matrices.Length; offs += INSTANCE_BATCH_MAX_COUNT)
        {
            var count = Mathf.Min(INSTANCE_BATCH_MAX_COUNT, matrices.Length - offs);
            Graphics.RenderMeshInstanced(_RenderParams, _mesh, 0, matrices, count, offs);
        }

        Profiler.EndSample();
    }

    private void OnDrawGizmos()
    {
        if (frustumCulling)
        {
            if (_camera != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawFrustum(_camera.transform.position, _camera.fieldOfView, _camera.farClipPlane, _camera.nearClipPlane, _camera.aspect);
            }

            if (_buffer != null)
            {
                foreach (var info in _AABBInfo)
                {
                    Gizmos.color = info.color;
                    Gizmos.DrawWireCube(info.bound.center, Vector3.one);
                }
            }
        }
    }
}
