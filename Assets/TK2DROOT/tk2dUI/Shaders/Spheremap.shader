Shader "Mobile/Spheremap"
{
     Properties
     {
     	_Blend ("Blend", Range (0, 1) ) = 0.5 
     	_MainTex ("MainTexture (RGB)", 2D) = "white" {}
        _EnvMap ("EnvMap", 2D) = "black" { TexGen SphereMap }
     }
 
     SubShader
     {
        Lighting on
 
        Pass
        {  
			SetTexture [_MainTex]
            {
                combine texture + primary
            }
 
            SetTexture [_EnvMap]
            {
                ConstantColor (0,0,0, [_Blend]) 
				Combine texture Lerp(constant) previous
            }
        }
    }
}