using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using static RenderMesIndirect;

public class IndirectGrass : MonoBehaviour
{
    private static readonly int COMMAND_COUNT = 1;

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct GrassAABBInfo
    {
        public Vector3 center;
        public Vector3 extents;
    }

    // Shader Property ID's
    private static readonly int MATRICES_BUFFER_ID = Shader.PropertyToID("_MatricesBuffer");
    private static readonly int BOUNDS_BUFFER_ID = Shader.PropertyToID("_BoundsBuffer");
    private static readonly int OBJECTS_BUFFER_ID = Shader.PropertyToID("_ObjectsBuffer");
    private static readonly int CULLING_RESULT_BUFFER_ID = Shader.PropertyToID("_CullingResultBuffer");

    private static readonly int TOTALCOUNT_ID = Shader.PropertyToID("totalCount");
    private static readonly int CAMERA_VP_MATRIX_ID = Shader.PropertyToID("cameraVPMatrix");

    // �s
    [SerializeField] public int row = 0;
    // ��
    [SerializeField] public int column = 0;
    // ���S
    [SerializeField] public Vector3 centerOffset = Vector3.zero;
    // ���a
    [SerializeField] public float grassRadius;
    // ���x
    [SerializeField] public float grassHeight;
    // �Ԋu
    [SerializeField] public float grassIntervalDistance;

    [SerializeField] private Camera _camera;
    [SerializeField] private Mesh _mesh = null;
    [SerializeField] private Material _material = null;
    [SerializeField] private ComputeShader _cullingComputeShader;

    // IndirectDraw�`��p�o�b�t�@
    private GraphicsBuffer _indirectBuffer = null;
    // IndirectDraw�`��p�p�����[�^
    private GraphicsBuffer.IndirectDrawIndexedArgs[] _commandDatas = null;

    // ���W�o�b�t�@
    private GraphicsBuffer _matricesBuffer = null;
    // ���E���o�b�t�@
    private GraphicsBuffer _boundsBuffer = null;
    // �`�敨�o�b�t�@
    private GraphicsBuffer _objectsBuffer = null;
    // �J�����O���ʃo�b�t�@
    private GraphicsBuffer _cullingResultBuffer = null;

    // �����_���[�p�����[�^
    private RenderParams _renderParams;

    // AABB�p���
    private List<GrassAABBInfo> _AABBInfos = new List<GrassAABBInfo>();

    // �R���s���[�g�V�F�[�_�[�J�[�l���C���f�b�N�X
    private int _kernelIndex;
    private int _totalCount = 0;
    private int _groupX = 0;
    private int _groupY = 0;

    private Matrix4x4 _cameraVPMatrix;

    // Start is called before the first frame update
    private void Awake()
    {
        if (_camera == null ||
            _material == null ||
            _mesh == null)
        {
            return;
        }

        SetupArgsBuffer();
    }

    void OnDestroy()
    {
        _matricesBuffer?.Dispose();
        _matricesBuffer = null;
        _boundsBuffer?.Dispose();
        _boundsBuffer = null;
        _objectsBuffer?.Dispose();
        _objectsBuffer = null;
        _cullingResultBuffer?.Dispose();
        _cullingResultBuffer = null;
        _indirectBuffer?.Dispose();
        _indirectBuffer = null;
    }


    // Start is called before the first frame update
    private void Update()
    {
        // �J����VP
        _cameraVPMatrix = math.mul(_camera.projectionMatrix, _camera.worldToCameraMatrix);

        // �J�����O���ʃ��Z�b�g
        _cullingResultBuffer.SetCounterValue(0);
        _cullingComputeShader.SetInt(TOTALCOUNT_ID, _totalCount);
        _cullingComputeShader.SetMatrix(CAMERA_VP_MATRIX_ID, _cameraVPMatrix);
        _cullingComputeShader.SetBuffer(_kernelIndex, CULLING_RESULT_BUFFER_ID, _cullingResultBuffer);
        _cullingComputeShader.Dispatch(_kernelIndex, _groupX, _groupY, 1);

        // StructuredBuffer > �V�F�[�_ 
        _material.SetBuffer(CULLING_RESULT_BUFFER_ID, _cullingResultBuffer);

        // �J�����O��̕`�搔����
        GraphicsBuffer.CopyCount(_cullingResultBuffer, _indirectBuffer, sizeof(uint));

        Graphics.RenderMeshIndirect(_renderParams, _mesh, _indirectBuffer, COMMAND_COUNT);
    }

