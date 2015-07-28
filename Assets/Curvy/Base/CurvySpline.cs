// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Curvy Spline class
/// </summary>
[ExecuteInEditMode]
public class CurvySpline : CurvySplineBase
{
    public const string Version = "1.51.0";

    #region ### Public Fields and Properties ###
    /// <summary>
    /// The interpolation method used by this spline
    /// </summary>
    public CurvyInterpolation Interpolation = CurvyInterpolation.CatmulRom;
    /// <summary>
    /// Whether this spline is closed or not
    /// </summary>
    public bool Closed;
    /// <summary>
    /// Whether the first/last Control Point should act as the end tangent, too.
    /// </summary>
    /// <remarks>Ignored by linear splines</remarks>
    public bool AutoEndTangents=true;
    /// <summary>
    /// Determines how the splines' initial Up-Vector will be found
    /// </summary>
    /// <remarks>This is needed for tangent orientation</remarks>
    public CurvyInitialUpDefinition InitialUpVector = CurvyInitialUpDefinition.MinAxis;
    /// <summary>
    /// Orientation mode
    /// </summary>
    public CurvyOrientation Orientation = CurvyOrientation.Tangent;
    /// <summary>
    /// If set, Control Point's rotation will be set to the calculated Up-Vector3
    /// </summary>
    /// <remarks>This is particularly useful when connecting splines, but doesn't work with Bezier curves!</remarks>
    public bool SetControlPointRotation = false;
    /// <summary>
    /// Swirling Mode
    /// </summary>
    public CurvyOrientationSwirl Swirl = CurvyOrientationSwirl.None;
    /// <summary>
    /// Turns to swirl
    /// </summary>
    public float SwirlTurns;
    /// <summary>
    /// The Granularity of the approximation
    /// </summary>
    /// <remarks>Granularity determines the approximation points created for each segment. These points are used by
    /// methods and properties working with approximations, like Length, GetNearestTF and all xxxFast-Methods. If the spline uses
    /// linear interpolation, a Granularity of 1 is sufficient.
    /// </remarks>
    public int Granularity = 20;
    /// <summary>
    /// Whether gizmos show the Approximation as well
    /// </summary>
    public bool ShowApproximation;
    /// <summary>
    /// Whether gizmos show the Orientation as well
    /// </summary>
    public bool ShowOrientation=true;
    /// <summary>
    /// Whether gizmos show tangents as well
    /// </summary>
    public bool ShowTangents = false;
    /// <summary>
    /// Whether the spline should automatically refresh when a Control Point's position change
    /// </summary>
    public bool AutoRefresh=true;
    /// <summary>
    /// Whether the length should be recalculated when refreshing the spline
    /// </summary>
    /// <remarks>This is only necessary if you use methods working on Distance or Length</remarks>
    public bool AutoRefreshLength = true;
    /// <summary>
    /// Whether normalized tangents and Up Vectors should be recalculated when refreshing the spline
    /// </summary>
    public bool AutoRefreshOrientation = true;
    /// <summary>
    /// Size of custom values array
    /// </summary>
    public int UserValueSize = 0;
    /// <summary>
    /// Whether user values should be printed in the scene view
    /// </summary>
    public bool ShowUserValues = false;
    /// <summary>
    /// Whether labels should be printed in the scene view
    /// </summary>
    public bool ShowLabels = false;
    /// <summary>
    /// Color used by spline gizmo
    /// </summary>
    public static Color GizmoColor = Color.red;
    /// <summary>
    /// Color used by spline gizmo for selected Control Points / Segments
    /// </summary>
    public static Color GizmoSelectionColor = Color.white;
    /// <summary>
    /// Size of control point gizmos
    /// </summary>
    public static float GizmoControlPointSize = 0.15f;
    /// <summary>
    /// Size of orientation gizmo
    /// </summary>
    public static float GizmoOrientationLength = 1f;
    /// <summary>
    /// Global Tension
    /// </summary>
    /// <remarks>This only applies to TCB interpolation</remarks>
    public float Tension;
    /// <summary>
    /// Global Continuity
    /// </summary>
    /// <remarks>This only applies to TCB interpolation</remarks>
    public float Continuity;
    /// <summary>
    /// Global Bias
    /// </summary>
    /// <remarks>This only applies to TCB interpolation</remarks>
    public float Bias;

    /// <summary>
    /// Gets the number of Segments
    /// </summary>
    public int Count { get { return mSegments.Count; } }
    /// <summary>
    /// Gets the number of Control Points
    /// </summary>
    public int ControlPointCount { get { return mControlPoints.Count; } }
    
    /// <summary>
    /// Gets the Segment at a certain index
    /// </summary>
    /// <param name="idx">an index in the range 0..Count</param>
    /// <returns>the corresponding spline segment</returns>
    public CurvySplineSegment this[int idx]
    {
        get
        {
            return (idx>-1 && idx<mSegments.Count) ? mSegments[idx] :null;
        }
    }

    /// <summary>
    /// Access the list of Segments
    /// </summary>
    public List<CurvySplineSegment> Segments
    {
        get
        {
            return mSegments;
        }
    }

    /// <summary>
    /// Access the list of Control Points
    /// </summary>
    public List<CurvySplineSegment> ControlPoints
    {
       get
       {
           return mControlPoints;
       }
    }

    /// <summary>
    /// To disable gizmo drawing by code
    /// </summary>
    public bool ShowGizmos = true;
    
    #endregion
    
    List<CurvySplineSegment>mControlPoints=new List<CurvySplineSegment>(); // all Controlpoints
    List<CurvySplineSegment> mSegments = new List<CurvySplineSegment>(); // Controlpoints that start a valid spline segment
    float mStepSize;
    
    /// <summary>
    /// Creates an empty spline
    /// </summary>
    public static CurvySpline Create()
    {
        CurvySpline spl = new GameObject("Curvy Spline", typeof(CurvySpline)).GetComponent<CurvySpline>();
        return spl;
    }

    /// <summary>
    /// Creates an empty spline with the same settings as another spline
    /// </summary>
    /// <param name="takeOptionsFrom">another spline</param>
    public static CurvySpline Create(CurvySpline takeOptionsFrom)
    {
        CurvySpline spl = Create();
        if (takeOptionsFrom) {
            spl.Interpolation = takeOptionsFrom.Interpolation;
            spl.Closed = takeOptionsFrom.Closed;
            spl.AutoEndTangents = takeOptionsFrom.AutoEndTangents;
            spl.Granularity = takeOptionsFrom.Granularity;
            spl.Orientation = takeOptionsFrom.Orientation;
            spl.InitialUpVector = takeOptionsFrom.InitialUpVector;
            spl.Swirl = takeOptionsFrom.Swirl;
            spl.SwirlTurns = takeOptionsFrom.SwirlTurns;
            spl.AutoRefresh = takeOptionsFrom.AutoRefresh;
            spl.AutoRefreshLength = takeOptionsFrom.AutoRefreshLength;
            spl.AutoRefreshOrientation = takeOptionsFrom.AutoRefreshOrientation;
            spl.UserValueSize = takeOptionsFrom.UserValueSize;
            spl.ShowApproximation = takeOptionsFrom.ShowApproximation;
            spl.ShowGizmos = takeOptionsFrom.ShowGizmos;
            spl.ShowOrientation = takeOptionsFrom.ShowOrientation;
            spl.ShowTangents = takeOptionsFrom.ShowTangents;
            spl.ShowUserValues = takeOptionsFrom.ShowUserValues;
        }
        return spl;
    }
 
