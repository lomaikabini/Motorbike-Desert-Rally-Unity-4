// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


namespace Curvy.Utils
{
    public class CurvyEditorUtility
    {
        public static bool IsSingleSelection
        {
            get
            {
                return Selection.transforms.Length <= 1;
            }
        }

        public static bool IsJoinSelection
        {
            get
            {
                if (Selection.transforms.Length!=2)
                    return false;

                CurvySplineSegment a = Selection.activeTransform.GetComponent<CurvySplineSegment>();
                CurvySplineSegment b = (Selection.activeTransform==Selection.transforms[0]) ?
                                         Selection.transforms[1].GetComponent<CurvySplineSegment>() :
                                         Selection.transforms[0].GetComponent<CurvySplineSegment>();
                
                return (a.Spline != b.Spline);
            }
        }

        // the last selected is the target!
        public static string JoinSelectionInfo
        {
            get
            {
                if (!IsJoinSelection)
                    return "Join Splines";
                CurvySplineSegment dst = Selection.activeTransform.GetComponent<CurvySplineSegment>();
                CurvySplineSegment src = (Selection.activeTransform == Selection.transforms[0]) ?
                                         Selection.transforms[1].GetComponent<CurvySplineSegment>() :
                                         Selection.transforms[0].GetComponent<CurvySplineSegment>();
                return string.Format("Insert {0} after {1}", new object[]{src.Spline.name, dst.name});
            }
        }

        public static CurvySplineSegment JoinSource
        {
            get
            {
                if (!IsJoinSelection)
                    return null;
                
                return (Selection.activeTransform == Selection.transforms[0]) ?
                                         Selection.transforms[1].GetComponent<CurvySplineSegment>() :
                                         Selection.transforms[0].GetComponent<CurvySplineSegment>();
                
            }
        }

        public static CurvySplineSegment JoinTarget
        {
            get
            {
                if (!IsJoinSelection)
                    return null;

                return Selection.activeTransform.GetComponent<CurvySplineSegment>();

            }
        }

        public static bool IsConnectSelection
        {
            get
            {
                if (Selection.transforms.Length != 2)
                    return false;

                CurvySplineSegment tgt = Selection.activeTransform.GetComponent<CurvySplineSegment>();
                CurvySplineSegment src = (Selection.activeTransform == Selection.transforms[0]) ?
                                         Selection.transforms[1].GetComponent<CurvySplineSegment>() :
                                         Selection.transforms[0].GetComponent<CurvySplineSegment>();

                return (CanConnect(src,tgt));
            }
        }

        // the last selected is the target!
        public static string ConnectSelectionInfo
        {
            get
            {
                if (!IsConnectSelection)
                    return "Connect";
                CurvySplineSegment dst = Selection.activeTransform.GetComponent<CurvySplineSegment>();
                CurvySplineSegment src = (Selection.activeTransform == Selection.transforms[0]) ?
                                         Selection.transforms[1].GetComponent<CurvySplineSegment>() :
                                         Selection.transforms[0].GetComponent<CurvySplineSegment>();
                return string.Format("Connect {0} with {1}", new object[]{src.Spline.name, dst.name});
            }
        }

        public static CurvySplineSegment ConnectionSource
        {
            get
            {
                if (!IsConnectSelection)
                    return null;

                return (Selection.activeTransform == Selection.transforms[0]) ?
                                         Selection.transforms[1].GetComponent<CurvySplineSegment>() :
                                         Selection.transforms[0].GetComponent<CurvySplineSegment>();

            }
        }

        public static CurvySplineSegment ConnectionTarget
        {
            get
            {
                if (!IsConnectSelection)
                    return null;

                return Selection.activeTransform.GetComponent<CurvySplineSegment>();

            }
        }

        /// <summary>
        /// Gets whether a Control Point can initiate a connection to another Control Point
        /// </summary>
        public static bool CanConnect(CurvySplineSegment src, CurvySplineSegment tgt)
        {
            if (!src || !tgt || src.Spline == tgt.Spline)
                return false;

            if (!src.Spline.AutoEndTangents)
                return false;

            if (!src.IsFirstSegment && src != src.Spline.ControlPoints[src.Spline.ControlPointCount - 1])
                return false;

            return (src.Connection==null);
        }

