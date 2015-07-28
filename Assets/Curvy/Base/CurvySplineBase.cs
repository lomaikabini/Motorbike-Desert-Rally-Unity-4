// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Base class both CurvySpline and CurvySplineGroup derive from
/// </summary>
public class CurvySplineBase : MonoBehaviour
{
    public delegate void RefreshEvent(CurvySplineBase sender);
    /// <summary>
    /// Callback when the spline/group refreshes
    /// </summary>
    public event RefreshEvent OnRefresh;

    protected bool mNeedLengthRefresh;
    protected bool mNeedOrientationRefresh;
    protected bool mSkipRefreshIfInitialized;

    protected void OnRefreshEvent(CurvySplineBase sender)
    {
        if (OnRefresh != null) {
            OnRefresh(sender);
        }
    }

    /// <summary>
    /// Gets transform
    /// </summary>
    public Transform Transform
    {
        get
        {
            if (!mTransform)
                mTransform = transform;
            return mTransform;
        }
    }

    /// <summary>
    /// Whether the spline is fully initialized and all segments loaded
    /// </summary>
    public bool IsInitialized { get { return mIsInitialized; } }
    /// <summary>
    /// Gets the total length of the Spline or SplineGroup
    /// </summary>
    /// <remarks>The accuracy depends on the current Granularity (higher Granularity means more exact values)</remarks>
    public float Length { get { return mLength; } }

    #region ### Virtual Methods implemented in derived classes ###

    /// <summary>
    /// Gets the interpolated position for a certain TF
    /// </summary>
    /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
    /// <param name="tf">TF value identifying position on spline (0..1)</param>
    /// <returns>the interpolated position</returns>
    public virtual Vector3 Interpolate(float tf) { return Vector3.zero; }
    /// <summary>
    /// Gets the interpolated position for a certain TF
    /// </summary>
    /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
    /// <param name="tf">TF value identifying position on spline (0..1)</param>
    /// <param name="interpolation">the interpolation to use</param>
    /// <returns>the interpolated position</returns>
    public virtual Vector3 Interpolate(float tf, CurvyInterpolation interpolation) { return Vector3.zero; }
    /// <summary>
    /// Gets the interpolated position for a certain TF using a linear approximation
    /// </summary>
    /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
    /// <param name="tf">TF value reflecting position on spline (0..1)</param>
    /// <returns>the interpolated position</returns>
    public virtual Vector3 InterpolateFast(float tf) { return Vector3.zero; }
    /// <summary>
    /// Gets an interpolated User Value for a certain TF
    /// </summary>
    /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
    /// <param name="tf">TF value reflecting position on spline(0..1)</param>
    /// <param name="index">the UserValue array index</param>
    /// <returns>the interpolated value</returns>
    public virtual Vector3 InterpolateUserValue(float tf, int index) { return Vector3.zero; }
    /// <summary>
    /// Gets an interpolated Scale for a certain TF
    /// </summary>
    /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
    /// <param name="tf">TF value reflecting position on spline(0..1)</param>
    /// <returns>the interpolated value</returns>
    public virtual Vector3 InterpolateScale(float tf) { return Vector3.zero; }
    /// <summary>
    /// Gets the Up-Vector for a certain TF based on the splines' Orientation mode
    /// </summary>
    /// <param name="tf">TF value reflecting position on spline (0..1)</param>
    /// <returns>the Up-Vector</returns>
    public virtual Vector3 GetOrientationUpFast(float tf) { return Vector3.zero; }
    /// <summary>
    /// Gets a rotation looking to Tangent with the head upwards along the Up-Vector
    /// </summary>
    /// <param name="tf">TF value reflecting position on spline (0..1)</param>
    /// <returns>a rotation</returns>
    public virtual Quaternion GetOrientationFast(float tf) { return Quaternion.identity; }
    /// <summary>
    /// Gets the normalized tangent for a certain TF
    /// </summary>
    /// <param name="tf">TF value identifying position on spline (0..1)</param>
    /// <returns>a tangent vector</returns>
    public virtual Vector3 GetTangent(float tf) { return Vector3.zero; }
    /// <summary>
    /// Gets the normalized tangent for a certain TF with a known position for TF
    /// </summary>
    /// <remarks>This saves one interpolation</remarks>
    /// <param name="tf">TF value identifying position on spline (0..1)</param>
    /// <param name="position">The interpolated position for TF</param>
    /// <returns>a tangent vector</returns>
    public virtual Vector3 GetTangent(float tf, Vector3 position) { return Vector3.zero; }
    /// <summary>
    /// Gets the normalized tangent for a certain TF using a linear approximation
    /// </summary>
    /// <param name="tf">TF value identifying position on spline (0..1)</param>
    /// <returns>a tangent vector</returns>
    public virtual Vector3 GetTangentFast(float tf) { return Vector3.zero; }
    
