// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections;
/* Drop this script to a transform you'd like to move along a Curvy spline with connections!
 * 
 * This script automatically adds it's current spline's name as a tag to the move method. If moving backwards, ReverseSuffix is
 * added to the tag.
 * 
 * If you want to follow a connection, be sure to set the connection's tags correct.
 * 
 * This is just an example script (see the JunctionWalker example). If you plan to utilize connections while moving, you should roll
 * your own script with appropriate tag logic.
 */

[ExecuteInEditMode]
public class SplineWalkerCon : MonoBehaviour {
    public CurvySpline Spline;
    public CurvyClamping Clamping = CurvyClamping.Clamp; // What to do if we reach the spline's end?
    public bool SetOrientation = true; // Rotate to match orientation?
    public bool FastInterpolation; // use cached values?
    public bool MoveByWorldUnits = false; // move at a constant speed regardless of segment length?
    public float InitialF; // the starting position
    public float Speed; // the moving speed, either in F or world units (depending on MoveByWorldUnits)
    public string AdditionalTags = string.Empty;
    public int MinTagMatches = 1;
    public string ReverseSuffix = "Rev";
    public bool Forward = true;

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
            bool f=(value >= 0);
            if (f != Forward)
                Forward = f;
        }
    }

    public string[] ResultingTags
    {
        get { return buildTags(); }
    }

    float mTF;
    
    Transform mTransform;

    // Use this for initialization
    IEnumerator Start()
    {
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
    void Update()
    {
        
        if (!Spline || !Spline.IsInitialized) return;
        // Runtime processing
        if (Application.isPlaying) {
            int dir = Dir;    
            // Move at a constant speed?
            if (MoveByWorldUnits) {
                // either used cached values(slightly faster) or interpolate position now (more exact)
                // Note that we pass mTF and mDir by reference. These values will be changed by the Move methods
                mTransform.position = (FastInterpolation) ?
                    Spline.MoveByConnectionFast(ref Spline, ref mTF, ref dir, Speed * Time.deltaTime, Clamping, MinTagMatches,buildTags()) : // linear interpolate cached values
                    Spline.MoveByConnection(ref Spline, ref mTF, ref dir, Speed * Time.deltaTime, Clamping, MinTagMatches, buildTags()); // interpolate now
            }
            else { // Move at constant F
                // either used cached values(slightly faster) or interpolate position now (more exact)
                // Note that we pass Spline, mTF and mDir by reference. These values will be changed by the MoveConnection methods
                mTransform.position = (FastInterpolation) ?
                    Spline.MoveConnectionFast(ref Spline, ref mTF, ref dir, Speed * Time.deltaTime, Clamping, MinTagMatches, buildTags()): // interpolate now
                    Spline.MoveConnection(ref Spline, ref mTF, ref dir, Speed * Time.deltaTime, Clamping, MinTagMatches, buildTags()); // interpolate now
                
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
        if (Spline.Interpolate(InitialF) != mTransform.position)
            mTransform.position = Spline.Interpolate(InitialF);
        // Rotate the transform to match the spline's orientation?
        if (SetOrientation && mTransform.rotation != Spline.GetOrientationFast(InitialF))
            mTransform.rotation = Spline.GetOrientationFast(InitialF);
    }

    string[] buildTags()
    {
        if (Forward)
            return (Spline.name+" "+AdditionalTags).Split(' ');
        else
            return (Spline.name + ReverseSuffix+" " + AdditionalTags).Split(' ');
    }

    
}
