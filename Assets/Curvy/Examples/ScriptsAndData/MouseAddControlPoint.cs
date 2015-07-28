using UnityEngine;
using System.Collections;

public class MouseAddControlPoint : MonoBehaviour {
    public bool RemoveUnusedSegments=true;
    CurvySpline mSpline;
    SplineWalkerDistance Walker;

	// Use this for initialization
	IEnumerator Start () {
        mSpline = GetComponent<CurvySpline>();
        Walker = GameObject.FindObjectOfType(typeof(SplineWalkerDistance)) as SplineWalkerDistance;
        while (!mSpline.IsInitialized)
            yield return null;
	}

    // Update is called once per frame
    void Update()
    {
        // Add Control Point by mouseclick
        if (Input.GetMouseButtonDown(0)) {
            Vector3 p = Input.mousePosition;
            p.z = 10;
            p = Camera.main.ScreenToWorldPoint(p);
            mSpline.Add(p);
            // remove the oldest segment, if it's no longer used
            if (RemoveUnusedSegments){
                var seg=mSpline.DistanceToSegment(Walker.Distance);
                if (seg!=mSpline[0]) {
                    Walker.Distance -= mSpline[0].Length;
                    mSpline.Delete(mSpline[0]);
                }
            }
        }
	}

    
}
