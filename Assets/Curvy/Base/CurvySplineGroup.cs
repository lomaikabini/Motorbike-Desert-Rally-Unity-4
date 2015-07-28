// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Curvy SplineGroup class
/// </summary>
[ExecuteInEditMode]
public class CurvySplineGroup : CurvySplineBase
{
    #region ### Public Fields and Properties ###
    /// <summary>
    /// Splines contained in this SplineGroup
    /// </summary>
    public List<CurvySpline> Splines = new List<CurvySpline>();

    /// <summary>
    /// Distance in world units of each spline in the group, starting from the first spline's segment
    /// </summary>
    public float[] Distances { get; private set; }

    /// <summary>
    /// Number of splines in the SplineGroup
    /// </summary>
    public int Count { get { return Splines.Count; } }

    /// <summary>
    /// Gets a spline from the group
    /// </summary>
    /// <param name="idx">index of the spline</param>
    /// <returns>a Spline</returns>
    public CurvySpline this[int idx]
    {
        get
        {
            return (idx > -1 && idx < Splines.Count) ? Splines[idx] : null;
        }
    }

    #endregion

    /// <summary>
    /// Creates a new GameObject with a CurvySplineGroup
    /// </summary>
    /// <param name="splines">the splines to add to the group</param>
    /// <returns>the new CurvySplineGroup component</returns>
    public static CurvySplineGroup Create(params CurvySpline[] splines)
    {
        CurvySplineGroup grp = new GameObject("Curvy Spline Group", typeof(CurvySplineGroup)).GetComponent<CurvySplineGroup>();
        grp.Add(splines);
        return grp;
    }
    
    #region ### Unity Callbacks ###

    IEnumerator Start()
    {
        for (int i = 0; i < Count; i++) {
            while (!this[i].IsInitialized)
                yield return 0;
        }
        
        RefreshImmediately(true,true,false);
    }

    void OnDisable()
    {
        foreach (CurvySpline spl in Splines)
            spl.OnRefresh -= OnSplineRefresh;
    }

    void Update()
    {
        for (int i=0;i<Count;i++){
            this[i].OnRefresh -= OnSplineRefresh;
            this[i].OnRefresh += OnSplineRefresh;
        }
        if (mNeedLengthRefresh || mNeedOrientationRefresh)
            RefreshImmediately(mNeedLengthRefresh, mNeedOrientationRefresh, mSkipRefreshIfInitialized);
    }


    #endregion

    #region ### Methods based on TF (total fragment) ###
    
    /// <summary>
    /// Gets the interpolated position for a certain group TF
    /// </summary>
    /// <remarks>TF (Total Fragment) relates to the total length of the group</remarks>
    /// <param name="tf">TF value identifying position in the group (0..1)</param>
    /// <returns>the interpolated position</returns>
    public override Vector3 Interpolate(float tf)
    {
        float localTF;
        CurvySpline spl = TFToSpline(tf, out localTF);

        return spl.Interpolate(localTF);
    }

    /// <summary>
    /// Gets the interpolated position for a certain group TF using a linear approximation
    /// </summary>
    /// <remarks>TF (Total Fragment) relates to the total length of the group</remarks>
    /// <param name="tf">TF value reflecting position in the group (0..1)</param>
    /// <returns>the interpolated position</returns>
    public override Vector3 InterpolateFast(float tf)
    {
        float localTF;
        CurvySpline spl = TFToSpline(tf, out localTF);

        return spl.InterpolateFast(localTF);
    }

    /// <summary>
    /// Gets an interpolated User Value for a certain group TF
    /// </summary>
    /// <remarks>TF (Total Fragment) relates to the total length of the group</remarks>
    /// <param name="tf">TF value reflecting position in the group (0..1)</param>
    /// <param name="index">the UserValue array index</param>
    /// <returns>the interpolated value</returns>
    public override Vector3 InterpolateUserValue(float tf, int index)
    {
        float localTF;
        CurvySpline spl = TFToSpline(tf, out localTF);
        return spl.InterpolateUserValue(localTF, index);
    }

    /// <summary>
    /// Gets an interpolated Scale for a certain group TF
    /// </summary>
    /// <remarks>TF (Total Fragment) relates to the total length of the group</remarks>
    /// <param name="tf">TF value reflecting position in the group(0..1)</param>
    /// <returns>the interpolated value</returns>
    public override Vector3 InterpolateScale(float tf)
    {
        float localTF;
        CurvySpline spl = TFToSpline(tf, out localTF);
        return spl.InterpolateScale(localTF);
    }

