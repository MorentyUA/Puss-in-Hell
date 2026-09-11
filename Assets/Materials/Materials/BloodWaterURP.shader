// BloodWaterURP.shader
Shader "Custom/BloodWaterURP"
{
    Properties
    {
        [Header(Colors)]
        _BaseColor ("Blood Color", Color) = (0.35, 0.0, 0.0, 0.92)
        _DeepColor ("Deep Color", Color) = (0.15, 0.0, 0.0, 1)
        _FresnelColor ("Edge Fresnel Color", Color) = (0.5, 0.05, 0.05, 1)

        [Header(Waves)]
        _WaveSpeed ("Wave Speed", Range(0, 2)) = 0.4
        _WaveStrength ("Wave Height", Range(0, 0.3)) = 0.03
        _WaveScale ("Wave Scale", Range(0.1, 10)) = 2

        [Header(Surface)]
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 0.6
        _NormalScale ("Normal Tiling", Range(0.1, 5)) = 1

        [Header(Look)]
        _Smoothness ("Smoothness", Range(0, 1)) = 0.85
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 3

        [Header(Depth Fade)]
        _DepthFadeDistance ("Edge Fade Distance", Range(0, 2)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "BloodWater"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _DeepColor;
                float4 _FresnelColor;
                float _WaveSpeed;
                float _WaveStrength;
                float _WaveScale;
                float _NormalStrength;
                float _NormalScale;
                float _Smoothness;
                float _FresnelPower;
                float _DepthFadeDistance;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
                float3 tangentWS : TEXCOORD5;
                float3 bitangentWS : TEXCOORD6;
                float fogFactor : TEXCOORD7;
            };

            // Волны Герстнера (более реалистичные)
            float3 GerstnerWave(float2 pos, float time, float2 dir, float steepness, float wavelength)
            {
                float k = 2.0 * PI / wavelength;
                float c = sqrt(9.8 / k);
                float2 d = normalize(dir);
                float f = k * (dot(d, pos) - c * time);
                float a = steepness / k;

                return float3(
                    d.x * a * cos(f),
                    a * sin(f),
                    d.y * a * cos(f)
                );
            }

            float GetWaves(float2 pos, float time)
            {
                float wave = 0;
                float t = time * _WaveSpeed;
                float2 p = pos * _WaveScale;

                // Несколько слоёв волн
                wave += sin(p.x * 1.0 + t * 1.0) * 0.5;
                wave += sin(p.y * 1.2 + t * 0.8) * 0.4;
                wave += sin((p.x + p.y) * 0.7 + t * 1.2) * 0.3;
                wave += sin((p.x - p.y) * 0.9 + t * 0.9) * 0.2;

                // Мелкая рябь
                wave += sin(p.x * 3.0 + p.y * 2.5 + t * 2.0) * 0.1;

                return wave * _WaveStrength;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                // Волны на вертексах
                float wave = GetWaves(input.positionOS.xz, _Time.y);
                input.positionOS.y += wave;

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.uv = input.uv;
                output.normalWS = normInputs.normalWS;
                output.tangentWS = normInputs.tangentWS;
                output.bitangentWS = normInputs.bitangentWS;
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(posInputs.positionWS);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Анимированные UV для normal map
                float2 uv1 = input.uv * _NormalScale + _Time.y * _WaveSpeed * float2(0.03, 0.02);
                float2 uv2 = input.uv * _NormalScale * 1.4 - _Time.y * _WaveSpeed * float2(0.02, 0.04);
                float2 uv3 = input.uv * _NormalScale * 0.7 + _Time.y * _WaveSpeed * float2(-0.01, 0.03);

                // Три слоя нормалей для реалистичной ряби
                float3 n1 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv1));
                float3 n2 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv2));
                float3 n3 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv3));

                float3 normalTS = normalize(float3(
                    (n1.xy + n2.xy * 0.5 + n3.xy * 0.25) * _NormalStrength,
                    1.0
                ));

                // TBN матрица
                float3x3 TBN = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                float3 normalWS = normalize(mul(normalTS, TBN));

                // Fresnel
                float NdotV = saturate(dot(normalWS, input.viewDirWS));
                float fresnel = pow(1.0 - NdotV, _FresnelPower);

                // Depth fade (мягкие края у стенок раковины)
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float sceneDepth = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float surfaceDepth = input.screenPos.w;
                float depthDiff = saturate((sceneDepth - surfaceDepth) / _DepthFadeDistance);

                // Цвет
                half4 color = lerp(_DeepColor, _BaseColor, depthDiff);
                color = lerp(color, _FresnelColor, fresnel * 0.6);

                // Освещение
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normalWS, mainLight.direction));

                // Specular
                float3 halfDir = normalize(mainLight.direction + input.viewDirWS);
                float spec = pow(saturate(dot(normalWS, halfDir)), _Smoothness * 256.0);

                // Финальный цвет
                float3 finalColor = color.rgb * (0.6 + NdotL * 0.4);
                finalColor += spec * mainLight.color * 0.4;
                finalColor += fresnel * _FresnelColor.rgb * 0.2;

                // Fog
                finalColor = MixFog(finalColor, input.fogFactor);

                // Alpha с depth fade для мягких краёв
                float alpha = color.a * depthDiff;

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