    /// <summary>
    /// Converts a TF value to a distance
    /// </summary>
    /// <param name="tf">a TF value in the range 0..1</param>
    /// <returns>distance from the first segment's Control Point</returns>
    public virtual float TFToDistance(float tf) { return 0;}
    /// <summary>
    /// Converts a distance to a TF value
    /// </summary>
    /// <param name="distance">distance in the range 0..Length</param>
    /// <returns>a TF value in the range 0..1</returns>
    public virtual float DistanceToTF(float distance) { return 0; }
    
    /// <summary>
    /// Gets the TF value that is nearest to p
    /// </summary>
    /// <param name="p">a point in space</param>
    /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
    /// <returns>a TF value in the range 0..1 or -1 on error</returns>
    public virtual float GetNearestPointTF(Vector3 p) { return 0; }
    /// <summary>
    /// Removes all control points
    /// </summary>
    public virtual void Clear() { }
    /// <summary>
    /// Gets an array containing all approximation points
    /// </summary>
    /// <remarks>This can be used to feed meshbuilders etc...</remarks>
    /// <returns>an array of world positions</returns>
    public virtual Vector3[] GetApproximation() { return GetApproximation(false); }
    /// <summary>
    /// Gets an array containing all approximation points
    /// </summary>
    /// <param name="local">get coordinates local to the spline</param>
    /// <remarks>This can be used to feed meshbuilders etc...</remarks>
    /// <returns>an array of world/local positions</returns>
    public virtual Vector3[] GetApproximation(bool local) {return new Vector3[0];}
    /// <summary>
    /// Gets an array containing all approximation tangents
    /// </summary>
    /// <remarks>This can be used to feed meshbuilders etc...</remarks>
    /// <returns>an array of tangents</returns>
    public virtual Vector3[] GetApproximationT() { return new Vector3[0];}
    /// <summary>
    /// Gets an array containing all approximation Up-Vectors
    /// </summary>
    /// <remarks>This can be used to feed meshbuilders etc...</remarks>
    /// <returns>an array of Up-Vectors</returns>
    public virtual Vector3[] GetApproximationUpVectors(){ return new Vector3[0];}

    #endregion

    protected float mLength;
    protected Transform mTransform;
    protected bool mIsInitialized;

    #region ### Methods based on TF ###

    /// <summary>
    /// Alter TF to reflect a movement over a certain portion of the spline/group
    /// </summary>
    /// <remarks>fDistance relates to the total spline, so longer splines will result in faster movement for constant fDistance</remarks>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="fDistance">the percentage of the spline/group to move</param>
    /// <param name="clamping">clamping mode</param>
    /// <returns>the interpolated position</returns>
    public virtual Vector3 Move(ref float tf, ref int direction, float fDistance, CurvyClamping clamping)
    {
        tf += fDistance * direction;
        ClampTF(ref tf, ref direction, clamping);
        return Interpolate(tf);
    }

    /// <summary>
    /// Alter TF to reflect a movement over a certain portion of the spline/group, respecting Clamping. Unlike Move() a linear approximation will be used
    /// </summary>
    /// <remarks>fDistance relates to the total spline, so longer splines will result in faster movement for constant fDistance</remarks>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="fDistance">the percentage of the spline/group to move</param>
    /// <param name="clamping">clamping mode</param>
    /// <returns>the interpolated position</returns>
    public virtual Vector3 MoveFast(ref float tf, ref int direction, float fDistance, CurvyClamping clamping)
    {
        tf += fDistance * direction;
        ClampTF(ref tf, ref direction, clamping);
        return InterpolateFast(tf);
    }