    /// <summary>
    /// Gets the Up-Vector for a certain TF based on the splines' Orientation mode
    /// </summary>
    /// <param name="tf">TF value reflecting position in the group (0..1)</param>
    /// <returns>the Up-Vector</returns>
    public override Vector3 GetOrientationUpFast(float tf)
    {
        float localTF;
        CurvySpline spl = TFToSpline(tf, out localTF);

        return spl.GetOrientationUpFast(localTF);
    }

    /// <summary>
    /// Gets a rotation looking to Tangent with the head upwards along the Up-Vector
    /// </summary>
    /// <param name="tf">TF value reflecting position in the group (0..1)</param>
    /// <returns>a rotation</returns>
    public override Quaternion GetOrientationFast(float tf)
    {
        float localTF;
        CurvySpline spl = TFToSpline(tf, out localTF);
        return spl.GetOrientationFast(localTF);
    }

    /// <summary>
    /// Gets the normalized tangent for a certain TF
    /// </summary>
    /// <param name="tf">TF value identifying position in the group (0..1)</param>
    /// <returns>a tangent vector</returns>
    public override Vector3 GetTangent(float tf)
    {
        float localTF;
        CurvySpline spl = TFToSpline(tf, out localTF);
        return spl.GetTangent(localTF);
    }

    /// <summary>
    /// Gets the normalized tangent for a certain TF with a known position for TF
    /// </summary>
    /// <remarks>This saves one interpolation</remarks>
    /// <param name="tf">TF value identifying position in the group (0..1)</param>
    /// <param name="position">The interpolated position for TF</param>
    /// <returns>a tangent vector</returns>
    public override Vector3 GetTangent(float tf, Vector3 position)
    {
        float localTF;
        CurvySpline spl = TFToSpline(tf, out localTF);
        return spl.GetTangent(localTF,position);
    }

    /// <summary>
    /// Gets the normalized tangent for a certain TF using a linear approximation
    /// </summary>
    /// <param name="tf">TF value identifying position in the group (0..1)</param>
    /// <returns>a tangent vector</returns>
    public override Vector3 GetTangentFast(float tf)
    {
        float localTF;
        CurvySpline spl = TFToSpline(tf, out localTF);
        return spl.GetTangentFast(localTF);
    }

    /// <summary>
    /// Alter TF to reflect a movement over a certain distance.
    /// </summary>
    /// <remarks>MoveByLengthFast is used internally, so stepSize is ignored</remarks>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="distance">the distance in world units to move</param>
    /// <param name="clamping">clamping mode</param>
    /// <param name="stepSize">stepSize defines the accuracy</param>
    /// <returns>the interpolated position</returns>
    public override Vector3 MoveBy(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize)
    {
        return MoveByLengthFast(ref tf, ref direction, distance, clamping);
    }

    /// <summary>
    /// Alter TF to reflect a movement over a certain distance.
    /// </summary>
    /// <remarks>MoveByLengthFast is used internally, so stepSize is ignored</remarks>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="distance">the distance in world units to move</param>
    /// <param name="clamping">clamping mode</param>
    /// <param name="stepSize">stepSize defines the accuracy</param>
    /// <returns>the interpolated position</returns>
    public override Vector3 MoveByFast(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize)
    {
        return MoveByLengthFast(ref tf, ref direction, distance, clamping);
    }

    

    /// <summary>
    /// Converts a group TF value to a group distance
    /// </summary>
    /// <param name="tf">a TF value in the range 0..1</param>
    /// <returns>distance from the first spline's first segment's Control Point</returns>
    public override float TFToDistance(float tf)
    {
        float localTF;
        int idx = TFToSplineIndex(tf, out localTF);
        return Distances[idx]+this[idx].TFToDistance(localTF);
    }

    /// <summary>
    /// Gets the spline a certan TF lies in
    /// </summary>
    /// <param name="tf">the TF value in the range 0..1</param>
    /// <returns>the spline the given group TF is inside</returns>
    public CurvySpline TFToSpline(float tf)
    {
        float localTF;
        int idx = TFToSplineIndex(tf, out localTF);
        return (idx == -1) ? null : this[idx];
    }

    /// <summary>
    /// Gets the spline and the spline's TF for a certain group TF
    /// </summary>
    /// <param name="tf">the TF value in the range 0..1</param>
    /// <param name="localTF">gets the remaining TF in the range 0..1</param>
    /// <returns>the spline the given group TF is inside</returns>
    public CurvySpline TFToSpline(float tf, out float localTF)
    {
        int idx = TFToSplineIndex(tf, out localTF);
        return (idx == -1) ? null : this[idx];
    }

    

