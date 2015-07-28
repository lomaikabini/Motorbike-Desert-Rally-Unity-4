using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

public class BackIAS_Example : MonoBehaviour
{
	
	// Set variables for the preclose screen adverts (These probably won't need to be modified)
	private string preCloseUrl = "http://ias.i6.com/ad/31.json";
	public bool preReady = false;
	private static int adLimit = 3; // Only store a maximum of adLimit ads in our lists
	
	// Using lists because we don't need to redefine the amount of ads we are going to store
	private List<string> preBannerURL = new List<string>();
	private List<string> preBannerImageURL = new List<string>();
	private List<Texture> preBannerTextures = new List<Texture>();
	
	// This package identifier is compared with the package identifiers in the URL of the adverts
	// If you are already setting the bundle identifier in another script then you should just use
	// that instead of defining it several times and confusing yourself later when using the same
	// source on other projects, I would suggest having a main game manager / config file storing
	// this if you are using it in other scripts too (Change line 77 if so)
	public string bundleIdentifier;
	
	public static BackIAS_Example Instance;
	
	void Awake()
	{
		// When the game object is created set the instance to the current script
		Instance = this;
	}
	
	void Start()
	{
		// When everything is ready in the scene start the coroutine to fetch the preclose banner data
		StartCoroutine("FetchPreCloseBannerData");
	}
	
	public string GetPreAdURL(int bannerIndex)
	{
		// Just for ease of use for some developers minus 1 from the bannerIndex so the advert ID starts at 1 instead of 0
		return preBannerURL[bannerIndex-1];
	}
	
	public Texture GetPreBannerTexture(int bannerIndex)
	{
		// Just for ease of use for some developers minus 1 from the bannerIndex so the advert ID starts at 1 instead of 0
		return preBannerTextures[bannerIndex-1];
	}
	
	// Method to fetch the banner data and assign the URL and texture to arrays
	private IEnumerator FetchPreCloseBannerData()
	{
		// Request JSON data from the url
		WWW wwwJSON = new WWW(preCloseUrl);
		
		// Wait for the JSON data to be collected from the url
		yield return wwwJSON;
		
		// Parse the JSON data we just read into usable data
		JSONNode rootNode = JSON.Parse (wwwJSON.text);
		
		// Loop for each "slot" in the JSON data
		foreach(JSONNode node in rootNode["slots"].AsArray)
		{
			// We need to work with the nodes as strings so just assign them to local variables
			string sloturl = node["adurl"];
			string slotimg = node["imgurl"];
			string slotid = node["slotid"];
			
			// Break out of the foreach if we have hit the max amount of preBanners to display
			if(preBannerURL.Count >= adLimit)
				break;
			
			// If the current ad is an advert for this game then use game id 4 instead
			if(sloturl.Contains(bundleIdentifier))
				continue;
			
			// Take the last value from the slotid name
			int bannerID = int.Parse(new string(slotid[slotid.Length-1], 1));
			
			// Add the advert URL as an item in the list
			preBannerURL.Add(sloturl);
			
			// Set the ad image url to a new item in the list
			preBannerImageURL.Add (slotimg);
			
			
		}
		
		// Dispose of the JSON data
		wwwJSON.Dispose();
		
		// Download the images
		for(int i = 0; i < preBannerURL.Count; i++)
		{      
			// Request the banner image URL
			WWW wwwImage = new WWW(preBannerImageURL[i]);
			
			yield return wwwImage;
			
			// Set the texture
			preBannerTextures.Add(wwwImage.texture);
			
			// Dispose of the image data
			wwwImage.Dispose();            
		}
		
		// Set the preReady variable as true once all the ads are loaded and ready
		preReady = true;
	}
	
}