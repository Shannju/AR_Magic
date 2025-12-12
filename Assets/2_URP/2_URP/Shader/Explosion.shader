Shader "LightningStrikeEffect/URP/Explosion"
{
    Properties
    {
        _DepthFadeDistance("Fade Distance", float) = 1.0
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}

        [Header(Main)]
        _MainTex_Power("MainTex_Power", float) = 1.0
        _MainTex_Intensity("MainTex_Intensity", float) = 1.0
        
        [Header(Core)]
        _CoreColor_Opacity_Value("CoreColor_Opacity_Value", float) = 0.0
        _CoreColor_Opacity_Blur("CoreColor_Opacity_Blur", float) = 0.0

        [Header(Smoke)]
        [HDR] _SmokeColor("SmokeColor", color) = (1,1,1)
        [HDR] _EmissionColor("EmissionColor", color) = (1,1,1)
        [HDR] _CoreColor("CoreColor", color) = (1,1,1)
    
        [Header(Opacity)]
        _OpacityPower("OpacityPower", float) = 1.0
        _OpacityIntensity("OpacityIntensity", float) = 1.0

        [Header(Noise)]
        _Noise_0("Noise_0", 2D) = "black"{}
        _Noise_1("Noise_1", 2D) = "black"{}

        
 



        [Header(Emission)]
        _EmissionColor_Opacity_Value_0("EmissionColor_Opacity_Value_0", float) = 0.0
        _EmissionColor_Opacity_Value_1("EmissionColor_Opacity_Value_1", float) = 0.0
        _EmissionColor_Opacity_Blur("EmissionColor_Opacity_Blur", float) = 0.0



    }
        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
     

            HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

                CBUFFER_START(UnityPerMaterial)
                    float _MainTex_Power;
                    float _MainTex_Intensity;

                    float _CoreColor_Opacity_Value;
                    float _CoreColor_Opacity_Blur;

                    float3 _SmokeColor;
                    float3 _EmissionColor;
                    float3 _CoreColor;

                    float _OpacityPower;
                    float _OpacityIntensity;

                    float _EmissionColor_Opacity_Value_0;
                    float _EmissionColor_Opacity_Value_1;
                    float _EmissionColor_Opacity_Blur;

             
                    float4 _Noise_0_ST;
                    float4 _Noise_1_ST;
                    float4 _MainTex_ST;

                    float _DepthFadeDistance;
            CBUFFER_END
        ENDHLSL

        Pass
        {

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off


            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
         

            struct Attributes
            {
                float4 texcoord : TEXCOORD0;
                float customData : TEXCOORD1;
                float4 positionOS	: POSITION;
               
       
                float4 color : COLOR;
            };

            struct Varyings
            {

                float2 uv : TEXCOORD0;
                float2 uv_1 : TEXCOORD1;

                float4 screenPos : TEXCOORD2;
                float3 positionVS : TEXCOORD3;
                float customData : TEXCOORD4;
                real fogFactor : TEXCOORD5;
                float4 positionCS	: SV_POSITION;
                float4 color : COLOR;
            };

            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_Noise_0);
            SAMPLER(sampler_Noise_0);
            TEXTURE2D(_Noise_1);
            SAMPLER(sampler_Noise_1);
 




            Varyings vert(Attributes IN)
            {
                Varyings OUT;


                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                OUT.positionCS = positionInputs.positionCS;
                OUT.uv = TRANSFORM_TEX(IN.texcoord.xy, _MainTex);
                OUT.customData = IN.customData.r;
                OUT.uv_1 = IN.texcoord.zw;
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionVS = TransformWorldToView(positionWS);
      
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float opacity = saturate(pow(abs(col.a), _OpacityPower) * _OpacityIntensity);
                col.a = opacity;

    



                float noise_0_Opacity = SAMPLE_TEXTURE2D(_Noise_0, sampler_Noise_0, TRANSFORM_TEX(IN.uv_1, _Noise_0)).r;
                float noise_1_Opacity = SAMPLE_TEXTURE2D(_Noise_1, sampler_Noise_1, TRANSFORM_TEX(IN.uv_1, _Noise_1)).r;
                float emission_Opacity = lerp(_EmissionColor_Opacity_Value_0, _EmissionColor_Opacity_Value_1, IN.customData.r);
             
               
                emission_Opacity = smoothstep(emission_Opacity, emission_Opacity + _EmissionColor_Opacity_Blur, noise_0_Opacity);
          


                float coreColor_Opacity = smoothstep(_CoreColor_Opacity_Value, _CoreColor_Opacity_Value + _CoreColor_Opacity_Blur, emission_Opacity) * noise_1_Opacity;
                float3 smoke = saturate(pow(abs(col.rgb), _MainTex_Power) * _MainTex_Intensity) * _SmokeColor.rgb;
                float3 emission = smoke + _EmissionColor.rgb;
                float3 smoke_emission = lerp(smoke, emission, emission_Opacity);
                float3 smoke_emission_core = lerp(smoke_emission, smoke_emission + _CoreColor.rgb, coreColor_Opacity);
                col.rgb = smoke_emission_core;
                col.a *= IN.color.a;





                float fragmentEyeDepth = -IN.positionVS.z;
                float rawDepth = SampleSceneDepth(IN.screenPos.xy / IN.screenPos.w);
                float sceneEyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float depthFade = saturate((sceneEyeDepth - fragmentEyeDepth)/_DepthFadeDistance);

                col.a *= depthFade;
                col.rgb = MixFog(col.rgb, IN.fogFactor);





                return col;
            }
            ENDHLSL
        }
    }
}