    /// <summary>
    /// Gets a TF value from a group's spline and a spline's TF
    /// </summary>
    /// <param name="spline">a spline of this group</param>
    /// <param name="splineTF">TF of the spline in the range 0..1</param>
    /// <returns>a group TF value in the range 0..1</returns>
    public float SplineToTF(CurvySpline spline, float splineTF)
    {
        return ((float)Splines.IndexOf(spline) / Count) + (1f / Count) * splineTF;
    }

    #endregion

    #region ### Methods working on distance from the first control point ###

    /// <summary>
    /// Converts a distance to a TF value
    /// </summary>
    /// <param name="distance">distance in the range 0..Length</param>
    /// <returns>a TF value in the range 0..1</returns>
    public override float DistanceToTF(float distance)
    {
        float localDistance;
        CurvySpline spl = DistanceToSpline(distance, out localDistance);
        return SplineToTF(spl, spl.DistanceToTF(localDistance));
    }

    /// <summary>
    /// Gets the spline a certain distance lies within
    /// </summary>
    /// <param name="distance">a distance in the range 0..Length</param>
    /// <returns>a spline or null</returns>
    public CurvySpline DistanceToSpline(float distance)
    {
        float d;
        return DistanceToSpline(distance, out d);
    }

    /// <summary>
    /// Gets the spline a certain distance lies within
    /// </summary>
    /// <param name="distance">a distance in the range 0..Length</param>
    /// <param name="localDistance">gets the remaining distance inside the spline</param>
    /// <returns>a spline or null</returns>
    public CurvySpline DistanceToSpline(float distance, out float localDistance)
    {
        distance = Mathf.Clamp(distance, 0, Length);
        localDistance = 0;

        for (int i = 1; i < Count; i++) {
            if (Distances[i] >= distance) {
                localDistance = distance - Distances[i - 1];
                return this[i - 1];
            }
        }
        localDistance = distance-Distances[Count-1];
        return this[Count - 1];
    }

    #endregion

    #region ### General Methods ###
    
    /// <summary>
    /// Add splines to the group
    /// </summary>
    /// <param name="splines">splines to add</param>
    public void Add(params CurvySpline[] splines)
    {
        Splines.AddRange(splines);
        Refresh();
    }

    /// <summary>
    /// Remove a spline from the group
    /// </summary>
    /// <param name="spline">the spline to remove</param>
    public void Delete(CurvySpline spline)
    {
        Splines.Remove(spline);
        Refresh();
    }

    /// <summary>
    /// Remove all splines from the group
    /// </summary>
    public override void Clear()
    {
        Splines.Clear();
        Refresh();
    }

    /// <summary>
    /// Gets an array containing all approximation points
    /// </summary>
    /// <param name="local">get coordinates local to the group</param>
    /// <remarks>This can be used to feed meshbuilders etc...</remarks>
    /// <returns>an array of world or local positions</returns>
    public override Vector3[] GetApproximation(bool local)
    {
        
        List<Vector3[]> ap = new List<Vector3[]>(Count);
        
        int n = 0;
        bool con;
        for (int i=0;i<Count;i++) {
            Vector3[] p = this[i].GetApproximation(local);
            con = NextSplineConnected(i);
            if (con) {
                System.Array.Resize<Vector3>(ref p,p.Length-1);
            }
            ap.Add(p);
            n += ap[i].Length;
        }

        Vector3[] apps = new Vector3[n];
        n = 0;
        for (int i = 0; i < Count; i++) {
            ap[i].CopyTo(apps, n);
            n += ap[i].Length;
        }
        return apps;
    }
    /// <summary>
    /// Gets an array containing all approximation tangent points
    /// </summary>
    /// <returns>an array of normalized tangent directions</returns>
    public override Vector3[] GetApproximationT()
    {
        List<Vector3[]> ap = new List<Vector3[]>(Count);

        int n = 0;
        bool con;
        for (int i = 0; i < Count; i++) {
            Vector3[] p = this[i].GetApproximationT();
            con = NextSplineConnected(i);
            if (con) {
                System.Array.Resize<Vector3>(ref p, p.Length - 1);
            }
            ap.Add(p);
            n += ap[i].Length;
        }

        Vector3[] apps = new Vector3[n];
        n = 0;
        for (int i = 0; i < Count; i++) {
            ap[i].CopyTo(apps, n);
            n += ap[i].Length;
        }

        return apps;
    }
    /// <summary>
    /// Gets an array containing all approximation Up-Vectors
    /// </summary>
    /// <returns>an array of Up-Vectors</returns>
    public override Vector3[] GetApproximationUpVectors()
    {
        List<Vector3[]> ap = new List<Vector3[]>(Count);

        int n = 0;
        bool con;
        for (int i = 0; i < Count; i++) {
            Vector3[] p = this[i].GetApproximationUpVectors();
            con = NextSplineConnected(i);
            if (con) {
                System.Array.Resize<Vector3>(ref p, p.Length - 1);
            }
            ap.Add(p);
            n += ap[i].Length;
        }

        Vector3[] apps = new Vector3[n];
        n = 0;
        for (int i = 0; i < Count; i++) {
            ap[i].CopyTo(apps, n);
            n += ap[i].Length;
        }

        return apps;
    }

