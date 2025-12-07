Shader "Lineage/URP/ice"
{
    Properties
    {
        [Header(Texture)]
        [Enum(Off,0,On,1)]_Zwrite("Zwrite",int)=0

        _MainTex("RGB(diffuse)),A:(OA))", 2D) = "white" {}
        [Normal]_Normal("normal",2D)="bump"{}
        _SpecTex("RGB(SpecTex),A(SpecTex_pow)",2D)="white"{}
        _EmiTex("RGB:emisstion",2D)="black"{}
        _Cubmap("Cubmap",Cube)="_skybox"{}

        [Header(Diffuse)]
        [HDR]_MainCol("diffuseColor",color) = (1,1,1,1)
        _EnvDiffInt("EnvDiffInt",range(0,1))=0.2
        _EnvUpCol("topColor",color)=(1,1,1,1)
        _EnvSideCol("SideColor",color)=(0.5,0.5,0.5,1)
        _EnvDownCol("DownColor",color)=(1,1,1,1)

        [Header(specular)]
        [PowerSlider(0.5)]_SpecPow("SpecPow",range(1,200))=10
        _EnvSpecInt("EnvSpecInt",range(0,5))=1
        _FresnelPow("FresnelPow",range(0,10))=1
        _CubemapMip("CubemapMip",range(1,7))=0
        [Header(Emission)]
        [HDR] _EmiCol("emission",color)=(1,1,1,1)
        _EmitInt("emissionIntensity",Range(1,10))=1
        _mask("mask",2D) = "white" {}
        // // _dissolve("溶解贴图",2D) = "white" {}
        // // _CutOffIntensity("溶解强度",range(0,1))=0
        // _CufOffSoft("软硬",range(0,0.5))=0

        _NormalScale("normalScale",range(0,1.5))=1
        _Light("LightPosition",vector)=(0,0,0,0)
        _distor("distorIntensity ",range(0,1))=0




    }

    SubShader
    {
        Tags { "Queue"="Transparent"  "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        // 
        Blend SrcAlpha OneMinusSrcAlpha


        Pass
        {
            // Tags{"LightMode"="ShadowCaster"}
            ZWrite [_Zwrite]
            //    ZTest LEqual
            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
                float4 normal :NORMAL;
                float4 tangent:TANGENT;
            };

            struct Varyings
            {
                float4 positionCS       : SV_POSITION;
                float4 uv           : TEXCOORD0;
                float4 posWS : TEXCOORD1;//世界空间顶点坐标
                float4 nDirWS : TEXCOORD2;//法线
                float3 tDirWS : TEXCOORD3;//切线
                float3 bDirWS : TEXCOORD4;//负切线
                float4 uv2:TEXCOORD5;
                float2 normaluv:TEXCOORD6;


            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST,_dissolve_ST,_Normal_ST,_EmiTex_ST;
                half4 _MainCol,_EnvUpCol,_EnvSideCol,_EnvDownCol,_EmiCol;
                half _EnvDiffInt,_CubemapMip;
                half4 _Light;
                float _SpecPow,_EnvSpecInt,_FresnelPow,_EmitInt,_CutOffIntensity,_distor,_NormalScale;


            CBUFFER_END
            TEXTURE2D (_MainTex);SAMPLER(sampler_MainTex);
            TEXTURE2D (_Normal);SAMPLER(sampler_Normal);
            TEXTURE2D (_SpecTex);SAMPLER(sampler_SpecTex);
            TEXTURE2D (_mask);SAMPLER(sampler_mask);
            TEXTURE2D (_dissolve);SAMPLER(sampler_dissolve);
            TEXTURE2D (_CameraOpaqueTexture);    SAMPLER(sampler_CameraOpaqueTexture); //


            

            
            TEXTURE2D (_EmiTex);SAMPLER(sampler_EmiTex);
            TEXTURECUBE (_Cubmap);SAMPLER(sampler_Cubmap);


            



            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                o.uv2.xy=v.uv;
                o.uv2.zw= v.uv;


                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.zw=TRANSFORM_TEX(v.uv, _EmiTex);
                o.normaluv=TRANSFORM_TEX(v.uv, _Normal);
                o.posWS.xyz=TransformObjectToWorld(v.positionOS.xyz);//位置OS>WS
                o.nDirWS.xyz=TransformObjectToWorldNormal(v.normal.xyz);
                o.tDirWS.xyz = TransformObjectToWorldDir(v.tangent.xyz);//本地切线>世界
                half sign = v.tangent.w * GetOddNegativeScale();
                o.bDirWS.xyz =normalize (cross(o.nDirWS.xyz, o.tDirWS) * sign);



                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                //采样扭曲
                i.uv2.xy+=_Time.y*0.01;
                half distor = SAMPLE_TEXTURE2D(_SpecTex, sampler_SpecTex, i.uv2.xy).r; 
                half2 distorintensity=lerp(i.uv.xy,i.uv.xy*distor.r,_distor);
                //向量准备
                //法线采样解码  TBN矩阵将切线空间法线转世界空间
                //  UnpackNormalScale   区别于 UnpackNormal    多了个参数调节发现强度
                half3 normalMap = UnpackNormalScale(SAMPLE_TEXTURE2D(_Normal, sampler_Normal, i.normaluv),_NormalScale);
                half3 normalWS = mul(normalMap,half3x3(i.tDirWS.xyz, i.bDirWS.xyz, i.nDirWS.xyz));
                
                float2 ScreenUV=i.positionCS.xy/_ScreenParams.xy;

                float3 VdirWS=normalize(_WorldSpaceCameraPos.xyz - i.posWS.xyz);
                float3 VRdirWS=reflect(-VdirWS,normalWS);//反射

                //获取光源
                Light mainLight = GetMainLight();
                // half3 lDirWS=normalize(mainLight.direction);//灯光位置
                half3 lDirWS= _Light.xyz;
                half3 lrDirWS=reflect(-lDirWS,normalWS); //根据入射光方向向量 I ，和顶点法向量 N ，计算反射光方向

                //准备点击
                float NdotL=dot(normalWS,lDirWS);//漫反射
                float vDotr=dot(VdirWS,lrDirWS);//phong 高光
                float vDotN=dot(VdirWS,normalWS);//菲尼尔


                //纹理采样
                


                half4 mask = SAMPLE_TEXTURE2D(_mask,sampler_mask, i.uv.xy);

                half4 var_MainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, distorintensity);
                half4 var_SpecTex = SAMPLE_TEXTURE2D(_SpecTex, sampler_SpecTex, i.uv.xy);
                half2 GrafTexRUV=ScreenUV+ScreenUV*normalWS.xy*_Light.w;
                half4 GrafTexR=SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,GrafTexRUV);

                half4 A;
                // A= lerp(GrafTexR,var_MainTex,mask*_Light.w);
                // return GrafTexR*var_SpecTex;


                half4 var_EmiTex = SAMPLE_TEXTURE2D(_EmiTex, sampler_EmiTex, distorintensity);
                // return var_EmiTex.x;
                half3 var_Cubmap = SAMPLE_TEXTURECUBE_LOD(_Cubmap, sampler_Cubmap, VRdirWS,lerp(_CubemapMip,0,var_SpecTex.a)).rgb;//lerp(_CubemapMip,0,var_SpecTex.a

                //光照模型(直接光照)
                float3 baseCol=var_MainTex.xyz*_MainCol.xyz;
                float3 speCol=var_SpecTex.rgb;
                float lambert=max(00.2,NdotL);
                float specPow=lerp(1,_SpecPow,var_SpecTex.a);

                // specPow*=_EmiCol;

                var_Cubmap*=var_SpecTex.a;


                //高光
                float phong=pow(max(0,vDotr),specPow);
                float3 dirLighting=((baseCol*lambert+phong*speCol)*mainLight.color);//  *****mainLight.color灯光颜色亮度*****

                //环境光
                float upMask=max(0,normalWS.g);
                float downMask=max(0,-normalWS.g);
                float sideMask=1-upMask-downMask;

                float3 encCol=_EnvUpCol.rgb*upMask+_EnvSideCol.rgb*sideMask+_EnvDownCol.rgb*downMask;

                float3 envDiff=baseCol*encCol*_EnvDiffInt;
                //  return half4(envDiff,1);


                //环境镜面反射
                float frsnel= pow(max(0,1-vDotN),_FresnelPow);
                float3 envSpec=var_Cubmap*frsnel*_EnvSpecInt;//  地裂不需要
                // float3 envSpec=0;



                //漫反射 镜面反射混合     
                float occlusion=var_MainTex.a;//环境遮挡
                
                float3 envLighting=(envDiff+envSpec)*occlusion;
                //自发光
                float emisSinTime=(sin((_Time.z))+1.5);
                float3 emission=var_EmiTex.rgb*_EmitInt*_EmiCol.rgb*emisSinTime*(envSpec);

                //溶解    -0.2为了让面板0 到1 更好看
                //    float  alpha2=smoothstep((_CutOffIntensity-0.2),_CutOffIntensity-0.2+_CufOffSoft,cutoffTex.r);


                //最终混合 
                float3 finaRGB=dirLighting+envLighting+emission*1;
                half4 c=1; 
                c.rgb=finaRGB+frsnel;
                // return saturate(max(0.4,frsnel*var_SpecTex.a));
                half GrabLerp=saturate(max(0.6,frsnel*var_SpecTex.a*mask.r));
                c.rgb=lerp(c.rgb,GrafTexR.rgb,GrabLerp);
                // c.a*=alpha2;

                return c;




            }
            ENDHLSL
        }

    }

}
