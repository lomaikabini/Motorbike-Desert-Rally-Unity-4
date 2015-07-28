using UnityEngine;
using System.Collections;

public class ShopNew : MonoBehaviour {

	public GameObject itmsObj;
	public BikeCamera cam;
	public GameObject[] bikes;
	public UILabel bikeInfo;
	public UILabel cashView;
	public UILabel choisePopupLabel;

	public GameObject loadingScreen;
	public GameObject buyPopup;
	public GameObject nomoneyPopup;
	public GameObject choisePopup;
	public GameObject closePopup;
	public GameObject precloseScreen;

	public GameObject unlockBtn;
	public GameObject playBtn;

	public UISlider topSpeedSlider;
	public UISlider AccelerationSlider;
	public UISlider leanSlider;
	public UISlider gripSlider;

	private int curBike;
	private GameData data;

	private string[] bikeNames = {"Super Bike","Speed Bike","Offroad Bike","Motocross Bike","Police Bike","Bike 06"};
	public static int[] prices = {0,20000,40000,65000,80000,100000};

	private bool isAction = false;

	void Start()
	{

		data = GameData.Get ();
		curBike = data.currentBike;
		curBike = getLastOpenedBike ();
		cam.target = bikes [curBike].transform;
		cashView.text = "Points: " + data.cash.ToString ();
		bikeInfo.text = bikeNames [curBike];
		chooseBike (curBike);
		showStatistic (curBike);
		showInfo ();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if(!precloseScreen.activeSelf)
			{
				PreClosePopup.showPopup = true;
				precloseScreen.SetActive(true);
			}
		}
	}

	private int getLastOpenedBike(){
		int numLastBike = 0;
		//data.allowLvls
//		for (int i = 0; i < GameSettings.getListUnlockingBike().Length; i++) {
//			if(GameSettings.getListUnlockingBike()[i] <= data.allowLvls)
//				numLastBike = i;
//		}
		numLastBike = data.currentBike;
		return numLastBike;
	}

	private void chooseBike(int currentBike){
		for (int i=0; i<bikes.Length; i++) {
			if(i == currentBike)
				bikes [i].gameObject.SetActive(true);
			else
				bikes [i].gameObject.SetActive(false);
		}
	}

	public void Left()
	{
		if(isAction) return;

		if (curBike - 1 >= 0)
			curBike -= 1;
		else
			curBike = bikes.Length - 1;
		//cam.target = bikes [curBike].transform;
		chooseBike (curBike);
		showStatistic (curBike);
		showInfo ();
	}

	public void Right()
	{
		if(isAction) return;

		if (curBike + 1 < bikes.Length)
				curBike += 1;
		else
				curBike = 0;
		//cam.target = bikes [curBike].transform;
		chooseBike (curBike);
		showStatistic (curBike);
		showInfo ();
	}

	void showStatistic(int currentBike){
		BikeStatics bikeStat = GameSettings.getCurrentBikeStatistics (currentBike);
		topSpeedSlider.value = bikeStat.topSpeed;
		AccelerationSlider.value = bikeStat.acceleration;
		leanSlider.value = bikeStat.lean;
		gripSlider.value = bikeStat.grip;
	}

	void showInfo ()
	{
		if(data.allowLvls >= GameSettings.getLevelForUnlockBike(curBike))
		{
			unlockBtn.SetActive(false);
			playBtn.SetActive(true);
			bikeInfo.text = bikeNames[curBike];
		}
		else
		{
			playBtn.SetActive(false);
			unlockBtn.SetActive(true);
			unlockBtn.GetComponentInChildren<UILabel>().text = "Will be unlocked at level " + GameSettings.getLevelForUnlockBike(curBike).ToString();
			bikeInfo.text =bikeNames[curBike];
		}
	}

	IEnumerator showIntersential()
	{
		yield return new WaitForEndOfFrame ();
		AdMob_Manager.Instance.showInterstitial ();
		yield return null;
	}

	public void showBuyPopup()
	{
		isAction = true;
		if(data.cash >= prices[curBike])
		{
			buyPopup.SetActive(true);
		}
		else
			nomoneyPopup.SetActive(true);
	}

	public void areaClick()
	{
		if(isAction) return;

		isAction = true;
		choisePopupLabel.text = "Use "+bikeNames[curBike]+" to play?";
		choisePopup.SetActive (true);
	}
	public void buyBike()
	{
		data.cash -= prices [curBike];
		data.allowBikes.Add (curBike);
		data.currentBike = curBike;
		data.save ();
		bikeInfo.text = bikeNames[curBike];
		cashView.text = "Points: " + data.cash.ToString ();
		closeBuyPopup ();
		unlockBtn.SetActive (false);
	}

	public void choise()
	{
		data.currentBike = curBike;
		data.save ();
		loadingScreen.SetActive (true);
		GoTo.LoadMegaCity ();
		StartCoroutine (showIntersential ());
		//GoTo.LoadEnvironmentChoose ();
	}

	public void closeBuyPopup()
	{
		isAction = false;
		buyPopup.SetActive (false);
	}
	public void closeNoMOneyPopup()
	{
		isAction = false;
		nomoneyPopup.SetActive (false);
	}
	public void closeChoisePopup()
	{
		isAction = false;
		choisePopup.SetActive (false);
		//itmsObj.SetActive (true);
	}
	public void closeClosePopup()
	{
		isAction = false;
		closePopup.SetActive (false);
		//itmsObj.SetActive (true);
	}

	public void menuClick()
	{
		if(isAction) return;
		isAction = true;
		closePopup.SetActive (true);
		//itmsObj.SetActive (false);
	}
	public void goMainMenu()
	{
		loadingScreen.SetActive (true);
		GoTo.LoadMenu ();
	}

	public void ChoiseBikeFromAll(GameObject obj)
	{
		//itmsObj.SetActive (false);
		curBike = System.Convert.ToInt32 (obj.name) - 1;
		areaClick ();
	}
}