    #region ### Unity Callbacks ###

    void OnEnable()
    {
        if (!Application.isPlaying) 
            Refresh(AutoRefreshLength, AutoRefreshOrientation, false);
    }

    void OnDisable()
    {
        mIsInitialized = false;
    }

    void OnDestroy()
    {
        for (int i = 0; i < ControlPointCount; i++)
            ControlPoints[i].ClearConnections();
    }

    void Start()
    {
        Refresh(AutoRefreshLength, AutoRefreshOrientation, true);
    }

    void Update()
    {
        if (Application.isPlaying && !IsInitialized) {
            RefreshImmediately(AutoRefreshLength, AutoRefreshOrientation, false);
        
        }
        if (!Application.isPlaying || AutoRefresh) {
            bool refresh = false;
            for (int i = 0; i < mControlPoints.Count; i++) {
                if (mControlPoints[i]!=null && (mControlPoints[i]._PositionHasChanged() || mControlPoints[i]._RotationHasChanged())) {
                    refresh = true;
                    //break;
                }
            }

            if (refresh)
                if (!Application.isPlaying)
                    Refresh(true, true,false);
                else
                    Refresh(AutoRefreshLength, AutoRefreshOrientation, false);
        }
        if (mNeedLengthRefresh || mNeedOrientationRefresh)
            RefreshImmediately(mNeedLengthRefresh, mNeedOrientationRefresh, mSkipRefreshIfInitialized);
    }

    #endregion

    #region ### Methods based on TF (total fragment) ###

    /// <summary>
    /// Gets the interpolated position for a certain TF
    /// </summary>
    /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
    /// <param name="tf">TF value identifying position on spline (0..1)</param>
    /// <returns>the interpolated position</returns>
    public override Vector3 Interpolate(float tf)
    {
        return Interpolate(tf, Interpolation);
    }

    /// <summary>
    /// Gets the interpolated position for a certain TF
    /// </summary>
    /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
    /// <param name="tf">TF value identifying position on spline (0..1)</param>
    /// <param name="interpolation">the interpolation to use</param>
    /// <returns>the interpolated position</returns>
    public override Vector3 Interpolate(float tf, CurvyInterpolation interpolation) 
    {
        float localF;
        CurvySplineSegment seg = TFToSegment(tf, out localF);
        return seg.Interpolate(localF, interpolation);
    }

    /// <summary>
    /// Gets the interpolated position for a certain TF using a linear approximation
    /// </summary>
    /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
    /// <param name="tf">TF value reflecting position on spline (0..1)</param>
    /// <returns>the interpolated position</returns>
    public override Vector3 InterpolateFast(float tf)
    {
        float localF;
        CurvySplineSegment seg = TFToSegment(tf, out localF);

        return seg.InterpolateFast(localF);
    }

    /// <summary>
    /// Gets an interpolated User Value for a certain TF
    /// </summary>
    /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
    /// <param name="tf">TF value reflecting position on spline(0..1)</param>
    /// <param name="index">the UserValue array index</param>
    /// <returns>the interpolated value</returns>
    public override Vector3 InterpolateUserValue(float tf, int index)
    {
        float localF;
        CurvySplineSegment seg = TFToSegment(tf, out localF);
        return seg.InterpolateUserValue(localF, index);
    }

    /// <summary>
    /// Gets an interpolated Scale for a certain TF
    /// </summary>
    /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
    /// <param name="tf">TF value reflecting position on spline(0..1)</param>
    /// <returns>the interpolated value</returns>
    public override Vector3 InterpolateScale(float tf)
    {
        float localF;
        CurvySplineSegment seg = TFToSegment(tf, out localF);
        return seg.InterpolateScale(localF);
    }

    /// <summary>
    /// Gets the Up-Vector for a certain TF based on the splines' Orientation mode
    /// </summary>
    /// <param name="tf">TF value reflecting position on spline (0..1)</param>
    /// <returns>the Up-Vector</returns>
    public override Vector3 GetOrientationUpFast(float tf)
    {
        float localF;
        CurvySplineSegment seg = TFToSegment(tf, out localF);

        return seg.GetOrientationUpFast(localF);
    }

    /// <summary>
    /// Gets a rotation looking to Tangent with the head upwards along the Up-Vector
    /// </summary>
    /// <param name="tf">TF value reflecting position on spline (0..1)</param>
    /// <returns>a rotation</returns>
    public override Quaternion GetOrientationFast(float tf)
    {
        float localF;
        CurvySplineSegment seg = TFToSegment(tf, out localF);
        return seg.GetOrientationFast(localF);
    }

    /// <summary>
    /// Gets the normalized tangent for a certain TF
    /// </summary>
    /// <param name="tf">TF value identifying position on spline (0..1)</param>
    /// <returns>a tangent vector</returns>
    public override Vector3 GetTangent(float tf) 
    {
        float localF;
        CurvySplineSegment seg = TFToSegment(tf, out localF);
        return seg.GetTangent(localF);
    }

    /// <summary>
    /// Gets the normalized tangent for a certain TF with a known position for TF
    /// </summary>
    /// <remarks>This saves one interpolation</remarks>
    /// <param name="tf">TF value identifying position on spline (0..1)</param>
    /// <param name="position">The interpolated position for TF</param>
    /// <returns>a tangent vector</returns>
    public override Vector3 GetTangent(float tf, Vector3 position)
    {
        float localF;
        CurvySplineSegment seg = TFToSegment(tf, out localF);
        return seg.GetTangent(localF, ref position);
    }

    /// <summary>
    /// Gets the normalized tangent for a certain TF using a linear approximation
    /// </summary>
    /// <param name="tf">TF value identifying position on spline (0..1)</param>
    /// <returns>a tangent vector</returns>
    public override Vector3 GetTangentFast(float tf)
    {
        float localF;
        CurvySplineSegment seg = TFToSegment(tf, out localF);
        return seg.GetTangentFast(localF);
    }

    /// <summary>
    /// Converts a TF value to a distance
    /// </summary>
    /// <param name="tf">a TF value in the range 0..1</param>
    /// <returns>distance from the first segment's Control Point</returns>
    public override float TFToDistance(float tf) 
    {
        float localF;
        CurvySplineSegment seg= TFToSegment(tf, out localF);
        return seg.Distance + seg.LocalFToDistance(localF);
    }

    /// <summary>
    /// Gets the segment a certan TF lies in
    /// </summary>
    /// <param name="tf"></param>
    /// <returns></returns>
    public CurvySplineSegment TFToSegment(float tf)
    {
        float lF;
        return TFToSegment(tf, out lF);
    }

