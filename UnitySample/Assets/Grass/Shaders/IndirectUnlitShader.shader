Shader "Unlit/IndirectUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
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
            // Indirect�L��
            #pragma  multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 posOS    : POSITION;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 posCS        : SV_POSITION;
                float2 uv           : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
            float4      _Color;
            CBUFFER_END

            // ���W�f�[�^�̍\���̃o�b�t�@
            StructuredBuffer<float4x4> _MatricesBuffer;

            v2f vert(Attributes IN, uint instanceID : SV_InstanceID)
            {
                v2f OUT = (v2f)0;

                float4x4 m =_MatricesBuffer[instanceID];
                float4 worldPosition = mul(m, IN.posOS);

                OUT.posCS = TransformWorldToHClip(worldPosition.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.uv;
                return _Color;
            }
            ENDHLSL
        }
    }
}
