using UnityEngine;
using System.Collections;

public class Distances : MonoBehaviour {
    public CurvySpline Spline;
    public Transform Cube;
    public int Amount = 10;
    public float Speed = 1;

    Transform[] cubes;
    float[] tf;
    int[] dir;

	// Use this for initialization
	IEnumerator Start () {
        if (Spline && Cube) {
            while (!Spline.IsInitialized)
                yield return null;

            cubes = new Transform[Amount];
            tf = new float[Amount];
            dir = new int[Amount];
            cubes[0] = Cube;
            tf[0] = 0;
            dir[0] = (Speed >= 0) ? 1 : -1;
            // Scale Cube depending on Spline length and number of cubes
            float sc=Spline.Length/Amount;
            Cube.localScale = new Vector3(sc*0.7f, sc*0.7f, sc*0.7f);
            // Create and position cubes
            Cube.position = Spline.InterpolateByDistance(0);
            for (int i = 1; i < Amount; i++) {
                {
                    tf[i] = Spline.DistanceToTF(i * sc);
                    cubes[i] = getCube();
                    cubes[i].position = Spline.Interpolate(tf[i]);
                    cubes[i].rotation = Spline.GetOrientationFast(tf[i]);
                    dir[i] = (Speed >= 0) ? 1 : -1;
                }
            }
            
            Speed = Mathf.Abs(Speed);
        }
	}
	
	void Update () {
        if (cubes==null || !Spline.IsInitialized) return;
        
        // Move Cubes by a certain distance
        for (int i=0;i<cubes.Length;i++) {
            cubes[i].position = Spline.MoveBy(ref tf[i],ref dir[i],Speed*Time.deltaTime,CurvyClamping.Loop);
            cubes[i].rotation = Spline.GetOrientationFast(tf[i]);
        }
	}

    Transform getCube()
    {
        return (Transform)GameObject.Instantiate(Cube);
    }
}
