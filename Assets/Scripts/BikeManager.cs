using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BikeManager : MonoBehaviour {

	public static BikeManager instance;
	public BikeCamera cam;
	public Transform[] positionsWrapers;
	public GameObject arrowUI;
	public UILabel speedUI;
	public UILabel gearstUI;
	public UIWidget nitroUI;
	public List<BikeControl> bikesContols;
	ItemRotator rotator;
	BikeControl extrabike;
	Transform bikePositions;
	GameData data;

	float cameraDistance = 4.29f;
	float cameraHeight = 1f;
	float cameraAngle = 6f;

	float extraValue = 25f;
	bool isExtra = false;


	void Awake()
	{
		instance = this;
		data = GameData.Get ();
		//TODO: remove this string if need to have different default bike on game start
		//data.currentBike = 0;

		if(data.currentLvl % 2 == 0)
			bikePositions = positionsWrapers[0];
		else
			bikePositions = positionsWrapers[1];

		cam.distance = cameraDistance;
		cam.haight = cameraHeight;
		cam.Angle = cameraAngle;

		bikesContols = new List<BikeControl> ();
		if(GameObject.Find("Motorbike Extra") != null)
		{
			extrabike = GameObject.Find("Motorbike Extra").GetComponent<BikeControl>();
			BikeGUI bikeGui= extrabike.gameObject.GetComponent<BikeGUI>();
			bikeGui.arrowUI = arrowUI;
			bikeGui.speedUI = speedUI;
			bikeGui.gearstUI = gearstUI;
			bikeGui.nitroUI = nitroUI;

			Transform pos = bikePositions.FindChild("Position Extra").transform; 
			extrabike.rigidbody.velocity = Vector3.zero;
			extrabike.transform.position = pos.position ;
			extrabike.transform.rotation = pos.rotation;

			extrabike.currentGear = 1;
			extrabike.curTorque = 0f;
			extrabike.shiftDelay = 0f;
			extrabike.gameObject.SetActive(false);
		}
		for(int i = 0; i < bikePositions.childCount; i++)
		{
			if(GameObject.Find("Motorbike "+(i+1).ToString()) == null) continue;

			GameObject b = GameObject.Find("Motorbike "+(i+1).ToString());
			BikeControl bikeControl = b.GetComponent<BikeControl>();
			bikesContols.Add(bikeControl);
			BikeGUI bikeGui= b.GetComponent<BikeGUI>();
			bikeGui.arrowUI = arrowUI;
			bikeGui.speedUI = speedUI;
			bikeGui.gearstUI = gearstUI;
			bikeGui.nitroUI = nitroUI;
			Transform pos = bikePositions.FindChild("Position "+(i+1).ToString()).transform; 
			b.rigidbody.velocity = Vector3.zero;
			b.transform.position = pos.position ;
			b.transform.rotation = pos.rotation;
			bikeControl.currentGear = 1;
			bikeControl.curTorque = 0f;
			bikeControl.shiftDelay = 0f;
			b.SetActive(false);
		}
		if(data.extraBike)
		{
			bikesContols.Add(extrabike);
		}
		setBikeProperties ();
	}
	public void SetRotator(ItemRotator itm)
	{
		rotator = itm;
		rotator.target = bikesContols [data.currentBike].transform;
	}
	public void SetAdditionalBike()
	{
		releaseAll ();
		bikesContols[data.currentBike].transform.GetComponent<BikeGUI> ().enabled = false;
		bikesContols [data.currentBike].gameObject.SetActive (false);
		if(!data.extraBike)
		{
			bikesContols.Add (extrabike);
			data.extraBike = true;
		}
		data.currentBike = bikesContols.Count - 1;
		data.save ();
		setBikeProperties ();
	}

//	public void SetExtraSpeed ()
//	{
//		isExtra = true;
//		for(int i = 0 ; i < bikesContols.Count; i++)
//		{
//			bikesContols[i].bikeSetting.bikePower += extraValue;
//			bikesContols[i].bikeSetting.shiftPower += extraValue;
//		}
//	}
//
//	void removeExtraSpeed()
//	{
//		for(int i = 0 ; i < bikesContols.Count; i++)
//		{
//			bikesContols[i].bikeSetting.bikePower -= extraValue;
//			bikesContols[i].bikeSetting.shiftPower -= extraValue;
//		}
//	}

	public void Reset()
	{
		for(int  i = 0; i< bikesContols.Count; i++)
			bikesContols[i].gameObject.SetActive(true);
//		if(isExtra)
//			removeExtraSpeed();
		releaseAll ();
		bikesContols [data.currentBike].transform.GetComponent<BikeGUI> ().enabled = false;
	}

	void setBikeProperties ()
	{
		BikeControl targetBike = bikesContols[data.currentBike];
		cam.target = targetBike.transform;
		cam.BikeScript = targetBike;
		CarAI.motik = cam.target;
		targetBike.rigidbody.isKinematic = false;
		targetBike.transform.GetComponent<BikeGUI> ().enabled = true;
		targetBike.gameObject.SetActive (true);
		Transform[] positionView = {targetBike.transform.FindChild("Components").FindChild("ForestView").FindChild("View-1").transform,
			targetBike.transform.FindChild("Components").FindChild("ForestView").FindChild("View-2").transform,
			targetBike.transform.FindChild("Components").FindChild("ForestView").FindChild("View-3").transform};
		cam.cameraSwitchView = positionView;
	}

	public void OnReset()
	{
		Transform tr;
		if(data.extraBike && data.currentBike == bikesContols.Count - 1 )
			tr = bikePositions.FindChild ("Position Extra").transform;
		else
			tr = bikePositions.FindChild ("Position " + (data.currentBike + 1).ToString ()).transform;
		bikesContols [data.currentBike].transform.position = tr.position;
		bikesContols [data.currentBike].transform.rotation = tr.rotation;
		bikesContols [data.currentBike].rigidbody.velocity = Vector3.zero;
	}

	public void OnChangeBike()
	{
		releaseAll ();
		bikesContols[data.currentBike].transform.GetComponent<BikeGUI> ().enabled = false;
		bikesContols [data.currentBike].gameObject.SetActive (false);
		if(data.currentBike >= bikesContols.Count - 1)
			data.currentBike = 0;
		else
			data.currentBike++;

		data.save ();
		setBikeProperties ();
	}

	void releaseAll()
	{
		for(int i = 0; i < bikesContols.Count; i++)
		{
			bikesContols[i].ReleaseMoveDownBtn();
			bikesContols[i].ReleaseMoveLeftBtn();
			bikesContols[i].ReleaseMoveRightBtn();
			bikesContols[i].ReleaseMoveUpBtn();
			bikesContols[i].ReleaseNitroBtn();
			bikesContols[i].showBrakeParticles = false;
		}
	}

	public void OnTiltPress()
	{
		for(int i = 0; i < bikesContols.Count; i++)
		{
			bikesContols[i].OnTiltPress();
		}
	}

	public void OnArrowPress()
	{
		for(int i = 0; i < bikesContols.Count; i++)
		{
			bikesContols[i].OnArrowsPress();
		}
	}

	public void PressMoveUpBtn()
	{
		bikesContols [data.currentBike].PressMoveUpBtn ();
	}
	public void ReleaseMoveUpBtn()
	{
		bikesContols [data.currentBike].ReleaseMoveUpBtn ();
	}

	public void PressMoveDownBtn()
	{
		bikesContols [data.currentBike].bikeSetting.brakePower = 800f;
		bikesContols [data.currentBike].PressMoveDownBtn();
	}
	public void ReleaseMoveDownBtn()
	{
		bikesContols [data.currentBike].ReleaseMoveDownBtn ();
	}

	public void PreesBrakeBtn()
	{
		bikesContols [data.currentBike].showBrakeParticles = true;
		bikesContols [data.currentBike].bikeSetting.brakePower = 100f;
		bikesContols [data.currentBike].PressMoveDownBtn();
	}

	public void ReleaseBrakeBtn()
	{
		bikesContols [data.currentBike].showBrakeParticles = false;
		bikesContols [data.currentBike].ReleaseMoveDownBtn ();
	}

	public void PressMoveRightBtn()
	{
		bikesContols [data.currentBike].PressMoveRightBtn ();
	}
	public void ReleaseMoveRightBtn()
	{
		bikesContols [data.currentBike].ReleaseMoveRightBtn();
	}

	public void PressMoveLeftBtn()
	{
		bikesContols [data.currentBike].PressMoveLeftBtn ();
	}
	public void ReleaseMoveLeftBtn()
	{
		bikesContols [data.currentBike].ReleaseMoveLeftBtn();
	}

	public void PressNitroBtn()
	{
		bikesContols [data.currentBike].PressNitroBtn ();
	}

	public void ReleaseNitroBtn()
	{
		bikesContols [data.currentBike].ReleaseNitroBtn ();
	}

	public void ToggleControlTypeBtn()
	{
		for(int i = 0; i < bikesContols.Count; i++)
		{
			bikesContols[i].ToggleControlTypeBtn();
		}
		releaseAll ();
	}
}
