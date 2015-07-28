// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections;


/// <summary>
/// Class to dynamically clone objects along a spline
/// </summary>
[ExecuteInEditMode]
public class SplinePathCloneBuilder : MonoBehaviour
{
    const int MAXCLONES = 2000; // increase this if you really want more than 2000 clones
    #region ### Public Fields and Properties ###

    /// <summary>
    /// The spline or spline group to use
    /// </summary>
    public CurvySplineBase Spline;
    /// <summary>
    /// Typecasts the Target as a CurvySpline
    /// </summary>
    /// <remarks>This is particular useful when working with playMaker</remarks>
    public CurvySpline TargetSpline { get { return Spline as CurvySpline; } }
    /// <summary>
    /// Typecasts the Target as a CurvySplineGroup
    /// </summary>
    /// <remarks>This is particular useful when working with playMaker</remarks>
    public CurvySplineGroup TargetSplineGroup { get { return Spline as CurvySplineGroup; } }
    /// <summary>
    /// Base coordinates off spline's position?
    /// </summary>
    public bool UseWorldPosition = false;
    /// <summary>
    /// List of source objects 
    /// </summary>
    /// <remarks>Each source object needs at least a collider OR a mesh. If both are present, the collider will be used for space calculation</remarks>
    public GameObject[] Source = new GameObject[0];
    /// <summary>
    /// The gap in world units between each object
    /// </summary>
    public float Gap;
    /// <summary>
    /// Determines the way groups are handled
    /// </summary>
    public SplinePathCloneBuilderMode Mode;
    /// <summary>
    /// Whether the clone path should automatically adapt to spline changes
    /// </summary>
    public bool AutoRefresh = true;
    /// <summary>
    /// The refreshing speed
    /// </summary>
    public float AutoRefreshSpeed = 0;
    /// <summary>
    /// Gets the number of cloned objects
    /// </summary>
    public int ObjectCount { get { return mTransform.childCount; } }

    public delegate GameObject OnGetCloneEvent(SplinePathCloneBuilder sender, GameObject source);
    public delegate void OnReleaseCloneEvent(SplinePathCloneBuilder sender, GameObject clone);
    /// <summary>
    /// This event is called when a copy of a source gameobject is needed
    /// </summary>
    /// <remarks>You may want to subscribe to this event to use a pooling manager</remarks>
    public event OnGetCloneEvent OnGetClone;
    /// <summary>
    /// This event is called when a clone can be released
    /// </summary>
    /// <remarks>You may want to subscribe to this event to use a pooling manager</remarks>
    public event OnReleaseCloneEvent OnReleaseClone;
    #endregion

    Transform mTransform;
    float mLastRefresh;

    #region ### Unity Callbacks ###

    void OnEnable()
    {
        mTransform = transform;
    }

    void OnDisable()
    {
        if (Spline) 
            Spline.OnRefresh -= OnSplineRefresh;
    }

    void OnDestroy()
    {
        Clear();
    }
	
	void Update () {
        if (!Spline || !Spline.IsInitialized) return;
        Spline.OnRefresh -= OnSplineRefresh;
        if (AutoRefresh) 
            Spline.OnRefresh += OnSplineRefresh;
	}

    #endregion

    #region ### Public Methods ###

    /// <summary>
    /// Creates a GameObject with a SplinePathCloneBuilder script attached
    /// </summary>
    public static SplinePathCloneBuilder Create()
    {
        SplinePathCloneBuilder o = new GameObject("CurvyClonePath", typeof(SplinePathCloneBuilder)).GetComponent<SplinePathCloneBuilder>();
        return o;
    }

    

    /// <summary>
    /// Rebuilds the path.
    /// </summary>
    /// <param name="force">If true, all existing clones will be destroyed, otherwise they will be reused</param>
    /// <remarks>If you change the Source array by code, call Refresh(true)</remarks>
    public void Refresh(bool force)
    {
        if (Spline == null || !Spline.IsInitialized) 
            return;
        
        // we need a spline length
        if (Spline.Length==0)
            Spline.Refresh(true, false, false);

        checkSources();
        if (Source.Length == 0) {
            Clear();
            return;
        }

        
        // get size of clones and calculate number of clones needed
        float totaldepth;
        float[] depths = getSourceDepths(out totaldepth);

        int count=0;
        if (!Mathf.Approximately(0, totaldepth)) {
            switch (Mode) {
                case SplinePathCloneBuilderMode.CloneGroup:
                    count = Mathf.FloorToInt(Spline.Length / totaldepth) * Source.Length;
                    break;
                default: // Individual
                    float d = Spline.Length;
                    int i = 0;
                    while (d > 0 && count<MAXCLONES) {
                        d -= depths[i++] + Gap;
                        count++;
                        if (i == Source.Length)
                            i = 0;
                    }
                    if (count!=MAXCLONES)
                        count--;
                    break;
            }
        }

        // Constrain max clones
        if (count >= MAXCLONES) {
            Debug.LogError("SplinePathCloneBuilder: MAXCLONES reached, ensure to have proper colliders in place! If you really want to clone more than " + MAXCLONES + " objects, increase MAXCLONES in SplinePathCloneBuilder.cs (Line 15)!");
        }
        else {

            // Clear
            if (force)
                Clear(); // Clear all clones
            else
                Clear(count); // Smart Clear only unneeded


            int idx = 0;
            float distance = 0;
            int current = -1;
            int existing = ObjectCount;

            while (++current < count) {
                float tf = Spline.DistanceToTF(distance + depths[idx] / 2);

                if (current < existing) {
                    Transform T = mTransform.GetChild(current);
                    if (UseWorldPosition)
                        T.position = Spline.Interpolate(tf);

                    else
                        T.localPosition = Spline.Interpolate(tf);
                    T.rotation = Spline.GetOrientationFast(tf) * Source[idx].transform.rotation;
                }
                else {
                    GameObject clone;
                    if (OnGetClone != null)
                        clone = OnGetClone(this, Source[idx]);
                    else
                        clone = CloneObject(Source[idx]);
                    if (clone) {
                        Transform T = clone.transform;
                        T.parent = transform;
                        clone.name = string.Format("{0:0000}", current) + clone.name;

                        if (UseWorldPosition)
                            T.position = Spline.Interpolate(tf);
                        else
                            T.localPosition = Spline.Interpolate(tf);
                        T.rotation = Spline.GetOrientationFast(tf) * Source[idx].transform.rotation;
                    }
                }
                distance += depths[idx] + Gap;
                if (++idx == Source.Length)
                    idx = 0;
            }
        }
         
    }

