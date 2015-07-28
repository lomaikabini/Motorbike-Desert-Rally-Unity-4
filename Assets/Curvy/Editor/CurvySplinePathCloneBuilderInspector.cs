// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplinePathCloneBuilder))]
public class CurvySplinePathCloneBuilderInspector : Editor {

    SplinePathCloneBuilder Target { get { return target as SplinePathCloneBuilder; } }

    SerializedProperty tSpline;
    SerializedProperty tWorld;
    SerializedProperty tSource;
    SerializedProperty tGap;
    SerializedProperty tMode;
    SerializedProperty tAutoRefresh;
    SerializedProperty tAutoRefreshSpeed;

    Texture2D mTexRefresh;
    Texture2D mTexDetach;
    Texture2D mTexClear;

    void OnEnable()
    {
        tSpline = serializedObject.FindProperty("Spline");
        tWorld = serializedObject.FindProperty("UseWorldPosition");
        tSource = serializedObject.FindProperty("Source");
        tGap = serializedObject.FindProperty("Gap");
        tMode = serializedObject.FindProperty("Mode");
        tAutoRefresh = serializedObject.FindProperty("AutoRefresh");
        tAutoRefreshSpeed = serializedObject.FindProperty("AutoRefreshSpeed");

        mTexRefresh = Resources.Load("curvyrefresh") as Texture2D;
        mTexDetach = Resources.Load("curvydetach") as Texture2D;
        mTexClear = Resources.Load("curvyclear") as Texture2D;
    }


    
    static public void CreateCloneBuilder()
    {
        var path = SplinePathCloneBuilder.Create();
        if (Selection.activeGameObject) {
            CurvySplineBase spl = Selection.activeGameObject.GetComponent<CurvySplineBase>();
            
            if (spl)
                path.Spline = spl;
        }
        Selection.activeGameObject = path.gameObject;
    }

    public override void  OnInspectorGUI()
    {
        //DrawDefaultInspector();
        EditorGUILayout.PropertyField(tSpline, new GUIContent("Spline", "Spline or Spline Group to use"));
        EditorGUILayout.PropertyField(tWorld, new GUIContent("Use World Position", "Create clone path at spline's location?"));
        EditorGUILayout.PropertyField(tSource, new GUIContent("Source", "GameObjects/Transforms used for cloning"),true);
        EditorGUILayout.PropertyField(tGap, new GUIContent("Gap", "Gap between individual objects"));
        EditorGUILayout.PropertyField(tMode, new GUIContent("Mode", "Mode to handle multiple Sources"));
        
        EditorGUILayout.PropertyField(tAutoRefresh, new GUIContent("Auto Refresh", "Auto Refresh mesh when spline changes?"));
        EditorGUILayout.PropertyField(tAutoRefreshSpeed, new GUIContent("Auto Refresh Speed", "Refresh rate in seconds"));
        
        EditorGUILayout.LabelField("Path Info", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(new GUIContent("Objects: " + Target.ObjectCount,"# of cloned Sources"));
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (serializedObject.targetObject && serializedObject.ApplyModifiedProperties() || GUILayout.Button(new GUIContent(mTexRefresh, "Force Refresh"),GUILayout.ExpandWidth(false))) {
            Target.Refresh(true);
            SceneView.RepaintAll();
        }
        
        if (GUILayout.Button(new GUIContent(mTexClear,"Clear"),GUILayout.ExpandWidth(false)))
            Target.Clear();
        if (GUILayout.Button(new GUIContent(mTexDetach,"Clone to an individual GameObject"),GUILayout.ExpandWidth(false)))
            Selection.activeTransform=Target.Detach();
        EditorGUILayout.EndHorizontal();
    }
}
