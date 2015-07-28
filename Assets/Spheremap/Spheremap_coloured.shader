Shader "Mobile/SpheremapColoured"
{
     Properties
     {
     	_Blend ("Blend", Range (0, 1) ) = 0.5 
     	_MainTex ("MainTexture (RGB)", 2D) = "white" {}
        _EnvMap ("EnvMap", 2D) = "black" { TexGen SphereMap }
        _Color ("Main Color", Color) = (1,1,1,1)
     }
		
     SubShader
     {
 
        Pass
        {  
 			Material
	        {
	        	Diffuse (1,1,1,1)
	        }
	        Lighting On
	       
            SetTexture [_MainTex]
            {
                Combine texture * primary DOUBLE, texture * primary
            }
 
 			SetTexture [_MainText]
 			{
 				ConstantColor [_Color]
 				Combine previous * constant
 			}
 
            SetTexture [_EnvMap]
            {
                ConstantColor (0,0,0, [_Blend]) 
				Combine texture Lerp(constant) previous
            }
        }
    }
}