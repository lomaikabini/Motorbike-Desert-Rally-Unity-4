using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {

	public BikeCamera cam;
	public GameType type;
	//public UIConfig conf;

	public GameObject firstBike;
	public GameObject secondBike;
	public GameObject fallDownMessage;
	public Material[] bikeMaterials;

	public GameObject popup;
	public GameObject homePopup;
	public GameObject shopPopup;
	public GameObject preStartMenu;
	public GameObject buttons;
	public GameObject arrowControls;
	public GameObject tiltControls;
	public GameObject nitroBtn;
	public GameObject earningView;
	public GameObject bikeAvailable;
	public GameObject infoShow;
	public GameObject missionShow;
	public GameObject buttonsList;
	
	public UILabel taskView;
	public UITexture taskImg;

	public Texture[] lvlTextures;

	public Transform missionsObj;

	public MusicSfx soundOBJ;

	public UILabel forCash;

	GameObject itemsWrapper;
	GameData data;

	string[] missionDescription = 
	{
		"Bank robbery reported. Robbers got away but dumped cash bags around the city. It's your job to go find them.",
		"Reports of a stolen vehicle in the area. Drive around and locate the stolen vehicle.",
		"Robbers are hiding the stashes around the city. We have reports of their known locations, find them.",
		"We’re getting reports of a stolen truck full of gold bars. Locate the stolen truck.",
		"Well done for finding the truck, however the gold bars seemed to have fallen out of the back while driving. You know what's coming next. Go find the gold bars before they come back for them.",
		"Reports of street racers have been sighted in the city. Locate and retrieve racing cars."
	};

	string[] itemFoundPreText = 
	{
		"",
		"",
		"",
		"",
		"",
		"Car clamped, "
	};

	string[] itemFoundAfterText = 
	{
		" items remain!",
		" items remain!",
		" items remain!",
		" items remain!",
		" items remain!",
		" cars remain!"
	};

	string[] endLvlMessage = 
	{
		"Congratulations! You have found all the cash bags!",
		"Congratulations! You have found stolen car!",
		"Congratulations! You have found all the drugs!",
		"Congratulations! You have found stolen truck!",
		"Congratulations! You have found all the gold bars!",
		"Congratulations! You have found all the stolen cars!"
	};

	public static bool isRunning;
	public static Game instance;
	private bool isHomeShow = false;
	private float scale = 0f;

	private int circleRemaining;

	public enum GameType
	{
		collectForCount,
		collectForPoints
	}

	void Awake()
	{
		instance = this;
		data = GameData.Get ();
	}

	void Start()
	{
//		Transform citys = GameObject.Find("Citys").transform;
//		if(data.currentLvl % 2 == 0)
//		{
//			citys.GetChild(0).gameObject.SetActive(true);
//			citys.GetChild(1).gameObject.SetActive(false);
//		}
//		else
//		{
//			citys.GetChild(0).gameObject.SetActive(false);
//			citys.GetChild(1).gameObject.SetActive(true);
//		}

		taskView.text = missionDescription [data.currentLvl - 1];
		taskImg.mainTexture = lvlTextures [data.currentLvl - 1];
		setMissionItem ();
		circleRemaining = itemsWrapper.transform.childCount;
		circleRemaining -= data.GetFoundItemsCount ();
		hideFoundItems ();
		showScore ();
		StartCoroutine (checkBanner ());
	}

	IEnumerator checkBanner()
	{
		yield return new WaitForEndOfFrame ();
		while(true)
		{
			if(AdMob_Manager.Instance.isBannerVisible)
				AdMob_Manager.Instance.hideBanner();
			yield return new WaitForSeconds(1f);
		}
		yield return null;
	}

	void hideFoundItems ()
	{
		for(int i = 0; i < data.collectedItems[data.currentLvl].Count;i++)
		{
			itemsWrapper.transform.GetChild(data.collectedItems[data.currentLvl][i]).gameObject.SetActive(false);
		}
	}

	void setMissionItem ()
	{
		string name = "Mission " + data.currentLvl.ToString ();
		itemsWrapper = missionsObj.FindChild (name).gameObject;
		for(int i = 0; i < missionsObj.childCount; i++)
		{
			if(missionsObj.GetChild(i).name != name)
				missionsObj.GetChild(i).gameObject.SetActive(false);
			else
				GameObject.Find("BikeManager").GetComponent<BikeManager>().SetRotator(missionsObj.GetChild(i).GetComponent<ItemRotator>());
		}
	}

	public void ShowFallDownMsg()
	{
		fallDownMessage.SetActive (true);
	}

	public void HideFallDownMsg()
	{
		if(fallDownMessage.activeSelf)
			fallDownMessage.SetActive (false);
	}

	public void OnMissionShowClick()
	{
		missionShow.SetActive (false);
		GameObject.Find ("AdmobAdAgent").GetComponent<AdMob_Manager> ().showBanner();
	}

	public void StartGame(bool toHideLRButtons)
	{
		preStartMenu.SetActive (false);
		buttons.SetActive (true);
		isRunning = true;
		ShowLeftRightButtons(!toHideLRButtons);
	}
	
	public void ShowLeftRightButtons(bool toShow) 
	{
		arrowControls.SetActive (toShow);
		tiltControls.SetActive (!toShow);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if(!isHomeShow)
			{
				isRunning = false;
				PreClosePopup.showPopup = true;
				soundOBJ.muteTMP();
				popup.SetActive(true);
				scale = Time.timeScale;
				Time.timeScale = 0f;
			}
			else
			{
				hideHomePopup();
			}
		}
	}


	public void showScore(int points = 0, int id = -1)
	{
		if(type == GameType.collectForPoints)
		{
			if(points != 0)
				circleRemaining -=1;
			addPoints (points);
			ShowEarning (points);
			if(forCash == null)
				forCash = GameObject.Find("forCash").GetComponent<UILabel>();
			forCash.text = "Points: " + data.cash.ToString ();
			if(circleRemaining == 0)
				StartCoroutine(refreshCircles());
		}
		else if (type == GameType.collectForCount)
		{
			if(points != 0)
			{
				data.addFoundItem(id);
				data.save();
				circleRemaining -=1;
				string textErn = "";
				if(circleRemaining != 0)
					textErn = itemFoundPreText[data.currentLvl-1] + circleRemaining.ToString() + itemFoundAfterText[data.currentLvl-1];
				else
				{
					//GameObject.Find ("BikeManager").GetComponent<BikeManager> ().SetAdditionalBike();
					textErn = endLvlMessage[data.currentLvl-1];
					if(data.currentLvl == data.allowLvls)
					{
						data.allowLvls ++;
						data.save();
					}
					StartCoroutine(goToLvlChoose());
				}

				earningView.GetComponent<UILabel>().text =textErn;
				earningView.GetComponent<Animator>().Play("earning",0,0f);
			}
			if(forCash == null)
				forCash = GameObject.Find("forCash").GetComponent<UILabel>();
			forCash.text = (itemsWrapper.transform.childCount - circleRemaining).ToString() + " / "+itemsWrapper.transform.childCount.ToString();
		}
	}

	IEnumerator goToLvlChoose()
	{
		yield return new WaitForSeconds (3.5f);
		GameObject.Find ("BikeManager").GetComponent<BikeManager> ().Reset ();
		GoTo.LoadEnvironmentChoose ();
		yield return null;
	}

	IEnumerator refreshCircles ()
	{
		yield return new WaitForSeconds(1f);
		circleRemaining = itemsWrapper.transform.childCount;
		for(int i = 0; i< itemsWrapper.transform.childCount;i++)
		{
			itemsWrapper.transform.GetChild(i).GetComponent<Circle>().refresh();
			yield return new WaitForSeconds(0.03f);
		}
		yield return null;
	}

	void addPoints (int points)
	{
		if (points <= 0)
			return;

		List<bool> pre = availableBike (data.cash);
		List<bool> after = availableBike (data.cash + points);

		bool newBike = false;
		for(int i = 0; i< pre.Count; i++)
		{
			if(pre[i] != after[i])
			{
				newBike = true;
				break;
			}
		}

		if(newBike)
		{
			StartCoroutine(showAvailableBike());
		}

		data.cash += points;
		data.save ();
	}

	List<bool> availableBike (int points)
	{
		bool result = false;
		List<bool> bikesAllow = new List<bool> ();
		for(int i = 0; i < Shop.prices.Length; i++)
		{
			bool equal = false;
			for(int j = 0;j < data.allowBikes.Count; j++ )
			{
				if(i == data.allowBikes[j])
				{
					equal = true;
					break;
				}
			}

			if(!equal && Shop.prices[i]<= points)
			{
				result = true;
				bikesAllow.Add(true);
				//break;
			}
			else if(!equal)
				bikesAllow.Add(false);
		}
		return bikesAllow;
	}

	IEnumerator showAvailableBike()
	{
		yield return new WaitForEndOfFrame ();
		bikeAvailable.SetActive (true);
		yield return new WaitForSeconds (5.0f);
		bikeAvailable.SetActive (false);
		yield return null;
	}
	void ShowEarning (int points)
	{
		if (points <= 0)
			return;
		if(earningView == null)
			earningView = GameObject.Find("earningView");
		earningView.GetComponent<UILabel>().text = "You got "+points+" coins!";
		earningView.GetComponent<Animator>().Play("earning",0,0f);
	}

	public void showHomePopup()
	{
		isHomeShow = true;
		homePopup.SetActive (true);
	}
	public void hideHomePopup()
	{
		isHomeShow = false;
		homePopup.SetActive(false);
	}
	public void mainMenu()
	{
		GameObject.Find ("BikeManager").GetComponent<BikeManager> ().Reset ();
		Time.timeScale = 1f;
		isRunning = false;
		GoTo.LoadMenu ();
	}

	public void showShopPopup()
	{
		shopPopup.SetActive (true);
	}
	public void hideShopPopup()
	{
		shopPopup.SetActive (false);
	}

	public void goShop()
	{
		Time.timeScale = 1f;
		isRunning = false;
		GoTo.LoadShop ();
	}

	public void onInfoClick()
	{
		infoShow.SetActive (false);
		//GameObject.Find ("AdmobAdAgent").GetComponent<AdMob_Manager> ().showBanner();
	}

	public void onListClick()
	{
		float val = 150f;
		//nitroBtn.SetActive (buttonsList.activeSelf);
		Vector3 pos = nitroBtn.transform.localPosition;
		if(buttonsList.activeSelf)
		{
			pos.x -= val;
			buttonsList.SetActive(false);
		}
		else
		{
			pos.x += val;
			buttonsList.SetActive(true);
		}
		nitroBtn.transform.localPosition = pos;
	}
}
