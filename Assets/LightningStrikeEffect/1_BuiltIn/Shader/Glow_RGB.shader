Shader "LightningStrikeEffect/BuiltIn/Glow_RGB"
{
    Properties
    {
        _DepthFadeDistance("Fade Distance", float) = 1.0
         [Header(Texture)]
        _MainTex("Texture", 2D) = "white" {}
        _LightIntensity("LightIntensity", float) = 1.0
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
            float _LightIntensity;




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
       

                col.rgb *= i.color.rgb;
                col.rgb *= _LightIntensity;
                col.rgb *= i.color.a;


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
