Shader "Unlit/IndirectGrass"
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
            // vertという名前の関数がvertexシェーダーです　と宣言してGPUに教える。
            #pragma vertex vert
            // fragという名前の関数がfragmentシェーダーです　と宣言してGPUに教える。
            #pragma fragment frag
            // シェーダモデル4.5
            #pragma target 4.5
            // Indirect有効
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

            // インスタンスデータ
            StructuredBuffer<float4x4> _MatricesBuffer;
            StructuredBuffer<instanceData> _CullingResultBuffer;

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
                return float4(uv.y,uv.y,uv.y,1.0);
            }
            ENDHLSL
        }
    }
}
