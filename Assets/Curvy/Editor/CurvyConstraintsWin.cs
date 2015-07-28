// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;

public class CurvyConstraintsWin : EditorWindow {

    CurvySplineSegment Target;
    bool xCon;
    bool yCon;
    bool zCon;
    float maxLength;
    bool active;

    static public void Create()
    {
        var win = GetWindow<CurvyConstraintsWin>(false, "Constraints", true);
        win.minSize = new Vector2(330, 45);
        win.maxSize = new Vector3(330, 90);
        win.Init(Selection.activeGameObject);
    }


    void OnDestroy()
    {
        CurvySplineSegmentInspector.ConstrainXAxis = false;
        CurvySplineSegmentInspector.ConstrainYAxis = false;
        CurvySplineSegmentInspector.ConstrainZAxis = false;
        CurvySplineSegmentInspector.ConstraintSplineLength = 0;
    }

    void Init(GameObject o)
    {
        if (o)
            Target = o.GetComponent<CurvySplineSegment>();
        else
            Target = null;

        if (Target) {
            if (maxLength == 0)
                maxLength = Target.Spline.Length;

            xCon = CurvySplineSegmentInspector.ConstrainXAxis;
            yCon = CurvySplineSegmentInspector.ConstrainYAxis;
            zCon = CurvySplineSegmentInspector.ConstrainZAxis;
        }
    }

    void OnSelectionChange()
    {
        Init(Selection.activeGameObject);
    }

    void OnGUI()
    {
        GUILayout.Label("Lock Axis", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        xCon = GUILayout.Toggle(xCon, new GUIContent("X", "Constrain X axis"), "button");
        yCon = GUILayout.Toggle(yCon, new GUIContent("Y", "Constrain Y axis"), "button");
        zCon = GUILayout.Toggle(zCon, new GUIContent("Z", "Constrain Z axis"), "button");
        EditorGUILayout.EndHorizontal();
        GUILayout.Label("Limit Spline Length", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        maxLength = EditorGUILayout.FloatField("Max. Length", maxLength);
        active = GUILayout.Toggle(active, new GUIContent("Active", "Toggle Length Constrain"), "button");
        EditorGUILayout.EndHorizontal();
        CurvySplineSegmentInspector.ConstraintSplineLength = (active) ? maxLength : 0;
        CurvySplineSegmentInspector.ConstrainXAxis = xCon;
        CurvySplineSegmentInspector.ConstrainYAxis = yCon;
        CurvySplineSegmentInspector.ConstrainZAxis = zCon;
        Repaint();
        if (GUI.changed)
            SceneView.RepaintAll();
    }

}
