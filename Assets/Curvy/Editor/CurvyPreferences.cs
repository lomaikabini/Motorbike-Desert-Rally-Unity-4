// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;
using System.Reflection;
using Curvy.Utils;
public class CurvyPreferences
{
    static bool prefsLoaded = false;

    public static Color GizmoColor = Color.red;
    public static Color GizmoSelectionColor = Color.white;
    public static float GizmoControlPointSize = 0.15f;
    public static float GizmoOrientationLength = 1f;
    public static EditorKeyDefinition kNext= new EditorKeyDefinition("CurvyCPNext", KeyCode.T, false, false, false, false);
    public static EditorKeyDefinition kPrev = new EditorKeyDefinition("CurvyCPPrev", KeyCode.T, true, false, false, false);
    public static EditorKeyDefinition kInsertAfter = new EditorKeyDefinition("CurvyCPInsertAfter", KeyCode.G, false, false, false,false);
    public static EditorKeyDefinition kInsertBefore = new EditorKeyDefinition("CurvyCPInsertBefore", KeyCode.G, true, false, false,false);
    public static EditorKeyDefinition kDelete = new EditorKeyDefinition("CurvyCPDelete", KeyCode.H, false, false, false, false);
    public static EditorKeyDefinition kToggleFreeMove = new EditorKeyDefinition("CurvyBezierToggleFree", KeyCode.N, false, false, false, false);

    public static void Open()
    {
        var asm = Assembly.GetAssembly(typeof(EditorWindow));
        var T=asm.GetType("UnityEditor.PreferencesWindow");
        var M=T.GetMethod("ShowPreferencesWindow", BindingFlags.NonPublic|BindingFlags.Static);
        M.Invoke(null, null);
    }


    [PreferenceItem("Curvy")]
    public static void PreferencesGUI()
    {
        if (!prefsLoaded) 
            Load();
        
        EditorGUILayout.LabelField("Gizmos", EditorStyles.boldLabel);
        GizmoColor = EditorGUILayout.ColorField("Spline color", GizmoColor);
        GizmoSelectionColor = EditorGUILayout.ColorField("Spline Selection color", GizmoSelectionColor);
        GizmoControlPointSize = EditorGUILayout.FloatField("Control Point Size", GizmoControlPointSize);
        GizmoOrientationLength = EditorGUILayout.FloatField("Orientation Length", GizmoOrientationLength);
        EditorGUILayout.LabelField("Shortcuts", EditorStyles.boldLabel);
        kNext.OnGUI("Control Point: Next");
        kPrev.OnGUI("Control Point: Previous");
        kInsertAfter.OnGUI("Control Point: Insert After");
        kInsertBefore.OnGUI("Control Point: Insert Before");
        kDelete.OnGUI("Control Point: Delete");
        kToggleFreeMove.OnGUI("Bezier: Toggle Free Move");

        if (GUI.changed)
            Save();
    }

    static void Load()
    {
        if (!EditorPrefs.HasKey("Curvy_ControlPointSize"))
            Save();
        GizmoColor = String2Color(EditorPrefs.GetString("Curvy_GizmoColor","1;0;0;1"));
        GizmoSelectionColor = String2Color(EditorPrefs.GetString("Curvy_GizmoSelectionColor","1;1;1;1"));
        GizmoControlPointSize = EditorPrefs.GetFloat("Curvy_ControlPointSize", 0.15f);
        GizmoOrientationLength = EditorPrefs.GetFloat("Curvy_OrientationLength", 1);
        prefsLoaded = true;
    }

    static void Save()
    {
        EditorPrefs.SetString("Curvy_GizmoColor",Color2String(GizmoColor));
        EditorPrefs.SetString("Curvy_GizmoSelectionColor",Color2String(GizmoSelectionColor));
        EditorPrefs.SetFloat("Curvy_ControlPointSize", GizmoControlPointSize);
        EditorPrefs.SetFloat("Curvy_OrientationLength", GizmoOrientationLength);
        kNext.SaveToPrefs();
        kPrev.SaveToPrefs();
        kInsertBefore.SaveToPrefs();
        kInsertAfter.SaveToPrefs();
        kDelete.SaveToPrefs();
    }

    public static void Get()
    {
        if (!prefsLoaded)
            Load();
        CurvySpline.GizmoColor = GizmoColor;
        CurvySpline.GizmoSelectionColor = GizmoSelectionColor;
        CurvySpline.GizmoControlPointSize = GizmoControlPointSize;
        CurvySpline.GizmoOrientationLength = GizmoOrientationLength;
    }

    public static string Color2String(Color c)
    {
        return string.Format("{0};{1};{2};{3}", new object[]{c.r, c.g, c.b, c.a});
    }

    public static Color String2Color(string s)
    {
        s=s.Replace(',', '.');
        string[] array = s.Split(';');
        if (array.Length != 4)
            return Color.white;

        float r, g, b, a;
        float.TryParse(array[0], out r);
        float.TryParse(array[1], out g);
        float.TryParse(array[2], out b);
        float.TryParse(array[3], out a);
        return new Color(r, g, b, a);
    }
}