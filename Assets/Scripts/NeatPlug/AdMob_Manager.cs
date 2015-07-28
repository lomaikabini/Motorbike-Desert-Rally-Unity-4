// i6 AdMob_Manager [Updated 31st July 2014]
// Attach this script to a persistent Game Object in your initial scene

using UnityEngine;
using System.Collections;

public class AdMob_Manager : MonoBehaviour {
	
	// Set the interstitial ID and banner ID in your scene's inspector for this gameobject!
	public string interstitialID;
	public string bannerID;

	public float timeForIntersentialRepeat = 5f;
	public bool loadInterstitialOnStart = false;	// Will the interstitial LOAD when this script is started
	public bool showInterstitialOnStart = false;	// Will the interstitial DISPLAY when the interstitial is done loading
	public float loadInterstitialWaitTime = 5f;		// How long should our loading screens wait for interstitials when they are pending
	
	public bool hideBannerOnInterstitial = false;	// Will the banner HIDE when an interstitial is opened then SHOW when closed
	
	public bool keepInterstitialsLoaded = false;	// Will we LOAD another interstitial when a previous interstitial has been closed
	
	// Static script instance allowing us to access public variables in this script externally
	public static AdMob_Manager Instance;

	[HideInInspector]
	public float curTime = 0f;
	// This information will allow us to get the state of adverts, see below for the public get variables
	private bool _isInterstitialReady = false;
	private bool _isInterstitialLoading = false;
	private bool _isInterstitialVisible = false;
	
	private bool _isBannerReady = false;
	private bool _isBannerLoading = false;
	private bool _isBannerVisible = false;
	
	private bool memBannerState = false;
	
	private bool _isAdvertsEnabled = true;
	
	// These public variables are using a public get and private set to allow us to get this information 
	// from this script but not allow us to change the values externally
	public bool isInterstitialReady { 
		get { return _isInterstitialReady; }
		private set { _isInterstitialReady = value; }
	}
	
	public bool isInterstitialLoading {
		get { return _isInterstitialLoading; }
		private set { _isInterstitialLoading = value; }
	}
	
	public bool isInterstitialVisible {
		get { return _isInterstitialVisible; }
		private set { _isInterstitialVisible = value; }
	}
	
	public bool isBannerReady {
		get { return _isBannerReady; }
		private set { _isBannerReady = value; }
	}
	
	public bool isBannerLoading {
		get { return _isBannerLoading; }
		private set { _isBannerLoading = value; }
	}
	
	public bool isBannerVisible {
		get { return _isBannerVisible; }
		private set { _isBannerVisible = value; }
	}
	
	public bool isAdvertsEnabled {
		get { return _isAdvertsEnabled; }
		private set { _isAdvertsEnabled = value; }
	}
	
	private static bool _instanceFound = false;
	
	void Awake()
	{
		if(!Instance){
			Instance = this;
			DontDestroyOnLoad(this);
		} else {
			Destroy(this.gameObject);
			Debug.LogWarning("You have duplicate AdMob Managers in your scene!");
		}
		
		AdmobAdAgent.RetainGameObject(ref _instanceFound, gameObject, null);
		AdmobAd.Instance();
	}
	
	void OnEnable()
	{
		// Register the ad events
		AdmobAdAgent.OnReceiveAd += OnReceiveAd;
		AdmobAdAgent.OnFailedToReceiveAd += OnFailedToReceiveAd;
		AdmobAdAgent.OnReceiveAdInterstitial += OnReceiveAdInterstitial;
		AdmobAdAgent.OnFailedToReceiveAdInterstitial += OnFailedToReceiveAdInterstitial;
		AdmobAdAgent.OnPresentScreenInterstitial += OnPresentScreenInterstitial;
		AdmobAdAgent.OnDismissScreenInterstitial += OnDismissScreenInterstitial;
		AdmobAdAgent.OnAdShown += OnAdShown;
		AdmobAdAgent.OnAdHidden += OnAdHidden;
		AdmobAdAgent.OnLeaveApplicationInterstitial += OnLeaveApplicationInterstitial;	
		AdmobAdAgent.OnLeaveApplication += OnLeaveApplication;
	}
	
