// i6 IAS_Manager.cs [Updated 20th October 2014]
// Attach this script to a persistent Game Object

using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

// Storage class for the main IAS advert IDs for selection
[System.Serializable]
public class IAS_Grouping
{
	private int _ScreenID;
	private List<string> _SlotIDs = new List<string>();
	
	public int ScreenID
	{
		get { return _ScreenID; }
		set { _ScreenID = value; }
	}
	
	public List<string> SlotIDs
	{
		get { return _SlotIDs; }
		set { _SlotIDs = value; }
	}
}

public class IAS_Manager : MonoBehaviour
{      
	public static IAS_Manager Instance;
	
	// Note: You should probably replace references to BundleID with a reference to your bundle ID from another script
	public string BundleID = "com.i6.GameNameHere";
	
	// Main IAS variables (Set your IAS Ad URL from the inspector of the object which this script is attached to)
	private bool _Main_IASReady = false;
	public bool Main_IASReady
	{
		get { return _Main_IASReady; }
		private set { _Main_IASReady = value; }
	}
	
	public List<int> includedScreenIDs = new List<int>();
	
	public string IAS_AdURL = "http://ias.i6.com/ad/37.json";
	private List<string> Main_BannerURLs = new List<string>();
	private List<string> Main_BannerImageURLs = new List<string>();
	private List<Texture> Main_BannerTextures = new List<Texture>();
	
	// Backscreen IAS variables (You don't need to change these variables)
	private bool _Backscreen_IASReady = false;
	public bool Backscreen_IASReady
	{
		get { return _Backscreen_IASReady; }
		private set { _Backscreen_IASReady = value; }
	}
	
	private int Backscreen_AdLimit = 3;
	private string IAS_Static_Ads = "http://ias.i6.com/ad/31.json";
	private List<string> Backscreen_BannerURLs = new List<string>();
	private List<string> Backscreen_BannerImageURLs = new List<string>();
	private List<Texture> Backscreen_BannerTextures = new List<Texture>();
	
	void Awake()
	{
		if(!Instance)
			Instance = this;
	}
	
	void Start()
	{
		// Fetch the main banners
		StartCoroutine(FetchMainBanners());
		
		// Fetch the Backscreen banners
		StartCoroutine(FetchBackscreenBanners());
	}
	
	public void ResetBackscreenBanners()
	{
		if(Backscreen_IASReady)
		{
			// Mark the Backscreen IAS ads as not ready whilst re-downloading the data
			Backscreen_IASReady = false;
			
			// Clear the Backscreen IAS lists
			Backscreen_BannerURLs.Clear();
			Backscreen_BannerImageURLs.Clear();
			Backscreen_BannerTextures.Clear();
			
			// Fetch the Backscreen banners
			StartCoroutine(FetchBackscreenBanners());
		}
	}
	
	public void ResetMainBanners()
	{
		if(Main_IASReady)
		{
			// Mark the main IAS ads as not ready whilst re-downloading the data
			Main_IASReady = false;
			
			// Clear the main IAS lists
			Main_BannerURLs.Clear ();
			Main_BannerImageURLs.Clear ();
			Main_BannerTextures.Clear ();
			
			// Fetch the main banners
			StartCoroutine(FetchMainBanners());
		}
	}
	
	public string GetAdURL(int bannerIndex, bool isBackscreen = false)
	{
		if(!isBackscreen)
		{
			return Main_BannerURLs[bannerIndex - 1];
		}
		else
		{
			return Backscreen_BannerURLs[bannerIndex - 1];
		}
	}
	
	public Texture GetAdTexture(int bannerIndex, bool isBackscreen = false)
	{
		if(!isBackscreen)
		{
			return Main_BannerTextures[bannerIndex - 1];
		}
		else
		{
			return Backscreen_BannerTextures[bannerIndex - 1];
		}
	}
	
	private IEnumerator FetchBackscreenBanners()
	{
		// Request JSON data from the URL
		WWW wwwJSON = new WWW(IAS_Static_Ads);
		
		// Wait for the JSON data to be collected from the URL
		yield return wwwJSON;
		
		// Parse the JSON data we just read into usable data
		JSONNode rootNode = JSON.Parse (wwwJSON.text);
		
		// Loop for each "slot" in the JSON data
		foreach(JSONNode node in rootNode["slots"].AsArray)
		{
			// We need to work with the nodes as strings so just set them to local variables
			string sloturl = node["adurl"];
			string slotimg = node["imgurl"];
			//string slotid = node["slotid"];
			
			// Break out of the foreach if we hit the max banners needed
			if(Backscreen_BannerURLs.Count >= Backscreen_AdLimit)
				break;
			
			// If the current ad is an advert for this game then skip it
			// Note: You should probably replace BundleID with a reference to your bundle ID from another script
			if(sloturl.Contains (BundleID))
				continue;
			
			// Take the last value from the slotid name
			//int bannerID = int.Parse (new string(slotid[slotid.Length-1], 1));
			
			// Add the advert URL as an item to the list
			Backscreen_BannerURLs.Add(sloturl);
			
			// Set the ad image URL to a new item in the list
			Backscreen_BannerImageURLs.Add(slotimg);
		}
		
		// Dispose of the JSON data
		wwwJSON.Dispose();
		
		// Download the images
		for(int i = 0; i < Backscreen_BannerURLs.Count; i++)
		{
			// Request the banner image URL
			WWW wwwImage = new WWW(Backscreen_BannerImageURLs[i]);
			
			// Wait for the image data to be downloaded
			yield return wwwImage;
			
			// Set the texture
			Backscreen_BannerTextures.Add(wwwImage.texture);
			
			// Dispose of the image data
			wwwImage.Dispose();
		}
		Backscreen_IASReady = true;
	}
	