    /// <summary>
    /// Alter TF to reflect a movement over a certain distance using a default stepSize of 0.002
    /// </summary>
    /// <remarks>MoveBy works by extrapolating current curvation, so results may be inaccurate for large distances</remarks>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="distance">the distance in world units to move</param>
    /// <param name="clamping">clamping mode</param>
    /// <returns>the interpolated position</returns>
    public virtual Vector3 MoveBy(ref float tf, ref int direction, float distance, CurvyClamping clamping)
    {
        return MoveBy(ref tf, ref direction, distance, clamping, 0.002f);
    }

    /// <summary>
    /// Alter TF to reflect a movement over a certain distance
    /// </summary>
    /// <remarks>MoveBy works by extrapolating current curvation, so results may be inaccurate for large distances</remarks>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="distance">the distance in world units to move</param>
    /// <param name="clamping">clamping mode</param>
    /// <param name="stepSize">stepSize defines the accuracy</param>
    /// <returns>the interpolated position</returns>
    public virtual Vector3 MoveBy(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize)
    {
        return Move(ref tf, ref direction, ExtrapolateDistanceToTF(tf, distance, stepSize), clamping);
    }

    /// <summary>
    /// Alter TF to reflect a movement over a certain distance. Unlike MoveBy, a linear approximation will be used
    /// </summary>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="distance">the distance in world units to move</param>
    /// <param name="clamping">clamping mode</param>
    /// <returns>the interpolated position</returns>
    public virtual Vector3 MoveByFast(ref float tf, ref int direction, float distance, CurvyClamping clamping)
    {
        return MoveByFast(ref tf, ref direction, distance, clamping, 0.002f);
    }

    /// <summary>
    /// Alter TF to reflect a movement over a certain distance. Unlike MoveBy, a linear approximation will be used
    /// </summary>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="distance">the distance in world units to move</param>
    /// <param name="clamping">clamping mode</param>
    /// <param name="stepSize">stepSize defines the accuracy</param>
    /// <returns>the interpolated position</returns>
    public virtual Vector3 MoveByFast(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize)
    {
        return MoveFast(ref tf, ref direction, ExtrapolateDistanceToTFFast(tf, distance, stepSize), clamping);
    }

    /// <summary>
    /// Alter TF to reflect a movement over a certain distance.
    /// </summary>
    /// <remarks>MoveByLengthFast works by using actual lengths</remarks>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="distance">the distance in world units to move</param>
    /// <param name="clamping">clamping mode</param>
    /// <returns>the interpolated position</returns>
    public virtual Vector3 MoveByLengthFast(ref float tf, ref int direction, float distance, CurvyClamping clamping)
    {
        float dist = TFToDistance(tf);
        float delta = (clamping != CurvyClamping.Clamp) ? (distance % Length) * direction : distance * direction;
        float newDist = dist + delta;
        if (newDist > Length || newDist < 0) {
            switch (clamping) {
                case CurvyClamping.Clamp:
                    tf = (delta < 0) ? 0 : 1;
                    break;
                case CurvyClamping.PingPong:
                    if (delta < 0)
                        newDist *= -1;
                    else
                        newDist -= delta - Length;
                    direction *= -1;
                    tf = DistanceToTF(newDist);
                    break;
                case CurvyClamping.Loop:
                    if (newDist < 0)
                        newDist += Length;
                    else
                        newDist -= Length;
                    tf = DistanceToTF(newDist);
                    break;
            }
        }
        else
            tf = DistanceToTF(newDist);

        return InterpolateFast(tf);
    }

