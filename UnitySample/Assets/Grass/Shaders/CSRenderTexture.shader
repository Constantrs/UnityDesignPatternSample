Shader "Custom/CSRenderTexture"
{
    Properties
    {
         [Header(Main)]
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex("MainTex", 2D) = "white" {}
     }

     SubShader
     {
        Tags { "Queue"= "Geometry" "RenderType"="Opaque" "PreviewType"="Plane" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 posOS    : POSITION;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 posCS        : SV_POSITION;
                float3 posWS        : TEXCOORD0;
                float2 uv           : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
            float4      _Color;
            CBUFFER_END

            v2f Vertex(Attributes IN)
            {
                v2f OUT = (v2f)0;
    
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.posOS.xyz);
    
                OUT.posCS = vertexInput.positionCS;
                OUT.posWS = vertexInput.positionWS;
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 Fragment(v2f IN) : SV_Target
            {
                float2 uv = IN.uv;
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                //float3 color = (texColor.rgb * _Color.rgb);
                return float4(texColor * _Color);
            }
            ENDHLSL
        }
    }
}