    /// <summary>
    /// Gets the segment and the local F for a certain TF
    /// </summary>
    /// <param name="tf">the TF value in the range 0..1</param>
    /// <param name="localF">gets the remaining localF in the range 0..1</param>
    /// <returns>the segment the given TF is inside</returns>
    public CurvySplineSegment TFToSegment(float tf, out float localF)
    {
        tf = Mathf.Clamp01(tf);
        localF = 0;
        if (Count == 0) return null;
        float f = tf * Count;
        int idx = (int)f;
        localF = f - idx;
        if (idx == Count) {
            idx--; localF = 1;
        }
        
        return this[idx];
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
        // Get the segment the distance lies within
        CurvySplineSegment seg = DistanceToSegment(distance, out localDistance);
        return (seg) ? SegmentToTF(seg, seg.DistanceToLocalF(localDistance)) : 0;
    }

    /// <summary>
    /// Gets the segment a certain distance lies within
    /// </summary>
    /// <param name="distance">a distance in the range 0..Length</param>
    /// <returns>a spline segment or null</returns>
    public CurvySplineSegment DistanceToSegment(float distance)
    {
        float d;
        return DistanceToSegment(distance, out d);
    }

    /// <summary>
    /// Gets the segment a certain distance lies within
    /// </summary>
    /// <param name="distance">a distance in the range 0..Length</param>
    /// <param name="localDistance">gets the remaining distance inside the segment</param>
    /// <returns>a spline segment</returns>
    public CurvySplineSegment DistanceToSegment(float distance, out float localDistance)
    {
        distance = Mathf.Clamp(distance,0, Length);
        localDistance = 0;
            
        CurvySplineSegment seg = mSegments[0];
        while (seg && seg.Distance+seg.Length < distance) {
            seg = NextSegment(seg);
        }
        if (seg == null)
            seg = this[Count - 1];
        localDistance = distance - seg.Distance;
        return seg;
    }

    #endregion

    #region ### Methods working with Connections ###

    /// <summary>
    /// Gets all connections within a certain F distance from the current position
    /// </summary>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="fDistance">the percentage of the spline to move</param>
    /// <param name="minMatchesNeeded">minimum number of tags that must match to be a valid connection</param>
    /// <param name="skipCurrent">if true ignore any connections at the current position and just look ahead</param>
    /// <param name="tags">list of tags to match</param>
    /// <returns>connections fulfilling all conditions </returns>
    public List<CurvyConnection> GetConnectionsWithin(float tf, int direction, float fDistance, int minMatchesNeeded, bool skipCurrent, params string[] tags)
    {
        List<CurvyConnection> res = new List<CurvyConnection>();
        // get Segments in the range TF => TF+f
        int fromIdx = 0;
        int toIdx = -1;
        float fdelta = fDistance * direction;
        float fromLocalF;
        float toLocalF;
        if (fdelta >= 0) {
            fromIdx = TFToSegment(tf, out fromLocalF).ControlPointIndex;
            toIdx = TFToSegment(tf + fdelta, out toLocalF).ControlPointIndex;
            if (fromLocalF > 0) // don't check a CP we already passed
                fromIdx++;
            if (toLocalF == 1)
                toIdx = Mathf.Min(ControlPointCount - 1, toIdx + 1);

            if (fromIdx == toIdx && (fromLocalF == 0 && skipCurrent)) // from on CP, skip it?
                    return res;
        }
        else {
            fromIdx = TFToSegment(tf + fdelta, out fromLocalF).ControlPointIndex;
            toIdx = TFToSegment(tf, out toLocalF).ControlPointIndex;

            if (fromIdx == toIdx) {
                if (fromLocalF > 0) // not reached cp yet
                    return res;
            }
            else {
                if (fromLocalF > 0)
                    fromIdx++;
                if (toLocalF == 0 && skipCurrent)
                    return res;
            }
        }

        for (int idx = fromIdx; idx <= toIdx; idx++) {
            res.AddRange(this.ControlPoints[idx].GetAllConnections(minMatchesNeeded,tags));
        }
        return res;
    }

    /// <summary>
    /// Alter TF to reflect a movement over a certain portion of the spline, using connections if conditions match
    /// </summary>
    /// <param name="spline">the current spline</param>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="fDistance">the percentage of the spline to move</param>
    /// <param name="clamping">clamping mode</param>
    /// <param name="minMatchesNeeded">minimum number of tags that must match to use a connection</param>
    /// <param name="tags">list of tags to match</param>
    /// <returns>the interpolated position</returns>
    public Vector3 MoveConnection(ref CurvySpline spline, ref float tf, ref int direction, float fDistance, CurvyClamping clamping, int minMatchesNeeded, params string[] tags)
    {
        List<CurvyConnection> cons = GetConnectionsWithin(tf, direction, fDistance, minMatchesNeeded,true, tags);
        if (cons.Count > 0) {
            CurvyConnection con;
            if (cons.Count == 1)
                con = cons[0];
            else
                con = CurvyConnection.GetBestMatchingConnection(cons, tags);
            CurvySplineSegment cp=con.GetFromSpline(this);
            float cptf = SegmentToTF(cp);
            fDistance-= cptf-tf;
            CurvySplineSegment counterp=con.GetCounterpart(cp);
            tf = counterp.LocalFToTF(0);
            spline = counterp.Spline;
            return spline.MoveConnection(ref spline, ref tf, ref direction, fDistance, clamping, minMatchesNeeded, tags);
        }
        else
            return spline.Move(ref tf, ref direction, fDistance, clamping);
     }

    /// <summary>
    /// Alter TF to reflect a movement over a certain portion of the spline, using connections if conditions match. Unlike MoveConnection() a linear approximation will be used
    /// </summary>
    /// <param name="spline">the current spline</param>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="fDistance">the percentage of the spline to move</param>
    /// <param name="clamping">clamping mode</param>
    /// <param name="minMatchesNeeded">minimum number of tags that must match to use a connection</param>
    /// <param name="tags">list of tags to match</param>
    /// <returns>the interpolated position</returns>
    public Vector3 MoveConnectionFast(ref CurvySpline spline, ref float tf, ref int direction, float fDistance, CurvyClamping clamping, int minMatchesNeeded, params string[] tags)
    {
        List<CurvyConnection> cons = GetConnectionsWithin(tf, direction, fDistance, minMatchesNeeded,true, tags);
        if (cons.Count > 0) {
            CurvyConnection con;
            if (cons.Count == 1)
                con = cons[0];
            else
                con = CurvyConnection.GetBestMatchingConnection(cons, tags);
            CurvySplineSegment cp = con.GetFromSpline(this);
            float cptf = SegmentToTF(cp);
            fDistance -= cptf - tf;
            CurvySplineSegment counterp = con.GetCounterpart(cp);
            tf = counterp.LocalFToTF(0);
            spline = counterp.Spline;
            return spline.MoveConnectionFast(ref spline, ref tf, ref direction, fDistance, clamping, minMatchesNeeded, tags);
        }
        else
            return MoveFast(ref tf, ref direction, fDistance, clamping);
    }

    /// <summary>
    /// Alter TF to reflect a movement over a certain distance using a default stepSize of 0.002, using connections if conditions match
    /// </summary>
    /// <param name="spline">the current spline</param>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="distance">the distance in world units to move</param>
    /// <param name="clamping">clamping mode</param>
    /// <param name="minMatchesNeeded">minimum number of tags that must match to use a connection</param>
    /// <param name="tags">list of tags to match</param>
    /// <returns>the interpolated position</returns>
    public Vector3 MoveByConnection(ref CurvySpline spline, ref float tf, ref int direction, float distance, CurvyClamping clamping, int minMatchesNeeded, params string[] tags)
    {
        return MoveByConnection(ref spline, ref tf, ref direction, distance, clamping, minMatchesNeeded, 0.002f, tags);
    }
    