    /// <summary>
    /// Clear all created clones
    /// </summary>
    public void Clear()
    {
        Transform[] childs = transform.GetComponentsInChildren<Transform>();
        if (OnReleaseClone!=null)
            for (int i = childs.Length - 1; i > 0; i--)
                OnReleaseClone(this, childs[i].gameObject);
        else
            for (int i = childs.Length - 1; i > 0; i--) 
                DestroyObject(childs[i].gameObject);

    }
    /// <summary>
    /// Clear all created clones that exceeds index
    /// </summary>
    /// <param name="index">the child index</param>
    public void Clear(int index)
    {
        int cc = mTransform.childCount;
        if (OnReleaseClone!=null)
            for (int i = cc - 1; i >= index; i--)
                OnReleaseClone(this,mTransform.GetChild(i).gameObject);
        else
            for (int i = cc - 1; i >= index; i--) 
                DestroyObject(mTransform.GetChild(i).gameObject);
        
    }

    /// <summary>
    /// Parent existing clones to an empty GameObject, making them persistent
    /// </summary>
    /// <returns>the new Transform</returns>
    public Transform Detach()
    {
        var T = new GameObject().transform;
        T.name = "CurvyClonePath_Detached";
        Detach(T);
        return T;
    }

    /// <summary>
    /// Parent existing clones to a given transform, making them persistent
    /// </summary>
    /// <returns></returns>
    public void Detach(Transform to)
    {
        Transform[] childs = transform.GetComponentsInChildren<Transform>();
        for (int i = childs.Length - 1; i > 0; i--)
            childs[i].parent = to;
    }

    #endregion

    

    #region ### Privates & internal Publics ###
    /*! \cond PRIVATE */
    /*! @name Internal Public
     *  Don't use them unless you know what you're doing!
     */
    //@{

    GameObject CloneObject(GameObject source)
    {
        return (source != null) ? GameObject.Instantiate(source) as GameObject : null;
    }

    void DestroyObject(GameObject obj)
    {
        if (Application.isPlaying)
            GameObject.Destroy(obj);
        else
            GameObject.DestroyImmediate(obj);
    }

    void checkSources()
    {
        ArrayList src = new ArrayList();
        for (int i = 0; i < Source.Length; i++)
            if (Source[i] != null) {
                src.Add(Source[i]);
            }

        Source = (src.Count == 0) ? new GameObject[0] : (GameObject[])src.ToArray(typeof(GameObject));
    }

    // returns z-depth of a collider or 0 on error
    float getDepth(GameObject o)
    {
        if (!o) return 0;
        // Use all colliders we can find
        Bounds bounds=new Bounds(o.transform.position,Vector3.zero);
        Collider[] cols = o.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++) {
            bounds.Encapsulate(cols[i].bounds);
        }
        
        return bounds.size.z;
    }

    // returns depths for all source elements
    float[] getSourceDepths(out float total)
    {
        float[] res=new float[Source.Length];
        total = 0;
        
        for (int i = 0; i < Source.Length; i++) {
            res[i] = getDepth(Source[i]);
            total += res[i];
        }
        
        total += res.Length * Gap;
        return res;
    }

    void OnSplineRefresh(CurvySplineBase sender)
    {
        if (Time.realtimeSinceStartup - mLastRefresh > AutoRefreshSpeed) {
            mLastRefresh = Time.realtimeSinceStartup;
            Refresh(false);
        }
    }


    //@}
    /*! \endcond */
    #endregion
}

/// <summary>
/// Modes for SplinePathCloneBuilder
/// </summary>
public enum SplinePathCloneBuilderMode
{
    /// <summary>
    /// Clone individual source objects
    /// </summary>
    CloneIndividual = 0,
    /// <summary>
    /// Clone only multiples of the whole source objects group
    /// </summary>
    CloneGroup = 1,
}
