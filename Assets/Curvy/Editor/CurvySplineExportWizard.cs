// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;
using Curvy.Utils;

public class CurvySplineExportWizard : EditorWindow {

    CurvySpline Spline;
    bool Planar;
    int IgnoreAxis;
    Vector3[] Vertices = new Vector3[0];
    int[] Triangles=new int[0];
    string MeshName="CurvyMesh";
    bool AutoClose = true;
    public enum VertexCalcMode { UseGranularity, AngleDiff }
    VertexCalcMode CalcMode = VertexCalcMode.UseGranularity;
    float calcAngle = 1;
    Material MeshMaterial;
    Matrix4x4 splMatrix;

    static public void Create()
    {
        var win = GetWindow<CurvySplineExportWizard>(true, "Export Curvy Spline", true);
        win.Init(Selection.activeGameObject.GetComponent<CurvySpline>());
        win.maxSize = new Vector2(400, 275);
        win.minSize = win.maxSize;
        
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
        if (Spline) 
            Spline.OnRefresh -= OnSplineRefresh;
        
        Spline = null;
        if (Selection.activeGameObject) {
            var spl = Selection.activeGameObject.GetComponent<CurvySpline>();
            if (spl)
                Init(spl);
            var cp = Selection.activeGameObject.GetComponent<CurvySplineSegment>();
            if (cp)
                Init(cp.Spline);
        }
        Repaint();
    }

    void OnSplineRefresh(CurvySplineBase spline)
    {
        Planar = CurvyUtility.isPlanar(Spline, out IgnoreAxis);
        if (Planar)
            Triangulate();
        Repaint();
    }

    void Init(CurvySpline spline)
    {
        Spline = spline;
        splMatrix = spline.Transform.localToWorldMatrix;
        Planar = CurvyUtility.isPlanar(Spline,out IgnoreAxis);
        if (Planar)
            Triangulate();
        Repaint();
        Spline.OnRefresh += OnSplineRefresh;
    }

    void OnGUI()
    {
        if (!Spline) {
            GUILayout.Label("Please select a Curvy Spline!", EditorStyles.boldLabel);
            return;
        }
        if (!Planar) {
            GUILayout.Label("Can't export! The spline needs to be planar!",EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Equalize X")) {
                Undo.RegisterUndo(EditorUtility.CollectDeepHierarchy(new Object[] { Spline }), "Equalize X");
                CurvyUtility.makePlanar(Spline, 0);
            }
            if (GUILayout.Button("Equalize Y")) {
                Undo.RegisterUndo(EditorUtility.CollectDeepHierarchy(new Object[] { Spline }), "Equalize Y");
                CurvyUtility.makePlanar(Spline, 1);
            }
            if (GUILayout.Button("Equalize Z")) {
                Undo.RegisterUndo(EditorUtility.CollectDeepHierarchy(new Object[] { Spline }), "Equalize Z");
                CurvyUtility.makePlanar(Spline, 2);
            }
            GUILayout.EndHorizontal();
            return;
        }
        GUILayout.Label("Please note:",EditorStyles.boldLabel);
        GUILayout.Label("Depending on curvation and vertices settings the resulting mesh might produce errors when fed to the MeshBuilder as a custom shape! The triangulation process will be optimized in the future!", EditorStyles.wordWrappedLabel);
        GUILayout.Space(5);
        GUILayout.Label("Options", EditorStyles.boldLabel);
        MeshName=EditorGUILayout.TextField("Object/Mesh Name",MeshName);
        MeshMaterial = EditorGUILayout.ObjectField(new GUIContent("Material"), MeshMaterial, typeof(Material), true) as Material;
        
        CalcMode = (VertexCalcMode)EditorGUILayout.EnumPopup(new GUIContent("Edge Vertices", "How to create Edge Vertices"), CalcMode);
        if (CalcMode == VertexCalcMode.AngleDiff)
            calcAngle = EditorGUILayout.FloatField(new GUIContent("Angle", "Angle Difference"), calcAngle);
        
        if (!Spline.Closed)
            AutoClose = EditorGUILayout.Toggle(new GUIContent("Close Edge Loop", "Connect First&last edge vertex"), AutoClose);
        if (GUILayout.Button("Preview"))
            Triangulate();
        if (Vertices.Length > 0) {
            GUILayout.Label(string.Format("Mesh Info: {0} Vertices, {1} Triangles", new object[]{Vertices.Length, Triangles.Length}));
        }
        GUILayout.Label("Export Mesh", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Save as Asset")) {
            string path = EditorUtility.SaveFilePanelInProject("Save Mesh", MeshName + ".asset", "asset", "Choose a file location");
            if (!string.IsNullOrEmpty(path)) {
                Mesh msh = createMesh();
                if (msh) {
                    msh.name = MeshName;
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.CreateAsset(msh, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log("Curvy Export: Mesh Asset saved!");
                }
                else
                    Debug.LogWarning("Curvy Export: Unable to triangulate spline!");
            }
        }

        if (GUILayout.Button("Create GameObject")) {
            Mesh msh = createMesh();
            if (msh) {
                msh.name = MeshName;
                var go = new GameObject(MeshName, typeof(MeshRenderer), typeof(MeshFilter));
                go.GetComponent<MeshFilter>().sharedMesh = msh;
                go.GetComponent<MeshRenderer>().sharedMaterial = MeshMaterial;
                Selection.activeGameObject = go;
                Debug.Log("Curvy Export: GameObject created!");
            }
            else
                Debug.LogWarning("Curvy Export: Unable to triangulate spline!");
            
        }
        
        GUILayout.EndHorizontal();
        if (GUI.changed)
            Triangulate();
    }

    void Triangulate()
    {
        var msh = createMesh();
        Vertices = new Vector3[0];
        Triangles = new int[0];

        if (msh) {
            Vertices = msh.vertices;
            Triangles = msh.triangles;
        }
        Mesh.DestroyImmediate(msh);

        SceneView.RepaintAll();
    }

    Mesh createMesh()
    {
        if (CalcMode == VertexCalcMode.UseGranularity)
            return MeshHelper.CreateSplineMesh(Spline, IgnoreAxis, AutoClose);
        else
            return MeshHelper.CreateSplineMesh(Spline, IgnoreAxis, AutoClose, calcAngle);
    }

    void Preview(SceneView sceneView)
    {
        Handles.color = Color.green;
        Handles.matrix = splMatrix;
        for (int t = 0; t < Triangles.Length; t += 3) {
            Handles.DrawLine(Vertices[Triangles[t]], Vertices[Triangles[t + 1]]);
            Handles.DrawLine(Vertices[Triangles[t+1]], Vertices[Triangles[t + 2]]);
            Handles.DrawLine(Vertices[Triangles[t+2]], Vertices[Triangles[t]]);
        }
        Handles.color = new Color(0, 0.2f, 0);
        for (int v = 0; v < Vertices.Length; v++)
            Handles.CubeCap(0, Vertices[v], Quaternion.identity, HandleUtility.GetHandleSize(Vertices[v]) * 0.06f);
        
    }

    
}