    // Update is called once per frame
    private void SetupArgsBuffer()
    {
        // �}�e���A��
        _renderParams = new RenderParams(_material)
        {
            worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one),
        };

        // ���W
        _totalCount = row * column;
        _groupX = Mathf.CeilToInt(row / 32);
        _groupY = Mathf.CeilToInt(column / 32);
        
        float width = (row - 1) * (grassRadius * 2.0f + grassIntervalDistance);
        float depth = (column - 1) * (grassRadius * 2.0f + grassIntervalDistance);
        float3 scale = math.float3(grassRadius * 2.0f, grassHeight, grassRadius * 2.0f);
        // �f�t�H���gXZ���W
        var matrices = new NativeArray<Matrix4x4>(_totalCount, Allocator.Persistent);
        var offs = 0;
        for (var i = 0; i < row; i++)
        {
            var x = -0.5f * width + i * (grassRadius * 2.0f + grassIntervalDistance);
            for (var j = 0; j < column; j++)
            {
                var z = -0.5f * depth + j * (grassRadius * 2.0f + grassIntervalDistance);
                var groundPos = math.float3(centerOffset.x + x, centerOffset.y, centerOffset.z + z);
                var centetPos = math.float3(centerOffset.x + x, centerOffset.y + (grassHeight * 0.5f), centerOffset.z + z);
                _AABBInfos.Add(new GrassAABBInfo() { center = centetPos, extents = new Vector3(grassRadius, 1.0f, grassRadius) });
                matrices[offs] = float4x4.TRS(groundPos, Quaternion.identity, scale);
                offs++;
            }
        }

        // �Ώ�
        var data = new NativeArray<int>(_totalCount, Allocator.Persistent);
        for (var i = 0; i < _totalCount; i++)
        {
            data[i] = i;
        }

        // �s��o�b�t�@
        _matricesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _totalCount, 4 * 4 * sizeof(float));
        _matricesBuffer.SetData(matrices);

        // ���E���o�b�t�@
        _boundsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _totalCount, 2 * 3 * sizeof(float));
        _boundsBuffer.SetData(_AABBInfos.ToArray());

        // �`�敨�o�b�t�@
        _objectsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _totalCount, sizeof(int));
        _objectsBuffer.SetData(data);

        // �J�����O���ʃo�b�t�@
        _cullingResultBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Append, _totalCount, sizeof(int));
        _cullingResultBuffer.SetCounterValue(0);

        // �R���s���[�g�V�F�[�_�[
        _kernelIndex = _cullingComputeShader.FindKernel("CSMain");
        _cullingComputeShader.SetBuffer(_kernelIndex, BOUNDS_BUFFER_ID, _boundsBuffer);
        _cullingComputeShader.SetBuffer(_kernelIndex, OBJECTS_BUFFER_ID, _objectsBuffer);

        // Indirect�R�}���h
        _commandDatas = new GraphicsBuffer.IndirectDrawIndexedArgs[COMMAND_COUNT];
        _commandDatas[0].indexCountPerInstance = _mesh.GetIndexCount(0);
        _commandDatas[0].instanceCount = (uint)_totalCount;
        _commandDatas[0].baseVertexIndex = _mesh.GetBaseVertex(0);
        _commandDatas[0].startIndex = _mesh.GetIndexStart(0);

        // Indirect�`��p�o�b�t�@
        _indirectBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, COMMAND_COUNT, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        _indirectBuffer.SetData(_commandDatas);

        // StructuredBuffer > �V�F�[�_ 
        _material.SetBuffer(MATRICES_BUFFER_ID, _matricesBuffer);

        matrices.Dispose();
        data.Dispose();
    }
}
