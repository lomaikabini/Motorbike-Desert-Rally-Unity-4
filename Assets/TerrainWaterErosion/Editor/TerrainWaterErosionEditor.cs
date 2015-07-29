/*
---------------------- Terrain Water Erosion Editor ----------------------
-- TerrainWaterErosionEditor.cs
--
-- Code and algorithm by Dmitry Soldatenkov
-- Based on Terrain Toolkit by Sándor Moldán. 
--
-------------------------------------------------------------------
*/
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(TerrainWaterErosion))]
public class TerrainWaterErosionEditor : Editor {

    public override void OnInspectorGUI () {
        EditorGUIUtility.LookLikeControls ();
        TerrainWaterErosion weTerrain = (TerrainWaterErosion)target as TerrainWaterErosion;
        if (!weTerrain.gameObject) {
            return;
        }
        Terrain terComponent = (Terrain)weTerrain.GetComponent (typeof(Terrain));
        if (terComponent == null) {
            NonTerrainMessage ();
            return;
        }

        if (GUI.changed) {
            EditorUtility.SetDirty (weTerrain);
        }
        GUI.changed = false;

        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.PrefixLabel ("Iterations");
        weTerrain.waterErosionIterations = (int)EditorGUILayout.Slider (weTerrain.waterErosionIterations, 1, 250);
        EditorGUILayout.EndHorizontal ();

        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.PrefixLabel ("Rainfall");
        weTerrain.waterErosionRainfall = EditorGUILayout.Slider (weTerrain.waterErosionRainfall, 0, 10);
        EditorGUILayout.EndHorizontal ();

        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.PrefixLabel ("Stream Turbulence");
        weTerrain.waterErosionConeThreshold = EditorGUILayout.Slider (weTerrain.waterErosionConeThreshold, 1, 5);
        EditorGUILayout.EndHorizontal ();

        Rect buttonRect = EditorGUILayout.BeginHorizontal ();
        buttonRect.x = buttonRect.width / 2 - 100;
        buttonRect.width = 200;
        buttonRect.height = 28;
        if (GUI.Button (buttonRect, "Apply Water Erosion")) {
            // Undo
            Terrain ter = (Terrain)weTerrain.GetComponent (typeof(Terrain));
            if (ter == null) {
                return;
            }
            TerrainData terData = ter.terrainData;
			//Undo.RecordObject (terData, "Terrain Water Erosion");
			Undo.RegisterUndo(terData, "Terrain Water Erosion");
            // Start time...
            DateTime startTime = DateTime.Now;
            TerrainWaterErosion.ErosionProgressDelegate erosionProgressDelegate = new TerrainWaterErosion.ErosionProgressDelegate (updateErosionProgress);
            weTerrain.processTerrain (erosionProgressDelegate);
            EditorUtility.ClearProgressBar ();
            TimeSpan processTime = DateTime.Now - startTime;
            Debug.Log ("Process complete in: " + processTime.ToString ());
            GUIUtility.ExitGUI ();
        }
        EditorGUILayout.EndHorizontal ();
        for (int i = 0; i < 6; i++)
            EditorGUILayout.Separator ();

        if (GUI.changed) {
            EditorUtility.SetDirty (weTerrain);
        }
    }

    public void updateErosionProgress (string titleString, string displayString, int iteration, int nIterations, float percentComplete) {
        EditorUtility.DisplayProgressBar (titleString, displayString + " Iteration " + iteration + " of " + nIterations + ". Please wait.", percentComplete);
    }

    public void NonTerrainMessage () {
        EditorGUILayout.Separator ();
        EditorGUILayout.BeginHorizontal ();
        GUILayout.Label ("The GameObject that Terrain Water Erosion is attached to", "errorText");
        EditorGUILayout.EndHorizontal ();
        EditorGUILayout.BeginHorizontal ();
        GUILayout.Label ("does not have a Terrain component.", "errorText");
        EditorGUILayout.EndHorizontal ();
        EditorGUILayout.Separator ();
        EditorGUILayout.BeginHorizontal ();
        GUILayout.Label ("Please attach a Terrain component.", "errorText");
        GUI.skin = null;
        EditorGUILayout.EndHorizontal ();
        EditorGUILayout.Separator ();
    }

}