    /// <summary>
    /// Gets the TF value that is nearest to p for a given set of segments
    /// </summary>
    /// <param name="p">a point in space</param>
    /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
    /// <returns>a TF value in the range 0..1 or -1 on error</returns>
    public override float GetNearestPointTF(Vector3 p)
    {
        float resTF = -1;
        float maxDist2 = float.MaxValue;
        int res = -1;
        for (int i = 0; i < Count; i++) {
            float tf = this[i].GetNearestPointTF(p);
            float dist2 = (p - this[i].Interpolate(tf)).sqrMagnitude;
            if (dist2 < maxDist2) {
                res = i;
                resTF = tf;
                maxDist2 = dist2;
            }
        }
        return SplineToTF(this[res], resTF);
    }


    /// <summary>
    /// Refreshs the spline group immediately
    /// </summary>
    /// <param name="refreshLength">whether the length should be recalculated</param>
    /// <param name="refreshOrientation">whether the orientation should be recalculated</param>
    /// <param name="skipIfInitialized">True to skip refresh if the spline is initialized</param>
    public override void RefreshImmediately(bool refreshLength, bool refreshOrientation, bool skipIfInitialized)
    {
        _RemoveEmptySplines();
        mLength = 0;
        Distances = new float[Count];
        for (int i = 0; i < Count; i++) {
            this[i].OnRefresh -= OnSplineRefresh;
            this[i].RefreshImmediately(refreshLength,refreshOrientation,skipIfInitialized);
            this[i].OnRefresh += OnSplineRefresh;
            Distances[i] = mLength;
            mLength += this[i].Length;
        }
        OnRefreshEvent(this);
        
        mIsInitialized = Count>0;
        base.RefreshImmediately(refreshLength, refreshOrientation, skipIfInitialized);
    }
    
    

    #endregion

    #region ### Privates & internal Publics ###

    /*! \cond PRIVATE */
    /*! @name Internal Public
     *  Don't use them unless you know what you're doing!
     */
    //@{

    //refresh length only
    void doRefreshLength()
    {
        mLength = 0;
        Distances = new float[Count];
        for (int i = 0; i < Count; i++) {
            Distances[i] = mLength;
            mLength += this[i].Length;
        }
        OnRefreshEvent(this);
    }

    /// <summary>
    /// Gets whether the next spline's (idx+1) start equals current spline's (idx) end
    /// </summary>
    /// <param name="idx">index of the current spline</param>
    /// <returns>true if end and start share the same position</returns>
    bool NextSplineConnected(int idx)
    {
        Mathf.Clamp(idx, 0, Count - 1);
        int n = (idx == Count - 1) ? 0 : idx + 1;
        return (idx < Count-1 && Mathf.Abs((this[idx].InterpolateFast(1) - this[n].InterpolateFast(0)).sqrMagnitude) <= float.Epsilon * float.Epsilon);
    }

    void OnSplineRefresh(CurvySplineBase spl)
    {
        if (!Splines.Contains((CurvySpline)spl)) {
            spl.OnRefresh -= OnSplineRefresh;
            return;
        }
        doRefreshLength();
    }

    public void _RemoveEmptySplines()
    {
        if (Splines.Count>0) {
            for (int i = Splines.Count - 1; i > -1; i--)
                if (Splines[i] == null)
                    Splines.RemoveAt(i);
        }
    }

    int TFToSplineIndex(float tf, out float localTF)
    {
        tf = Mathf.Clamp01(tf);
        localTF = 0;
        if (Count == 0) return -1;
        float f = tf * Count;
        int idx = (int)f;
        localTF = f - idx;
        if (idx == Count) {
            idx--; localTF = 1;
        }

        return idx;
    }

    //@}
    /*! \endcond */
    #endregion
}
