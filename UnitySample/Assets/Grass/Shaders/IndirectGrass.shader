Shader "Unlit/IndirectGrass"
{
    Properties
    {
        _BottomColor("Bottom Color", Color) = (0, 0, 0, 1)
		_TopColor("Top Color", Color) = (1, 1, 1, 1)
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
            #include "IndirectGrassCommon.hlsl"

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

            // �C���X�^���X�f�[�^
            StructuredBuffer<float4x4> _MatricesBuffer;
            StructuredBuffer<instanceData> _CullingResultBuffer;

            CBUFFER_START(UnityPerMaterial)
				float4 _BottomColor;
				float4 _TopColor;
			CBUFFER_END

            v2f vert(Attributes IN, uint instanceID : SV_InstanceID)
            {
                v2f OUT = (v2f)0;

                instanceData data = _CullingResultBuffer[instanceID];
                float4x4 m = _MatricesBuffer[data.matrixIndex];
                float4 worldPosition = mul(m, IN.posOS);

                OUT.posCS = TransformWorldToHClip(worldPosition.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.uv;
                float4 color = lerp( _BottomColor, _TopColor, uv.y);
                return  color;
            }
            ENDHLSL
        }
    }
}
