Shader "Universal Render Pipeline/Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,1,0,1)
        _OutlineWidth ("Outline Width", Float) = 0.03
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode"="UniversalForward" }

            Cull Front
            ZWrite Off
            ZTest Less
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _OutlineColor;
            float _OutlineWidth;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                posWS += normalWS * _OutlineWidth;

                OUT.positionHCS = TransformWorldToHClip(posWS);
                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }

            ENDHLSL
        }
    }
}