    /// <summary>
    /// Alter TF to move until the curvation reached angle.
    /// </summary>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="angle">the angle in degrees</param>
    /// <param name="clamping">the clamping mode. CurvyClamping.PingPong isn't supported!</param>
    /// <param name="stepSize">stepSize defines the accuracy</param>
    /// <returns>the interpolated position</returns>
    public virtual Vector3 MoveByAngle(ref float tf, ref int direction, float angle, CurvyClamping clamping, float stepSize)
    {
        if (clamping == CurvyClamping.PingPong) {
            Debug.LogError("CurvySplineBase.MoveByAngle: PingPong clamping isn't supported!");
            return Vector3.zero;
        }
        stepSize = Mathf.Max(0.0001f, stepSize);
        float initialTF = tf;
        Vector3 initialP = Interpolate(tf);
        Vector3 initialT = GetTangent(tf, initialP);
        Vector3 P2 = Vector3.zero;
        Vector3 T2;
        int deadlock = 10000;
        while (deadlock-- > 0) {
            tf += stepSize * direction;
            if (tf > 1) {
                if (clamping == CurvyClamping.Loop)
                    tf -= 1;
                else {
                    tf = 1;
                    return Interpolate(1);
                }
            }
            else if (tf < 0) {
                if (clamping == CurvyClamping.Loop)
                    tf += 1;
                else {
                    tf = 0;
                    return Interpolate(0);
                }
            }
            P2 = Interpolate(tf);
            T2 = P2 - initialP;
            float accAngle = Vector3.Angle(initialT, T2);
            if (accAngle >= angle) {
                tf = initialTF + (tf - initialTF) * angle / accAngle;
                return Interpolate(tf);
            }
        }
        return P2;

    }

    /// <summary>
    /// Alter TF to move until the curvation reached angle. Unlike MoveByAngle, a linear approximation will be used
    /// </summary>
    /// <param name="tf">the current TF value</param>
    /// <param name="direction">the current direction, 1 or -1</param>
    /// <param name="angle">the angle in degrees</param>
    /// <param name="clamping">the clamping mode. CurvyClamping.PingPong isn't supported!</param>
    /// <param name="stepSize">stepSize defines the accuracy</param>
    /// <returns>the interpolated position</returns>
    public virtual Vector3 MoveByAngleFast(ref float tf, ref int direction, float angle, CurvyClamping clamping, float stepSize)
    {
        if (clamping == CurvyClamping.PingPong) {
            Debug.LogError("CurvySplineBase.MoveByAngle: PingPong clamping isn't supported!");
            return Vector3.zero;
        }
        
        stepSize = Mathf.Max(0.0001f, stepSize);
        float initialTF = tf;
        Vector3 initialP = InterpolateFast(tf);
        Vector3 initialT = GetTangentFast(tf);
        Vector3 P2 = Vector3.zero;
        Vector3 T2;
        int deadlock = 10000;
        while (deadlock-- > 0) {
            tf += stepSize * direction;
            if (tf > 1) {
                if (clamping == CurvyClamping.Loop)
                    tf -= 1;
                else {
                    tf = 1;
                    return InterpolateFast(1);
                }
            }
            else if (tf < 0) {
                if (clamping == CurvyClamping.Loop)
                    tf += 1;
                else {
                    tf = 0;
                    return InterpolateFast(0);
                }
            }
            P2 = InterpolateFast(tf);
            T2 = P2 - initialP;
            float accAngle = Vector3.Angle(initialT, T2);
            if (accAngle >= angle) {
                tf = initialTF + (tf - initialTF) * angle / accAngle;
                return InterpolateFast(tf);
            }
        }
        return P2;
    }

    /// <summary>
    /// Gets the point at a certain radius and angle on the plane defined by P and it's Tangent
    /// </summary>
    /// <param name="tf">TF value identifying position on spline (0..1)</param>
    /// <param name="radius">radius in world units</param>
    /// <param name="angle">angle in degrees</param>
    /// <returns>the extruded point</returns>
    public virtual Vector3 GetExtrusionPoint(float tf, float radius, float angle)
    {
        Vector3 pos = Interpolate(tf);
        Vector3 tan = GetTangent(tf, pos);
        Quaternion R = Quaternion.AngleAxis(angle, tan);
        return pos + (R * GetOrientationUpFast(tf)) * radius;
    }

