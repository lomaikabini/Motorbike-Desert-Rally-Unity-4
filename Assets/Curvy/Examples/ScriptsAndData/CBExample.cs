/*
 * 
 * This example shows how to dynamically modify a CloneBuilder
 * 
 */
using UnityEngine;
using System.Collections;

public class CBExample : MonoBehaviour {
    public GameObject[] CloneGroup1;
    public GameObject[] CloneGroup2;
    public GameObject[] CloneGroup3;
    public string[] CloneGroupNames;

    SplinePathCloneBuilder mBuilder;
    CurvySpline mSpline;
    Transform mCPToMove;

    
    int mCurGroup;

	// Use this for initialization
	IEnumerator Start () {
        mBuilder = GetComponent<SplinePathCloneBuilder>();
        mSpline = (CurvySpline)mBuilder.Spline;
        while (!mSpline.IsInitialized)
            yield return 0;
        mCPToMove = mSpline.ControlPoints[1].Transform;
        UpdateClones();
	}

    void Update()
    {
        // simply move the middle control point
        if (mCPToMove)
            mCPToMove.Translate(Vector3.up * Mathf.Sin(Mathf.PingPong(Time.time, 2) - 1)*5*Time.deltaTime);
    }

    void OnGUI()
    {
        if (mSpline && mSpline.IsInitialized) {

            int cur = mCurGroup;

            GUILayout.BeginHorizontal();
            mCurGroup = GUILayout.SelectionGrid(mCurGroup, CloneGroupNames, 4);
            GUILayout.Label("Gap");
            mBuilder.Gap = GUILayout.HorizontalSlider(mBuilder.Gap, 0, 1, GUILayout.Width(150));
            GUILayout.Label("Swirl");
            mSpline.SwirlTurns = GUILayout.HorizontalSlider(mSpline.SwirlTurns, 0, 1, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            if (cur != mCurGroup) {
                cur = mCurGroup;
                UpdateClones();
            }

        }
    }

    void UpdateClones()
    {
        mBuilder.Clear();
        switch (mCurGroup) {
            case 0: mBuilder.Source = CloneGroup1; break;
            case 1: mBuilder.Source = CloneGroup2; break;
            case 2: mBuilder.Source = CloneGroup3; break;
        }
        mBuilder.Refresh(true);
    }
	
}
