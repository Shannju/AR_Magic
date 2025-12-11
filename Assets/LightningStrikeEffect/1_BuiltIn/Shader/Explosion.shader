Shader "LightningStrikeEffect/BuiltIn/Explosion"
{
    Properties
    {
        _DepthFadeDistance("Fade Distance", float) = 1.0
        [NoScaleOffset] _MainTex("Texture", 2D) = "white" {}

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
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float customData : TEXCOORD1;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv_1 : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float customData : TEXCOORD3;
                UNITY_FOG_COORDS(4)
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _Noise_0;
            sampler2D _Noise_1;



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
            sampler2D _CameraDepthTexture;





            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.customData = v.customData.r;
                o.uv_1 = v.uv.zw;
                o.color = v.color;
                o.screenPos = ComputeScreenPos(o.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                float4 col = tex2D(_MainTex, i.uv);
                float opacity = saturate(pow(abs(col.a), _OpacityPower) * _OpacityIntensity);
                col.a = opacity;


                float noise_0_Opacity = tex2D(_Noise_0, TRANSFORM_TEX(i.uv_1, _Noise_0)).r;
                float noise_1_Opacity = tex2D(_Noise_1, TRANSFORM_TEX(i.uv_1, _Noise_1)).r;
                float emission_Opacity = lerp(_EmissionColor_Opacity_Value_0, _EmissionColor_Opacity_Value_1, i.customData.r);
                
                emission_Opacity = smoothstep(emission_Opacity, emission_Opacity + _EmissionColor_Opacity_Blur, noise_0_Opacity);

                float coreColor_Opacity = smoothstep(_CoreColor_Opacity_Value, _CoreColor_Opacity_Value + _CoreColor_Opacity_Blur, emission_Opacity) * noise_1_Opacity;
                float3 smoke = saturate(pow(abs(col.rgb), _MainTex_Power) * _MainTex_Intensity) * _SmokeColor.rgb;
                float3 emission = smoke + _EmissionColor.rgb;
                float3 smoke_emission = lerp(smoke, emission, emission_Opacity);
                float3 smoke_emission_core = lerp(smoke_emission, smoke_emission + _CoreColor.rgb, coreColor_Opacity);
                col.rgb = smoke_emission_core;
                col.a *= i.color.a;


                //DepthFade
                float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos);
                float depth = LinearEyeDepth(depthSample).r;
                float depthFade = saturate((depth - i.screenPos.w) / _DepthFadeDistance);
                col.a *= depthFade;

        
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
