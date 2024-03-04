using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

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
    private static readonly int CULLING_RESULT_BUFFER_ID = Shader.PropertyToID("_CullingResultBuffer");

    private static readonly int CAMERA_VP_MATRIX_ID = Shader.PropertyToID("cameraVPMatrix");

    [SerializeField] private Camera _camera;
    [SerializeField] private Mesh _mesh = null;
    [SerializeField] private Material _material = null;
    [SerializeField] private int _row = 0;
    [SerializeField] private int _column = 0;
    [SerializeField] private Vector3 _centerOffset = Vector3.zero;

    [SerializeField] private ComputeShader _cullingComputeShader;
    [SerializeField] private bool _debugDisplayAABB;

    // IndirectDraw�`��p�o�b�t�@
    private GraphicsBuffer _indirectBuffer = null;
    // IndirectDraw�`��p�p�����[�^
    private GraphicsBuffer.IndirectDrawIndexedArgs[] _commandDatas = null;

    // ���W�o�b�t�@
    private GraphicsBuffer _matricesBuffer = null;
    // ���E���o�b�t�@
    private GraphicsBuffer _boundsBuffer = null;
    // �}�e���A���o�b�t�@
    private GraphicsBuffer _materialsBuffer = null;
    // �`�敨�o�b�t�@
    private GraphicsBuffer _objectsBuffer = null;
    // �J�����O���ʃo�b�t�@
    private GraphicsBuffer _cullingResultBuffer = null;

    // �����_���[�p�����[�^
    private RenderParams _renderParams;

    // AABB�p���
    private List<AABBBoundsInfo> _boundsInfos = new List<AABBBoundsInfo>();
    private List<Color> _debugBoundsColor = new List<Color>();

    // �R���s���[�g�V�F�[�_�[�J�[�l���C���f�b�N�X
    private int _kernelIndex;
    private int _totalCount = 0;
    
    private Matrix4x4 _cameraVPMatrix;

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
        _matricesBuffer?.Dispose();
        _matricesBuffer = null;
        _boundsBuffer?.Dispose();
        _boundsBuffer = null;
        _materialsBuffer?.Dispose();
        _materialsBuffer = null;
        _objectsBuffer?.Dispose();
        _objectsBuffer = null;
        _cullingResultBuffer?.Dispose();
        _cullingResultBuffer = null;
        _indirectBuffer?.Dispose();
        _indirectBuffer = null;
    }

    // Update is called once per frame
    void Update()
    {
        // �J����VP
        _cameraVPMatrix = math.mul(_camera.projectionMatrix, _camera.worldToCameraMatrix); 

        UpdateDebugAABBInfo();

        // �J�����O���ʃ��Z�b�g
        _cullingResultBuffer.SetCounterValue(0);
        _cullingComputeShader.SetMatrix(CAMERA_VP_MATRIX_ID, _cameraVPMatrix);
        _cullingComputeShader.SetBuffer(_kernelIndex, CULLING_RESULT_BUFFER_ID, _cullingResultBuffer);
        _cullingComputeShader.Dispatch(_kernelIndex, (_totalCount / 32), 1, 1);

        // StructuredBuffer > �V�F�[�_ 
        _material.SetBuffer(CULLING_RESULT_BUFFER_ID, _cullingResultBuffer);

        // �J�����O��̕`�搔����
        GraphicsBuffer.CopyCount(_cullingResultBuffer, _indirectBuffer, sizeof(uint));

        Graphics.RenderMeshIndirect(_renderParams, _mesh, _indirectBuffer, COMMAND_COUNT);
    }

    void SetupArgsBuffer()
    {
        // �}�e���A��
        _renderParams = new RenderParams(_material)
        {
            worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one),
        };

        // ���W
        _totalCount = _row * _column;
        _centerOffset = new Vector3(0.0f, 0.0f, (float)_column / 2.0f);

        // �f�t�H���gXZ���W
        var matrices = new NativeArray<Matrix4x4>(_totalCount, Allocator.Persistent);
        var offs = 0;
        for (var i = 0; i < _row; i++)
        {
            var x = i - _row * 0.5f + 1.0f;
            for (var j = 0; j < _column; j++)
            {
                var z = j - _column * 0.5f + 1.0f;
                var p = math.float3(_centerOffset.x + x, _centerOffset.y, _centerOffset.z + z);
                _boundsInfos.Add(new AABBBoundsInfo() { center = p, extents = new Vector3(0.5f, 0.5f, 0.5f) });
                _debugBoundsColor.Add(Color.white);
                matrices[offs] = float4x4.Translate(p);
                offs++;
            }
        }

        // �f�t�H���g�J���[
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

        // �Ώ�
        var data = new NativeArray<int2>(_totalCount, Allocator.Persistent);
        for (var i = 0; i < _totalCount; i++)
        {
            int randomColor = UnityEngine.Random.Range(0, 12);
            data[i] = math.int2(i, randomColor);
        }

        // �s��o�b�t�@
        _matricesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _totalCount, 4 * 4 * sizeof(float));
        _matricesBuffer.SetData(matrices);

        // ���E���o�b�t�@
        _boundsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _totalCount, 2 * 3 * sizeof(float));
        _boundsBuffer.SetData(_boundsInfos.ToArray());

        // �J���[�o�b�t�@
        _materialsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _totalCount, 4 * sizeof(float));
        _materialsBuffer.SetData(colors);

        // �`�敨�o�b�t�@
        _objectsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _totalCount, 2 * sizeof(uint));
        _objectsBuffer.SetData(data);

        // �J�����O���ʃo�b�t�@
        _cullingResultBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Append, _totalCount, 2 * sizeof(uint));
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
        _material.SetBuffer(MATERIALS_BUFFER_ID, _materialsBuffer);
        //_material.SetBuffer(OBJECTS_BUFFER_ID, _objectsBuffer);

        matrices.Dispose();
        colors.Dispose();
        data.Dispose();
    }

    void UpdateDebugAABBInfo()
    {
        if (_debugDisplayAABB)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(_camera);

            for (var i = 0; i < _totalCount; i++)
            {
                var boundInfo = _boundsInfos[i];
                var bound = new Bounds(boundInfo.center, boundInfo.extents);
                if (GeometryUtility.TestPlanesAABB(planes, bound))
                {
                    _debugBoundsColor[i] = Color.red;
                }
                else
                {
                    _debugBoundsColor[i] = Color.green;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_debugDisplayAABB)
        {
            if (_camera != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawFrustum(_camera.transform.position, _camera.fieldOfView, _camera.farClipPlane, _camera.nearClipPlane, _camera.aspect);
            }

            if (_boundsInfos != null)
            {
                for (var i = 0; i < _totalCount; i++)
                {
                    var boundInfo = _boundsInfos[i];
                    Vector3 size = boundInfo.extents * 2.0f;
                    Gizmos.color = _debugBoundsColor[i];
                    Gizmos.DrawWireCube(boundInfo.center, size);
                }
            }
        }
    }
}
