// Unlit shader.
// - no lighting
// - no lightmap support
 
Shader "AndroidTerrain/Unlit" {
Properties {
   [HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
   [HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
   [HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
   [HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
   [HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
   // used in fallback on old cards & base map
   [HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
   [HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
}
SubShader {
   Tags {
     "SplatCount" = "4"
     "Queue" = "Geometry-100"
     "RenderType" = "Opaque"
   }
 
   Pass {
     CGPROGRAM
       #pragma vertex vert
       #pragma fragment frag
 
       #include "UnityCG.cginc"
 
       struct appdata_t {
         float4 vertex : POSITION;
         float2 uv_Control : TEXCOORD0;
         //float2 uv_Splat : TEXCOORD1;
         float2 uv_Splat0 : TEXCOORD1;
         //float2 uv_Splat1 : TEXCOORD2;
         //float2 uv_Splat2 : TEXCOORD3;
        //float2 uv_Splat3 : TEXCOORD4;
       };
 
       struct v2f {
         float4 vertex : SV_POSITION;
         float2 uv_Control : TEXCOORD0;
         //float2 uv_Splat : TEXCOORD1;
         float2 uv_Splat0 : TEXCOORD1;
         //float2 uv_Splat1 : TEXCOORD2;
         //float2 uv_Splat2 : TEXCOORD3;
         //float2 uv_Splat3 : TEXCOORD4;
       };
 
       //sampler2D _MainTex;
       //float4 _MainTex_ST;
 
       sampler2D _Control;
       float4 _Control_ST;
       sampler2D _Splat0;
       sampler2D _Splat1;
       sampler2D _Splat2;
       sampler2D _Splat3;
       //float4 _Splat_ST;
       float4 _Splat0_ST;
       //float4 _Splat1_ST;
       //float4 _Splat2_ST;
       //float4 _Splat3_ST;
     
       v2f vert (appdata_t v)
       {
         v2f o;
         o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
         o.uv_Control = TRANSFORM_TEX(v.uv_Control, _Control);
         //o.uv_Splat = TRANSFORM_TEX(v.uv_Splat, _Splat);
         o.uv_Splat0 = TRANSFORM_TEX(v.uv_Splat0, _Splat0);
         //o.uv_Splat1 = TRANSFORM_TEX(v.uv_Splat1, _Splat1);
         //o.uv_Splat2 = TRANSFORM_TEX(v.uv_Splat2, _Splat2);
         //o.uv_Splat3 = TRANSFORM_TEX(v.uv_Splat3, _Splat3);
         return o;
       }
     
       fixed4 frag (v2f i) : SV_Target
       {
         //fixed4 col = tex2D(_Control, i.uv_Control);
         //return col;
 
         fixed4 splat_control = tex2D (_Control, i.uv_Control);
         fixed3 col;
         col  = splat_control.r * tex2D (_Splat0, i.uv_Splat0).rgb;
         col += splat_control.g * tex2D (_Splat1, i.uv_Splat0).rgb;
         col += splat_control.b * tex2D (_Splat2, i.uv_Splat0).rgb;
         col += splat_control.a * tex2D (_Splat3, i.uv_Splat0).rgb;
 
         fixed4 output;
         output.rgb = col;
         output.a = 0.0f;
         return output;
       }
     ENDCG
   }
}
 
Dependency "AddPassShader" = "AndroidTerrain/Unlit"
Dependency "BaseMapShader" = "Diffuse"
Dependency "Details0"  = "Hidden/TerrainEngine/Details/Vertexlit"
Dependency "Details1"  = "Hidden/TerrainEngine/Details/WavingDoublePass"
Dependency "Details2"  = "Hidden/TerrainEngine/Details/BillboardWavingDoublePass"
Dependency "Tree0"  = "Hidden/TerrainEngine/BillboardTree"
 
// Fallback to Diffuse
Fallback "Diffuse"
 
}