using UnityEngine;
using UnityEditor; 
using System;
using System.Collections.Generic; 
using System.IO;


[CustomEditor(typeof(BikeControl))]


public class BikeEditor : Editor
{

    public void OnInspectorGUI() { BikeControl myPlayer = (BikeControl)target; }

}

	
	
	
	
	
	
