// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;
using Curvy.Utils;

[CustomEditor(typeof(CurvySpline)), CanEditMultipleObjects]
public class CurvySplineInspector : Editor {

    CurvySpline Target { get { return target as CurvySpline; } }
    bool IsActive
    {
        get { return Target.Transform == Selection.activeTransform; }
    }

    SerializedProperty tInterpolation;
    SerializedProperty tClosed;
    SerializedProperty tAutoEndTangents;
    SerializedProperty tInitialUp;
    SerializedProperty tOrientation;
    SerializedProperty tSetCPRotation;
    SerializedProperty tSwirl;
    SerializedProperty tSwirlTurns;
    SerializedProperty tGranularity;
    SerializedProperty tShowApprox;
    SerializedProperty tShowOrientation;
    SerializedProperty tShowTangents;
    SerializedProperty tAutoRefresh;
    SerializedProperty tAutoRefreshLength;
    SerializedProperty tAutoRefreshOrientation;
    SerializedProperty tT;
    SerializedProperty tC;
    SerializedProperty tB;
    SerializedProperty tShowGizmos;
    SerializedProperty tUserValueSize;
    SerializedProperty tShowUserValues;
    SerializedProperty tShowLabels;

    Texture2D mTexAlignWizard;
    Texture2D mTexExportWizard;
    Texture2D mTexCenterPivot;
    Texture2D mTexClonePath;
    Texture2D mTexMeshPath;
    Texture2D mTexPrefs;
    Texture2D mTexFlip;
    Texture2D mTexGroup;
    Texture2D mTexSelect;
    
    Texture2D mTexHelp;

    GUIStyle mUserValuesLabel;

    [MenuItem("GameObject/Create Other/Curvy/Spline",false,0)]
    static void CreateCurvySpline()
    {
        CurvySpline spl=CurvySpline.Create();
        spl.Interpolation=CurvyInterpolation.CatmulRom;
        spl.AutoEndTangents=true;
        spl.Closed=true;
        spl.Add(new Vector3(-2, -1, 0), new Vector3(0, 2, 0), new Vector3(2, -1, 0));
        Selection.activeObject = spl;
    }

    void OnEnable()
    {
        
        mTexAlignWizard = Resources.Load("curvyalignwiz") as Texture2D;
        mTexExportWizard = Resources.Load("curvyexportwiz") as Texture2D;
        mTexCenterPivot = Resources.Load("curvycenterpivot") as Texture2D;
        mTexClonePath = Resources.Load("curvyclonepath") as Texture2D;
        mTexMeshPath = Resources.Load("curvymeshpath") as Texture2D;
        mTexHelp = Resources.Load("curvyhelp") as Texture2D;
        mTexPrefs = Resources.Load("curvyprefs") as Texture2D;
        mTexFlip = Resources.Load("curvyflip") as Texture2D;
        mTexGroup = Resources.Load("curvygroup") as Texture2D;
        mTexSelect = Resources.Load("curvyselect") as Texture2D;
        CurvyPreferences.Get();

        tInterpolation = serializedObject.FindProperty("Interpolation");
        tClosed = serializedObject.FindProperty("Closed");
        tAutoEndTangents = serializedObject.FindProperty("AutoEndTangents");
        tInitialUp = serializedObject.FindProperty("InitialUpVector");
        tOrientation = serializedObject.FindProperty("Orientation");
        tSetCPRotation = serializedObject.FindProperty("SetControlPointRotation");
        tSwirl = serializedObject.FindProperty("Swirl");
        tSwirlTurns = serializedObject.FindProperty("SwirlTurns");
        tGranularity = serializedObject.FindProperty("Granularity");
        tShowApprox = serializedObject.FindProperty("ShowApproximation");
        tShowOrientation = serializedObject.FindProperty("ShowOrientation");
        tShowTangents = serializedObject.FindProperty("ShowTangents");
        tAutoRefresh = serializedObject.FindProperty("AutoRefresh");
        tAutoRefreshLength = serializedObject.FindProperty("AutoRefreshLength");
        tAutoRefreshOrientation = serializedObject.FindProperty("AutoRefreshOrientation");
        tT = serializedObject.FindProperty("Tension");
        tC = serializedObject.FindProperty("Continuity");
        tB = serializedObject.FindProperty("Bias");
        tShowGizmos = serializedObject.FindProperty("ShowGizmos");
        tUserValueSize = serializedObject.FindProperty("UserValueSize");
        tShowUserValues = serializedObject.FindProperty("ShowUserValues");
        tShowLabels = serializedObject.FindProperty("ShowLabels");

        Target.Refresh(true, true,false);
        SceneView.RepaintAll();

        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
    }

