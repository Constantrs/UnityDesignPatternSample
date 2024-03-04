Shader "Unlit/InstancedUnlitShader"
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
               // vertという名前の関数がvertexシェーダーです　と宣言してGPUに教える。
            #pragma vertex vert
            // fragという名前の関数がfragmentシェーダーです　と宣言してGPUに教える。
            #pragma fragment frag
            // GPUインスタンシングを有効にする。
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 posOS    : POSITION;
                float2 uv       : TEXCOORD0;
                // GPUインスタンシングに必要な変数
                UNITY_VERTEX_INPUT_INSTANCE_ID
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

            v2f vert(Attributes IN, uint instanceID : SV_InstanceID)
            {
                // 先ほど宣言した構造体のオブジェクトを作る。
                v2f OUT = (v2f)0;

                // GPUインスタンシングに必要な変数を設定する。
                UNITY_SETUP_INSTANCE_ID(IN);

                OUT.posCS = TransformObjectToHClip(IN.posOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.uv;
                //float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                //float3 color = (texColor.rgb * _Color.rgb);
                return _Color;
            }
            ENDHLSL
        }
    }
}
