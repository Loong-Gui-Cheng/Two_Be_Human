Shader "CustomURP/Post-Material/ToonLighting"
{
    Properties
    {
        [MainColor] _MainColor ("Color", Color) = (0, 0, 0, 1)
        [HDR] _AmbientColor ("Ambient", Color) = (0, 1, 0, 1)
        [HDR] _SpecularColor ("Specular", Color) = (0.9, 0.9, 0.9, 1)
        [HDR] _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimAmount ("Rim Amount", Range(0, 1)) = 0.716
        _RimThreshold ("Rim Threshold", Range(0, 1)) = 0.1
        _Samples ("Sample", Float) = 1
        _Glossiness ("Glossiness", Float) = 32
    }
    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"= "Opaque"
            "LightMode" = "SRPDefaultUnlit"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_ST;
        half4 _MainColor;
        half4 _AmbientColor;
        half4 _SpecularColor;
        half4 _RimColor;
        float _RimAmount;
        float _RimThreshold;
        float _Samples;
        float _Glossiness;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            // Vertex Input
            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            // Fragment Input
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                VertexPositionInputs VPI = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs VNI = GetVertexNormalInputs(input.normalOS.xyz);

                output.positionCS = VPI.positionCS;
                output.color = input.color;
                output.normalWS = VNI.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.viewDir = GetWorldSpaceNormalizeViewDir(input.positionOS.xyz);
                return output;
            }

            half4 Fragment(Varyings input) : SV_TARGET
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // Calculate Diffuse
                float3 normal = normalize(input.normalWS);
                float NDotL = dot(_MainLightPosition, normal);
                float lightIntensity = smoothstep(0, 0.01, NDotL);
                float4 light = lightIntensity * _MainLightColor * _Samples;

                // Calculate Specular 
                float3 viewDir = normalize(input.viewDir);
                float3 halfVector = normalize(_MainLightPosition + viewDir);
                float NDotH = dot(normal, halfVector);
                float specularIntensity = pow(NDotH * lightIntensity, _Glossiness * _Glossiness);
                float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
                float specular = specularIntensitySmooth * _SpecularColor;

                // Rim Lighting
                float4 rimDot = 1 - dot(viewDir, normal);
                float rimIntensity = rimDot * pow(NDotL, _RimThreshold);
                rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);

                float4 rim = rimIntensity * _RimColor;


                return _MainColor * (_AmbientColor + light + specular + rim);
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Lit"
}
