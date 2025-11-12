Shader "CustomURP/Post-Processing/SobelOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineThickness ("Outline Thickness", Range(0, 1)) = 0.1
        _DepthMultiplier ("Depth Multiplier", Float) = 0.5
        _DepthBias ("Depth Bias", Float) = 0.5
        _NormalMultiplier ("Normal Multiplier", Float) = 0.5
        _NormalBias ("Normal Bias", Float) = 0.5
    }
    SubShader
    {
        Cull Off 
        ZWrite Off
        ZTest Always

        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"= "Opaque"
            "LightType" = "UniversalGBuffer"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        TEXTURE2D(_CameraDepthTexture);
        SAMPLER(sampler_CameraDepthTexture);

        TEXTURE2D(_GBuffer2);
        SAMPLER(sampler_GBuffer2);

        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_ST;
        float4 _CameraDepthTexture_ST;
        float4 _GBuffer2_ST;
        half4 _OutlineColor;
        float _OutlineThickness;
        float _DepthMultiplier;
        float _DepthBias;
        float _NormalMultiplier;
        float _NormalBias;
        CBUFFER_END

        float3 SobelDepth(float center, float left, float right, float up, float down)
        {
            return abs(left - center) +
            abs(right - center) + 
            abs(up - center)+
            abs(down - center);
        }

        float4 SobelNormal(float4 center, float4 left, float4 right, float4 up, float4 down)
        {
            return abs(left - center) +
            abs(right - center) + 
            abs(up - center)+
            abs(down - center);
        }

        float3 SobelSampleDepth(Texture2D t, SamplerState s, float2 uv, float3 offset)
        {
            float pixelCenter = LinearEyeDepth(SAMPLE_TEXTURE2D(t, s, uv).r, _ZBufferParams);
            float pixelLeft = LinearEyeDepth(SAMPLE_TEXTURE2D(t, s, uv - offset.xz).r, _ZBufferParams);
            float pixelRight = LinearEyeDepth(SAMPLE_TEXTURE2D(t, s, uv + offset.xz).r, _ZBufferParams);
            float pixelUp = LinearEyeDepth(SAMPLE_TEXTURE2D(t, s, uv + offset.zy).r, _ZBufferParams);
            float pixelDown = LinearEyeDepth(SAMPLE_TEXTURE2D(t, s, uv - offset.zy).r, _ZBufferParams);
            return SobelDepth(pixelCenter, pixelLeft, pixelRight, pixelUp, pixelDown);
        }

        float4 SobelSampleNormal(Texture2D t, SamplerState s, float2 uv, float3 offset)
        {
            float4 pixelCenter = SAMPLE_TEXTURE2D(t, s, uv);
            float4 pixelLeft = SAMPLE_TEXTURE2D(t, s, uv - offset.xz);
            float4 pixelRight = SAMPLE_TEXTURE2D(t, s, uv + offset.xz);
            float4 pixelUp = SAMPLE_TEXTURE2D(t, s, uv + offset.zy);
            float4 pixelDown = SAMPLE_TEXTURE2D(t, s, uv - offset.zy);
            return SobelNormal(pixelCenter, pixelLeft, pixelRight, pixelUp, pixelDown);
        }
        ENDHLSL

        Pass
        {
            Name "GMALDING"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            // Vertex Input
            struct Attributes
            {
                uint vertexID : SV_VertexID;
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Fragment Input
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
                // float3 viewDir : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID 
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs VPI = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs VNI = GetVertexNormalInputs(input.normalOS.xyz);

                output.positionCS = VPI.positionCS;
                output.color = input.color;
                output.normalWS = VNI.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                // output.viewDir = GetWorldSpaceNormalizeViewDir(input.positionOS.xyz);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Sample Scene and Depth Buffer
                half3 offset = half3((1.0 / _ScreenParams.x), (1.0 / _ScreenParams.y), 0.0) * _OutlineThickness;

                float3 sceneColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).rgb;
                half sobelDepth = SobelSampleDepth(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv.xy, offset).r;
                sobelDepth = pow(abs(saturate(sobelDepth) * _DepthMultiplier) , _DepthBias);

                float3 sobelNormalVec = SobelSampleNormal(_GBuffer2, sampler_GBuffer2, input.uv.xy, offset).rgb;
                float sobelNormal = sobelNormalVec.x + sobelNormalVec.y + sobelNormalVec.z;
                sobelNormal = pow(sobelNormal * _NormalMultiplier, _NormalBias);

                // Modulate the outline color based on transparency
                float3 outlineColor = lerp(sceneColor, _OutlineColor.rgb, _OutlineColor.a);

                // Calculate final scene color
                float sobelOutline = saturate(max(sobelDepth, sobelNormal));
                half3 color = lerp(sceneColor, outlineColor, sobelOutline);
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
