// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections;
/* Drop this script to a transform you'd like to move along a Curvy spline!
 * 
 * Basically you just need two methods (Spline.Move and Spline.GetOrientationFast) to align an object to a spline,
 * so you can easily incorporate spline control into your own scripts.
 */

/// <summary>
/// Move a Transform along a spline
/// </summary>
[ExecuteInEditMode]
public class SplineWalker : MonoBehaviour {
    public CurvySplineBase Spline; // The spline to use
    public CurvyClamping Clamping = CurvyClamping.Clamp; // What to do if we reach the spline's end?
    public bool SetOrientation = true; // Rotate to match orientation?
    public bool FastInterpolation; // use cached values?
    public bool MoveByWorldUnits = false; // move at a constant speed regardless of segment length?
    public float InitialF; // the starting position
    public float Speed; // the moving speed, either in F or world units (depending on MoveByWorldUnits)
    public bool Forward = true; //

    /// <summary>
    /// Relative position on the spline
    /// </summary>
    public float TF 
    {
        get { return mTF; }
        set { mTF = value; }
    }
    /// <summary>
    /// Direction to travel (1=forward, -1=backwards)
    /// </summary>
    public int Dir
    {
        get
        {
            return (Forward) ? 1 : -1;
        }
        set
        {
            bool f = (value >= 0);
            if (f != Forward)
                Forward = f;
        }
    }


    float mTF;
    
    Transform mTransform;

	// Use this for initialization
	IEnumerator Start () {
        
        mTF = InitialF;
        Speed = Mathf.Abs(Speed);
        mTransform = transform;
        if (Spline) {
            // Wait until the spline is fully intialized before accessing it:
            while (!Spline.IsInitialized)
                yield return null;
            // now we're safe to use it
            InitPosAndRot();
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (!Spline || !Spline.IsInitialized) return;
        // Runtime processing
        if (Application.isPlaying) {
            int dir = Dir;
            // Move at a constant speed?
            if (MoveByWorldUnits) {
                // either used cached values(slightly faster) or interpolate position now (more exact)
                // Note that we pass mTF and mDir by reference. These values will be changed by the Move methods
                mTransform.position = (FastInterpolation) ?
                    Spline.MoveByFast(ref mTF, ref dir, Speed * Time.deltaTime, Clamping) : // linear interpolate cached values
                    Spline.MoveBy(ref mTF, ref dir, Speed * Time.deltaTime, Clamping); // interpolate now
            }
            else { // Move at constant F
                // either used cached values(slightly faster) or interpolate position now (more exact)
                // Note that we pass mTF and mDir by reference. These values will be changed by the Move methods
                mTransform.position = (FastInterpolation) ?
                    Spline.MoveFast(ref mTF, ref dir, Speed * Time.deltaTime, Clamping) : // linear interpolate cached values
                    Spline.Move(ref mTF, ref dir, Speed * Time.deltaTime, Clamping); // interpolate now
            }
            // Rotate the transform to match the spline's orientation
            if (SetOrientation) {
                transform.rotation = Spline.GetOrientationFast(mTF);
            }
            
            Dir = dir;
        }
        else // Editor processing: continuously place the transform to reflect property changes in the editor
            InitPosAndRot();
	}

    void InitPosAndRot()
    {
        if (!Spline) return;
        // move the transform onto the spline
        if (Spline.Interpolate(InitialF)!=mTransform.position)
            mTransform.position = Spline.Interpolate(InitialF);
        // Rotate the transform to match the spline's orientation?
        if (SetOrientation && mTransform.rotation != Spline.GetOrientationFast(InitialF))
            mTransform.rotation = Spline.GetOrientationFast(InitialF);
    }

}
