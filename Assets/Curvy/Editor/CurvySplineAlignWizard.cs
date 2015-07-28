// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class CurvySplineAlignWizard : EditorWindow {
    CurvySpline Spline;
    float StartOffset=0;
    float EndOffset=0;
    float Step;
    bool UseWorldUnits = false;
    bool SetPosition = true;
    bool SetOrientation=true;
    int OrientationType = 0;

    int selcount;

    Vector3[] pos = new Vector3[0];
    Vector3[] up = new Vector3[0];
    Vector3[] tan = new Vector3[0];

    static public void Create()
    {
        var win=GetWindow<CurvySplineAlignWizard>(true, "Align Transforms to spline",true);
        win.Init(Selection.activeGameObject.GetComponent<CurvySpline>());
        win.maxSize = new Vector2(400, 205);
        win.minSize = win.maxSize;
        Selection.activeTransform = null;
        SceneView.onSceneGUIDelegate -= win.Preview;
        SceneView.onSceneGUIDelegate += win.Preview;
    }

    void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= Preview;
    }

    void OnFocus()
    {
        SceneView.onSceneGUIDelegate -= Preview;
        SceneView.onSceneGUIDelegate += Preview;
    }

    void OnSelectionChange()
    {
        if (Selection.activeGameObject){
            var spl = Selection.activeGameObject.GetComponent<CurvySpline>();
            if (spl)
                Init(spl);
        }
        Repaint();
    }

    void Init(CurvySpline spline)
    {
        Spline = spline;
    }

    void OnGUI()
    {
        selcount = (Selection.transforms != null) ? Selection.transforms.Length : 0;

        GUILayout.Label("Spline '"+Spline.name+"': Length=" + string.Format("{0:0.00}", new object[]{Spline.Length}) + " / Selected: " + selcount + " transforms");
        GUILayout.Label("Select Transforms and hit Apply!", EditorStyles.boldLabel);
        
        StartOffset=EditorGUILayout.FloatField("Offset: Start", StartOffset);
        EndOffset=EditorGUILayout.FloatField("Offset: End", EndOffset);
        EditorGUILayout.BeginHorizontal();
        Step=EditorGUILayout.FloatField("Step", Step);
        if (GUILayout.Button("Auto"))
            SetAutoStep();
        EditorGUILayout.EndHorizontal();
        UseWorldUnits=EditorGUILayout.Toggle("Use World Units", UseWorldUnits);
        
        SetPosition=EditorGUILayout.Toggle("Set Position",SetPosition);
        SetOrientation=EditorGUILayout.Toggle("Set Orientation",SetOrientation);
        if (SetOrientation) {
            EditorGUILayout.BeginHorizontal();
            OrientationType = GUILayout.SelectionGrid(OrientationType, new GUIContent[] { new GUIContent("Up-Vector","Rotate to match Up-Vectors"),new GUIContent("Tangent","Rotate to match Tangent")},2);
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("Apply"))
            DoAlign();
        GUI.enabled = true;
        Calculate();
        if (SceneView.lastActiveSceneView)
            SceneView.lastActiveSceneView.Repaint();
    }

    void SetAutoStep()
    {
        if (selcount == 0) return;
        float len = (UseWorldUnits) ? Spline.Length - StartOffset - EndOffset : 1 - StartOffset - EndOffset;
        if (selcount>1)
            Step = len / (selcount-1);
        else
            Step = len / (selcount - 1);
    }

    void Calculate()
    {
        if (selcount == 0) return;
        pos = new Vector3[selcount];
        up = new Vector3[selcount];
        tan = new Vector3[selcount];

        for (int i = 0; i < selcount; i++) {
            pos[i] = (UseWorldUnits) ? Spline.InterpolateByDistance(StartOffset + Step * i) : Spline.Interpolate(StartOffset + Step * i);
            up[i] = (UseWorldUnits) ? Spline.GetOrientationUpFast(Spline.DistanceToTF(StartOffset + Step * i)) : Spline.GetOrientationUpFast(StartOffset + Step * i);
            tan[i] = (UseWorldUnits) ? Spline.GetTangentByDistance(StartOffset + Step * i) : Spline.GetTangent(StartOffset + Step * i);
        }
    }

    void DoAlign()
    {
        if (selcount == 0) return;
        List<Transform> trans = new List<Transform>(Selection.transforms);
        trans.Sort((a, b) => string.Compare(a.name, b.name));
        Undo.SetSnapshotTarget(Selection.transforms, "AlignToSpline");
        Undo.CreateSnapshot();
        Undo.RegisterSnapshot();

        for (int i = 0; i < selcount; i++) {
            if (SetPosition)
                trans[i].position = pos[i];
            if (SetOrientation) {
                switch (OrientationType) {
                    case 0: 
                        trans[i].rotation = Quaternion.LookRotation(tan[i], up[i]);
                        break;
                    case 1:
                        trans[i].rotation = Quaternion.LookRotation(up[i], tan[i]);
                        break;
                }
            }
        }
    }


    void Preview(SceneView sceneView)
    {
        Handles.color = Color.blue;
        for (int i=0;i<pos.Length;i++){
            Vector3 rv = (OrientationType == 0) ? up[i] : tan[i];
            Handles.ArrowCap(0, pos[i], (rv!=Vector3.zero) ? Quaternion.LookRotation(rv):Quaternion.identity, 2);
        }
    }
}
