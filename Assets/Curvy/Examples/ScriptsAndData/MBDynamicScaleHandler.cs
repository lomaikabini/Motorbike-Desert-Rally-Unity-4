/*
 * 
 * This example shows how to dynamically modify a MeshBuilder's scale
 * 
 */
using UnityEngine;
using System.Collections;

public class MBDynamicScaleHandler : MonoBehaviour {
    public SplinePathMeshBuilder SplineMesh; // the mesh we want to scale
    SplineWalker Walker; // the moving sphere
    float meshMinTF; // TF at which we need to begin altering the scale
    float meshMaxTF; // TF at which we can stop altering the scale
    float sphereRadius = 1.5f; // guess...
    
    bool insideMesh;

	// Use this for initialization
	IEnumerator Start () {

        Walker = GetComponent<SplineWalker>();
        // As usual: Wait for the spline
        while (!Walker.Spline.IsInitialized)
            yield return null;
        // register for the OnGetScale event of the MeshBuilder        
        SplineMesh.OnGetScale += new SplinePathMeshBuilder.OnGetScaleEvent(SplineMesh_OnGetScale);
        // store min/max TF where we have to change the scale of the mesh
        meshMinTF = SplineMesh.Spline.DistanceToTF(SplineMesh.Spline.TFToDistance(SplineMesh.FromTF) - sphereRadius);
        meshMaxTF = SplineMesh.Spline.DistanceToTF(SplineMesh.Spline.TFToDistance(SplineMesh.ToTF) + sphereRadius);
	}

    void OnDestroy()
    {
        SplineMesh.OnGetScale -= SplineMesh_OnGetScale;
    }

    void Update()
    {
        // First we determine if the sphere is in range of the mesh (or just exited). If so, we rebuild the mesh.
        if (SplineMesh) {
            bool cur = insideMesh;
            insideMesh = (Walker.TF >= meshMinTF && Walker.TF <= meshMaxTF);
            if (insideMesh || cur != insideMesh)
                SplineMesh.Refresh(); // Rebuild mesh. SplineMesh_OnGetScale will be called by the SplineMesh now

            cur = insideMesh;
        }
    }

    // this method is called by SplinePathMeshBuilder to get the mesh scale for a certain tf
    Vector3 SplineMesh_OnGetScale(SplinePathMeshBuilder sender, float tf)
    {
        if (insideMesh)  {
            // determine the scale we want at the sphere's position
            float spherePos = Walker.Spline.TFToDistance(Walker.TF);
            float scalePos = Walker.Spline.TFToDistance(tf);
            float delta = spherePos - scalePos;
            float d=Mathf.Abs(delta);
            float s=0;
            if (d<=sphereRadius){
                s = Mathf.Cos(d / sphereRadius);
            }
            return Vector3.one + Vector3.one*s;
        }
        return Vector3.one;
    }

   
	
	
}
