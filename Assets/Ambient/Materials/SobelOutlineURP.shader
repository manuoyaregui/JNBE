Shader "Hidden/SobelOutlineURP"
{
    Properties
    {
        _EdgeColor ("Edge Color", Color) = (1,1,1,1)
        _EdgeThickness ("Edge Thickness", Range(0.5, 3)) = 1
        _DepthSensitivity ("Depth Sensitivity", Range(0.1, 5)) = 1
        _NormalSensitivity ("Normal Sensitivity", Range(0.1, 5)) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            
            TEXTURE2D(_CameraNormalsTexture);
            SAMPLER(sampler_CameraNormalsTexture);
            
            float4 _EdgeColor;
            float _EdgeThickness;
            float _DepthSensitivity;
            float _NormalSensitivity;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float CompareDepth(float2 uv1, float2 uv2)
            {
                float d1 = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv1).r;
                float d2 = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv2).r;
                return abs(d1 - d2);
            }

            float CompareNormal(float2 uv1, float2 uv2)
            {
                float3 n1 = UnpackNormal(SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv1));
                float3 n2 = UnpackNormal(SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv2));
                return 1 - dot(n1, n2);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float2 step = _EdgeThickness / _ScreenParams.xy;

                float edgeDepth = 0;
                float edgeNormal = 0;

                // vecinos
                edgeDepth += CompareDepth(uv, uv + float2(step.x, 0));
                edgeDepth += CompareDepth(uv, uv - float2(step.x, 0));
                edgeDepth += CompareDepth(uv, uv + float2(0, step.y));
                edgeDepth += CompareDepth(uv, uv - float2(0, step.y));

                edgeNormal += CompareNormal(uv, uv + float2(step.x, 0));
                edgeNormal += CompareNormal(uv, uv - float2(step.x, 0));
                edgeNormal += CompareNormal(uv, uv + float2(0, step.y));
                edgeNormal += CompareNormal(uv, uv - float2(0, step.y));

                float edge = saturate(edgeDepth * _DepthSensitivity + edgeNormal * _NormalSensitivity);

                return half4(_EdgeColor.rgb * edge, edge);
            }
            ENDHLSL
        }
    }
}
