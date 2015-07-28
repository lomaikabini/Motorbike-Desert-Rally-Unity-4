// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;
using Curvy.Utils;

[CustomEditor(typeof(CurvySplineGroup)), CanEditMultipleObjects]
public class CurvySplineGroupInspector : Editor {

    public CurvySplineGroup Target { get { return target as CurvySplineGroup; } }

    GUIStyle mLargeFont;

    SerializedProperty tSplines;

    Texture2D mTexClonePath;
    Texture2D mTexMeshPath;

    [MenuItem("GameObject/Create Other/Curvy/Spline Group",false,1)]
    static void CreateCurvySplineGroup()
    {
        CurvySplineGroup grp = CurvySplineGroup.Create();
        Selection.activeObject = grp;
    }

    void OnEnable()
    {
        mLargeFont = new GUIStyle();
        mLargeFont.normal.textColor = new Color(0.8f, 0.8f, 1, 0.5f);
        mLargeFont.fontSize = 60;
        Target.Refresh();
        tSplines = serializedObject.FindProperty("Splines");

        mTexClonePath = Resources.Load("curvyclonepath") as Texture2D;
        mTexMeshPath = Resources.Load("curvymeshpath") as Texture2D;
    }

    void OnSceneGUI()
    {
        Target._RemoveEmptySplines();
        for (int i = 0; i < Target.Count; i++) 
            Handles.Label(Target[i].Interpolate(0.5f), i.ToString(), mLargeFont);
        
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(tSplines,new GUIContent("Splines","Splines in the Group"),true);
        
        EditorGUILayout.LabelField("Group Info", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Total Length: " + Target.Length);

        if (serializedObject.targetObject && serializedObject.ApplyModifiedProperties()) {
            Target.RefreshImmediately(true, true, false);
            SceneView.RepaintAll();
        }

        EditorGUILayout.LabelField("Components", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUI.enabled = CurvyEditorUtility.IsSingleSelection;
        if (GUILayout.Button(new GUIContent(mTexClonePath, "Create Clone Path"), GUILayout.ExpandWidth(false)))
            CurvySplinePathCloneBuilderInspector.CreateCloneBuilder();
        if (GUILayout.Button(new GUIContent(mTexMeshPath, "Create Mesh Path"), GUILayout.ExpandWidth(false)))
            CurvySplinePathMeshBuilderInspector.CreateMeshBuilder();
        GUILayout.EndHorizontal();
    }

   
}
