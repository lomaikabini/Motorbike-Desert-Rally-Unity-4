// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplinePathMeshBuilder))]
public class CurvySplinePathMeshBuilderInspector : Editor
{

    SplinePathMeshBuilder Target { get { return target as SplinePathMeshBuilder; } }
    SerializedProperty tSpline;
    SerializedProperty tFrom;
    SerializedProperty tTo;
    SerializedProperty tFast;
    SerializedProperty tWorld;
    SerializedProperty tExtrusion;
    SerializedProperty tExtrusionParam;
    SerializedProperty tCapShape;
    SerializedProperty tCapWidth;
    SerializedProperty tCapHeight;
    SerializedProperty tCapHollow;
    SerializedProperty tCapSegments;
    SerializedProperty tStartCap;
    SerializedProperty tStartMesh;
    SerializedProperty tEndCap;
    SerializedProperty tEndMesh;
    SerializedProperty tUV;
    SerializedProperty tUVParam;
    SerializedProperty tCalcTangents;
    SerializedProperty tScale;
    SerializedProperty tScaleUserSlot;
    SerializedProperty tScaleModifierCurve;
    SerializedProperty tAutoRefresh;
    SerializedProperty tAutoRefreshSpeed;

    Texture2D mTexRefresh;
    Texture2D mTexDetach;
    Texture2D mTexSaveAsset;

    
    public static void CreateMeshBuilder()
    {
        var path = SplinePathMeshBuilder.Create();
        var prim = GameObject.CreatePrimitive(PrimitiveType.Plane);
#if UNITY_3_5_7
        prim.active = false;
#else
        prim.SetActive(false);
#endif
        path.GetComponent<MeshRenderer>().sharedMaterial = prim.GetComponent<MeshRenderer>().sharedMaterial;
        DestroyImmediate(prim);
        if (Selection.activeGameObject){
            CurvySplineBase spl=Selection.activeGameObject.GetComponent<CurvySplineBase>();
            if (spl)
                path.Spline = spl;
        }
        Selection.activeGameObject = path.gameObject;
    }