    void OnDisable()
    {
        EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
    }

    void OnSceneGUI()
    {
        Handles.BeginGUI();
        GUILayout.Window(Target.GetInstanceID(), new Rect(10, 40, 150, 20), DoWin, Target.name);

        mUserValuesLabel= new GUIStyle(EditorStyles.boldLabel);
        mUserValuesLabel.normal.textColor = Color.green;

        

        if (Target.UserValueSize>0 && Target.ShowUserValues) 
            foreach (CurvySplineSegment cp in Target.ControlPoints)
                UserValueReadOut(cp.Transform.position,cp.UserValues);

        if (Target.ShowLabels) {
            GUIStyle stLabel = new GUIStyle(EditorStyles.boldLabel);
            stLabel.normal.textColor = Color.white;
            Handles.Label(Target.Transform.position - new Vector3(-0.5f, 0.2f, 0), Target.name,stLabel);
            foreach (CurvySplineSegment cp in Target.ControlPoints)
                Handles.Label(cp.Position + new Vector3(-0.5f, HandleUtility.GetHandleSize(cp.Position) * 0.35f, 0), cp.name, stLabel);
        }

        Handles.EndGUI();

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.T && Target.ControlPointCount>0) {
            if (Event.current.shift)
                Selection.activeObject = Target.ControlPoints[Target.ControlPointCount - 1];
            else
                Selection.activeObject = Target.ControlPoints[0];
        }
      
    }

    

    void UserValueReadOut(Vector3 p, Vector3[] values)
    {
        string uvalues = "";
        for (int i = 0; i < values.Length; i++)
            uvalues += string.Format("{0}: {1}\n", new object[]{i, values[i]});
        Handles.Label(p + new Vector3(0, -0.2f, 0), uvalues, mUserValuesLabel);
    }

    void OnHierarchyWindowItemOnGUI(int instanceid, Rect selectionrect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceid) as GameObject;
        if (obj) {
            var cp = obj.GetComponent<CurvySplineSegment>();
            if (cp) {
                CurvyConnection con = cp.ConnectionAny;
                if (con != null) {
                    Color c = GUI.color;
                    switch (con.Sync) {
                        case CurvyConnection.SyncMode.NoSync: GUI.color = new Color(0, 0, 0); break;
                        case CurvyConnection.SyncMode.SyncPosAndRot: GUI.color = new Color(1, 1, 1); break;
                        case CurvyConnection.SyncMode.SyncRot: GUI.color = new Color(1, 1, 0); break;
                        case CurvyConnection.SyncMode.SyncPos: GUI.color = CurvySpline.GizmoColor; break;
                    }
                    GUI.DrawTexture(new Rect(selectionrect.xMax - 14, selectionrect.yMin + 4, 10, 10), mTexSelect);
                    GUI.color = c;
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        
        EditorGUILayout.PropertyField(tInterpolation,new GUIContent("Interpolation","Interpolation Method"));
        EditorGUILayout.PropertyField(tClosed,new GUIContent("Close Spline","Close spline?"));
        GUI.enabled = !tClosed.boolValue && tInterpolation.enumNames[tInterpolation.enumValueIndex]!="Linear" && tInterpolation.enumNames[tInterpolation.enumValueIndex]!="Bezier";
        EditorGUILayout.PropertyField(tAutoEndTangents,new GUIContent("Auto End Tangents","Handle End Control Points automatically?"));
        GUI.enabled = true;
        EditorGUILayout.PropertyField(tGranularity, new GUIContent("Granularity", "Approximation resolution"));
        tGranularity.intValue = Mathf.Max(1, tGranularity.intValue);
        EditorGUILayout.LabelField("Orientation", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(tOrientation, new GUIContent("Orientation", "How the Up-Vector should be calculated"));
        GUI.enabled = Target.Interpolation != CurvyInterpolation.Bezier;
        EditorGUILayout.PropertyField(tSetCPRotation, new GUIContent("Set CP Rotation","Rotate CP to match calculated Up-Vector3"));
        GUI.enabled = true;
        if (tOrientation.enumNames[tOrientation.enumValueIndex] == "Tangent") {
            EditorGUILayout.PropertyField(tInitialUp, new GUIContent("Initial Up-Vector", "How the first Up-Vector should be determined"));
            EditorGUILayout.PropertyField(tSwirl, new GUIContent("Swirl", "Orientation swirl mode"));
            if (tSwirl.enumNames[tSwirl.enumValueIndex] != "None")
                EditorGUILayout.PropertyField(tSwirlTurns, new GUIContent("Turns", "Swirl turns"));
        }
        EditorGUILayout.LabelField("Updates", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(tAutoRefresh, new GUIContent("Auto Refresh", "Refresh when Control Point position change?"));
        EditorGUILayout.PropertyField(tAutoRefreshLength, new GUIContent("Auto Refresh Length", "Recalculate Length on Refresh?"));
        EditorGUILayout.PropertyField(tAutoRefreshOrientation, new GUIContent("Auto Refresh Orientation", "Recalculate tangent normals and Up-Vectors on Refresh?"));
        
        if (tInterpolation.enumNames[tInterpolation.enumValueIndex] == "TCB") {
            EditorGUILayout.LabelField("TCB", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(tT, new GUIContent("Tension", "Tension for TCB-Spline"));
            EditorGUILayout.PropertyField(tC, new GUIContent("Continuity", "Continuity for TCB-Spline"));
            EditorGUILayout.PropertyField(tB, new GUIContent("Bias", "Bias for TCB-Spline"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Set Catmul", "Set TCB to match Catmul Rom"))) {
                tT.floatValue = 0; tC.floatValue = 0; tB.floatValue = 0;
            }
            if (GUILayout.Button(new GUIContent("Set Cubic", "Set TCB to match Simple Cubic"))) {
                tT.floatValue = -1; tC.floatValue = 0; tB.floatValue = 0;
            }
            if (GUILayout.Button(new GUIContent("Set Linear", "Set TCB to match Linear"))) {
                tT.floatValue = 0; tC.floatValue = -1; tB.floatValue = 0;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.LabelField("Miscellaneous", EditorStyles.boldLabel);
        int cnt = tUserValueSize.intValue;
        EditorGUILayout.PropertyField(tUserValueSize, new GUIContent("User Value Size", "Size of User Value array"));
        if (cnt != tUserValueSize.intValue)
            tUserValueSize.intValue = Mathf.Max(0, tUserValueSize.intValue);
        
        EditorGUILayout.LabelField("Gizmos & Handles",EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(tShowGizmos, new GUIContent("Show Gizmos", "Show Spline Gizmos"));
        EditorGUILayout.PropertyField(tShowApprox, new GUIContent("Show Approximation", "Show Approximation"));
        EditorGUILayout.PropertyField(tShowTangents, new GUIContent("Show Tangents", "Show Tangents"));
        EditorGUILayout.PropertyField(tShowOrientation, new GUIContent("Show Orientation", "Show Orientation"));
        EditorGUILayout.PropertyField(tShowLabels, new GUIContent("Show Labels", "Show Labels in Scene View"));
        EditorGUILayout.PropertyField(tShowUserValues, new GUIContent("Show User Values", "Show User Values in Scene View"));
        if (serializedObject.targetObject && serializedObject.ApplyModifiedProperties()) {
            Target.Refresh(true,true,false);
            SceneView.RepaintAll();
        }
        
        EditorGUILayout.LabelField("Spline Info",EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Total Length: " + Target.Length);
            EditorGUILayout.LabelField("Control Points: " + Target.ControlPointCount);
            EditorGUILayout.LabelField("Segments: " + Target.Count);
        
        EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
                GUI.enabled = CurvyEditorUtility.IsSingleSelection;
                if (GUILayout.Button(new GUIContent(mTexAlignWizard,"Align wizard"),GUILayout.ExpandWidth(false)))
                    CurvySplineAlignWizard.Create();
                if (GUILayout.Button(new GUIContent(mTexExportWizard,"Mesh Export wizard"),GUILayout.ExpandWidth(false)))
                    CurvySplineExportWizard.Create();
                GUI.enabled = true;
                if (GUILayout.Button(new GUIContent(mTexCenterPivot,"Center Pivot"),GUILayout.ExpandWidth(false))){
                    Undo.RegisterUndo(EditorUtility.CollectDeepHierarchy(new Object[]{Target}), "Center Spline Pivot");
                    CurvyUtility.centerPivot(Target);
                }
                if (GUILayout.Button(new GUIContent(mTexFlip, "Flip Direction"), GUILayout.ExpandWidth(false))) {
                    Undo.RegisterUndo(EditorUtility.CollectDeepHierarchy(new Object[] { Target }), "Flip Direction");
                    CurvyUtility.FlipSpline(Target);
                }
            EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Components", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
                GUI.enabled = CurvyEditorUtility.IsSingleSelection;
                if (GUILayout.Button(new GUIContent(mTexClonePath, "Create Clone Path"), GUILayout.ExpandWidth(false)))
                    CurvySplinePathCloneBuilderInspector.CreateCloneBuilder();
                if (GUILayout.Button(new GUIContent(mTexMeshPath, "Create Mesh Path"), GUILayout.ExpandWidth(false)))
                    CurvySplinePathMeshBuilderInspector.CreateMeshBuilder();
                GUI.enabled = true;
                if (GUILayout.Button(new GUIContent(mTexGroup, "Form SplineGroup"), GUILayout.ExpandWidth(false))) {
                    Undo.RegisterSceneUndo("Form SplineGroup");
                    Selection.activeObject=CurvySplineGroup.Create(CurvyEditorUtility.GetSelection<CurvySpline>());
                }
            GUILayout.EndHorizontal();
        EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
         
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent(mTexPrefs, "Preferences"), GUILayout.ExpandWidth(false)))
            CurvyPreferences.Open();
        
        if (GUILayout.Button(new GUIContent(mTexHelp, "Help"), GUILayout.ExpandWidth(false))) {
            var mnu = new GenericMenu();
            mnu.AddItem(new GUIContent("Online Manual"), false, new GenericMenu.MenuFunction(OnShowManual));
            mnu.AddItem(new GUIContent("Website"), false, new GenericMenu.MenuFunction(OnShowWeb));
            mnu.AddSeparator("");
            mnu.AddItem(new GUIContent("Report a bug"), false, new GenericMenu.MenuFunction(OnBugReport));
            mnu.ShowAsContext();
        }
        
        GUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Curvy v"+CurvySpline.Version, EditorStyles.miniLabel);

        
        //Repaint();   
    }

    void OnShowManual()
    {
        Application.OpenURL("http://docs.fluffyunderware.com/curvy");
    }

    void OnShowWeb()
    {
        Application.OpenURL("http://www.fluffyunderware.com/pages/unity-plugins/curvy.php");
    }

    void OnBugReport()
    {
        Application.OpenURL("mailto:bugreport@fluffyunderware.com?subject=[BUG] Curvy "+CurvySpline.Version+"&body=* Please give a brief description of the bug (please attach any screenshots or example projects that might be helpful) :%0A%0A* How to reproduce the bug:%0A%0A");
    }

    void DoWin(int id)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add")) {
            Undo.RegisterSceneUndo("Insert Control Point");
            Selection.activeTransform = Target.Add().transform;
        }
        GUI.enabled = Target.ControlPointCount > 0;
        if (GUILayout.Button("Clear") && EditorUtility.DisplayDialog("Clear Spline","Really remove all Control Points?","Yes","No"))
            Target.Clear();
        if (GUILayout.Button("First CP"))
            Selection.activeObject = Target.ControlPoints[0];
        GUI.enabled = true;
        GUILayout.EndHorizontal();
    }
}