    /// <summary>
    /// Alter TF to reflect a movement over a certain distance, using connections if conditions match
    /// </summary>
    /// <param name="spline">the current spline</param>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="distance">the distance in world units to move</param>
    /// <param name="clamping">clamping mode</param>
    /// <param name="minMatchesNeeded">minimum number of tags that must match to use a connection</param>
    /// <param name="stepSize">stepSize defines the accuracy</param>
    /// <param name="tags">list of tags to match</param>
    /// <returns>the interpolated position</returns>
    public Vector3 MoveByConnection(ref CurvySpline spline, ref float tf, ref int direction, float distance, CurvyClamping clamping, int minMatchesNeeded, float stepSize, params string[] tags)
    {
        return MoveConnection(ref spline, ref tf, ref direction, ExtrapolateDistanceToTF(tf,distance,stepSize), clamping, minMatchesNeeded, tags);
    }

    /// <summary>
    /// Alter TF to reflect a movement over a certain distance using a default stepSize of 0.002, using connections if conditions match.
    /// Unlike MoveByConnection() a linear approximation will be used.
    /// </summary>
    /// <param name="spline">the current spline</param>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="distance">the distance in world units to move</param>
    /// <param name="clamping">clamping mode</param>
    /// <param name="minMatchesNeeded">minimum number of tags that must match to use a connection</param>
    /// <param name="tags">list of tags to match</param>
    /// <returns>the interpolated position</returns>
    public Vector3 MoveByConnectionFast(ref CurvySpline spline, ref float tf, ref int direction, float distance, CurvyClamping clamping, int minMatchesNeeded, params string[] tags)
    {
        return MoveByConnectionFast(ref spline, ref tf, ref direction, distance, clamping, minMatchesNeeded, 0.002f, tags);
    }

    /// <summary>
    /// Alter TF to reflect a movement over a certain distance, using connections if conditions match. Unlike MoveByConnection() a linear approximation will be used.
    /// </summary>
    /// <param name="spline">the current spline</param>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="distance">the distance in world units to move</param>
    /// <param name="clamping">clamping mode</param>
    /// <param name="minMatchesNeeded">minimum number of tags that must match to use a connection</param>
    /// <param name="stepSize">stepSize defines the accuracy</param>
    /// <param name="tags">list of tags to match</param>
    /// <returns>the interpolated position</returns>
    public Vector3 MoveByConnectionFast(ref CurvySpline spline, ref float tf, ref int direction, float distance, CurvyClamping clamping, int minMatchesNeeded, float stepSize, params string[] tags)
    {
        return MoveConnectionFast(ref spline, ref tf, ref direction, ExtrapolateDistanceToTFFast(tf, distance, stepSize), clamping, minMatchesNeeded, tags);
    }

    #endregion

    #region ### General methods ###

    /// <summary>
    /// Adds a Control Point and refreshes the spline
    /// </summary>
    /// <returns>a Control Point</returns>
    public CurvySplineSegment Add() { return Add(null,true); }

    /// <summary>
    /// Adds a Control Point and refreshes the spline
    /// </summary>
    /// <param name="insertAfter">an ancestor Control Point</param>
    /// <returns>a Control Point</returns>
    public CurvySplineSegment Add(CurvySplineSegment insertAfter) { return Add(insertAfter, true); }

    /// <summary>
    /// Adds several Control Points at once and refresh the spline
    /// </summary>
    /// <param name="controlPoints">one or more positions</param>
    /// <returns>an array containing the new Control Points</returns>
    public CurvySplineSegment[] Add(params Vector3[] controlPoints)
    {
        CurvySplineSegment[] cps = new CurvySplineSegment[controlPoints.Length];
        for (int i = 0; i < controlPoints.Length; i++) {
            cps[i] = Add(null, false);
            cps[i].Position = controlPoints[i];
        }
        RefreshImmediately();
        return cps;
    }

    /// <summary>
    /// Adds a Control Point
    /// </summary>
    /// <remarks>If you add several Control Points in a row, just refresh the last one!</remarks>
    /// <param name="insertAfter">an ancestor Control Point</param>
    /// <param name="refresh">whether the spline should be recalculated.</param>
    /// <returns>a Control Point</returns>
    public CurvySplineSegment Add(CurvySplineSegment insertAfter, bool refresh)
    {
        GameObject go = new GameObject("NewCP", typeof(CurvySplineSegment));
        go.transform.parent = transform;
        CurvySplineSegment cp = go.GetComponent<CurvySplineSegment>();
        int idx = mControlPoints.Count;
        if (insertAfter) {
            if (insertAfter.IsValidSegment)
                go.transform.position = insertAfter.Interpolate(0.5f);
            else if (insertAfter.NextTransform)
                go.transform.position = Vector3.Lerp(insertAfter.NextTransform.position, insertAfter.Transform.position, 0.5f);
            
            idx = insertAfter.ControlPointIndex + 1;

        }

        mControlPoints.Insert(idx,cp);
        _RenameControlPointsByIndex();
        _RefreshHierarchy();
        if (refresh)
            RefreshImmediately();

        return cp;
    }

    /// <summary>
    /// Adds a Control Point
    /// </summary>
    /// <remarks>If you add several Control Points in a row, just refresh the last one!</remarks>
    /// <param name="refresh">whether the spline should be recalculated.</param>
    /// <param name="insertBefore">an descendant Control Point</param>
    /// <returns>a Control Point</returns>
    public CurvySplineSegment Add(bool refresh, CurvySplineSegment insertBefore)
    {
        GameObject go = new GameObject("NewCP", typeof(CurvySplineSegment));
        go.transform.parent = transform;
        CurvySplineSegment cp = go.GetComponent<CurvySplineSegment>();
        int idx = 0;
        if (insertBefore) {
            if (insertBefore.PreviousSegment)
                go.transform.position = insertBefore.PreviousSegment.Interpolate(0.5f);
            else
                if (insertBefore.PreviousTransform)
                    go.transform.position = Vector3.Lerp(insertBefore.PreviousTransform.position, insertBefore.Transform.position, 0.5f);
            
            idx = Mathf.Max(0,insertBefore.ControlPointIndex);
        }

        mControlPoints.Insert(idx, cp);
        _RenameControlPointsByIndex();
        _RefreshHierarchy();
        if (refresh)
            RefreshImmediately();

        return cp;
    }

    

    /// <summary>
    /// Removes all control points
    /// </summary>
    public override void Clear()
    {
        foreach (CurvySplineSegment seg in mControlPoints) {
            if (Application.isPlaying)
                Destroy(seg.gameObject);
            else
                DestroyImmediate(seg.gameObject);
        }
        mControlPoints.Clear();
        RefreshImmediately();
    }

    /// <summary>
    /// Deletes a Control Point
    /// </summary>
    /// <param name="controlPoint">a Control Point</param>
    public void Delete(CurvySplineSegment controlPoint) { Delete(controlPoint, true); }

