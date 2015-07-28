// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;
using Curvy.Utils;

[CustomEditor(typeof(CurvySplineSegment)), CanEditMultipleObjects]
public class CurvySplineSegmentInspector : Editor {
    public static float ConstraintSplineLength;
    public static bool ConstrainXAxis;
    public static bool ConstrainYAxis;
    public static bool ConstrainZAxis;
    public static float SmoothingOffset = 0.3f;

    CurvySplineSegment Target { get { return target as CurvySplineSegment; } }

    
    bool mValid;
    SerializedProperty tSmoothTangent;
    SerializedProperty tSyncStartEnd;
    SerializedProperty tT0;
    SerializedProperty tB0;
    SerializedProperty tC0;
    SerializedProperty tT1;
    SerializedProperty tB1;
    SerializedProperty tC1;
    SerializedProperty tOT;
    SerializedProperty tOB;
    SerializedProperty tOC;
    SerializedProperty tHandleIn;
    SerializedProperty tHandleOut;
    SerializedProperty tFreeHandles;
    SerializedProperty tHandleScale;

    Texture2D mTexSetFirstCP;
    Texture2D mTexConstraints;
    Texture2D mTexSplit;
    Texture2D mTexJoin;
    Texture2D mTexConnection;
    Texture2D mTexDelete;
    Texture2D mTexSelect;

    
    bool customMoveTool;

    bool IsActive
    {
        get { return Target.Transform == Selection.activeTransform; }
    }

    bool SceneIsSelected
    {
        get
        {
            return SceneView.focusedWindow == SceneView.currentDrawingSceneView;
        }
    }

    void OnEnable()
    {
        CurvyPreferences.Get();
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        tSmoothTangent = serializedObject.FindProperty("SmoothEdgeTangent");
        tSyncStartEnd = serializedObject.FindProperty("SynchronizeTCB");
        tT0 = serializedObject.FindProperty("StartTension");
        tC0 = serializedObject.FindProperty("StartContinuity");
        tB0 = serializedObject.FindProperty("StartBias");
        tT1 = serializedObject.FindProperty("EndTension");
        tC1 = serializedObject.FindProperty("EndContinuity");
        tB1 = serializedObject.FindProperty("EndBias");
        tOT = serializedObject.FindProperty("OverrideGlobalTension");
        tOC = serializedObject.FindProperty("OverrideGlobalContinuity");
        tOB = serializedObject.FindProperty("OverrideGlobalBias");
        tHandleIn = serializedObject.FindProperty("HandleIn");
        tHandleOut = serializedObject.FindProperty("HandleOut");
        tFreeHandles = serializedObject.FindProperty("FreeHandles");
        tHandleScale = serializedObject.FindProperty("HandleScale");

        mTexSetFirstCP = Resources.Load("curvysetfirstcp") as Texture2D;
        mTexConstraints = Resources.Load("curvyconstraints") as Texture2D;
        mTexSplit = Resources.Load("curvysplit") as Texture2D;
        mTexJoin = Resources.Load("curvyjoin") as Texture2D;
        mTexConnection = Resources.Load("curvyconnection") as Texture2D;
        mTexDelete = Resources.Load("curvydelete") as Texture2D;
        mTexSelect = Resources.Load("curvyselect") as Texture2D;
        
    }

    void OnDisable()
    {
        EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
        if (Tools.current == Tool.None)
            Tools.current = Tool.Move;
    }
    
