Shader "LightningStrikeEffect/URP/Lit_Transparent_EmissionControlled"
{
	Properties{
		[Header(BaseColor)]
		[NoScaleOffset] _BaseMap("BaseMap", 2D) = "white" {}
		_BaseMapScaleOffset("BaseMapScaleOffset", vector) = (1,1,0,0)
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
		[Toggle(_NORMALMAP_ON)] _NORMALMAP_ON("NORMALMAP_ON", float) = 0
		[NoScaleOffset] _BumpMap("NormalMap", 2D) = "bump" {}
		_BumpScale("BumpScale", float) = 1.0


		[HideInInspector] _Cutoff("Alpha Cutoff", Float) = 0.0
		[HideInInspector] _Cull("__cull", Float) = 1.0
	}


		SubShader{


			HLSLINCLUDE
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				CBUFFER_START(UnityPerMaterial)

					float4 _BaseMap_ST;
					float4 _BaseColor;

					float _RoughnessIntensity;
					float _MetallicIntensity;

					float _EmissionIntensity;



					float _Cutoff;
					float _BumpScale;
				CBUFFER_END
			ENDHLSL


			Tags
			{
				"RenderType" = "Transparent"
				"Queue" = "Transparent"
				"RenderPipeline" = "UniversalPipeline"
				"UniversalMaterialType" = "Lit"
				"IgnoreProjector" = "True"
			}

			Pass 
			{
				Name "ForwardLit"
				Tags
				{
					"LightMode" = "UniversalForward"
			
				}
				Blend SrcAlpha OneMinusSrcAlpha
				ZWrite Off


				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag


				#pragma shader_feature_local _ _NORMALMAP_ON
				#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
				#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
				#pragma multi_compile _ _SHADOWS_SOFT
				#pragma multi_compile_fog
			

				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

				struct Attributes {
					float4 positionOS	: POSITION;
					float3 uv 		: TEXCOORD0;
	
					float2 staticLightmapUV   : TEXCOORD2;
					float4 color		: COLOR;
					float4 normalOS		: NORMAL;

					#ifdef _NORMALMAP_ON
						float4 tangentOS    : TANGENT;
					#endif

				};

				struct Varyings {
					float4 positionCS	: SV_POSITION;
					float3 uv		: TEXCOORD0;
			
					float4 color		: COLOR;

					float3 normalWS		: NORMAL;
					float3 positionWS	: TEXCOORD2;



					real fogFactor : TEXCOORD3;
					DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 4);

					#ifdef _ADDITIONAL_LIGHTS_VERTEX
						float3 vertexLighting : TEXCOORD5;
					#endif

					#ifdef _NORMALMAP_ON
						half4 tangentWS	: TEXCOORD6;
					#endif
				};



				half3 ApplyHue(float3 aColor, float hue)
				{
					float angle = radians(hue);
					float3 k = float3(0.57735, 0.57735, 0.57735);
					float cosAngle = cos(angle);
					//Rodrigues' rotation formula
					return aColor * cosAngle + cross(k, aColor) * sin(angle) + k * dot(k, aColor) * (1 - cosAngle);
				}
				half3 ApplySaturation(float3 inputCol, float saturation)
				{
					float3 intensity = dot(inputCol.rgb, float3(0.299, 0.587, 0.114));
					return lerp(intensity, inputCol.rgb, saturation);
				}
				half4 ApplyContrast(float4 color, float contrast) {
					return saturate(lerp(float4(0.5, 0.5, 0.5, 0.5), color, contrast));
				}
				half3 SampleNormal(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), half scale = half(1.0))
				{
					half4 n = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
					return UnpackNormalScale(n, scale);
				}



				TEXTURE2D(_BaseMap);
				SAMPLER(sampler_BaseMap);

				TEXTURE2D(_BumpMap);
				SAMPLER(sampler_BumpMap);

				TEXTURE2D(_RoughnessMap);
				SAMPLER(sampler_RoughnessMap);

				TEXTURE2D(_MetallicMap);
				SAMPLER(sampler_MetallicMap);

				TEXTURE2D(_EmissionMap);
				SAMPLER(sampler_EmissionMap);



				Varyings vert(Attributes IN) {
					Varyings OUT;

					VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
					OUT.positionCS = positionInputs.positionCS;
					OUT.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);


					OUT.uv = float3(TRANSFORM_TEX(IN.uv.xy, _BaseMap), IN.uv.z);
					OUT.color = IN.color;

					OUT.positionWS = positionInputs.positionWS;
					#ifdef _NORMALMAP_ON
						VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS.xyz, IN.tangentOS);
					#else
						VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS.xyz);
					#endif

					OUT.normalWS = normalInputs.normalWS;

					//normalmap
					#ifdef _NORMALMAP_ON
						real sign = IN.tangentOS.w * GetOddNegativeScale();
						half4 tangentWS = half4(normalInputs.tangentWS.xyz, sign);
						OUT.tangentWS = tangentWS;
					#endif


					OUT.vertexSH = SampleSH(OUT.normalWS);
					#ifdef _ADDITIONAL_LIGHTS_VERTEX
						OUT.vertexLighting = VertexLighting(OUT.positionWS, OUT.normalWS);
					#endif


					return OUT;
				}

				half4 frag(Varyings IN) : SV_Target{





				
					half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv.xy);
					half roughness = SAMPLE_TEXTURE2D(_RoughnessMap, sampler_RoughnessMap, IN.uv.xy).r * _RoughnessIntensity;
					half metallic = SAMPLE_TEXTURE2D(_MetallicMap, sampler_MetallicMap, IN.uv.xy).r * _MetallicIntensity;
					half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv.xy).rgb * _EmissionIntensity * IN.uv.z;//customData
					half4 color = baseMap * _BaseColor * IN.color;









					//NormalMap
					#ifdef _NORMALMAP_ON
						half3 normalTS = SampleNormal(IN.uv.xy, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
						float sgn = IN.tangentWS.w;      // should be either +1 or -1
						float3 bitangent = sgn * cross(IN.normalWS.xyz, IN.tangentWS.xyz);
						half3x3 tangentToWorld = half3x3(IN.tangentWS.xyz, bitangent.xyz, IN.normalWS.xyz);
						IN.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
					#endif


					SurfaceData surfaceData;
					surfaceData.albedo = color.rgb;
					surfaceData.specular = half3(0,0,0);
					surfaceData.metallic = metallic;
					surfaceData.smoothness = 1.0 - roughness;
					surfaceData.normalTS = half3(0, 0, 1);
					surfaceData.emission = half3(0, 0, 0);
					surfaceData.occlusion = 1.0;
					surfaceData.alpha = color.a;
					surfaceData.clearCoatMask = 0;
					surfaceData.clearCoatSmoothness = 1;


					BRDFData brdfData;
					InitializeBRDFData(surfaceData, brdfData);
					BRDFData noClearCoat = (BRDFData)0;

					half4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS.xyz);
					Light mainLight = GetMainLight(shadowCoord);
					IN.normalWS = NormalizeNormalPerPixel(IN.normalWS);
					half3 viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);



					half3 bakedGI = half3(0, 0, 0);
					bakedGI = IN.vertexSH;




					half3 GI_Base = GlobalIllumination(brdfData, bakedGI, 1, IN.normalWS, viewDirectionWS);
					half3 giColor = GI_Base;
					half3 lightingColor = LightingPhysicallyBased(brdfData, noClearCoat, mainLight, IN.normalWS, viewDirectionWS, 0.0, false);


					#if defined(_ADDITIONAL_LIGHTS)
					uint pixelLightCount = GetAdditionalLightsCount();
					LIGHT_LOOP_BEGIN(pixelLightCount)
						Light light = GetAdditionalLight(lightIndex, IN.positionWS);
						#ifdef _LIGHT_LAYERS
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						#endif
						{
							lightingColor += LightingPhysicallyBased(brdfData, noClearCoat, light, IN.normalWS, viewDirectionWS, 0.0, false);

						}
					LIGHT_LOOP_END

					#endif

					color.rgb = lightingColor.rgb;
					#ifdef _ADDITIONAL_LIGHTS_VERTEX
					color.rgb += surfaceData.albedo * IN.vertexLighting;
					#endif




					color.rgb += giColor;



					//Emission
					color.rgb = color.rgb + emission;



					//Fog
					color.rgb = MixFog(color.rgb, IN.fogFactor);

					//cutouf
					clip(color.a - _Cutoff);
					return color;


					return color;
						}
						ENDHLSL
					}

					Pass {

					Name "ShadowCaster"
					Tags { "LightMode" = "ShadowCaster" }

					ZWrite On
					ZTest LEqual

					HLSLPROGRAM
					#pragma vertex ShadowPassVertex
					#pragma fragment ShadowPassFragment


					#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
					#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
					#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"


					half3 _LightDirection;
					half3 _LightPosition;


					TEXTURE2D(_MainTex);
					SAMPLER(sampler_MainTex);


					struct Attributes
					{
						half4 positionOS   : POSITION;
						half3 normalOS     : NORMAL;
						half2 texcoord     : TEXCOORD0;

					};

					struct Varyings
					{
						half2 uv           : TEXCOORD0;
						half4 positionCS   : SV_POSITION;
					};

					half4 GetShadowPositionHClip(Attributes input)
					{
						half3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
						half3 normalWS = TransformObjectToWorldNormal(input.normalOS);

					#if _CASTING_PUNCTUAL_LIGHT_SHADOW
						half3 lightDirectionWS = normalize(_LightPosition - positionWS);
					#else
						half3 lightDirectionWS = _LightDirection;
					#endif

						half4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

					#if UNITY_REVERSED_Z
						positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
					#else
						positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
					#endif

						return positionCS;
					}

					Varyings ShadowPassVertex(Attributes input)
					{
						Varyings output;
						UNITY_SETUP_INSTANCE_ID(input);

						output.uv = input.texcoord;
						output.positionCS = GetShadowPositionHClip(input);
						return output;
					}

					half4 ShadowPassFragment(Varyings input) : SV_TARGET
					{
						half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * _BaseColor.a;
						clip(alpha - _Cutoff);
						return 0;
					}
					ENDHLSL

					}
					Pass {
						Name "DepthOnly"
						Tags { "LightMode" = "DepthOnly" }


						ColorMask 0
						ZWrite On
						ZTest LEqual


						HLSLPROGRAM
						#pragma vertex DepthOnlyVertex
						#pragma fragment DepthOnlyFragment
						#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
						#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
						#include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
						ENDHLSL
					}


		}
}