    void OnEnable()
    {
         tSpline=serializedObject.FindProperty("Spline");
         tFrom = serializedObject.FindProperty("FromTF");
         tTo = serializedObject.FindProperty("ToTF");
         tFast = serializedObject.FindProperty("FastInterpolation");
         tWorld = serializedObject.FindProperty("UseWorldPosition");
         tExtrusion = serializedObject.FindProperty("Extrusion");
         tExtrusionParam=serializedObject.FindProperty("ExtrusionParameter");
         tCapShape = serializedObject.FindProperty("CapShape");
         tCapWidth = serializedObject.FindProperty("CapWidth");
         tCapHeight = serializedObject.FindProperty("CapHeight");
         tCapHollow = serializedObject.FindProperty("CapHollow");
         tCapSegments = serializedObject.FindProperty("CapSegments");
         tStartCap = serializedObject.FindProperty("StartCap");
         tStartMesh = serializedObject.FindProperty("StartMesh");
         tEndCap = serializedObject.FindProperty("EndCap");
         tEndMesh = serializedObject.FindProperty("EndMesh");
         tUV = serializedObject.FindProperty("UV");
         tUVParam = serializedObject.FindProperty("UVParameter");
         tCalcTangents = serializedObject.FindProperty("CalculateTangents");
         tScale = serializedObject.FindProperty("ScaleModifier");
         tScaleUserSlot = serializedObject.FindProperty("ScaleModifierUserValueSlot");
         tScaleModifierCurve = serializedObject.FindProperty("ScaleModifierCurve");
         tAutoRefresh = serializedObject.FindProperty("AutoRefresh");
         tAutoRefreshSpeed = serializedObject.FindProperty("AutoRefreshSpeed");

         mTexRefresh = Resources.Load("curvyrefresh") as Texture2D;
         mTexDetach = Resources.Load("curvydetach") as Texture2D;
         mTexSaveAsset = Resources.Load("curvysave") as Texture2D;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(tSpline, new GUIContent("Spline", "Spline or Spline Group to use"));
        EditorGUILayout.PropertyField(tFrom,new GUIContent("From TF","Start TF"));
        EditorGUILayout.PropertyField(tTo,new GUIContent("To TF","End TF"));
        EditorGUILayout.PropertyField(tFast, new GUIContent("Fast Interpolation", "Use a linear approximation?"));
        EditorGUILayout.PropertyField(tWorld, new GUIContent("Use World Position", "Create Mesh at spline's location?"));
        EditorGUILayout.LabelField("Extrusion", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(tExtrusion,new GUIContent("Extrusion Mode","Vertex generation mode"));
        
        GUIContent paramDesc = new GUIContent("Extrusion Parameter");
        switch (tExtrusion.enumNames[tExtrusion.enumValueIndex]) {
            case "FixedF":
                tExtrusionParam.floatValue = Mathf.Clamp01(tExtrusionParam.floatValue);
                paramDesc = new GUIContent("Step Width","Step width in F");
                break;
            case "FixedDistance":
                if (Target.Spline)
                    tExtrusionParam.floatValue = Mathf.Clamp(tExtrusionParam.floatValue, 0, Target.Spline.Length);
                paramDesc = new GUIContent("Step Width", "Step width in world units");
                break;
            case "Adaptiv":
                paramDesc = new GUIContent("Angle", "Trigger angle in degrees");
                break;
        }
        
        EditorGUILayout.PropertyField(tExtrusionParam,paramDesc);
        EditorGUILayout.LabelField("Shape", EditorStyles.boldLabel);
        int idx = tCapShape.enumValueIndex;
        EditorGUILayout.PropertyField(tCapShape, new GUIContent("Shape"));
        // If we switch shape from any to Custom, clear (auto generated) meshes!
        if (idx != tCapShape.enumValueIndex && tCapShape.enumNames[tCapShape.enumValueIndex] == "Custom") {
            tStartMesh.objectReferenceValue = null;
            tEndMesh.objectReferenceValue = null;
        }
        
        switch (tCapShape.enumNames[tCapShape.enumValueIndex]) {
            case "Line":
                    EditorGUILayout.PropertyField(tCapWidth, new GUIContent("Shape Width"));
                    break;
            case "Rectangle":
                    EditorGUILayout.PropertyField(tCapWidth,new GUIContent("Shape Width"));
                    EditorGUILayout.PropertyField(tCapHeight, new GUIContent("Shape Height"));
                    EditorGUILayout.PropertyField(tCapHollow, new GUIContent("Hollow Shape","Percentage of hollowness"));
                    break;
            case "NGon":
                    EditorGUILayout.PropertyField(tCapSegments, new GUIContent("Shape Segments"));
                    EditorGUILayout.PropertyField(tCapWidth,new GUIContent("Shape Radius"));
                    EditorGUILayout.PropertyField(tCapHollow, new GUIContent("Hollow Shape", "Percentage of hollowness"));
                    break;
            case "Custom":
                    EditorGUILayout.PropertyField(tStartMesh, new GUIContent("Start Mesh","Used as start cap and for extrusion"));
                    EditorGUILayout.PropertyField(tEndMesh, new GUIContent("End Mesh","Used as end cap"));
                    break;
        }
        
        EditorGUILayout.PropertyField(tStartCap,new GUIContent("Start Cap","Close Start Cap?"));
        EditorGUILayout.PropertyField(tEndCap, new GUIContent("End Cap", "Close End Cap?"));
        
        EditorGUILayout.LabelField("UV Mapping", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(tUV, new GUIContent("UV Mode","UV Mapping Mode"));
        paramDesc = new GUIContent("UV Parameter");
        switch (tUV.enumNames[tUV.enumValueIndex]) {
            case "StretchV":
                paramDesc = new GUIContent("Tiling", "Tile V over total length");
                break;
            case "StretchVSegment":
                paramDesc = new GUIContent("Tiling", "Tile V over each segment");
                break;
            case "Absolute":
                paramDesc = new GUIContent("Step Width", "Tile V by world units");
                break;
        }
        EditorGUILayout.PropertyField(tUVParam,paramDesc);
        EditorGUILayout.PropertyField(tCalcTangents);
        EditorGUILayout.LabelField("Scale Modifier", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(tScale, new GUIContent("Scale Mode","How to modify scale"));
        
        switch (tScale.enumNames[tScale.enumValueIndex]) {
            case "UserValue":
                EditorGUILayout.PropertyField(tScaleUserSlot, new GUIContent("UserValue Slot", "The UserValue slot to use"));
                break;
            case "AnimationCurve":
                EditorGUILayout.PropertyField(tScaleModifierCurve, new GUIContent("Curve", "Scale curve"));
                break;
        }
        
        EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(tAutoRefresh,new GUIContent("Auto Refresh","Auto Refresh mesh when spline changes?"));
        EditorGUILayout.PropertyField(tAutoRefreshSpeed, new GUIContent("Auto Refresh Speed","Refresh rate in seconds"));
        
        EditorGUILayout.LabelField("Mesh Info", EditorStyles.boldLabel);
        GUILayout.Label(Target.VertexCount+" Vertices, " + Target.TriangleCount+" Triangles");
        GUILayout.Label("Last Refresh (ms): " + string.Format("{0:0.0000}",new object[]{Target.DebugPerfTime}));
        
        bool refresh=false;
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent(mTexRefresh,"Force Mesh Refresh"),GUILayout.ExpandWidth(false)))
            refresh=true;
        if (GUILayout.Button(new GUIContent(mTexDetach,"Clone to an individual GameObject"),GUILayout.ExpandWidth(false))) {
            Selection.activeTransform=Target.Detach();
        }
        
        if (GUILayout.Button(new GUIContent(mTexSaveAsset,"Save mesh as asset"),GUILayout.ExpandWidth(false))) 
                SaveMesh();

        EditorGUILayout.EndHorizontal();

        if (serializedObject.targetObject && serializedObject.ApplyModifiedProperties() || refresh) {
            Target.Refresh();
            SceneView.RepaintAll();
        }
    }

    public void SaveMesh()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Mesh", Target.Mesh.name + ".asset", "asset","Choose a file location");
        if (!string.IsNullOrEmpty(path)) {
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(Target.Mesh, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
