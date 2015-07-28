// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
/* Renders curvy spline(s) approximation using GL.Draw
 * 
 * Add this script to a camera
 */


/// <summary>
/// Class to render a spline using GL.Draw
/// </summary>
/// <remarks>Useful for debugging</remarks>
public class GLCurvyRenderer : MonoBehaviour {
    public CurvySplineBase[] Splines;
    public Color[] Colors;
    Vector3[] Points;
    Material lineMaterial;
     
    void CreateLineMaterial()
    {
        if( !lineMaterial ) {
            lineMaterial = new Material( "Shader \"Lines/Colored Blended\" {" +
                "SubShader { Pass { " +
                "    Blend SrcAlpha OneMinusSrcAlpha " +
                "    ZWrite Off Cull Off Fog { Mode Off } " +
                "    BindChannels {" +
                "      Bind \"vertex\", vertex Bind \"color\", color }" +
                "} } }" );
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }
    }
     
    void OnPostRender()
    {
        if (Splines.Length==0)
            return;
        for (int s=0;s<Splines.Length;s++) {
            CurvySplineBase spline = Splines[s];
            Color lineColor = (s<Colors.Length) ? Colors[s] : Color.green;
            if (spline && spline.IsInitialized) {
                Points = spline.GetApproximation();
                CreateLineMaterial();
                lineMaterial.SetPass(0);
                GL.Begin(GL.LINES);
                GL.Color(lineColor);
                for (int i = 1; i < Points.Length; i++) {
                    GL.Vertex(Points[i - 1]);
                    GL.Vertex(Points[i]);
                }
                GL.End();
            }
        }
    }
}
