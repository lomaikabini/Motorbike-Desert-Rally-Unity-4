// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Curvy meets Superformula!
/// </summary>
[RequireComponent(typeof(CurvySpline))]
[ExecuteInEditMode]
public class SplineShaper : MonoBehaviour
{
    #region ### Public Fields and Properties ###

    /// <summary>
    /// Defines how to apply the modifier
    /// </summary>
    public enum ModifierMode { None, Absolute, Relative }

    /// <summary>
    /// The maximum number of generated Control Points
    /// </summary>
    public int Resolution = 10;

    /// <summary>
    /// Range in full circles (e.g. 2=720°)
    /// </summary>
    public float Range = 1;
    
    /// <summary>
    /// Base Radius
    /// </summary>
    public float Radius = 5;
    
    /// <summary>
    /// Radius modifier mode
    /// </summary>
    public ModifierMode RadiusModifier = ModifierMode.None;
    
    /// <summary>
    /// Radius modifier curve
    /// </summary>
    public AnimationCurve RadiusModifierCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    /// <summary>
    /// Base Z
    /// </summary>
    public float Z = 0;
    
    /// <summary>
    /// Z modifier mode
    /// </summary>
    public ModifierMode ZModifier = ModifierMode.None;
    
    /// <summary>
    /// Z modifier curve
    /// </summary>
    public AnimationCurve ZModifierCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    /// <summary>
    /// Name of the effect
    /// </summary>
    public string Name = string.Empty;

    // SuperFormula parameters:
    public float m;
    public float n1;
    public float n2;
    public float n3;
    public float a;
    public float b;

    /// <summary>
    /// Welding threshold. Control Point's within this distance will be removed
    /// </summary>
    public float WeldThreshold = 0.1f;
    /// <summary>
    /// Auto Refresh spline when parameter changes?
    /// </summary>
    public bool AutoRefresh = true;
    /// <summary>
    /// The Auto Refresh speed in seconds
    /// </summary>
    public float AutoRefreshSpeed = 0;

    /// <summary>
    /// The Spline of this GameObject
    /// </summary>
    public CurvySpline Spline
    {
        get
        {
            if (!mSpline)
                mSpline = GetComponent<CurvySpline>();
            return mSpline;
        }
    }

    #endregion

    CurvySpline mSpline;
    float mLastRefresh;
    bool mNeedRefresh;

    /// <summary>
    /// Create a GameObject containing a SplineShaper and a Spline
    /// </summary>
    public static SplineShaper Create()
    {
        var go = new GameObject("SplineShape", typeof(SplineShaper));
        // Set a basic circle
        SplineShaper shp=go.GetComponent<SplineShaper>();
        shp.Spline.ShowOrientation = false;
        return shp;
    }

    public void Reset()
    {
        Name = string.Empty;
        Resolution = 20;
        Range = 1;
        Radius = 5;
        RadiusModifier = ModifierMode.None;
        RadiusModifierCurve = AnimationCurve.Linear(0, 0, 1, 0);
        Z = 0;
        ZModifier = ModifierMode.None;
        ZModifierCurve = AnimationCurve.Linear(0, 0, 1, 0);
        Z = 0;
        m = 0;
        n1 = 1;
        n2 = 3;
        n3 = 4;
        a = 1;
        b = 1;
        WeldThreshold = 0.1f;
        AutoRefresh = true;
        AutoRefreshSpeed = 0;
    }

    #region ### Unity Callbacks ###

    void Update()
    {
        if (!Spline || !Spline.IsInitialized)
            return;

        if (mNeedRefresh ||(AutoRefresh && Time.realtimeSinceStartup - mLastRefresh > AutoRefreshSpeed)) {
            mLastRefresh = Time.realtimeSinceStartup;
            RefreshImmediately();
        }
    }

    #endregion

    /// <summary>
    /// Triggers a refresh on the next Update()
    /// </summary>
    public void Refresh()
    {
        mNeedRefresh = true;
    }

    /// <summary>
    /// Refresh the spline based on the current settings immediately
    /// </summary>
    public void RefreshImmediately()
    {
        mNeedRefresh = false;
        Resolution = Mathf.Max(2, Resolution);
        int cpCount = mSpline.ControlPointCount;
        int cpIdx = 0;
        float dmax = Range * Mathf.PI * 2;
        float dtf = (dmax) / (float)Resolution;

        double r = 0;
        float treshold2 = WeldThreshold * WeldThreshold;
        Vector3 lastp = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 p = Vector3.zero;
        CurvySplineSegment seg;
        float curZ = transform.position.z+Z;
        float curRadius = Radius;

        for (float dt = 0; dt < dmax; dt += dtf) {
            getEvaluatedValues(dt / dmax, ref curRadius, ref curZ);
            r = System.Math.Pow((Mathf.Pow(Mathf.Abs(Mathf.Cos(m * dt / 4.0f) / a), n2) + System.Math.Pow(Mathf.Abs(Mathf.Sin(m * dt / 4.0f) / b), n3)), -(1 / n1));

            p.x = (float)(r * Mathf.Cos(dt) * curRadius);
            p.y = (float)(r * Mathf.Sin(dt) * curRadius);
            p.z = curZ;

            if ((p - lastp).sqrMagnitude >= treshold2) {
                seg = (cpIdx < cpCount) ? Spline.ControlPoints[cpIdx] : Spline.Add(null, false);
                cpIdx++;
                seg.Transform.localPosition = p;
                lastp = p;
            }

        }

        // Shrink if we got too much Control Points
        if (cpCount > cpIdx)
            while (cpCount > cpIdx)
                Spline.ControlPoints[--cpCount].Delete();

            Spline.Refresh();
    }

    #region ### Privates & internal Publics ###
    /*! \cond PRIVATE */
    /*! @name Internal Public
     *  Don't use them unless you know what you're doing!
     */
    //@{

    void getEvaluatedValues(float percent, ref float radius, ref float zz)
    {
        switch (RadiusModifier) {
            case ModifierMode.Absolute:
                radius = RadiusModifierCurve.Evaluate(percent);
                break;
            case ModifierMode.Relative:
                radius = Radius * RadiusModifierCurve.Evaluate(percent);
                break;
            default:
                radius = Radius;
                break;
        }

        switch (ZModifier) {
            case ModifierMode.Absolute:
                zz = ZModifierCurve.Evaluate(percent);
                break;
            case ModifierMode.Relative:
                zz = Z * ZModifierCurve.Evaluate(percent);
                break;
            default:
                zz = Z;
                break;
        }
    }

    //@}
    /*! \endcond */
    #endregion
}
