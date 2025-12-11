Shader "LightningStrikeEffect/BuiltIn/Glow"
{
    Properties
    {
        _DepthFadeDistance("Fade Distance", float) = 1.0
        [Header(Color)]
        [HDR] _ColorBase("ColorBase", color) = (1,1,1,1)
        [HDR] _ColorBright("ColorBright", color) = (1,1,1,1)
        _LightIntensity("LightIntensity", float) = 1.0
        _ColorBlendValue("ColorBlendValue", float) = 0.0
        _ColorBlendBlur("ColorBlendBlur", float) = 0.0


        [Header(Texture)]
        _MainTex("Texture", 2D) = "white" {}
        _OpacityPower("OpacityPower", float) = 1.0
        _OpacityIntensity("OpacityIntensity", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
        Blend One One

        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 screenPos : TEXCOORD2;
 
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;


            float4 _ColorBase;
            float4 _ColorBright;
            float _ColorBlendValue, _ColorBlendBlur;
            float _LightIntensity;
            float _OpacityPower;
            float _OpacityIntensity;



            //Depth
            sampler2D _CameraDepthTexture;
            float _DepthFadeDistance;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.screenPos = ComputeScreenPos(o.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                float alpha = col.r;
                alpha = pow(abs(alpha), _OpacityPower) * _OpacityIntensity;
                float colorBlend = smoothstep(_ColorBlendValue, _ColorBlendValue + _ColorBlendBlur, col.r);
                col.rgb = lerp(_ColorBase.rgb, _ColorBright.rgb, colorBlend);


                col.rgb *= i.color.rgb;
                col.rgb *= _LightIntensity;
                col.rgb *= alpha * i.color.a;


                //DepthFade
                float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos);
                float depth = LinearEyeDepth(depthSample).r;
                float depthFade = saturate((depth - i.screenPos.w) / _DepthFadeDistance);
                col.rgb *= depthFade;
               

                return col;
            }
            ENDCG
        }
    }
}
