Shader "Custom/Skybox"
{
    Properties
    {
         [MainTexture] _BaseMap("Main Texture", 2D) = "white" {}
         [MainColor] _Color("Color", Color) = (1,1,1,1)
         _SunColor("SunColor", Color) = (1,1,1,1)
         _SunRadius ("SunRadius", float) = 1.0
         _SunStrength ("SunStrength", float) = 1.0
         _SunInnerBoundary ("SunInnerBoundary", float) = 0.2
         _SunOuterBoundary ("SunOuterBoundary", float) = 0.8
         _DaySkyColor("DaySkyColor", Color) = (1,1,1,1)
         _DayHorizonColor("DayHorizonColor", Color) = (1,1,1,1)
         _NightSkyColor("NightSkyColor", Color) = (1,1,1,1)
         _NightHorizonColor("NightHorizonColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags 
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "Procedural Skybox"
            Tags { "RenderType" = "Opaque" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" }

            HLSLPROGRAM
           
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #define PI 3.141592653589793

            struct Attributes
            {
                float4 positionOS   : POSITION;
                half3  normal       : NORMAL;
                float3 uv: TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                half3  positionWS     : TEXCOORD0;
                half3  normalWS     : TEXCOORD1;
                float3 uv: TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float4 _BaseMap_ST;
            float4 _SunColor;
            float _SunRadius;
            float _SunStrength;
            float _SunInnerBoundary;
            float _SunOuterBoundary;
            float4 _DaySkyColor;
            float4 _DayHorizonColor;
            float4 _NightSkyColor;
            float4 _NightHorizonColor;
            CBUFFER_END

            float FastAcosForAbsCos(float in_abs_cos) {
                float _local_tmp = ((in_abs_cos * -0.0187292993068695068359375 + 0.074261002242565155029296875) * in_abs_cos - 0.212114393711090087890625) * in_abs_cos + 1.570728778839111328125;
                return _local_tmp * sqrt(1.0 - in_abs_cos);
            }

            float FastAcos(float in_cos) {
                float local_abs_cos = abs(in_cos);
                float local_abs_acos = FastAcosForAbsCos(local_abs_cos);
                return in_cos < 0.0 ?  PI - local_abs_acos : local_abs_acos;
            }


            float Remap(float In, float InMin, float InMax, float OutMin, float OutMax)
            {
                return OutMin + (In - InMin) * (OutMax - OutMin) / (InMax - InMin);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normal);
                OUT.uv = IN.uv;
                //OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Main lightî•ñ‚ÌŽæ“¾.
                Light mainLight;
                mainLight = GetMainLight();

                //float2 rad = float2(atan2(dir.x, dir.z), FastAcos(dir.y));
                //float2 sphereCoords = rad / float2(2.0 * PI, PI / 2);

               //float3 dir = normalize(IN.positionWS);
               float3 dir = IN.uv;
               float3 sundir = -mainLight.direction;

                float sunDist = distance(dir, sundir);
                float sunArea = 1.0 - (sunDist / _SunRadius);
                //half3 sunAndMoon = (sunDisc * _SunColor.rgb);
                sunArea = smoothstep(_SunInnerBoundary, _SunOuterBoundary, sunArea * _SunStrength);
                half3 sunColor = sunArea * _SunColor.rgb;

                float height = abs(sundir.y);
                float dayNightStep =step(0.0, sundir.y);

                //float daynight = sundir.y * 0.5 + 0.5;
                //half3 gradientDay = lerp(_DayHorizonColor.xyz, _DaySkyColor.xyz, Remap( daynight, 0.5, 1.0, 0.0, 1.0));
                //half3 gradientNight = lerp(_NightHorizonColor.xyz, _NightSkyColor.xyz, Remap( daynight, 0.0, 0.5, 0.0, 1.0));
                
                //float daynight = sundir.y;
                //half3 gradientDay = _DaySkyColor.xyz;
                //half3 gradientNight = _NightSkyColor.xyz;
                //half3 skyGradients = lerp(gradientNight, gradientDay,  step(0.0,daynight));

                float2 uv = float2( sundir.y * 0.5 + 0.5, 0.5);
                half3 skyGradients = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).rgb;

                half3 combined = sunColor + skyGradients;
                return half4(combined, 1.0);
                //return half4(combined, 1.0);

                 //SUN
                //dist of skybox suface pixel and sun point
                //float sunDist = distance(i.uv.xyz, _WorldSpaceLightPos0);
	            //float sunArea = 1 - sunDist / _SunSize;
	            //sunArea = smoothstep(_SunInnerBound, _SunOuterBound, sunArea);
                //float3 fallSunColor = _SunColor.rgb * 0.4;
                //float3 finalSunColor = lerp(fallSunColor,_SunColor.rgb,smoothstep(-0.03,0.03,_WorldSpaceLightPos0.y)) * sunArea;

                //float3 gradientDay = lerp(_DayBottomColor, _DayTopColor, saturate(i.uv.y));
                //float3 gradientNight = lerp(_NightBottomColor, _NightTopColor, saturate(i.uv.y));
                //float3 skyGradients = lerp(gradientNight, gradientDay,saturate(mainLight.direction.y));
                
                // sun
                //float sun = distance(i.uv.xyz, mainLight.direction);
                //float sunDisc = 1 - (sun / _SunRadius);
                //sunDisc = saturate(sunDisc * 50);

                //float4 finalColor = float4(1.0, 1.0, 1.0, 1.0);

                //float4 color = float4(skyGradients, 1.0);
                
                //return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
