Shader "Unlit/IndirectUnlitShader2"
{
    Properties
    {

    }
    SubShader
    {
        Tags { "Queue"= "Geometry" "RenderType"="Opaque" "RenderPipeline" = "UniversalRenderPipeline" "PreviewType"="Sphere" }

        Pass
        {
            HLSLPROGRAM
            // vert�Ƃ������O�̊֐���vertex�V�F�[�_�[�ł��@�Ɛ錾����GPU�ɋ�����B
            #pragma vertex vert
            // frag�Ƃ������O�̊֐���fragment�V�F�[�_�[�ł��@�Ɛ錾����GPU�ɋ�����B
            #pragma fragment frag
            // �V�F�[�_���f��4.5
            #pragma target 4.5
            // Indirect�L��
            #pragma  multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "IndirectCommon.hlsl"

            struct Attributes
            {
                float4 posOS    : POSITION;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 posCS        : SV_POSITION;
                float2 uv           : TEXCOORD1;
                float4 color        : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // �C���X�^���X�f�[�^
            StructuredBuffer<float4x4> _MatricesBuffer;
            StructuredBuffer<float4> _MaterialsBuffer;
            StructuredBuffer<instanceData> _ObjectsBuffer;

            v2f vert(Attributes IN, uint instanceID : SV_InstanceID)
            {
                v2f OUT = (v2f)0;

                instanceData data = _ObjectsBuffer[instanceID];
                float4x4 m = _MatricesBuffer[data.matrixIndex];
                float4 worldPosition = mul(m, IN.posOS);

                OUT.posCS = TransformWorldToHClip(worldPosition.xyz);
                OUT.uv = IN.uv;
                OUT.color = _MaterialsBuffer[data.materialIndex];
                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.uv;
                return IN.color;
            }
            ENDHLSL
        }
    }
}
