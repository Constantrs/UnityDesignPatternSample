Shader "Custom/kelvinSkybox"
{
    Properties
    {
        [Header(Sun Settings)]
        _SunDir("Sun Dir", Vector) = (0, 0, 0, 1)
        _SunColor("Sun Color", Color) = (1, 1, 1, 1)
        _SunRadius ("Sun Radius", Range(0, 10)) = 0.05
        _SunExposure ("Sun Exposure", Range(-16, 16)) = 0
        _SunInnerBoundary ("Sun InnerBoundary", float) = 0.2
        _SunOuterBoundary ("Sun OuterBoundary", float) = 0.8
        _SunDisplayLimit ("Sun DisplayLimit", Range(-1, 0)) = -0.6

        [Header(Moon Settings)]
        _MoonDir ("Moon Dir", Vector) = (0, 0, 0, 1)
        _MoonColor("Moon Color", Color) = (1, 1, 1, 1)
        _MoonRadius ("Moon Radius", Range(0,1)) = 0.05
        _MoonExposure ("Moon Exposure", Range(-16, 16)) = 0
        _MoonDisplayLimit ("Moon DisplayLimit", Range(-1, 0)) = -0.6

        [Header(Sky Settings)]
        [NoScaleOffset] _SkyGradient ("Sky Gradient", 2D) = "white" {}
        [NoScaleOffset] _HorizonHazeGradient ("HorizonHaze Gradient", 2D) = "white" {}
        [NoScaleOffset] _SunBloomGradient ("SunBloom Gradient", 2D) = "white" {}

        [Header(Cloud Settings)]
		[NoScaleOffset] _CloudTexture("CloudTexture", 2D) = "black" {}
		_CloudCutoff("Cloud Cutoff",  Range(0, 1)) = 0.08
        _Fuzziness("Cloud Fuzziness",  Range(-5, 5)) = 0.04
		_CloudSpeed("Cloud Move Speed",  Range(-10, 10)) = 0.3
		_CloudScale("Cloud Scale",  Range(-10, 10)) = 0.3
        _DistortTexture("Distort Texture", 2D) = "black" {}
		_DistortScale("Distort Noise Scale",  Range(0, 1)) = 0.06
		_DistortionSpeed("Distortion Speed",  Range(-1, 1)) = 0.1
		_CloudNoise("Cloud Noise", 2D) = "black" {}
		_CloudNoiseScale("Cloud Noise Scale",  Range(0, 1)) = 0.2
		_CloudNoiseSpeed("Cloud Noise Speed",  Range(-1, 1)) = 0.1
		_CloudColorDayEdge("Cloud Color Day Edge", Color) = (0.0,0.2,0.1,1)
		_CloudColorDayMain("Cloud Color Day Main", Color) = (0.6,0.7,0.6,1)
        _CloudColorNightEdge("Cloud Color Night Edge", Color) = (0.0,0.2,0.1,1)
		_CloudColorNightMain("Cloud Color Night Main", Color) = (0.6,0.7,0.6,1)

        [Header(Star Settings)]
        [NoScaleOffset] _StarsTexture ("Stars Texture", 2D) = "black" {}
        _StarsCutoff("Stars Cutoff",  Range(0, 1)) = 0.1
        _StarsSpeed("Stars Move Speed",  Range(-10, 10)) = 0.3
		_StarScale("Star Scale",  Range(-10, 10)) = 0.3

    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        //Tags { "Queue"= "Geometry" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off 
        ZWrite Off

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

            TEXTURE2D(_StarsTexture);      
            SAMPLER(sampler_StarsTexture);

            TEXTURE2D(_CloudTexture);      
            SAMPLER(sampler_CloudTexture);

            TEXTURE2D(_DistortTexture);      
            SAMPLER(sampler_DistortTexture);

            TEXTURE2D(_CloudNoise);      
            SAMPLER(sampler_CloudNoise);

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
            float _CloudCutoff;
            float _Fuzziness;
		    float _CloudSpeed;
		    float _CloudScale;
            float _DistortScale;
            float _DistortionSpeed;
            float _CloudNoiseScale;
            float _CloudNoiseSpeed;
            float _StarsCutoff;
            float _StarsSpeed;
		    float _StarScale;
            float4 _CloudColorDayEdge;
		    float4 _CloudColorDayMain;
            float4 _CloudColorNightEdge;
		    float4 _CloudColorNightMain;
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
                float2 heightUV =  float2(height, 0.5);
                float3 skyColor = SAMPLE_TEXTURE2D(_SkyGradient,sampler_SkyGradient, heightUV).rgb;

                float viewMask = pow(saturate(1.0 - dir.y), 4);
                float3 horizonColor = viewMask * SAMPLE_TEXTURE2D(_HorizonHazeGradient,sampler_HorizonHazeGradient,heightUV).rgb;

                // Sun lighting
                float sunBloomMask = pow(saturate(sunViewDot), 4);
                float3 sunBloomColor = sunBloomMask * SAMPLE_TEXTURE2D(_SunBloomGradient, sampler_SunBloomGradient,heightUV).rgb;

                float2 skyUV = dir.xz / dir.y;
                // Cloud
                float baseNoise = SAMPLE_TEXTURE2D(_CloudTexture, sampler_CloudTexture, (skyUV + float2(_CloudSpeed, _CloudSpeed) * _Time.x) * _CloudScale).r;
                float distortnoise = SAMPLE_TEXTURE2D(_DistortTexture, sampler_DistortTexture, (skyUV + baseNoise - _DistortionSpeed) * _DistortScale).r;
				float noise2 = SAMPLE_TEXTURE2D(_CloudNoise, sampler_CloudNoise, (skyUV + distortnoise - _CloudNoiseSpeed) * _CloudNoiseScale).r;
				float finalNoise = saturate(distortnoise * noise2) * saturate(dir.y);
                float clouds = saturate(smoothstep(_CloudCutoff, _CloudCutoff + _Fuzziness, finalNoise));

                float3 cloudDayColor = lerp(_CloudColorDayEdge,  _CloudColorDayMain , clouds) * clouds;
                float3 cloudNightColor =  lerp(_CloudColorNightEdge,  _CloudColorNightMain , clouds) * clouds;
                float3 cloudColor = lerp(cloudNightColor, cloudDayColor,_SunDir.y);

                // Star
                float2 starUV = (skyUV + float2(_StarsSpeed, _StarsSpeed) * _Time.x) * _StarScale;
                float4 starTex = SAMPLE_TEXTURE2D(_StarsTexture, sampler_StarsTexture, starUV);
                float starMask = step(_StarsCutoff,  starTex.r) * saturate(-_SunDir.y);
                starMask = (1 - sunArea) * (1 - moonMask) * starMask;
                float3 starColor = starTex.rgb * starMask;

                starColor *= (1 - clouds);
                sunColor *= (1 - clouds);
                moonColor *= (1 - clouds);

                float3 skyCombineColor = skyColor + horizonColor + sunBloomColor;
                float3 col = skyCombineColor + sunColor + moonColor + cloudColor + starColor;
                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}