    /// <summary>
    /// Gets the point at a certain radius and angle on the plane defined by P and it's Tangent, using a linear interpolation
    /// </summary>
    /// <param name="tf">TF value identifying position on spline (0..1)</param>
    /// <param name="radius">radius in world units</param>
    /// <param name="angle">angle in degrees</param>
    /// <returns>the extruded point</returns>
    public virtual Vector3 GetExtrusionPointFast(float tf, float radius, float angle)
    {
        Vector3 pos = InterpolateFast(tf);
        Vector3 tan = GetTangentFast(tf);
        Quaternion R = Quaternion.AngleAxis(angle, tan);
        return pos + (R * GetOrientationUpFast(tf)) * radius;
    }

    #endregion

    #region ### Methods working on distance from the first control point ###
    
    /// <summary>
    /// Gets the normalized tangent by distance from the spline/group's start.
    /// </summary>
    /// <param name="distance">distance in the range of 0..Length</param>
    /// <returns>the tangent/direction</returns>
    public virtual Vector3 GetTangentByDistance(float distance)
    {
        return GetTangent(DistanceToTF(distance));
    }

    /// <summary>
    /// Gets the normalized tangent by distance from the spline/group's start. Unlike TangentByDistance() this uses a linear approximation
    /// </summary>
    /// <param name="distance">distance in the range of 0..Length</param>
    /// <returns>the tangent/direction</returns>
    public virtual Vector3 GetTangentByDistanceFast(float distance)
    {
        return GetTangentFast(DistanceToTF(distance));
    }
    /// <summary>
    /// Gets the interpolated position by distance from the spline/group's start
    /// </summary>
    /// <param name="distance">distance in the range of 0..Length</param>
    /// <returns>the interpolated position</returns>
    public virtual Vector3 InterpolateByDistance(float distance)
    {
        return Interpolate(DistanceToTF(distance));
    }

    /// <summary>
    /// Gets the interpolated position by distance from the spline/group's start. Unlike InterpolateByDistance() this uses a linear approximation
    /// </summary>
    /// <param name="distance">distance in the range of 0..Length</param>
    /// <returns>the interpolated position</returns>
    public virtual Vector3 InterpolateByDistanceFast(float distance)
    {
        return InterpolateFast(DistanceToTF(distance));
    }

    /// <summary>
    /// Converts a unit distance into TF-distance by extrapolating the curvation at a given point on the curve
    /// </summary>
    /// <param name="tf">the current TF value</param>
    /// <param name="distance">the distance in world units</param>
    /// <param name="stepSize">stepSize defines the accuracy</param>
    /// <returns>the distance in TF</returns>
    public float ExtrapolateDistanceToTF(float tf, float distance, float stepSize)
    {
        stepSize = Mathf.Max(0.0001f, stepSize);
        Vector3 p = Interpolate(tf);
        float tf1;
        if (tf == 1)
            tf1 = Mathf.Min(1f, tf - stepSize);
        else
            tf1 = Mathf.Min(1f, tf + stepSize);

        stepSize = Mathf.Abs(tf1 - tf);
        Vector3 p1 = Interpolate(tf1);
        float mag = (p1 - p).magnitude;

        if (mag != 0) {
            return (1 / mag) * stepSize * distance;
        }
        else return 0;
    }

    /// <summary>
    /// Converts a unit distance into TF-distance by extrapolating the curvation at a given point on the curve using a linear approximation
    /// </summary>
    /// <param name="tf">the current TF value</param>
    /// <param name="distance">the distance in world units</param>
    /// <param name="stepSize">stepSize defines the accuracy</param>
    /// <returns>the distance in TF</returns>
    public float ExtrapolateDistanceToTFFast(float tf, float distance, float stepSize)
    {
        stepSize = Mathf.Max(0.0001f, stepSize);
        Vector3 p = InterpolateFast(tf);
        float tf1;
        if (tf == 1)
            tf1 = Mathf.Min(1f, tf - stepSize);
        else
            tf1 = Mathf.Min(1f, tf + stepSize);

        stepSize = Mathf.Abs(tf1 - tf);
        Vector3 p1 = InterpolateFast(tf1);
        float mag = (p1 - p).magnitude;

        if (mag != 0) {
            return (1 / mag) * stepSize * distance;
        }
        else return 0;
    }

