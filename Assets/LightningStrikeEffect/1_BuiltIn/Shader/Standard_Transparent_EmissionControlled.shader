Shader "LightningStrikeEffect/BuiltIn/Standard_Transparent_EmissionControlled"
{
    Properties
    {
        [Header(BaseColor)]
        [NoScaleOffset] _BaseMap("BaseMap", 2D) = "white" {}
        _BaseColor("BaseColor", Color) = (0, 0.66, 0.73, 1)

        [Header(Roughness)]
        [NoScaleOffset] _RoughnessMap("RoughnessMap", 2D) = "white"{}
        _RoughnessIntensity("RoughnessIntensity", float) = 1.0

        [Header(Metallic)]
        [NoScaleOffset] _MetallicMap("MetallicMap", 2D) = "black"{}
        _MetallicIntensity("MetallicIntensity", float) = 1.0

        [Header(Emission)]
        [NoScaleOffset] _EmissionMap("EmissionMap", 2D) = "black"{}
        _EmissionIntensity("EmissionIntensity", float) = 1.0

        [Header(Normal)]
        [NoScaleOffset] _BumpMap("NormalMap", 2D) = "bump" {}
        _BumpScale("BumpScale", float) = 1.0



    }

	SubShader {


		Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZWrite Off


		Pass {
			Tags {
				"LightMode" = "ForwardBase"
			}

			CGPROGRAM

			#pragma target 3.0
			#pragma multi_compile_fog
			#pragma multi_compile _ VERTEXLIGHT_ON
			#pragma vertex vert
			#pragma fragment frag
			#define FORWARD_BASE_PASS
			#include "CustomStandardLighting.cginc"

			ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "ForwardAdd"
			}

			Blend One One
			ZWrite Off

			CGPROGRAM

			#pragma target 3.0
			#pragma multi_compile_fwdadd
			#pragma vertex vert
			#pragma fragment frag
			#include "CustomStandardLighting.cginc"

			ENDCG
		}
	}
}