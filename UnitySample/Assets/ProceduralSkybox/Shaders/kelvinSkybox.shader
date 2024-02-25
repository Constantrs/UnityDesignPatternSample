Shader "Custom/kelvinSkybox"
{
    Properties
    {
        [NoScaleOffset] _SkyGradient ("SkyGradient", 2D) = "white" {}
        [NoScaleOffset] _HorizonHazeGradient ("HorizonHazeGradient", 2D) = "white" {}
        [NoScaleOffset] _SunBloomGradient ("SunBloomGradient", 2D) = "white" {}
        [NoScaleOffset] _StarCubeMap ("Star cube map", Cube) = "black" {}
        _SunDir("SunDir", Vector) = (0, 0, 0, 1)
        _SunColor("SunColor", Color) = (1, 1, 1, 1)
        _SunRadius ("SunRadius", Range(0, 10)) = 0.05
        _SunExposure ("SunExposure", Range(-16, 16)) = 0
        _SunInnerBoundary ("SunInnerBoundary", float) = 0.2
        _SunOuterBoundary ("SunOuterBoundary", float) = 0.8
        _SunDisplayLimit ("SunDisplayLimit", Range(-1, 0)) = -0.6
        _MoonDir ("MoonDir", Vector) = (0, 0, 0, 1)
        _MoonColor("MoonColor", Color) = (1, 1, 1, 1)
        _MoonRadius ("MoonRadius", Range(0,1)) = 0.05
        _MoonDisplayLimit ("MoonDisplayLimit", Range(-1, 0)) = -0.6
        _MoonExposure ("MoonExposure", Range(-16, 16)) = 0
        _StarExposure ("StarExposure", Range(-16, 16)) = 0
        _StarPower ("StarPower", Range(1,5)) = 1

    }
    SubShader
    {
        //Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Tags { "Queue"= "Geometry" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off 
        //ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float GetSunMaskByDot(float sunViewDot, float sunRadius)
            {
                float stepRadius = 1 - sunRadius * sunRadius;
                return step(stepRadius, sunViewDot);
            }

            float GetSunMaskByDistance(float3 UV, float3 sunDir, float sunRadius)
            {
                float sunDist = distance(UV, sunDir);
                return 1.0 - (sunDist / sunRadius);
            }

            float sphIntersect(float3 rayDir, float3 spherePos, float radius)
            {
                float3 oc = -spherePos;
                float b = dot(oc, rayDir);
                float c = dot(oc, oc) - radius * radius;
                float h = b * b - c;
                if(h < 0.0) return -1.0;
                h = sqrt(h);
                return -b - h;
            }

            struct Attributes
            {
                float4 posOS    : POSITION;
                float3 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 posCS        : SV_POSITION;
                float3 posWS        : TEXCOORD0;
                float3 uv           : TEXCOORD1;
            };

            TEXTURE2D(_SkyGradient);
            SAMPLER(sampler_SkyGradient);

            TEXTURE2D(_HorizonHazeGradient);
            SAMPLER(sampler_HorizonHazeGradient);
            
            TEXTURE2D(_SunBloomGradient);
            SAMPLER(sampler_SunBloomGradient);

            TEXTURECUBE(_StarCubeMap);      
            SAMPLER(sampler_StarCubeMap);

            CBUFFER_START(UnityPerMaterial)
            float3 _SunDir;
            float4 _SunColor;
            float _SunRadius;
            float _SunExposure;
            float _SunInnerBoundary;
            float _SunOuterBoundary;
            float _SunDisplayLimit;
            float3 _MoonDir;
            float4 _MoonColor;
            float _MoonRadius;
            float _MoonDisplayLimit;
            float _MoonExposure;
            float _StarExposure;
            float _StarPower;
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

            float4 Fragment (v2f IN) : SV_TARGET
            {
                // uv.xyz‚Æ“¯‚¶?
                float3 dir = normalize(IN.posWS);
                //float3 dir = IN.uv;
                
                // Main angles
                float sunViewDot = dot(_SunDir, dir);
                float sunViewDot01 = sunViewDot * 0.5 + 0.5;
                //float sunMoonDot = distance(_SunDir, _MoonDir);

                float sunDist = distance(dir, _SunDir.xyz);
                float height = _SunDir.y * 0.5 + 0.5;

                // The sun
                //float stepRadius = 1 - _SunRadius * _SunRadius;
                //float sunArea =  step(stepRadius, sunViewDot);

                float sunMask = lerp(sunDist / _SunRadius, 1.0f, step(_SunDir.y, _SunDisplayLimit));
                float sunArea = 1.0 - sunMask;
                sunArea = smoothstep(_SunInnerBoundary, _SunOuterBoundary, sunArea);
                float3 sunColor = sunArea *  exp2(_SunExposure) *  _SunColor.rgb;

                // The moon
                float moonIntersect = sphIntersect(dir, _MoonDir, _MoonRadius);
                //float moonMask = lerp(0,  step(_MoonDisplayLimit, _MoonDir.y), 1.0 - step( moonIntersect, -1));
                float moonMask = moonIntersect > -1 ? 1 : 0;
                float3 moonNormal = normalize(_MoonDir - (dir * moonIntersect));
                float moonNdotL = saturate(dot(moonNormal, -_SunDir));
                float3 moonColor = moonMask *  moonNdotL * exp2(_MoonExposure) * _MoonColor.rgb;

                // The sky
                //float skyArea = step(sunArea , sunMask);
                float2 skyUV =  float2(height, 0.5);
                float3 skyColor = SAMPLE_TEXTURE2D(_SkyGradient,sampler_SkyGradient, skyUV).rgb;

                float viewMask = pow(saturate(1.0 - dir.y), 4);
                float3 horizonColor = viewMask * SAMPLE_TEXTURE2D(_HorizonHazeGradient,sampler_HorizonHazeGradient, skyUV).rgb;

                // Sun lighting
                float sunBloomMask = pow(saturate(sunViewDot), 4);
                float3 sunBloomColor = sunBloomMask * SAMPLE_TEXTURE2D(_SunBloomGradient, sampler_SunBloomGradient, skyUV).rgb;

                //
                float3 starColor = SAMPLE_TEXTURECUBE(_StarCubeMap, sampler_StarCubeMap, dir).rgb;
                float starStrength = (1 - sunViewDot01) * (saturate(-_SunDir.y));
                starColor = pow(abs(starColor), _StarPower);
                starColor *= (1 - sunArea) * (1 - moonMask) * exp2(_StarExposure) * starStrength;

                float3 skyCombineColor = skyColor + horizonColor + sunBloomColor + starColor;
                float3 col = skyCombineColor + sunColor + moonColor;
                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}