    #endregion

    #region ### General Methods ###

    /// <summary>
    /// Destroys the spline/spline group
    /// </summary>
    public virtual void Destroy()
    {
        if (Application.isPlaying)
            GameObject.Destroy(gameObject);
        else
            GameObject.DestroyImmediate(gameObject);
    }

    /// <summary>
    /// Calculate the bounds of the spline or group
    /// </summary>
    /// <param name="local">get bounds in coordinates local to the spline/group</param>
    /// <remarks>Bounds are calculated using approximation points</remarks>
    public virtual Bounds GetBounds(bool local)
    {
        Vector3[] p = GetApproximation(local);
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < p.Length; i++) {
            min.x = Mathf.Min(min.x, p[i].x);
            min.y = Mathf.Min(min.y, p[i].y);
            min.z = Mathf.Min(min.z, p[i].z);
            max.x = Mathf.Max(max.x, p[i].x);
            max.y = Mathf.Max(max.y, p[i].y);
            max.z = Mathf.Max(max.z, p[i].z);
        }
        return new Bounds(min + (max - min) / 2, (max - min));
    }

    /// <summary>
    /// Refreshs the spline/group at the next call to Update()
    /// </summary>
    public virtual void Refresh() { Refresh(true, true, false); }
    /// <summary>
    /// Refreshs the spline/group at the next call to Update()
    /// </summary>
    /// <param name="refreshLength">whether the length should be recalculated</param>
    /// <param name="refreshOrientation">whether the orientation should be recalculated</param>
    /// <param name="skipIfInitialized">True to skip refresh if the spline is initialized</param>
    public virtual void Refresh(bool refreshLength, bool refreshOrientation, bool skipIfInitialized) 
    {
        mNeedLengthRefresh = refreshLength;
        mNeedOrientationRefresh = refreshOrientation;
        if (mSkipRefreshIfInitialized)
            mSkipRefreshIfInitialized = skipIfInitialized;
    }

    /// <summary>
    /// Refreshs the spline/group immediately
    /// </summary>
    public virtual void RefreshImmediately() { RefreshImmediately(true, true, false); }

    /// <summary>
    /// Refreshs/Updates the spline/spline group immediately
    /// </summary>
    /// <param name="refreshLength">whether the length should be recalculated</param>
    /// <param name="refreshOrientation">whether the orientation should be recalculated</param>
    /// <param name="skipIfInitialized">True to skip refresh if the spline is initialized</param>
    public virtual void RefreshImmediately(bool refreshLength, bool refreshOrientation, bool skipIfInitialized)
    {
        mNeedLengthRefresh = false;
        mNeedOrientationRefresh = false;
        mSkipRefreshIfInitialized = true;
    }

    

    #endregion

    



    #region ### Privates & internal Publics ###
    /*! \cond PRIVATE */
    /*! @name Internal Public
     *  Don't use them unless you know what you're doing!
     */
    //@{

    protected virtual bool ClampTF(ref float tf, ref int dir, CurvyClamping clamping)
    {
        switch (clamping) {
            case CurvyClamping.Loop:
                if (tf < 0) {
                    tf = 1 - Mathf.Abs(tf % 1);
                    return true;
                }
                else if (tf > 1) {
                    tf = tf % 1;
                    return true;
                }
                break;
            case CurvyClamping.PingPong:
                if (tf < 0) {
                    tf = (tf % 1) * -1;
                    dir *= -1;
                    return true;
                }
                else if (tf > 1) {
                    tf = 1 - tf % 1;
                    dir *= -1;
                    return true;
                }
                break;
            default: // Clamp
                tf = Mathf.Clamp01(tf);
                break;
        }
        return false;
    }

    //@}
    /*! \endcond */
    #endregion
}