    /// <summary>
    /// Deletes a Control Point
    /// </summary>
    /// <param name="controlPoint">a Control Point</param>
    /// <param name="refreshSpline">whether the spline should refresh</param>
    public void Delete(CurvySplineSegment controlPoint, bool refreshSpline)
    {
        mControlPoints.Remove(controlPoint);
        
        if (Application.isPlaying)
            Destroy(controlPoint.gameObject);
        else
            DestroyImmediate(controlPoint.gameObject);

        _RefreshHierarchy();
        if (refreshSpline)
            RefreshImmediately();
    }

   

    /// <summary>
    /// Gets an array containing all approximation points
    /// </summary>
    /// <remarks>This can be used to feed meshbuilders etc...</remarks>
    /// <returns>an array of world positions</returns>
    public override Vector3[] GetApproximation() { return GetApproximation(false); }
    /// <summary>
    /// Gets an array containing all approximation points
    /// </summary>
    /// <param name="local">get coordinates local to the spline</param>
    /// <remarks>This can be used to feed meshbuilders etc...</remarks>
    /// <returns>an array of world/local positions</returns>
    public override Vector3[] GetApproximation(bool local)
    {
        Vector3[] apps = new Vector3[Count * Granularity+1];
        int idx = 0;
        for (int si = 0; si < Count; si++) {
            this[si].Approximation.CopyTo(apps, idx);
            idx += Mathf.Max(0,this[si].Approximation.Length-1);
        }

        if (local) {
            Matrix4x4 m = Transform.worldToLocalMatrix;
            for (int i = 0; i < apps.Length; i++)
                apps[i] = m.MultiplyPoint3x4(apps[i]);
        }

        return apps;
    }
    
    /// <summary>
    /// Gets an array containing all approximation tangents
    /// </summary>
    /// <remarks>This can be used to feed meshbuilders etc...</remarks>
    /// <returns>an array of tangents</returns>
    public override Vector3[] GetApproximationT()
    {
        Vector3[] apps = new Vector3[Count * Granularity+1];
        int idx = 0;
        for (int si = 0; si < Count; si++) {
            this[si].ApproximationT.CopyTo(apps, idx);
            idx += Mathf.Max(0, this[si].ApproximationT.Length - 1);
        }
        return apps;
    }

    /// <summary>
    /// Gets an array containing all approximation Up-Vectors
    /// </summary>
    /// <remarks>This can be used to feed meshbuilders etc...</remarks>
    /// <returns>an array of Up-Vectors</returns>
    public override Vector3[] GetApproximationUpVectors()
    {
        Vector3[] apps = new Vector3[Count * Granularity+1];
        int idx = 0;
        for (int si = 0; si < Count; si++) {
            this[si].ApproximationUp.CopyTo(apps, idx);
            idx += Mathf.Max(0, this[si].ApproximationUp.Length - 1);
        }
        return apps;
    }

    /// <summary>
    /// Gets the TF value that is nearest to p
    /// </summary>
    /// <param name="p">a point in space</param>
    /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
    /// <returns>a TF value in the range 0..1 or -1 on error</returns>
    public override float GetNearestPointTF(Vector3 p)
    {
        return GetNearestPointTFExt(p,Segments.ToArray());
    }

    /// <summary>
    /// Gets the TF value that is nearest to p for a given set of segments
    /// </summary>
    /// <param name="p">a point in space</param>
    /// <param name="segmentsToCheck">the segments to check</param>
    /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
    /// <returns>a TF value in the range 0..1 or -1 on error</returns>
    public float GetNearestPointTFExt(Vector3 p, params CurvySplineSegment[] segmentsToCheck)
    {
        if (Count == 0 || segmentsToCheck.Length==0) return -1;
        
        float dist = float.MaxValue;
        int foundIndex=0;
        CurvySplineSegment foundSeg=null;
        float cur;

        for (int i = 0; i < segmentsToCheck.Length; i++) {
            for (int ap = 0; ap < segmentsToCheck[i].Approximation.Length; ap++) {
                cur = (segmentsToCheck[i].Approximation[ap] - p).sqrMagnitude;
                if (cur < dist) {
                    dist = cur;
                    foundIndex = ap;
                    foundSeg = segmentsToCheck[i];
                }
            }
        }

        // foundIndex is the nearest mApproximation point, check against the two lines this index belongs to
        //return foundSeg.LocalFToTF(foundSeg.getApproximationLocalF(foundIndex));
        
        CurvySplineSegment[] pseg=new CurvySplineSegment[3];
        int[] pidx = new int[3];
        pseg[1] = foundSeg; pidx[1] = foundIndex;
        if (!getPreviousApproximationPoint(foundSeg, foundIndex, out pseg[0], out pidx[0],ref segmentsToCheck)){
            
            pseg[0] = pseg[1]; pidx[0] = pidx[1];
        }
        if (!getNextApproximationPoint(foundSeg, foundIndex, out pseg[2], out pidx[2], ref segmentsToCheck)) {

            pseg[2] = pseg[1]; pidx[2] = pidx[1];
        }

        float[] frag=new float[2];
        float[] ldist=new float[2];
        ldist[0] = LinePointDistanceSqr(pseg[0].Approximation[pidx[0]], pseg[1].Approximation[pidx[1]], p, out frag[0]);
        ldist[1] = LinePointDistanceSqr(pseg[1].Approximation[pidx[1]], pseg[2].Approximation[pidx[2]], p, out frag[1]);
        if (ldist[0] < ldist[1]) {
            return pseg[0].LocalFToTF(pseg[0]._getApproximationLocalF(pidx[0]) + frag[0] * mStepSize);
        }
        else {
            return pseg[1].LocalFToTF(pseg[1]._getApproximationLocalF(pidx[1]) + frag[1] * mStepSize);
        }
    }

    /// <summary>
    /// Gets the next Control Point of a certain Control Point
    /// </summary>
    /// <param name="controlpoint">a Control Point</param>
    /// <returns>a Control Point or null</returns>
    public CurvySplineSegment NextControlPoint(CurvySplineSegment controlpoint)
    {
        if (mControlPoints.Count == 0) return null;

        int i = controlpoint.ControlPointIndex + 1;
        if (i >= mControlPoints.Count) {
            if (Closed)
                return mControlPoints[0];
            else
                return null;
        }
        else
            return mControlPoints[i];
    }

    /// <summary>
    /// Gets the previous Control Point of a certain Control Point
    /// </summary>
    /// <param name="controlpoint">a Control Point</param>
    /// <returns>a Control Point or null</returns>
    public CurvySplineSegment PreviousControlPoint(CurvySplineSegment controlpoint)
    {
        if (mControlPoints.Count == 0) return null;

        int i = controlpoint.ControlPointIndex - 1;
        if (i < 0) {
            if (Closed)
                return mControlPoints[mControlPoints.Count - 1];
            else
                return null;
        }
        else
            return mControlPoints[i];
    }

    /// <summary>
    /// Gets the next segment of a certain segment
    /// </summary>
    /// <param name="segment">a segment</param>
    /// <returns>a segment or null</returns>
    public CurvySplineSegment NextSegment(CurvySplineSegment segment)
    {
        if (mSegments.Count == 0 || !segment.IsValidSegment) return null;

        int i = segment.SegmentIndex + 1;
        if (i >= mSegments.Count)
            return (Closed) ? mSegments[0] : null;
        else
            return mSegments[i];
    }

