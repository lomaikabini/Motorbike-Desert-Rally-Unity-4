using UnityEngine;
using System.Collections;

public class GizmoObject : MonoBehaviour {


    public Color GColor = Color.white;
    public float GSize=1.0f;


    void OnDrawGizmos()
    {


        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one*GSize);
        Gizmos.matrix = rotationMatrix;

        Gizmos.color = GColor;
        Gizmos.DrawCube(Vector3.zero, Vector3.one * GSize);

    }




}