        public static Vector3 PositionHandle(int id, Vector3 position, Quaternion rotation, float size, bool lockX, bool lockY, bool lockZ)
        {
            Undo.SetSnapshotTarget(Selection.transforms, "Move " + ((Selection.transforms.Length != 1) ? string.Empty : Selection.activeGameObject.name));
            float rectSize = HandleUtility.GetHandleSize(position) * 0.15f;
            float sliderSize = HandleUtility.GetHandleSize(position) * size;

            Vector3 snap = new Vector3(EditorPrefs.GetFloat("MoveSnapX", 1f), EditorPrefs.GetFloat("MoveSnapY", 1f), EditorPrefs.GetFloat("MoveSnapZ", 1f));

            Vector3 newPos = position;
            // X
            if (!lockX) {
                //GUI.SetNextControlName(name);
                Handles.color = new Color(0.9f, 0.3f, 0.1f);
                newPos += Handles.Slider(position, rotation * Vector3.right,sliderSize,Handles.ArrowCap,snap.x)-position;
            }
           

            // Y
            if (!lockY) {
                //GUI.SetNextControlName(name);
                Handles.color = new Color(0.6f, 0.9f, 0.3f);
                newPos += Handles.Slider(position, rotation * Vector3.up, sliderSize, Handles.ArrowCap, snap.y) - position;
            }
            

            // Z
            if (!lockZ) {
                //GUI.SetNextControlName(name);
                Handles.color = new Color(0.2f, 0.4f, 0.9f);
                newPos += Handles.Slider(position, rotation * Vector3.forward, sliderSize, Handles.ArrowCap, snap.z) - position;
            }

            // X/Y Slider
            if (!lockX && !lockY) {
                //GUI.SetNextControlName(name);
                Handles.color = new Color(0.2f, 0.4f, 0.9f);
                newPos+=Handles.Slider2D(id,position, rotation*new Vector3(rectSize, rectSize, 0), rotation*Vector3.forward, rotation*Vector3.up, rotation*Vector3.right, rectSize, Handles.RectangleCap, new Vector2(snap.x,snap.y))-position;            
            }
            // X/Z Slider
            if (!lockX && !lockZ) {
                //GUI.SetNextControlName(name);
                Handles.color = new Color(0.6f, 0.9f, 0.3f);
                newPos += Handles.Slider2D(id - 1, position, rotation * new Vector3(rectSize, 0, -rectSize), rotation * Vector3.up, rotation * Vector3.right, rotation * Vector3.forward, rectSize, Handles.RectangleCap, new Vector2(snap.x, snap.z)) - position;            
            }
            // Y/Z Slider
            if (!lockY && !lockZ) {
                //GUI.SetNextControlName(name);
                Handles.color = new Color(0.9f, 0.3f, 0.1f);
                newPos += Handles.Slider2D(id - 2, position, rotation * new Vector3(0, rectSize, -rectSize), rotation * Vector3.left, rotation * Vector3.up, rotation * Vector3.forward, rectSize, Handles.RectangleCap, new Vector2(snap.y, snap.z)) - position;            
            }

            return newPos;
        }

        public static T[] GetSelection<T>() where T:Object
        {
            List<T> res = new List<T>();
            Object[] obj=Selection.GetFiltered(typeof(T), SelectionMode.TopLevel);
            foreach (T r in obj)
                res.Add(r);
            res.Sort((a, b) => { return a.name.CompareTo(b.name); });
            return res.ToArray();
        }

        
    }

    [System.Serializable]
    public class EditorKeyDefinition
    {
        public string Name;
        public KeyCode Key;
        public bool Shift;
        public bool Control;
        public bool Alt;
        public bool Command;

        public EditorKeyDefinition(string prefsName, KeyCode defKey, bool defShift, bool defControl, bool defAlt, bool defCommand)
        {
            Name = prefsName;
            Key = (KeyCode)EditorPrefs.GetInt(Name + "_Key", (int)defKey);
            Shift = EditorPrefs.GetBool(Name + "_Shift", defShift);
            Control = EditorPrefs.GetBool(Name + "_Control", defControl);
            Alt = EditorPrefs.GetBool(Name + "_Alt", defAlt);
            Command = EditorPrefs.GetBool(Name + "_Command", defCommand);
        }

        public void SaveToPrefs()
        {
            EditorPrefs.SetInt(Name + "_Key", (int)Key);
            EditorPrefs.SetBool(Name + "_Shift", Shift);
            EditorPrefs.SetBool(Name + "_Control", Control);
            EditorPrefs.SetBool(Name + "_Alt", Alt);
            EditorPrefs.SetBool(Name + "_Command", Command);
        }

        public bool IsKeyDown(Event ev)
        {
            return (ev.type==EventType.KeyDown &&
                    ev.keyCode == Key &&
                    ev.shift == Shift &&
                    ev.control == Control &&
                    ev.alt == Alt &&
                    ev.command == Command) ;
        }

        public void OnGUI(string displayName) { OnGUI(displayName, ""); }
        public void OnGUI(string displayName, string tooltip)
        {
            
            Key=(KeyCode)EditorGUILayout.EnumPopup(new GUIContent(displayName,tooltip), Key);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Shift = GUILayout.Toggle(Shift,"Shift");
            Control = GUILayout.Toggle(Control,"Ctrl");
            Alt = GUILayout.Toggle(Alt, "Alt");
            Command = GUILayout.Toggle(Command,"Cmd");
            EditorGUILayout.EndHorizontal();
        }
    }

}