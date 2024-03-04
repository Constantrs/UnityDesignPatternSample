using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;


public class RenderMeshIndirectTest : MonoBehaviour
{
    private static readonly int COMMAND_COUNT = 1;

    [SerializeField] private Camera _camera;
    [SerializeField] private Mesh _mesh = null;
    [SerializeField] private Material _material = null;
    [SerializeField] private int _row = 0;
    [SerializeField] private int _column = 0;
    [SerializeField] private Vector3 _centerOffset = Vector3.zero;

    [SerializeField] private ComputeShader _sinwaveComputeShader;

    // IndirectDraw描画用バッファ
    private GraphicsBuffer _IndirectBuffer = null;
    // 行列バッファ
    private GraphicsBuffer _MatricesBuffer = null;
    // IndirectDraw描画用パラメータ
    private GraphicsBuffer.IndirectDrawIndexedArgs[] _CommandDatas = null;
    // レンダラーパラメータ
    private RenderParams _RenderParams;

    // コンピュートシェーダーカーネルインデックス
    private int _kernelIndex;

    // Start is called before the first frame update
    void Start()
    {
        SetupArgsBuffer();
    }

    void OnDestroy()
    {
        _MatricesBuffer?.Dispose();
        _MatricesBuffer = null;
        _IndirectBuffer?.Dispose();
        _IndirectBuffer = null;
    }

    // Update is called once per frame
    void Update()
    {
        // CPU版 
        //_TransformBuffer.Update(Time.time);
        // CPUデータ > StructuredBuffer
        //_MatricesBuffer.SetData(_TransformBuffer.Matrices);

        _sinwaveComputeShader.SetFloat("totalTime", Time.time);
        _sinwaveComputeShader.SetVector("centerOffset", _centerOffset);
        _sinwaveComputeShader.Dispatch(_kernelIndex, _row, _column, 1);

        // StructuredBuffer > シェーダ 
        _material.SetBuffer("_MatricesBuffer", _MatricesBuffer);

        Graphics.RenderMeshIndirect(_RenderParams, _mesh, _IndirectBuffer, COMMAND_COUNT);
    }

    private void SetupArgsBuffer()
    {
        // マテリアル
        _RenderParams = new RenderParams(_material)
        {
            worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one),
            //matProps = new MaterialPropertyBlock()
        };

        // 座標
        int count =_row * _column;
        _centerOffset = new Vector3(0.0f, 0.0f, (float)_column / 2.0f);
        // GPU版TransformBuffer不要
        //_TransformBuffer = new PositionBuffer(_row, _column, _centerOffset);
        
        // デフォルトXZ座標
        var matrices = new NativeArray<Matrix4x4>(count, Allocator.Persistent);
        var offs = 0;
        for (var i = 0; i < _row; i++)
        {
            var x = i - _row * 0.5f + 0.5f;
            for (var j = 0; j < _column; j++)
            {
                var z = j - _column * 0.5f + 0.5f;
                var p = math.float3(_centerOffset.x + x, _centerOffset.y, _centerOffset.z + z);
                matrices[offs] = float4x4.Translate(p);
                offs++;
            }
        }

        // 行列バッファ
        _MatricesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, 4 * 4 * sizeof(float));
        _MatricesBuffer.SetData(matrices);

        // コンピュートシェーダー
        _kernelIndex = _sinwaveComputeShader.FindKernel("CSMain");
        _sinwaveComputeShader.SetBuffer(_kernelIndex, "_MatricesBuffer", _MatricesBuffer);
        _sinwaveComputeShader.SetInt("dimsX", _row);
        _sinwaveComputeShader.SetInt("dimsY", _column);

        // Indirectコマンド
        _CommandDatas = new GraphicsBuffer.IndirectDrawIndexedArgs[COMMAND_COUNT];
        _CommandDatas[0].indexCountPerInstance = _mesh.GetIndexCount(0);
        _CommandDatas[0].instanceCount = (uint)count;
        _CommandDatas[0].baseVertexIndex = _mesh.GetBaseVertex(0);
        _CommandDatas[0].startIndex = _mesh.GetIndexStart(0);

        // Indirect描画用バッファ
        _IndirectBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, COMMAND_COUNT, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        _IndirectBuffer.SetData(_CommandDatas);

        matrices.Dispose();
    }
}