    /// <summary>
    /// Gets the previous segment of a certain segment
    /// </summary>
    /// <param name="segment">a segment</param>
    /// <returns>a segment or null</returns>
    public CurvySplineSegment PreviousSegment(CurvySplineSegment segment)
    {
        if (mSegments.Count == 0) return null;
        if (segment.SegmentIndex==-1) {
            int cp = segment.ControlPointIndex - 1;
            if (cp >= 0 && mControlPoints[cp].SegmentIndex > -1)
                return mControlPoints[cp];
            else
                return null;
        }
        int i = segment.SegmentIndex - 1;
        if (i < 0)
            return (Closed) ? mSegments[mSegments.Count - 1] : null;
        else
            return mSegments[i];
    }

    /// <summary>
    /// Gets the next Transform of a certain Control Point
    /// </summary>
    /// <param name="controlpoint">a Control Point </param>
    /// <returns>a Transform or Null</returns>
    public Transform NextTransform(CurvySplineSegment controlpoint)
    {
        CurvySplineSegment seg = NextControlPoint(controlpoint);
        if (seg)
            return seg.Transform;
        else
            if (AutoEndTangents && Interpolation != CurvyInterpolation.Linear && Interpolation!= CurvyInterpolation.Bezier) 
                return controlpoint.Transform;

        return null;
    }

    /// <summary>
    /// Gets the previous Transform of a certain Control Point
    /// </summary>
    /// <param name="controlpoint">a Control Point </param>
    /// <returns>a Transform or Null</returns>
    public Transform PreviousTransform(CurvySplineSegment controlpoint)
    {
        CurvySplineSegment seg = PreviousControlPoint(controlpoint);
        if (seg)
            return seg.Transform;
        else
            if (AutoEndTangents && Interpolation != CurvyInterpolation.Linear && Interpolation!= CurvyInterpolation.Bezier) 
                return controlpoint.Transform;
        return null;
    }

   

    /// <summary>
    /// Refreshs the spline at the next call to Update()
    /// </summary>
    /// <remarks>When AutoRefresh is disabled, use this after moving Control Points or changing spline parameters.</remarks>
    public override void Refresh()
    {
        Refresh(AutoRefreshLength, AutoRefreshOrientation, false);
    }

    /// <summary>
    /// Refreshs the spline at the next call to Update()
    /// </summary>
    /// <remarks>When AutoRefresh is disabled, use this after moving Control Points or changing spline parameters.</remarks>
    public override void RefreshImmediately()
    {
        RefreshImmediately(AutoRefreshLength, AutoRefreshOrientation, false);
    }

    /// <summary>
    /// Refreshs/Updates the spline or spline group immediately
    /// </summary>
    /// <param name="refreshLength">whether the length should be recalculated</param>
    /// <param name="refreshOrientation">whether the orientation should be recalculated</param>
    /// <param name="skipIfInitialized">True to skip refresh if the spline is initialized</param>
    public override void RefreshImmediately(bool refreshLength, bool refreshOrientation, bool skipIfInitialized)
    {
        if (skipIfInitialized && IsInitialized) {
            base.RefreshImmediately(refreshLength, refreshOrientation, skipIfInitialized); // reset flags
            return;
        }
        
        mStepSize = 1f/Granularity;
        UserValueSize = Mathf.Max(0, UserValueSize);
        mSegments.Clear();

        _RefreshHierarchy();
        
        for (int i = 0; i < mControlPoints.Count; i++) {
            if (mControlPoints[i].IsValidSegment) {
                mSegments.Add(mControlPoints[i]);
                mControlPoints[i]._UpdateApproximation();
            }
        }

        if (refreshOrientation) {
            doRefreshTangents();
            doRefreshOrientation();
            // refresh orientation of connected splines in the editor            
            if (!Application.isPlaying && ControlPointCount>0 && ControlPoints[ControlPointCount - 1].ConnectedBy.Count > 0) {
                ControlPoints[ControlPointCount - 1].ConnectedBy[0].Spline.Refresh(false, true, false);
            }
        }

        if (refreshLength)
            doRefreshLength();

        mIsInitialized = true;
        OnRefreshEvent(this);
        base.RefreshImmediately(refreshLength, refreshOrientation, skipIfInitialized); // reset flags
    }
    
    /// <summary>
    /// Gets a TF value from a segment
    /// </summary>
    /// <param name="segment">a segment</param>
    /// <returns>a TF value in the range 0..1</returns>
    public float SegmentToTF(CurvySplineSegment segment) { return SegmentToTF(segment, 0); }

    /// <summary>
    /// Gets a TF value from a segment and a local F
    /// </summary>
    /// <param name="segment">a segment</param>
    /// <param name="localF">F of this segment in the range 0..1</param>
    /// <returns>a TF value in the range 0..1</returns>
    public float SegmentToTF(CurvySplineSegment segment, float localF)
    {
        if (!segment) return 0;
        if (segment.SegmentIndex == -1)
            return (segment.ControlPointIndex > 0) ? 1 : 0;
        return ((float)segment.SegmentIndex / Count) + (1f / Count) * localF;
    }

    

    /// <summary>
    /// Gets the point at a certain radius and angle on the plane defined by P and it's Tangent
    /// </summary>
    /// <param name="P">a point on the spline or spline group</param>
    /// <param name="Tangent">the normalized tangent of this point</param>
    /// <param name="Up">the Up-Vector of the point</param>
    /// <param name="radius">radius in world units</param>
    /// <param name="angle">angle in degrees</param>
    /// <returns>the extruded point</returns>
    [System.Obsolete("Use GetExtrusionPoint(tf,radius,angle) instead")]
    public Vector3 GetExtrusionPoint(Vector3 P, Vector3 Tangent, Vector3 Up, float radius, float angle)
    {
        Quaternion R = Quaternion.AngleAxis(angle, Tangent);
        return P + (R * Up) * radius;
    }


    #endregion

    #region ### Interpolation methods ###

