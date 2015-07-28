// Shader created with Shader Forge Beta 0.36 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.36;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:0,bsrc:0,bdst:0,culm:0,dpts:2,wrdp:True,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:32719,y:32712|diff-148-OUT,spec-8-RGB,normal-10-RGB;n:type:ShaderForge.SFN_Tex2d,id:2,x:33141,y:32081,ptlb:diffuse,ptin:_diffuse,tex:b2af5af34860d474db18f5d1bbbd11d8,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:8,x:33151,y:32814,ptlb:Specular,ptin:_Specular,tex:f49ef869fb83e8847ac06746fdd16fd1,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:10,x:33098,y:32980,ptlb:Normal,ptin:_Normal,tex:5688e6ad4ef81ce48a70aede5405e030,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Tex2d,id:31,x:33490,y:32326,ptlb:Mask 1,ptin:_Mask1,tex:539a688462be0914f998bc28a7dc3d02,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Color,id:38,x:33453,y:32087,ptlb:Diffuse Color 1,ptin:_DiffuseColor1,glob:False,c1:1,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Lerp,id:85,x:33228,y:32279|A-2-RGB,B-109-OUT,T-31-R;n:type:ShaderForge.SFN_Multiply,id:109,x:32949,y:32282|A-2-RGB,B-38-RGB;n:type:ShaderForge.SFN_Color,id:131,x:33856,y:32437,ptlb:Diffuse Color 2,ptin:_DiffuseColor2,glob:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Tex2d,id:132,x:33896,y:32659,ptlb:Mask 2,ptin:_Mask2,tex:d1494dd074fd7ee4f83c065ff7a052c6,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Lerp,id:148,x:33121,y:32526|A-85-OUT,B-155-OUT,T-132-R;n:type:ShaderForge.SFN_Multiply,id:155,x:33534,y:32523|A-2-RGB,B-131-RGB;proporder:2-38-131-8-10-31-132;pass:END;sub:END;*/

Shader "Custom/Motorbike" {
    Properties {
        _diffuse ("diffuse", 2D) = "white" {}
        _DiffuseColor1 ("Diffuse Color 1", Color) = (1,0.5,0.5,1)
        _DiffuseColor2 ("Diffuse Color 2", Color) = (0.5,0.5,0.5,1)
        _Specular ("Specular", 2D) = "white" {}
        _Normal ("Normal", 2D) = "bump" {}
        _Mask1 ("Mask 1", 2D) = "white" {}
        _Mask2 ("Mask 2", 2D) = "white" {}
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        LOD 200
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _diffuse; uniform float4 _diffuse_ST;
            uniform sampler2D _Specular; uniform float4 _Specular_ST;
            uniform sampler2D _Normal; uniform float4 _Normal_ST;
            uniform sampler2D _Mask1; uniform float4 _Mask1_ST;
            uniform float4 _DiffuseColor1;
            uniform float4 _DiffuseColor2;
            uniform sampler2D _Mask2; uniform float4 _Mask2_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 binormalDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(float4(v.normal,0), _World2Object).xyz;
                o.tangentDir = normalize( mul( _Object2World, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.binormalDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(_Object2World, v.vertex);
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.binormalDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
/////// Normals:
                float2 node_159 = i.uv0;
                float3 normalLocal = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(node_159.rg, _Normal))).rgb;
                float3 normalDirection =  normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = dot( normalDirection, lightDirection );
                float3 diffuse = max( 0.0, NdotL) * attenColor + UNITY_LIGHTMODEL_AMBIENT.rgb;
///////// Gloss:
                float gloss = 0.5;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                NdotL = max(0.0, NdotL);
                float3 specularColor = tex2D(_Specular,TRANSFORM_TEX(node_159.rg, _Specular)).rgb;
                float3 specular = (floor(attenuation) * _LightColor0.xyz) * pow(max(0,dot(halfDirection,normalDirection)),specPow) * specularColor;
                float3 finalColor = 0;
                float3 diffuseLight = diffuse;
                float4 node_2 = tex2D(_diffuse,TRANSFORM_TEX(node_159.rg, _diffuse));
                finalColor += diffuseLight * lerp(lerp(node_2.rgb,(node_2.rgb*_DiffuseColor1.rgb),tex2D(_Mask1,TRANSFORM_TEX(node_159.rg, _Mask1)).r),(node_2.rgb*_DiffuseColor2.rgb),tex2D(_Mask2,TRANSFORM_TEX(node_159.rg, _Mask2)).r);
                finalColor += specular;
/// Final Color:
                return fixed4(finalColor,1);
            }
            ENDCG
        }
        Pass {
            Name "ForwardAdd"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            Fog { Color (0,0,0,0) }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _diffuse; uniform float4 _diffuse_ST;
            uniform sampler2D _Specular; uniform float4 _Specular_ST;
            uniform sampler2D _Normal; uniform float4 _Normal_ST;
            uniform sampler2D _Mask1; uniform float4 _Mask1_ST;
            uniform float4 _DiffuseColor1;
            uniform float4 _DiffuseColor2;
            uniform sampler2D _Mask2; uniform float4 _Mask2_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 binormalDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(float4(v.normal,0), _World2Object).xyz;
                o.tangentDir = normalize( mul( _Object2World, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.binormalDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(_Object2World, v.vertex);
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.binormalDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
/////// Normals:
                float2 node_160 = i.uv0;
                float3 normalLocal = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(node_160.rg, _Normal))).rgb;
                float3 normalDirection =  normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = dot( normalDirection, lightDirection );
                float3 diffuse = max( 0.0, NdotL) * attenColor;
///////// Gloss:
                float gloss = 0.5;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                NdotL = max(0.0, NdotL);
                float3 specularColor = tex2D(_Specular,TRANSFORM_TEX(node_160.rg, _Specular)).rgb;
                float3 specular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow) * specularColor;
                float3 finalColor = 0;
                float3 diffuseLight = diffuse;
                float4 node_2 = tex2D(_diffuse,TRANSFORM_TEX(node_160.rg, _diffuse));
                finalColor += diffuseLight * lerp(lerp(node_2.rgb,(node_2.rgb*_DiffuseColor1.rgb),tex2D(_Mask1,TRANSFORM_TEX(node_160.rg, _Mask1)).r),(node_2.rgb*_DiffuseColor2.rgb),tex2D(_Mask2,TRANSFORM_TEX(node_160.rg, _Mask2)).r);
                finalColor += specular;
/// Final Color:
                return fixed4(finalColor * 1,0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