    void OnSceneGUI()
    {
        Handles.ArrowCap(0, Target.Position, Quaternion.LookRotation(Target.Transform.up), 2);

        // Handle custom Tools
        if (Tools.current == Tool.Rotate || Tools.current == Tool.Scale || Tools.current == Tool.View)
            customMoveTool = false;
        else {
            Tools.current = Tool.None;
            customMoveTool = true;
        }

        if (IsActive) {
            bool hasLocalHandles = Target.Spline.Interpolation == CurvyInterpolation.Bezier;
            int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
            
          
            if (customMoveTool) {
                Vector3 delta=CurvyEditorUtility.PositionHandle(controlID,Tools.handlePosition, Tools.handleRotation,1f,
                                                                ConstrainXAxis,
                                                                ConstrainYAxis,
                                                                ConstrainZAxis)-Tools.handlePosition;
                if (GUI.changed) {
                    Transform[] transforms = Selection.transforms;
                    for (int i = 0; i < transforms.Length; i++) 
                        transforms[i].position += delta;
                    Target.Spline.Refresh(true, true, false);
                    if (ConstraintSplineLength > 0 && Target.Spline.Length > ConstraintSplineLength) {
                        for (int i = 0; i < transforms.Length; i++)
                            transforms[i].position -= delta;
                        Target.Spline.Refresh(true, true, false);
                    }
                }

            }
            
            
            // Bezier-Handles
            if (hasLocalHandles) {
                Handles.color = Color.grey;
                Vector3 handleOut = Target.HandleOutPosition;
                Vector3 handleIn = Target.HandleInPosition;


                Target.HandleOutPosition = CurvyEditorUtility.PositionHandle(GUIUtility.GetControlID(FocusType.Passive), Target.HandleOutPosition, Tools.handleRotation,0.5f,
                                                                ConstrainXAxis,
                                                                ConstrainYAxis,
                                                                ConstrainZAxis);
                Handles.CubeCap(0, Target.HandleOutPosition, Quaternion.identity, HandleUtility.GetHandleSize(Target.HandleOutPosition) * 0.1f);
                Handles.DrawLine(Target.HandleOutPosition, Target.Position);

                Target.HandleInPosition = CurvyEditorUtility.PositionHandle(GUIUtility.GetControlID(FocusType.Passive), Target.HandleInPosition, Tools.handleRotation,0.5f,
                                                                ConstrainXAxis,
                                                                ConstrainYAxis,
                                                                ConstrainZAxis);
                Handles.CubeCap(0, Target.HandleInPosition, Quaternion.identity, HandleUtility.GetHandleSize(Target.HandleInPosition) * 0.1f);
                Handles.DrawLine(Target.HandleInPosition, Target.Position);

                if (GUI.changed) {
                    Target.Spline.RefreshImmediately(true, true, false);
                    if (ConstraintSplineLength > 0 && Target.Spline.Length > ConstraintSplineLength) {
                        Target.HandleOutPosition = handleOut;
                        Target.HandleInPosition = handleIn;
                        Target.Spline.RefreshImmediately(true, true, false);
                    }
                }
            }
            
            // Window
            Handles.BeginGUI();
            GUILayout.Window(GetInstanceID(), new Rect(10, 40, 150, 20), DoWin, Target.name);
            Handles.EndGUI();

            // Shortcut-Keys

            if (CurvyPreferences.kNext.IsKeyDown(Event.current) && Target.NextControlPoint)
                Selection.activeObject = Target.NextControlPoint;
            else if (CurvyPreferences.kPrev.IsKeyDown(Event.current) && Target.PreviousControlPoint)
                Selection.activeObject = Target.PreviousControlPoint;
            else if (CurvyPreferences.kInsertBefore.IsKeyDown(Event.current))
                InsBefore();
            else if (CurvyPreferences.kInsertAfter.IsKeyDown(Event.current))
                InsAfter();
            else if (CurvyPreferences.kDelete.IsKeyDown(Event.current))
                Delete();
            else if (CurvyPreferences.kToggleFreeMove.IsKeyDown(Event.current))
                Target.FreeHandles = !Target.FreeHandles;
        }
    }

