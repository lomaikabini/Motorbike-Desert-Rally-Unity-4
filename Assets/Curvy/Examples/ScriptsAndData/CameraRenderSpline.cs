using UnityEngine;
using System.Collections;

public class CameraRenderSpline : MonoBehaviour {
    public CurvySpline Spline;

	void OnPostRender()
    {
        if (!Spline || !Spline.IsInitialized) return;
        Vector3[] approx = Spline.GetApproximation();
        GL.Color(Color.white);
        GL.Begin(GL.LINES);
        for (int i = 0; i < approx.Length-1; i++) {
            GL.Vertex(approx[i]);
            GL.Vertex(approx[i + 1]);
        }
        GL.End();
    }
}
