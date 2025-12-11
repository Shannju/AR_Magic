// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#if !defined(CUSTOMSTANDARDLIGHTING_INCLUDED)
#define CUSTOMSTANDARDLIGHTING_INCLUDED

#include "AutoLight.cginc"
#include "UnityPBSLighting.cginc"

float4 _BaseColor;
sampler2D _BaseMap;
float4 _BaseMap_ST;

sampler2D _RoughnessMap;
float _RoughnessIntensity;

sampler2D _MetallicMap;
float _MetallicIntensity;

sampler2D _EmissionMap;
float _EmissionIntensity;

sampler2D _BumpMap;
float _BumpScale;


struct appdata {
	float3 uv : TEXCOORD0;
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;

	float4 color : COLOR;
};

struct v2f {
	float4 vertex : SV_POSITION;
	float3 uv : TEXCOORD0;
	float3 normal : TEXCOORD1;
	float4 tangent : TEXCOORD2;
	float3 worldPos : TEXCOORD3;
	float4 color : COLOR;

	#if defined(VERTEXLIGHT_ON)
		float3 vertexLightColor : TEXCOORD4;
	#endif

	UNITY_FOG_COORDS(5)
};


float3 CreateBinormal (float3 normal, float3 tangent, float binormalSign) {
	return cross(normal, tangent.xyz) *
		(binormalSign * unity_WorldTransformParams.w);
}
v2f vert(appdata v) 
{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.worldPos = mul(unity_ObjectToWorld, v.vertex);
	o.normal = UnityObjectToWorldNormal(v.normal);
	o.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
	o.uv = float3(TRANSFORM_TEX(v.uv, _BaseMap), v.uv.z);
	o.color = v.color;

	#if defined(VERTEXLIGHT_ON)
		o.vertexLightColor = Shade4PointLights(
			unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
			unity_LightColor[0].rgb, unity_LightColor[1].rgb,
			unity_LightColor[2].rgb, unity_LightColor[3].rgb,
			unity_4LightAtten0, o.worldPos, o.normal
		);
	#endif
	UNITY_TRANSFER_FOG(o, o.vertex);
	return o;
}
UnityLight CreateLight (v2f o) {
	UnityLight light;

	#if defined(POINT) || defined(POINT_COOKIE) || defined(SPOT)
		light.dir = normalize(_WorldSpaceLightPos0.xyz - o.worldPos);
	#else
		light.dir = _WorldSpaceLightPos0.xyz;
	#endif
	
	UNITY_LIGHT_ATTENUATION(attenuation, 0, o.worldPos);
	light.color = _LightColor0.rgb * attenuation;
	light.ndotl = DotClamped(o.normal, light.dir);
	return light;
}
UnityIndirect CreateIndirectLight (v2f o) {
	UnityIndirect indirectLight;
	indirectLight.diffuse = 0;
	indirectLight.specular = 0;

	#if defined(VERTEXLIGHT_ON)
		indirectLight.diffuse = o.vertexLightColor;
	#endif

	#if defined(FORWARD_BASE_PASS)
		indirectLight.diffuse += max(0, ShadeSH9(float4(o.normal, 1)));
	#endif

	return indirectLight;
}



fixed4 frag(v2f i) : SV_Target
{

	//Normal calculation
	float3 normal = UnpackScaleNormal(tex2D(_BumpMap, i.uv.xy), _BumpScale);

	float3 binormal = CreateBinormal(i.normal, i.tangent.xyz, i.tangent.w);
	normal =  normalize(
		normal.x * i.tangent +
		normal.y * binormal +
		normal.z * i.normal
	);


	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
	float4 col = tex2D(_BaseMap, i.uv.xy) * _BaseColor * i.color;
	float alpha = col.a;
	float3 albedo = col.rgb;

	//smoothness
	float roughness = tex2D(_RoughnessMap, i.uv.xy).r * _RoughnessIntensity;
	float smoothness = 1.0 - saturate(roughness);



	//metallic
	float metallic = tex2D(_MetallicMap, i.uv.xy).r * _MetallicIntensity;
	metallic = saturate(metallic);


	//emission
	float3 emission = tex2D(_EmissionMap, i.uv.xy).rgb * _EmissionIntensity * i.uv.z;







	float3 specularTint;
	float oneMinusReflectivity;
	albedo = DiffuseAndSpecularFromMetallic(
		albedo, metallic, specularTint, oneMinusReflectivity
	);


	col = UNITY_BRDF_PBS(
		albedo, specularTint,
		oneMinusReflectivity, smoothness,
		normal, viewDir,
		CreateLight(i), CreateIndirectLight(i)
	);

	col.a = alpha;
	col.rgb += emission;

	UNITY_APPLY_FOG(i.fogCoord, col);
	return col;
}

#endif