	private IEnumerator FetchMainBanners()
	{
		// Request JSON data from the URL
		WWW wwwJSON = new WWW(IAS_AdURL);
		
		// Wait for the JSON data to be collected from the URL
		yield return wwwJSON;
		
		// Parse the JSON data we just read into usable data
		JSONNode rootNode = JSON.Parse (wwwJSON.text);
		
		// This method of sorting the JSON data isn't the cleanest but it works
		// We need to order the JSON data starting from screen 1 and counting up
		// Let us know if you can improve this code block
		string sortedJSON = "";
		
		// Wrap the sorted JSON within the slots array item
		sortedJSON += "{\"slots\":[";
		
		for(int i = rootNode["slots"].Count;i >= 0;i--)
		{
			if(rootNode["slots"].AsArray[i] != null)
			{
				sortedJSON += (rootNode["slots"].AsArray[i].ToString());
			}
		}
		
		// Close the slots wrapper
		sortedJSON += "]}";
		
		// Replace the rootNode with the new sortedJSON data
		rootNode = JSON.Parse (sortedJSON);
		
		// IAS_Grouping is a custom class defined at the top of this script
		// The class returns the values ScreenID (int) and SlotIDs (List<string>)
		List<IAS_Grouping> IAS_SlotGroups = new List<IAS_Grouping>();
		
		// Local int to store the previous slot value so we can compare if it has changed each iteration
		int prevSlotVal = 0;
		
		// List used to store the generated random slotIDs based from the available slots
		List<int> curSlotID = new List<int>();
		
		// Iterate through all slots from the JSON data
		foreach(JSONNode node in rootNode["slots"].AsArray)
		{
			string slotID = node["slotid"];
			string slotURL = node["adurl"];
			string slotChar = slotID[slotID.Length-1].ToString();
			int slotVal = int.Parse(slotID[slotID.Length-2].ToString());
			
			// Check if we have iterated onto a new screen ID
			if(slotVal != prevSlotVal)
			{
				// Add the IAS Grouping class item to the list as a new item
				IAS_SlotGroups.Add (new IAS_Grouping());
				
				// Set the stored screen ID for this list item and set the previous slot value too
				IAS_SlotGroups[slotVal-1].ScreenID = slotVal;
				
				// Set the previous slot value so we don't add this screen ID again
				prevSlotVal = slotVal;
			}
			
			// Skip any adverts which advertise its self and display the next ad instead
			if(slotURL.Contains (BundleID))
				continue;
			
			// Set the current slot ID inside the current screen ID list
			// Why is this not working ?!?
			IAS_SlotGroups[slotVal-1].SlotIDs.Add(slotChar);
		}
		
		// Ensure all the screen IDs have atleast 1 advert each
		foreach(IAS_Grouping screenSlotIDs in IAS_SlotGroups)
		{
			if(screenSlotIDs.SlotIDs.Count <= 0)
			{
				Debug.Log ("IAS Screen ID " + screenSlotIDs.ScreenID + " does not have any ad slots!");
			}
		}
		
		int curSlotCount = 0;
		prevSlotVal = 0;
		
		// Loop for each "slot" in the JSON data
		foreach(JSONNode node in rootNode["slots"].AsArray)
		{
			// We need to work with the nodes as strings so assign them to local variables
			string slotURL = node["adurl"];
			string slotIMG = node["imgurl"];
			string slotID = node["slotid"];
			int screenSlot = int.Parse(slotID[slotID.Length-2].ToString());
			
			// Skip any adverts which advertise its self and display the next ad instead
			if(slotURL.Contains (BundleID))
				continue;
			
			if(screenSlot != prevSlotVal){
				// Reset the slot count because we've moved to the next screen ID
				curSlotCount = 0;
				
				// Load information about the current slot id
				curSlotID.Add(PlayerPrefs.GetInt("IAS_ADSlot_" + (screenSlot - 1), 0));
				
				// Increase the curSlotID if it's lower than the max, else reset it to advert slot 1
				if(curSlotID[screenSlot - 1] + 1 < IAS_SlotGroups[screenSlot-1].SlotIDs.Count){
					// Increase the slot ID for the current screen
					curSlotID[screenSlot - 1]++;
				} else {
					// Set the slot ID for the current screen back to 1
					curSlotID[screenSlot - 1] = 0;
				}
				
				PlayerPrefs.SetInt("IAS_ADSlot_" + (screenSlot - 1), curSlotID[screenSlot - 1]);
				
				prevSlotVal = screenSlot;
			} else {
				// Increase the cur slot count
				curSlotCount++;
			}
			
			// Does the current slotID iteration match the randomly generated slotID for this screenID?
			if(curSlotCount == curSlotID[screenSlot - 1])
			{
				// Add the advert URL as an item to the list
				Main_BannerURLs.Add (slotURL);
				
				// Set the ad image URL to a new item in the list
				Main_BannerImageURLs.Add (slotIMG);                            
			}
		}
		
		// Dipose of the JSON data
		wwwJSON.Dispose ();
		
		// Download the images
		for(int i = 0; i < Main_BannerURLs.Count; i++)
		{
			Main_BannerTextures.Add (new Texture());
			
			// Limit the screen IDs being used for this game
			if(!includedScreenIDs.Contains(i+1) && includedScreenIDs.Count > 0)
				continue;
			
			// Request the banner image URL
			WWW wwwImage = new WWW(Main_BannerImageURLs[i]);
			
			// Wait for the image to be downloaded
			yield return wwwImage;
			
			// Add the texture to the list
			Main_BannerTextures[i] = (wwwImage.texture);
			
			// Dipose of the image data
			wwwImage.Dispose();
		}
		
		// Set the IASReady variable to true once all the ads are ready
		Main_IASReady = true;
	}
}