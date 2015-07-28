//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEditor;
using DigitalOpus.MB.Core;

[CustomEditor(typeof(MB2_TextureBaker))]
public class MB2_TextureBakerEditor : Editor {
	
	MB2_TextureBakerEditorInternal tbe = new MB2_TextureBakerEditorInternal();
	
	public override void OnInspectorGUI(){
		tbe.DrawGUI((MB2_TextureBaker) target, typeof(MB_MeshBakerEditorWindow));	
	}
	
}