using UnityEngine;
using System.Collections;

public class IAS_Handler : MonoBehaviour
{      
	public int bannerID;
	public bool backscreen_ad = false;
	public bool pixelPerfect = false;
	
	private bool textureSet = false;
	
	void Start()
	{
		if(!textureSet)
			LoadTexture();
	}
	
	void Update()
	{
		if(!textureSet)
			LoadTexture();
	}
	
	private void LoadTexture()
	{
		// Don't try display an IAS advert until it is ready!
		if((backscreen_ad && !IAS_Manager.Instance.Backscreen_IASReady) || !backscreen_ad && !IAS_Manager.Instance.Main_IASReady)
			return;
		
		if(IAS_Manager.Instance.GetAdTexture(bannerID, backscreen_ad) != null)
		{
			UITexture uiTexture = GetComponent<UITexture>();
			uiTexture.mainTexture = IAS_Manager.Instance.GetAdTexture(bannerID, backscreen_ad);
			
			if(pixelPerfect)
				uiTexture.MakePixelPerfect();
			
			textureSet = true;
		}
	}
	
	void OnClick()
	{
		// If an IAS advert is not ready then it can't be clicked
		if((backscreen_ad && !IAS_Manager.Instance.Backscreen_IASReady) || !backscreen_ad && !IAS_Manager.Instance.Main_IASReady)
			return;
		
		string url = IAS_Manager.Instance.GetAdURL(bannerID, backscreen_ad);
		
		// You'll want to track analytics here! Here's an example from one of our other games:
		//GoogleAnalytics.Instance.LogEvent((backscreen_ad ? "Backscreen " : "") + "IAS Click", "Screen: " + bannerID + ", URL: " + url.Replace("https://play.google.com/store/apps/details?id=", ""));
		
		if(url != "")
		{
			Application.OpenURL(url);
		}
	}
	
	
}