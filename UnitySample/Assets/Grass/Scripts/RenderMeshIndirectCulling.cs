using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class RenderMesIndirect : MonoBehaviour
{
    private static readonly int COMMAND_COUNT = 1;

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct AABBBoundsInfo
    {
        public Vector3 center;
        public Vector3 extents;
    }

    // Shader Property ID's
    private static readonly int MATRICES_BUFFER_ID = Shader.PropertyToID("_MatricesBuffer");
    private static readonly int BOUNDS_BUFFER_ID  = Shader.PropertyToID("_BoundsBuffer");
    private static readonly int MATERIALS_BUFFER_ID = Shader.PropertyToID("_MaterialsBuffer");
    private static readonly int OBJECTS_BUFFER_ID = Shader.PropertyToID("_ObjectsBuffer");

    [SerializeField] private Camera _camera;
    [SerializeField] private Mesh _mesh = null;
    [SerializeField] private Material _material = null;
    [SerializeField] private int _row = 0;
    [SerializeField] private int _column = 0;
    [SerializeField] private Vector3 _centerOffset = Vector3.zero;

    [SerializeField] private ComputeShader _cullingComputeShader;

    // IndirectDraw描画用バッファ
    private GraphicsBuffer _IndirectBuffer = null;
    // IndirectDraw描画用パラメータ
    private GraphicsBuffer.IndirectDrawIndexedArgs[] _CommandDatas = null;

    // 座標バッファ
    private GraphicsBuffer _MatricesBuffer = null;
    // 境界情報バッファ
    private GraphicsBuffer _BoundsBuffer = null;
    // マテリアルバッファ
    private GraphicsBuffer _MaterialsBuffer = null;
    // 描画物バッファ
    private GraphicsBuffer _ObjectsBuffer = null;

    // レンダラーパラメータ
    private RenderParams _RenderParams;

    // AABB用情報
    private List<AABBBoundsInfo> _BoundsInfos = new List<AABBBoundsInfo>();

    // コンピュートシェーダーカーネルインデックス
    private int _kernelIndex;
    private int _totalCount = 0;
    
    //private Matrix4x4 _mvpMatrix;

    // Start is called before the first frame update
    void Start()
    {
        if(_camera == null || 
            _material == null ||
            _mesh == null)
        {
            return;
        }

        SetupArgsBuffer();
    }

    void OnDestroy()
    {
        _MatricesBuffer?.Dispose();
        _MatricesBuffer = null;
        _BoundsBuffer?.Dispose();
        _BoundsBuffer = null;
        _MaterialsBuffer?.Dispose();
        _MaterialsBuffer = null;
        _ObjectsBuffer?.Dispose();
        _ObjectsBuffer = null;
        _IndirectBuffer?.Dispose();
        _IndirectBuffer = null;
    }

    // Update is called once per frame
    void Update()
    {
        //Matrix4x4 v = _camera.worldToCameraMatrix;
        //Matrix4x4 p = _camera.projectionMatrix;
        //_mvpMatrix = p * v;

        //_cullingComputeShader.SetMatrix("cameraMartixVP", _mvpMatrix);
        //_cullingComputeShader.Dispatch(_kernelIndex, _totalCount, 1, 1);

        //// StructuredBuffer > シェーダ 
        //_material.SetBuffer("_MVPBuffer", _MVPBuffer);
        //_material.SetBuffer("_MVPBuffer", _MVPBuffer);
        //_material.SetBuffer("_MVPBuffer", _MVPBuffer);

        Graphics.RenderMeshIndirect(_RenderParams, _mesh, _IndirectBuffer, COMMAND_COUNT);
    }

    void SetupArgsBuffer()
    {
        // マテリアル
        _RenderParams = new RenderParams(_material)
        {
            worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one),
        };

        // 座標
        _totalCount = _row * _column;
        _centerOffset = new Vector3(0.0f, 0.0f, (float)_column / 2.0f);

        // デフォルトXZ座標
        var matrices = new NativeArray<Matrix4x4>(_totalCount, Allocator.Persistent);
        var offs = 0;
        for (var i = 0; i < _row; i++)
        {
            var x = i - _row * 0.5f + 0.5f;
            for (var j = 0; j < _column; j++)
            {
                var z = j - _column * 0.5f + 0.5f;
                var p = math.float3(_centerOffset.x + x, _centerOffset.y, _centerOffset.z + z);
                _BoundsInfos.Add(new AABBBoundsInfo() { center = p, extents = Vector3.one });
                matrices[offs] = float4x4.Translate(p);
                offs++;
            }
        }

        // デフォルトカラー
        var colors = new NativeArray<float4>(12, Allocator.Persistent);
        colors[0] = math.float4(1.0f, 0.0f, 0.0f, 1.0f);
        colors[1] = math.float4(0.0f, 1.0f, 0.0f, 1.0f);
        colors[2] = math.float4(0.0f, 0.0f, 1.0f, 1.0f);
        colors[3] = math.float4(1.0f, 1.0f, 0.0f, 1.0f);
        colors[4] = math.float4(1.0f, 0.0f, 1.0f, 1.0f);
        colors[5] = math.float4(0.0f, 1.0f, 1.0f, 1.0f);
        colors[6] = math.float4(0.5f, 0.5f, 0.0f, 1.0f);
        colors[7] = math.float4(0.5f, 0.0f, 0.5f, 1.0f);
        colors[8] = math.float4(0.0f, 0.5f, 0.5f, 1.0f);
        colors[9] = math.float4(1.0f, 1.0f, 1.0f, 1.0f);
        colors[10] = math.float4(0.5f, 0.5f, 0.5f, 1.0f);
        colors[11] = math.float4(0.0f, 0.0f, 0.0f, 1.0f);

        // 対象
        var data = new NativeArray<int2>(_totalCount, Allocator.Persistent);
        for (var i = 0; i < _totalCount; i++)
        {
            int randomColor = UnityEngine.Random.Range(0, 12);
            data[i] = math.int2(i, randomColor);
        }

        // 行列バッファ
        _MatricesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _totalCount, 4 * 4 * sizeof(float));
        _MatricesBuffer.SetData(matrices);

        // 境界情報バッファ
        _BoundsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _totalCount, 2 * 3 * sizeof(float));
        _BoundsBuffer.SetData(_BoundsInfos.ToArray());

        // カラーバッファ
        _MaterialsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _totalCount, 4 * sizeof(float));
        _MaterialsBuffer.SetData(colors);

        // 描画物バッファ
        _ObjectsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _totalCount, 2 * sizeof(uint));
        _ObjectsBuffer.SetData(data);

        // コンピュートシェーダー
        _kernelIndex = _cullingComputeShader.FindKernel("CSMain");
        //_cullingComputeShader.SetBuffer(_kernelIndex, BOUNDS_BUFFER_ID, _BoundsBuffer);
        //_cullingComputeShader.SetBuffer(_kernelIndex, "_ObjectsBuffer", _ObjectsBuffer);

        // Indirectコマンド
        _CommandDatas = new GraphicsBuffer.IndirectDrawIndexedArgs[COMMAND_COUNT];
        _CommandDatas[0].indexCountPerInstance = _mesh.GetIndexCount(0);
        _CommandDatas[0].instanceCount = (uint)_totalCount;
        _CommandDatas[0].baseVertexIndex = _mesh.GetBaseVertex(0);
        _CommandDatas[0].startIndex = _mesh.GetIndexStart(0);

        // Indirect描画用バッファ
        _IndirectBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, COMMAND_COUNT, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        _IndirectBuffer.SetData(_CommandDatas);

        // StructuredBuffer > シェーダ 
        _material.SetBuffer(MATRICES_BUFFER_ID, _MatricesBuffer);
        _material.SetBuffer(MATERIALS_BUFFER_ID, _MaterialsBuffer);
        _material.SetBuffer(OBJECTS_BUFFER_ID, _ObjectsBuffer);

        matrices.Dispose();
        colors.Dispose();
        data.Dispose();
    }
}