    /// <summary>
    /// Cubic-Beziere Interpolation
    /// </summary>
    /// <param name="T0">HandleIn</param>
    /// <param name="P0">Pn</param>
    /// <param name="P1">Pn+1</param>
    /// <param name="T1">HandleOut</param>
    /// <param name="f">f in the range 0..1</param>
    /// <returns></returns>
    public static Vector3 Bezier(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
    {
        double Ft2=3; double Ft3=-3;
        double Fu1=3; double Fu2=-6; double Fu3=3;
        double Fv1=-3; double Fv2=3; 

        double FAX = -P0.x + Ft2 * T0.x + Ft3 * T1.x + P1.x;
        double FBX = Fu1 * P0.x + Fu2 * T0.x + Fu3 * T1.x;
        double FCX = Fv1 * P0.x + Fv2 * T0.x;
        double FDX = P0.x;

        double FAY = -P0.y + Ft2 * T0.y + Ft3 * T1.y + P1.y;
        double FBY = Fu1 * P0.y + Fu2 * T0.y + Fu3 * T1.y;
        double FCY = Fv1 * P0.y + Fv2 * T0.y;
        double FDY = P0.y;

        double FAZ = -P0.z + Ft2 * T0.z + Ft3 * T1.z + P1.z;
        double FBZ = Fu1 * P0.z + Fu2 * T0.z + Fu3 * T1.z;
        double FCZ = Fv1 * P0.z + Fv2 * T0.z;
        double FDZ = P0.z;
        
        float FX = (float)(((FAX * f + FBX) * f + FCX) * f + FDX);
        float FY = (float)(((FAY * f + FBY) * f + FCY) * f + FDY);
        float FZ = (float)(((FAZ * f + FBZ) * f + FCZ) * f + FDZ);

        return new Vector3(FX, FY, FZ);
    }

    /// <summary>
    /// Catmul-Rom Interpolation
    /// </summary>
    /// <param name="T0">Pn-1 (In Tangent)</param>
    /// <param name="P0">Pn</param>
    /// <param name="P1">Pn+1</param>
    /// <param name="T1">Pn+2 (Out Tangent)</param>
    /// <param name="f">f in the range 0..1</param>
    /// <returns>the interpolated position</returns>
    public static Vector3 CatmulRom(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
    {
        double Ft1 = -0.5; double Ft2 = 1.5; double Ft3 = -1.5; double Ft4 = 0.5;
        double Fu2 = -2.5; double Fu3 = 2; double Fu4 = -0.5;
        double Fv1 = -0.5; double Fv3 = 0.5;

        double FAX = Ft1 * T0.x + Ft2 * P0.x + Ft3 * P1.x + Ft4 * T1.x;
        double FBX = T0.x + Fu2 * P0.x + Fu3 * P1.x + Fu4 * T1.x;
        double FCX = Fv1 * T0.x + Fv3 * P1.x;
        double FDX = P0.x;

        double FAY = Ft1 * T0.y + Ft2 * P0.y + Ft3 * P1.y + Ft4 * T1.y;
        double FBY = T0.y + Fu2 * P0.y + Fu3 * P1.y + Fu4 * T1.y;
        double FCY = Fv1 * T0.y + Fv3 * P1.y;
        double FDY = P0.y;

        double FAZ = Ft1 * T0.z + Ft2 * P0.z + Ft3 * P1.z + Ft4 * T1.z;
        double FBZ = T0.z + Fu2 * P0.z + Fu3 * P1.z + Fu4 * T1.z;
        double FCZ = Fv1 * T0.z + Fv3 * P1.z;
        double FDZ = P0.z;

        float FX = (float)(((FAX * f + FBX) * f + FCX) * f + FDX);
        float FY = (float)(((FAY * f + FBY) * f + FCY) * f + FDY);
        float FZ = (float)(((FAZ * f + FBZ) * f + FCZ) * f + FDZ);

        return new Vector3(FX, FY, FZ);
    }

    /// <summary>
    /// Kochanek-Bartels/TCB-Interpolation
    /// </summary>
    /// <param name="T0">Pn-1 (In Tangent)</param>
    /// <param name="P0">Pn</param>
    /// <param name="P1">Pn+1</param>
    /// <param name="T1">Pn+2 (Out Tangent)</param>
    /// <param name="f">f in the range 0..1</param>
    /// <param name="FT0">Start Tension</param>
    /// <param name="FC0">Start Continuity</param>
    /// <param name="FB0">Start Bias</param>
    /// <param name="FT1">End Tension</param>
    /// <param name="FC1">End Continuity</param>
    /// <param name="FB1">End Bias</param>
    /// <returns>the interpolated position</returns>
    public static Vector3 TCB(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f, float FT0, float FC0, float FB0, float FT1, float FC1, float FB1)
    {
        double FFA = (1 - FT0) * (1 + FC0) * (1 + FB0);
        double FFB = (1 - FT0) * (1 - FC0) * (1 - FB0);
        double FFC = (1 - FT1) * (1 - FC1) * (1 + FB1);
        double FFD = (1 - FT1) * (1 + FC1) * (1 - FB1);

        double DD = 2;
        double Ft1 = -FFA / DD; double Ft2 = (+4 + FFA - FFB - FFC) / DD; double Ft3 = (-4 + FFB + FFC - FFD) / DD; double Ft4 = FFD / DD;
        double Fu1 = +2 * FFA / DD; double Fu2 = (-6 - 2 * FFA + 2 * FFB + FFC) / DD; double Fu3 = (+6 - 2 * FFB - FFC + FFD) / DD; double Fu4 = -FFD / DD;
        double Fv1 = -FFA / DD; double Fv2 = (FFA - FFB) / DD; double Fv3 = FFB / DD; 
        double Fw2 = +2 / DD; 

        double FAX = Ft1 * T0.x + Ft2 * P0.x + Ft3 * P1.x + Ft4 * T1.x;
        double FBX = Fu1 * T0.x + Fu2 * P0.x + Fu3 * P1.x + Fu4 * T1.x;
        double FCX = Fv1 * T0.x + Fv2 * P0.x + Fv3 * P1.x;
        double FDX = Fw2 * P0.x;

        double FAY = Ft1 * T0.y + Ft2 * P0.y + Ft3 * P1.y + Ft4 * T1.y;
        double FBY = Fu1 * T0.y + Fu2 * P0.y + Fu3 * P1.y + Fu4 * T1.y;
        double FCY = Fv1 * T0.y + Fv2 * P0.y + Fv3 * P1.y;
        double FDY = Fw2 * P0.y;

        double FAZ = Ft1 * T0.z + Ft2 * P0.z + Ft3 * P1.z + Ft4 * T1.z;
        double FBZ = Fu1 * T0.z + Fu2 * P0.z + Fu3 * P1.z + Fu4 * T1.z;
        double FCZ = Fv1 * T0.z + Fv2 * P0.z + Fv3 * P1.z;
        double FDZ = Fw2 * P0.z;

        float FX = (float)(((FAX * f + FBX) * f + FCX) * f + FDX);
        float FY = (float)(((FAY * f + FBY) * f + FCY) * f + FDY);
        float FZ = (float)(((FAZ * f + FBZ) * f + FCZ) * f + FDZ);

        return new Vector3(FX, FY, FZ);
    }

    

    #endregion

    #region ### Privates & internal Publics ###
    /*! \cond PRIVATE */
    /*! @name Internal Public
     *  Don't use them unless you know what you're doing!
     */
    //@{

    /// <summary>
    /// Recalculate spline length
    /// </summary>
    /// <remarks>The accuracy depends on Granulartiy</remarks>
    void doRefreshLength()
    {
        mLength = 0;
        for (int i = 0; i < Count; i++)
            mLength += this[i]._UpdateLength();
    }

    /// <summary>
    /// Recalculate normalized tangents
    /// </summary>
    void doRefreshTangents()
    {
        for (int i = 0; i < Count; i++)
            this[i]._UpdateTangents();
    }

    /// <summary>
    /// Recalculate Up-Vectors
    /// </summary>
    void doRefreshOrientation()
    {
        if (Count > 0) {
            Vector3 LastUp;
            switch (InitialUpVector) {
                case CurvyInitialUpDefinition.MinAxis:
                    LastUp=minAxis(this[0].GetTangent(0));
                    break;
                default:
                    LastUp = this[0].Transform.up;
                    break;
            }
            
            //Vector3.OrthoNormalize(ref this[0].ApproximationT[0], ref LastUp);
            for (int i = 0; i < Count; i++) {
                LastUp = this[i]._UpdateOrientation(LastUp);
                if (SetControlPointRotation &&Orientation == CurvyOrientation.Tangent)
                    this[i].Transform.up = this[i].ApproximationUp[0];
            }
            if (!Closed && SetControlPointRotation && Orientation == CurvyOrientation.Tangent && this[Count - 1].NextControlPoint) 
                this[Count - 1].NextControlPoint.Transform.up = this[Count - 1].ApproximationUp[Granularity];
            

            // Make PTFrames seamless
            if (Closed && Orientation == CurvyOrientation.Tangent) {
                float smoothingAngle = AngleSigned(this[Count - 1].ApproximationUp[Granularity - 1], this[0].ApproximationUp[0],this[0].ApproximationT[0]) / (Count * Granularity);
                float angle = smoothingAngle;
                LastUp = this[0].ApproximationUp[0];
                for (int i = 0; i < Count; i++) {
                    LastUp = this[i]._SmoothOrientation(LastUp, ref angle,smoothingAngle);
                }
                // last==first
                this[Count - 1].ApproximationUp[Granularity] = this[0].ApproximationUp[0];
            }
        }
    }

    float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
    {
        return Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)),Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }


