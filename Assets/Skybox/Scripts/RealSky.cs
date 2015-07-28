////////////////////////////////////////////
///                                      ///
///         RealSky Version 1.4          ///
///  Created by: Black Rain Interactive  ///
///                                      ///
////////////////////////////////////////////

// Super stripped down version                    //
// Edited to be just a sky which spins    //

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RealSky : MonoBehaviour {
	
	public Texture dayTime;
	public float skySpeed = 0.0f;
	
	// Disabling sky syncing means the sky will not move with your camera, useful if your skybox is a large room for example
	public bool syncSky = true;
	
	public List<Camera> cameras = new List<Camera>();
	public int skyBoxLayer = 8;
	
	GameObject skyCamera;
	
	void Awake(){
		
		if(skySpeed > 0f){
			if(syncSky){
				StartCoroutine("SkyRotation");
			} else {
				Debug.Log ("The RealSky sky rotation has been disabled because you have syncSky disabled!");
			}
		}
		
		if (cameras.Count <= 0){
			
			Debug.Log ("No cameras are attached to RealSky! Disabling RealSky..");
			this.enabled = false;
			return;
		}
		
		gameObject.layer = skyBoxLayer;
		
		skyCamera = new GameObject("SkyboxCamera");
		skyCamera.AddComponent<Camera>();
		skyCamera.camera.depth = -10;
		skyCamera.camera.clearFlags = CameraClearFlags.Color;
		skyCamera.camera.cullingMask = 1 << skyBoxLayer;
		skyCamera.transform.position = gameObject.transform.position;
		
		foreach(Camera curCamera in cameras){
			
			if(!curCamera)
				continue;
			
			//curCamera.cullingMask = 1;
			curCamera.clearFlags = CameraClearFlags.Depth;
			
		}
		
		if(!syncSky)
			this.enabled = false;
		
	}
	
	void Start(){
		
		renderer.material.SetTexture("_Texture01", dayTime);
		
	}
	
	void Update(){
		
		if (cameras.Count > 0){
			foreach(Camera curCamera in cameras){
				if(!curCamera)
					continue;
				
				if(curCamera.enabled)
					skyCamera.transform.rotation = curCamera.transform.rotation;
			}
		}
		
	}
	
	IEnumerator SkyRotation(){
		
		while (true){
			
			transform.Rotate(Vector3.up * Time.deltaTime * skySpeed, Space.World);
			yield return null;
			
		}
	}
	
}