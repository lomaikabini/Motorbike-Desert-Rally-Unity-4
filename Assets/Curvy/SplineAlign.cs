// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections;
/* Drop this script to a transform to place it onto a Curvy Spline
 */

/// <summary>
/// Position an Transform on a spline
/// </summary>
[ExecuteInEditMode]
public class SplineAlign : MonoBehaviour {
    public CurvySplineBase Spline; // Spline or group to use
    public float Distance; // Distance in TF or world units
    public bool UseWorldUnits; // Should Distance be TF or world units?
    public bool SetOrientation=true; // Rotate transform to match spline orientation?
    
	// Use this for initialization
	IEnumerator Start () {
        if (Spline) {
            // Wait until the spline is fully intialized before accessing it:
            while (!Spline.IsInitialized)
                yield return null;
            // now we're safe to use it
            Set();
        }
	}
	
	void Update () {
        // While in the editor, update contiuously
        if (Spline && !Application.isPlaying)
            Set();
	}

    void Set()
    {
        float tf;
        // First get the TF if needed
        if (UseWorldUnits) {
            if (Distance >= Spline.Length)
                Distance -= Spline.Length;
            else if (Distance < 0)
                Distance += Spline.Length;
            tf=Spline.DistanceToTF(Distance);
        }
        else {
            if (Distance >= 1)
                Distance -= 1;
            else if (Distance < 0)
                Distance += 1;
            tf=Distance;
        }

        // Set the position
        if (transform.position!=Spline.Interpolate(tf))
            transform.position = Spline.Interpolate(tf);
        // Set the rotation
        if (SetOrientation && transform.rotation!=Spline.GetOrientationFast(tf))
            transform.rotation = Spline.GetOrientationFast(tf);
    }
}