    public void _RefreshHierarchy()
    {
        if (ControlPointCount == 0) {
            mControlPoints = new List<CurvySplineSegment>(transform.childCount);
            int cc=Transform.childCount;
            for (int i = 0; i < cc; i++) {
                CurvySplineSegment cp = transform.GetChild(i).GetComponent<CurvySplineSegment>();
                if (cp) {
                    cp._ReSettle();
                    mControlPoints.Add(cp);
                }
            }
        }
        for (int i = mControlPoints.Count - 1; i > -1; i--)
            if (mControlPoints[i] == null)
                mControlPoints.RemoveAt(i);

        mControlPoints.Sort((a, b) => string.Compare(a.name, b.name));
        
        for (int i = 0; i < mControlPoints.Count; i++) {
            mControlPoints[i]._InitializeControlPoint();
        }
    }

    public void _RenameControlPointsByIndex()
    {
        for (int i = 0; i < mControlPoints.Count; i++) {
            if (mControlPoints[i] != null) {
                mControlPoints[i].name = "CP" + string.Format("{0:000}", i);
                mControlPoints[i]._InitializeControlPoint();
            }
        }
    }
   
    Vector3 minAxis(Vector3 v)
    {
        return v.x < v.y ? (v.x < v.z ? new Vector3(1, 0, 0) : new Vector3(0, 0, 1)) : (v.y < v.z ? new Vector3(0, 1, 0) : new Vector3(0, 0, 1));
    }


    Vector3 GetUpVector(Vector3 P, Vector3 P1, Vector3 LastUp)
    {
        Vector3 T = (P - P1).normalized;
        Vector3 B = Vector3.Cross(T, LastUp).normalized;
        return Vector3.Cross(B, T).normalized;
    }

    int ApproximationPointCount { get { return Count * (Granularity + 1); } }

    bool getPreviousApproximationPoint(CurvySplineSegment seg, int idx, out CurvySplineSegment res, out int residx, ref CurvySplineSegment[] validSegments)
    {
        residx = idx - 1;
        res = seg;
        if (residx <0) {
            res = PreviousSegment(seg);
            if (res) {
                residx = res.Approximation.Length - 2;
                for (int i = 0; i < validSegments.Length; i++)
                    if (validSegments[i] == res)
                        return true;
                return false;
            }
            return (false);
        }
        return true;
    }

    bool getNextApproximationPoint(CurvySplineSegment seg, int idx, out CurvySplineSegment res, out int residx, ref CurvySplineSegment[] validSegments)
    {
        residx = idx + 1;
        res = seg;
        if (residx == seg.Approximation.Length) {
            res = NextSegment(seg);
            if (res) {
                residx = 1;
                for (int i = 0; i < validSegments.Length; i++)
                    if (validSegments[i] == res)
                        return true;
            }
            return false;
        }
        return true;
    }

    bool iterateApproximationPoints(ref CurvySplineSegment seg, ref int idx)
    {
        idx++;
        if (idx == seg.Approximation.Length) {
            seg = NextSegment(seg);
            idx = 1;
            return (seg != null && !seg.IsFirstSegment);
        }
        return true;
    }

    float LinePointDistanceSqr(Vector3 l1, Vector3 l2, Vector3 p, out float frag)
    {
        Vector3 v = l2 - l1;
        Vector3 w = p - l1;
        float c1 = Vector3.Dot(w, v);
        if (c1 <= 0) {
            frag = 0;
            return (p - l1).sqrMagnitude;
        }
        float c2 = Vector3.Dot(v, v);
        if (c2 <= c1) {
            frag = 1;
            return (p - l2).sqrMagnitude;
        }
        frag=c1 / c2;
        Vector3 pb = l1 + frag * v;
        return (p - pb).sqrMagnitude;
    }
    //@}
    /*! \endcond */
    #endregion

}

/// <summary>
/// Determines the interpolation method
/// </summary>
public enum CurvyInterpolation
{
    /// <summary>
    ///  Linear interpolation
    /// </summary>
    Linear = 0,
    /// <summary>
    /// Catmul-Rom splines
    /// </summary>
    CatmulRom = 1,
    /// <summary>
    /// Kochanek-Bartels (TCB)-Splines
    /// </summary>
    TCB = 2,
    /// <summary>
    /// Cubic Bezier-Splines
    /// </summary>
    Bezier = 3
}

/// <summary>
/// Determines the clamping method used by Move-methods
/// </summary>
public enum CurvyClamping
{
    /// <summary>
    /// Stop at splines ends
    /// </summary>
    Clamp =0,
    /// <summary>
    /// Start over
    /// </summary>
    Loop = 1,
    /// <summary>
    /// Switch direction
    /// </summary>
    PingPong = 2
}

/// <summary>
/// Determines Orientation mode
/// </summary>
public enum CurvyOrientation
{
    /// <summary>
    /// Ignore rotation
    /// </summary>
    None=0,
    /// <summary>
    /// Use the splines' tangent and up vectors to create a look rotation 
    /// </summary>
    Tangent=1,
    /// <summary>
    /// Interpolate between the Control Point's rotation
    /// </summary>
    ControlPoints=2
}

public enum CurvyOrientationSwirl
{
    /// <summary>
    /// No Swirl
    /// </summary>
    None=0,
    /// <summary>
    /// Swirl per segment
    /// </summary>
    Segment=1,
    /// <summary>
    /// Swirl equal over spline's segments
    /// </summary>
    Spline=2
}

/// <summary>
/// How to define the first Up-Vector
/// </summary>
public enum CurvyInitialUpDefinition
{
    /// <summary>
    /// Use the Up-Vector of the first segments' Control Point to define splines' Up-Vector
    /// </summary>
    ControlPoint = 0,
    /// <summary>
    /// Use the nearest axis to define splines' Up-Vector
    /// </summary>
    MinAxis = 1,
}

