using UnityEngine;
using System.Collections;
using System.Collections.Generic;
	
	public class GoogleAnalytics : MonoBehaviour
	{
		
		public string AppName = "Bike Racing: Offroad Motocross";
		public string PropertyID = "UA-56263344-12";
		public string BundleID = "com.i6.bike_racing_offroad_motocross";
		public string AppVersion = "1.00";
		
		public static GoogleAnalytics Instance;
		
		private string screenResolution;
		private string clientID;
		
		private string userLanguage;   
		
		void Awake()
		{
			if(!Instance)
				Instance = this;
		}
		
		void Start()
		{
			// Get the device resolution
			screenResolution = Screen.width + "x" + Screen.height;
			
			// Get a unique identifier for the device http://docs.unity3d.com/Documentation/ScriptReference/SystemInfo-deviceUniqueIdentifier.html
			clientID = WWW.EscapeURL(SystemInfo.deviceUniqueIdentifier);
			
			// HTMLEscape our variables so it doesn't break the URL request
			AppName = WWW.EscapeURL(AppName);
			PropertyID = WWW.EscapeURL(PropertyID);
			BundleID = WWW.EscapeURL(BundleID);
			AppVersion = WWW.EscapeURL(AppVersion);
			
			// Lets get some extra information about this user
			userLanguage = Application.systemLanguage.ToString().ToLower();
			
			// Always log the initial Analytics start as "Start"
			LogScreen("Start");
		}
		
		public void LogScreen(string title)
		{
			// Get the htmlchars escaped title of the screen so it doesn't break the URL request
			title = WWW.EscapeURL(title);
			
			// URL which will be pinged to log the requested screen and include details about the user
			var url = "http://www.google-analytics.com/collect?v=1&ul="+userLanguage+"&t=appview&sr="+screenResolution+"&an="+AppName+"&tid="+PropertyID+"&aid="+BundleID+"&cid="+clientID+"&_u=.sB&av="+AppVersion+"&_v=ma1b3&cd="+title+"&qt=2500&z=185";
			
			// Process the URL
			StartCoroutine(Process(new WWW(url)));
		}
		
		public void LogEvent(string titleCat, string titleAction, string titleLabel = "", int value = 0)       
		{
			// Get the htmlchars escaped category and action of the event so it doesn't break the URL request
			titleCat = WWW.EscapeURL(titleCat);
			titleAction = WWW.EscapeURL(titleAction);
			
			// If we sent the action as a string of CLIENT_ID then replace it with the actual client ID
			titleLabel = (titleLabel == "CLIENT_ID" ? clientID : titleLabel);
			
			// URL which will be pinged to log the event and include details about the user
			var url = "http://www.google-analytics.com/collect?v=1&ul="+userLanguage+"&t=event&sr="+screenResolution+"&an="+AppName+"&tid="+PropertyID+"&aid="+BundleID+"&cid="+clientID+"&_u=.sB&av="+AppVersion+"&_v=ma1b3&ec="+titleCat+"&ea="+titleAction+"&ev="+value+"&el="+titleLabel+"&qt=2500&z=185";
			
			// Process the URL
			StartCoroutine(Process(new WWW(url)));
		}
		
		public void LogError(string description, bool isFatal)
		{
			// Get the htmlchars escaped description so it doesn't break the URL request
			description = WWW.EscapeURL(description);
			
			int fatal = (isFatal ? 1 : 0);
			
			// URL which will be pinged to log the requested screen and include details about the user
			var url = "http://www.google-analytics.com/collect?v=1&ul="+userLanguage+"&t=exception&sr="+screenResolution+"&an="+AppName+"&tid="+PropertyID+"&aid="+BundleID+"&cid="+clientID+"&_u=.sB&av="+AppVersion+"&_v=ma1b3&exd="+description+"&exf="+fatal+"&qt=2500&z=185";
			
			// Process the URL
			StartCoroutine(Process(new WWW(url)));
		}
		
		private IEnumerator Process(WWW www)
		{
			// Wait for the URL to be processed
			yield return www;
			
			// Cleanup the request data
			www.Dispose();
		}
		
	}