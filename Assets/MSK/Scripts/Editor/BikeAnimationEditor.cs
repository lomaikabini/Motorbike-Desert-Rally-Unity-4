using UnityEngine;
using UnityEditor; 
using System;
using System.Collections.Generic; 
using System.IO;


[CustomEditor(typeof(BikeAnimation))]


public class BikeAnimationEditor : Editor
{

    public void OnInspectorGUI() { BikeAnimation myPlayer = (BikeAnimation)target; }

}

	
	
	
	
	
	