	void OnDisable()
	{
		// Unregister the ad events
		AdmobAdAgent.OnReceiveAd -= OnReceiveAd;
		AdmobAdAgent.OnFailedToReceiveAd -= OnFailedToReceiveAd;
		AdmobAdAgent.OnReceiveAdInterstitial -= OnReceiveAdInterstitial;
		AdmobAdAgent.OnFailedToReceiveAdInterstitial -= OnFailedToReceiveAdInterstitial;
		AdmobAdAgent.OnPresentScreenInterstitial -= OnPresentScreenInterstitial;
		AdmobAdAgent.OnDismissScreenInterstitial -= OnDismissScreenInterstitial;
		AdmobAdAgent.OnAdShown -= OnAdShown;
		AdmobAdAgent.OnAdHidden -= OnAdHidden;
		AdmobAdAgent.OnLeaveApplicationInterstitial -= OnLeaveApplicationInterstitial;
		AdmobAdAgent.OnLeaveApplication -= OnLeaveApplication;
	}	
	
	void Start()
	{
		// Set the banner ID and interstitial ID for this app
		AdmobAd.Instance().Init(bannerID, interstitialID);
		
		// Check if adverts are enabled
		isAdvertsEnabled = AdmobAd.Instance().IsAdEnabled();
		//TODO: my inserts
		loadBanner (AdmobAd.BannerAdType.Universal_Banner_320x50, AdmobAd.AdLayout.Top_Centered, true);
		////////////////
		if(!isAdvertsEnabled)
			enableAllAds();
		
		// LoadInterstitialAd takes a boolean stating if it should hold the advert until manually shown or not
		if(loadInterstitialOnStart){
			loadInterstitial(showInterstitialOnStart);
		}
		//StartCoroutine(AdMob_Manager.Instance.waitForInterstitialLoad());
	}
	void Update()
	{
		curTime+=Time.deltaTime;
		if(curTime > timeForIntersentialRepeat && GoTo.currentScene == "main_game_megaCity")
			{
				curTime = 0f;
				showInterstitial();
			}
	}
	// Not yet used or tested but should work
	public IEnumerator waitForInterstitialLoad()
	{
		// Loading screen should have already been displayed before running this
		if(isInterstitialLoading){
			float curWaitTime = 0f;
			
			do{
				curWaitTime += Time.deltaTime;
				yield return null;
			} while(curWaitTime < loadInterstitialWaitTime && !isInterstitialReady);
			
			if(isInterstitialReady){
				// Interstitial loaded fine, we can show it and record how long it took
				showInterstitial(false);
				
				GoogleAnalytics.Instance.LogEvent("Interstitial Load", "Success after " + curWaitTime + " seconds!");
			} else {
				// We waited but the interstitial never loaded :(
				// If the interstitial actually encounted an error then OnFailedToReceiveAdInterstitial
				// will be triggered and a seperate Analytics event will be recorded
				
				GoogleAnalytics.Instance.LogError("Interstitial expected but didn't load!", false);
			}
		} else {
			// Record Google Analytics event stating no interstitial was pending
			Debug.Log("succes");
			yield return true;
		}
	}
	
	// Load a interstitial advert from Google
	public void loadInterstitial(bool displayImmediately)
	{
		// Make sure there isn't already an interstitial advert loading or ready
		if(!isInterstitialLoading && !isInterstitialReady){
			// Load the interstitial and send the displayImmediately bool
			AdmobAd.Instance().LoadInterstitialAd(!displayImmediately);
			
			// Set the interstitial loading variable so we know we have a interstitial loading
			isInterstitialLoading = true;
		}
	}
	
	// Load a banner advert from Google
	public void loadBanner(AdmobAd.BannerAdType adSize, AdmobAd.AdLayout adPosition, bool displayImmediately = false)
	{
		// Make sure there isn't already a banner advert loading or ready
		if(!isBannerLoading && !isBannerReady){
			// Load the banner advert with adSize and adPosition as requested
			AdmobAd.Instance().LoadBannerAd(adSize, adPosition);
			
			// Set the banner loading variable so we know we have a banner loading
			isBannerLoading = true;
		}
		
		if(displayImmediately)
			showBanner(false, true);
	}
	
