using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class EnvironmentChoose : MonoBehaviour {

	public GameObject loadScreen;
	public GameObject backMenu;
	public GameData data;
	public List<UITexture> lvlsView;
	void Start()
	{
		data = GameData.Get ();
		setLvlsView ();
	}

	void setLvlsView ()
	{
		for(int i = 0; i < lvlsView.Count; i++)
		{
			if(i+1 <= data.allowLvls)
				lvlsView[i].color = Color.white;
			else
				lvlsView[i].color = Color.gray;
		}
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			PreClosePopup.showPopup = true;
			backMenu.SetActive(true);
		}
	}

	public void  LoadCityOne(GameObject lvl)
	{
		playGame(GoTo.LoadGameTownOne,Int32.Parse(lvl.name));
	}

	public void  LoadCityTwo(GameObject lvl)
	{
		playGame(GoTo.LoadGameTownTwo,Int32.Parse(lvl.name));
	}

	void playGame(Action func, int lvl)
	{
		if(lvl > data.allowLvls) return;

		GameObject.Find ("AdmobAdAgent").GetComponent<AdMob_Manager> ().hideBanner ();
		loadScreen.SetActive (true);
		data.currentLvl = lvl;
		data.save ();
		func ();
	}
}