    void OnHierarchyWindowItemOnGUI(int instanceid, Rect selectionrect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceid) as GameObject;
        if (obj) {
            var cp = obj.GetComponent<CurvySplineSegment>();
            if (cp) {
                CurvyConnection con = cp.ConnectionAny;
                if (con!=null) {
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
        if (targets.Length==1 && Target && Target.gameObject.GetInstanceID() == instanceid) {
            // Shortcut-Keys
            if (CurvyPreferences.kNext.IsKeyDown(Event.current) && Target.NextControlPoint) 
                Selection.activeObject = Target.NextControlPoint;
            else if (CurvyPreferences.kPrev.IsKeyDown(Event.current) && Target.PreviousControlPoint)
                Selection.activeObject = Target.PreviousControlPoint;
            else if (CurvyPreferences.kInsertBefore.IsKeyDown(Event.current))
                InsBefore();
            else if (CurvyPreferences.kInsertAfter.IsKeyDown(Event.current))
                InsAfter();
            else if (CurvyPreferences.kDelete.IsKeyDown(Event.current)) {
                Delete();
                EditorGUIUtility.ExitGUI();
            }
            else if (CurvyPreferences.kToggleFreeMove.IsKeyDown(Event.current))
                Target.FreeHandles = !Target.FreeHandles;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfDirtyOrScript();
        if (Event.current.type == EventType.Layout)
            mValid = Target.IsValidSegment;

        if (mValid && (Target.Spline.Closed || !Target.IsFirstSegment)) 
            EditorGUILayout.PropertyField(tSmoothTangent, new GUIContent("Smooth End Tangent", "Smooth end tangent?"));

        if (Target.Spline.Interpolation == CurvyInterpolation.Bezier) {
            EditorGUILayout.LabelField("Handles", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(tFreeHandles, new GUIContent("Free Move", "Move Handles individually?"));
            EditorGUILayout.Slider(tHandleScale, 0, 10, new GUIContent("Scale","Handle Scaling"));
       
            Vector3 v = tHandleIn.vector3Value;
            EditorGUILayout.PropertyField(tHandleIn);
            if (v != tHandleIn.vector3Value && !tFreeHandles.boolValue)
                tHandleOut.vector3Value = -tHandleIn.vector3Value;
            v = tHandleIn.vector3Value;
            EditorGUILayout.PropertyField(tHandleOut);
            if (v != tHandleOut.vector3Value && !tFreeHandles.boolValue)
                tHandleIn.vector3Value = -tHandleOut.vector3Value;

            EditorGUILayout.LabelField("Smooth Handles", EditorStyles.boldLabel);
            
            SmoothingOffset = EditorGUILayout.Slider(new GUIContent("Offset","Smoothing Offset"),SmoothingOffset, 0.1f, 1f);
            if (GUILayout.Button(new GUIContent("Smooth","Set Handles by Catmul-Rom"))) {
                Undo.RegisterUndo(targets,"Smooth Bezier Handles");
                foreach (CurvySplineSegment tgt in targets) {
                    CurvyUtility.InterpolateBezierHandles(CurvyInterpolation.CatmulRom, SmoothingOffset, tgt.FreeHandles, tgt);
                }
                Target.Spline.RefreshImmediately(true, true, false);
                SceneView.RepaintAll();
            }
            
        }

        
        if (mValid && Target.Spline.Interpolation == CurvyInterpolation.TCB) {
            EditorGUILayout.PropertyField(tSyncStartEnd, new GUIContent("Synchronize TCB","Synchronize Start and End Values"));
            EditorGUILayout.PropertyField(tOT, new GUIContent("Local Tension","Override Spline Tension?"));
            if (tOT.boolValue) {
                EditorGUILayout.PropertyField(tT0, Target.SynchronizeTCB ? new GUIContent("Tension","Tension") : new GUIContent("Start Tension","Start Tension"));
                if (!Target.SynchronizeTCB)
                    EditorGUILayout.PropertyField(tT1, new GUIContent("End Tension", "End Tension"));
                else
                    tT1.floatValue = tT0.floatValue;
            }
            EditorGUILayout.PropertyField(tOC, new GUIContent("Local Continuity","Override Spline Continuity?"));
            if (tOC.boolValue) {
                EditorGUILayout.PropertyField(tC0, Target.SynchronizeTCB ? new GUIContent("Continuity", "Continuity") : new GUIContent("Start Continuity", "Start Continuity"));
                if (!Target.SynchronizeTCB)
                    EditorGUILayout.PropertyField(tC1, new GUIContent("End Continuity","End Continuity"));
                else
                    tC1.floatValue = tC0.floatValue;
            }
            EditorGUILayout.PropertyField(tOB, new GUIContent("Local Bias","Override Spline Bias?"));
            if (tOB.boolValue) {
                EditorGUILayout.PropertyField(tB0, Target.SynchronizeTCB ? new GUIContent("Bias", "Bias") : new GUIContent("Start Bias", "Start Bias"));
                if (!Target.SynchronizeTCB)
                    EditorGUILayout.PropertyField(tB1, new GUIContent("End Bias","End Bias"));
                else
                    tB1.floatValue = tB0.floatValue;
            }

            if (tOT.boolValue || tOC.boolValue || tOB.boolValue) {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Set Catmul")) {
                    tT0.floatValue = 0; tC0.floatValue = 0; tB0.floatValue = 0;
                    tT1.floatValue = 0; tC1.floatValue = 0; tB1.floatValue = 0;
                }
                if (GUILayout.Button("Set Cubic")) {
                    tT0.floatValue = -1; tC0.floatValue = 0; tB0.floatValue = 0;
                    tT1.floatValue = -1; tC1.floatValue = 0; tB1.floatValue = 0;
                }
                if (GUILayout.Button("Set Linear")) {
                    tT0.floatValue = 0; tC0.floatValue = -1; tB0.floatValue = 0;
                    tT1.floatValue = 0; tC1.floatValue = -1; tB1.floatValue = 0;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        

        if (Target.UserValues != null && Target.UserValues.Length > 0) {
            EditorGUILayout.LabelField("User Values", EditorStyles.boldLabel);
            ArrayGUI(serializedObject, "UserValues", false);
        }

        
        if ((Target.Connection!=null || Target.ConnectedBy.Count > 0)) {
            GUILayout.Label("Connections", EditorStyles.boldLabel);
            ConnectionGUI();
        }
        
        GUILayout.Label("Tools", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent(mTexConstraints, "Constraints Tools"), GUILayout.ExpandWidth(false))) {
            CurvyConstraintsWin.Create();
        }
        GUI.enabled = CurvyEditorUtility.IsSingleSelection && Target.ControlPointIndex > 0;
        if (GUILayout.Button(new GUIContent(mTexSetFirstCP, "Set as first Control Point"), GUILayout.ExpandWidth(false))) {
            Undo.RegisterSceneUndo("Set First Control Point");
            CurvyUtility.setFirstCP(Target);
        }
        GUI.enabled = CurvyEditorUtility.IsSingleSelection && Target.IsValidSegment && !Target.IsFirstSegment;
        if (GUILayout.Button(new GUIContent(mTexSplit, "Split Spline"), GUILayout.ExpandWidth(false))) {
            Undo.RegisterSceneUndo("Split Spline");
            CurvyUtility.SplitSpline(Target);
        }
        GUI.enabled = CurvyEditorUtility.IsJoinSelection;
        if (GUILayout.Button(new GUIContent(mTexJoin, CurvyEditorUtility.JoinSelectionInfo), GUILayout.ExpandWidth(false))) {
            Undo.RegisterSceneUndo("Join Spline");
            var srcSeg = CurvyEditorUtility.JoinSource;
            var dstSeg = CurvyEditorUtility.JoinTarget;
            CurvyUtility.JoinSpline(srcSeg,dstSeg);
            Selection.activeTransform = dstSeg.Transform;
            SceneView.RepaintAll();
        }
        
        GUI.enabled = CurvyEditorUtility.IsConnectSelection;
        if (GUILayout.Button(new GUIContent(mTexConnection, CurvyEditorUtility.ConnectSelectionInfo), GUILayout.ExpandWidth(false))) {
            Undo.RegisterSceneUndo("Connect");
            var srcSeg = CurvyEditorUtility.ConnectionSource;
            var dstSeg = CurvyEditorUtility.ConnectionTarget;
            srcSeg.ConnectTo(dstSeg);
            srcSeg.SyncConnections();
            srcSeg.Spline.RefreshImmediately();
            dstSeg.Spline.RefreshImmediately();
            Selection.activeTransform = dstSeg.Transform;
        }
        
        GUI.enabled = true;

        GUILayout.EndHorizontal();
        
        if ((serializedObject.targetObject && serializedObject.ApplyModifiedProperties())) {
            Target.Spline.Refresh(true,true,false);
            SceneView.RepaintAll();
        }

        EditorGUILayout.LabelField("Segment Info", EditorStyles.boldLabel);
        if (mValid) {
                EditorGUILayout.LabelField("Distance: " + Target.Distance);
                EditorGUILayout.LabelField("Length: " + Target.Length);
        }
        EditorGUILayout.LabelField("Spline Length: " + Target.Spline.Length);
        
    }

    void ConnectionGUI()
    {
        if (Target.Connection!=null)
            ConnectionDetailsGUI(Target);

        
        for (int i = 0; i < Target.ConnectedBy.Count; i++) {
            ConnectionDetailsGUI(Target.ConnectedBy[i]);
        }
    }
    
    void ConnectionDetailsGUI(CurvySplineSegment seg)
    {
        if (!seg || seg.Connection==null) return;
        EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(mTexSelect, "Select Other"), GUILayout.ExpandWidth(false))) {
                Selection.activeObject = (seg == Target) ? seg.Connection.Other : seg;
            }
            EditorGUILayout.LabelField(seg.Spline.name + "->" + seg.Connection.Other.Spline.name + "." + seg.Connection.Other.name);
            if (GUILayout.Button(new GUIContent(mTexDelete, "Delete Connection"), GUILayout.ExpandWidth(false))) {
                Undo.RegisterSceneUndo("Delete Connection");
                seg.ConnectTo(null);
                SceneView.RepaintAll();
                return;
            }
        EditorGUILayout.EndHorizontal();
        seg.Connection.Heading = (CurvyConnection.HeadingMode)EditorGUILayout.EnumPopup(new GUIContent("Heading", "Heading Mode"), seg.Connection.Heading);
        seg.Connection.Sync = (CurvyConnection.SyncMode)EditorGUILayout.EnumPopup(new GUIContent("Synchronization", "Synchronization Mode"), seg.Connection.Sync);
        seg.Connection.Tags = EditorGUILayout.TextField(new GUIContent("Tags", "Identifier tags (space separated)"), seg.Connection.Tags);

        if (GUI.changed) {
            seg.SyncConnections();
            if (seg.Connection.Active)
                seg.Connection.Other.Spline.Refresh();
        }
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
    }
    
    void DoWin(int id)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Ins Before", "Shift-G")))
            InsBefore();

        if (GUILayout.Button(new GUIContent("Ins After", "G")))
            InsAfter();

        if (GUILayout.Button(new GUIContent("Delete", "H")))
            Delete();

        if (Target) {
            if (Target.PreviousTransform && GUILayout.Button(new GUIContent("Prev", "Shift-T")))
                Selection.activeTransform = Target.PreviousTransform;
            if (Target.NextTransform && GUILayout.Button(new GUIContent("Next", "T")))
                Selection.activeTransform = Target.NextTransform;
            if (GUILayout.Button("Spline"))
                Selection.activeTransform = Target.Spline.transform;
        }
        GUILayout.EndHorizontal();
        // TCB
        if (Target.Spline.Interpolation == CurvyInterpolation.TCB) {
            GUILayout.BeginHorizontal();
            Target.OverrideGlobalTension = GUILayout.Toggle(Target.OverrideGlobalTension, "T", GUILayout.ExpandWidth(false));
            if (Target.OverrideGlobalTension) {
                Target.StartTension = GUILayout.HorizontalSlider(Target.StartTension, -1, 1);
                if (!Target.SynchronizeTCB)
                    Target.EndTension = GUILayout.HorizontalSlider(Target.EndTension, -1, 1);
                else
                    Target.EndTension = Target.StartTension;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            
            Target.OverrideGlobalContinuity = GUILayout.Toggle(Target.OverrideGlobalContinuity, "C", GUILayout.ExpandWidth(false));
            
            if (Target.OverrideGlobalContinuity) {
                
                Target.StartContinuity = GUILayout.HorizontalSlider(Target.StartContinuity, -1, 1);
                if (!Target.SynchronizeTCB)
                    Target.EndContinuity = GUILayout.HorizontalSlider(Target.EndContinuity, -1, 1);
                else
                    Target.EndContinuity = Target.StartContinuity;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Target.OverrideGlobalBias = GUILayout.Toggle(Target.OverrideGlobalBias, "B", GUILayout.ExpandWidth(false));
            if (Target.OverrideGlobalBias) {
                Target.StartBias = GUILayout.HorizontalSlider(Target.StartBias, -1, 1);
                if (!Target.SynchronizeTCB)
                    Target.EndBias = GUILayout.HorizontalSlider(Target.EndBias, -1, 1);
                else
                    Target.EndBias = Target.StartBias;
            }
            GUILayout.EndHorizontal();
        }
        
        if (GUI.changed && Target) {
            EditorUtility.SetDirty(Target);
            Target.Spline.Refresh(true, true,false);
            SceneView.RepaintAll();
        }

    }

    #region ### Commands ###

    void InsBefore()
    {
        Undo.RegisterSceneUndo("Insert Control Point");
        
        Transform t;
        if (!Target.PreviousControlPoint && SceneIsSelected) {
            Vector3 p = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).GetPoint((SceneView.currentDrawingSceneView.camera.transform.position - Target.Position).magnitude);
            if (ConstrainXAxis)
                p.x = Target.Position.x;
            if (ConstrainYAxis)
                p.y = Target.Position.y;
            if (ConstrainZAxis)
                p.z = Target.Position.z;
            t = Target.Spline.Add(false, Target).transform;
            t.position = p;
        } else
            t = Target.Spline.Add(false, Target).transform;
        Target.Spline.RefreshImmediately();
        Selection.activeTransform = t;
    }

    void InsAfter()
    {
        Undo.RegisterSceneUndo("Insert Control Point");
        Transform t;
        if (!Target.NextControlPoint && SceneIsSelected) {
            Vector3 p = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).GetPoint((SceneView.currentDrawingSceneView.camera.transform.position - Target.Position).magnitude);
            if (ConstrainXAxis)
                p.x = Target.Position.x;
            if (ConstrainYAxis)
                p.y = Target.Position.y;
            if (ConstrainZAxis)
                p.z = Target.Position.z;
            t = Target.Spline.Add(p)[0].transform;
        } else
            t=Target.Spline.Add(Target).transform;
        Undo.RegisterCreatedObjectUndo(t, "Test");
        Target.Spline.RefreshImmediately();
        Selection.activeTransform = t;
    }

    void Delete()
    {
        Undo.RegisterSceneUndo("Delete Control Point");
        Selection.activeTransform = (Target.PreviousTransform) ? Target.PreviousTransform : Target.NextTransform;
        Target.Delete();
        
    }

    #endregion

    #region ### Helpers ###

    void ArrayGUI(SerializedObject obj, string name, bool resizeable)
    {

        int size = obj.FindProperty(name + ".Array.size").intValue;
        int newSize = size;
        if (resizeable) {
            newSize = EditorGUILayout.IntField(" Size", size);
            if (newSize != size)
                obj.FindProperty(name + ".Array.size").intValue = newSize;
        }
        EditorGUI.indentLevel = 3;
        for (int i = 0; i < newSize; i++) {
            var prop = obj.FindProperty(string.Format("{0}.Array.data[{1}]", new object[]{name, i}));
            EditorGUILayout.PropertyField(prop,true);
        }
        EditorGUI.indentLevel = 0;
    }

    #endregion
}