	// Show the interstitial we have loaded
	public void showInterstitial(bool force = false)
	{
		// Make sure we have a ready interstitial or we are forcing it
		if(isInterstitialReady || force){
			AdmobAd.Instance().ShowInterstitialAd();
		} else {
			GoogleAnalytics.Instance.LogError("Interstital was requested but it wasn't ready", false);
		}
	}
	
	// Show the banner advert
	public void showBanner(bool overlapScreen = false, bool force = false)
	{
		// If this is not an overlapping screen with the previous banner state as hidden
		if(!(overlapScreen && !memBannerState)){
			
			// Make sure we have a ready banner or we are forcing it
			if(isBannerReady || force)
				AdmobAd.Instance().ShowBannerAd();
			
		}
	}
	
	// Hide the banner advert from view
	public void hideBanner(bool overlapScreen = false)
	{
		// If this is an overlapping screen then remember the current banner state for when we close the overlapping window
		if(overlapScreen)
			memBannerState = isBannerVisible;
		
		// Just hiding the banner, it can also be re-shown again
		AdmobAd.Instance().HideBannerAd();
	}
	
	// Load a new banner advert
	public void refreshBanner()
	{
		// Note: Not yet sure if the banner is hidden whilst this loads a new ad or if the previous banner stays until refreshed
		AdmobAd.Instance().RefreshBannerAd();
	}
	
	// Destroy the banner advert
	public void destroyBanner()
	{
		isBannerReady = false;
		
		AdmobAd.Instance().DestroyBannerAd();
	}
	
	// Disable all interstitials and banner adverts
	public void disableAllAds()
	{
		AdmobAd.Instance().DisableAd();
		isAdvertsEnabled = false;
	}
	
	public void enableAllAds()
	{
		AdmobAd.Instance().EnableAd();
		isAdvertsEnabled = true;
	}
	
	
	/* ====== ADVERT EVENTS ====== */
	
	// Banner advert loaded and ready to be displayed
	void OnReceiveAd()
	{
		isBannerReady = true;
	}
	
	// Failed to receive banner advert
	void OnFailedToReceiveAd(string error)
	{
		isBannerLoading = false;
		
		GoogleAnalytics.Instance.LogError("Banner failed to load - " + error, false);
	}
	
	// Banner advert is visible
	void OnAdShown()
	{
		isBannerVisible = true;
	}
	
	// Banner advert is hidden
	void OnAdHidden()
	{
		isBannerVisible = false;
	}
	
	// Player clicked a banner advert and the app has minimized
	void OnLeaveApplication()
	{
		GoogleAnalytics.Instance.LogEvent("AdMob Clicks", "Banner clicked");
	}
	
	
	// Interstitial loaded and ready to be displayed
	void OnReceiveAdInterstitial()
	{
		isInterstitialReady = true;
		isInterstitialLoading = false;
	}
	
	// Failed to receive interstitial advert (e.g no internet connection)
	void OnFailedToReceiveAdInterstitial(string error)
	{
		isInterstitialLoading = false;
		
		GoogleAnalytics.Instance.LogError("Interstitial failed to load - " + error, false);
	}
	
	// When interstitial window opens
	void OnPresentScreenInterstitial()
	{
		isInterstitialReady = false;
		isInterstitialVisible = true;
		
		if(hideBannerOnInterstitial)
			AdmobAd.Instance().HideBannerAd();
	}
	
	// When interstitial window is closed (Via hardware back button or clicking the X)
	void OnDismissScreenInterstitial()
	{
		isInterstitialVisible = false;
		
		if(hideBannerOnInterstitial)
			AdmobAd.Instance().ShowBannerAd();
		
		if(keepInterstitialsLoaded)
			loadInterstitial(false);
	}
	
	// The player clicked an interstitial advert and the app has minimized
	void OnLeaveApplicationInterstitial()
	{
		GoogleAnalytics.Instance.LogEvent("AdMob Clicks", "Interstitial clicked");
	}
	
}
