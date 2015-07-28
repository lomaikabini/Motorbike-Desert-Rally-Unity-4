using UnityEngine;
using System.Collections;

public class BikeGUI : MonoBehaviour
{
//	public Texture2D arrow;
//	public Texture2D tachoMeter;
//	public GUISkin TachometerSkin;

	private int gearst = 0;
	private float thisAngle = -180;// -150;

//	public Texture2D shiftGUI;
//	public Texture2D barShiftGUI;
	[HideInInspector]
	public GameObject arrowUI;
	[HideInInspector]
	public UILabel speedUI;
	[HideInInspector]
	public UILabel gearstUI;
	[HideInInspector]
	public UIWidget nitroUI;

	BikeControl BikeScript;
	void Start()
	{
		BikeScript = transform.GetComponent<BikeControl>(); 
	}

	void initializeUI()
	{
		arrowUI = GameObject.Find ("Arrow").gameObject;
		speedUI = GameObject.Find ("speed").GetComponent<UILabel>();
		gearstUI = GameObject.Find ("peredacha").GetComponent<UILabel> ();
		nitroUI = GameObject.Find ("indikator").GetComponent<UIWidget> ();
	}

	void Update()
	{
		if(BikeScript ==  null)
			BikeScript = transform.GetComponent<BikeControl>(); 

		nitroUI.width = (int)BikeScript.powerShift * 2;
		nitroUI.height = 50;
		//Debug.Log (BikeScript.powerShift);
		gearst = BikeScript.currentGear;
		
		thisAngle = (BikeScript.motorRPM / 20) - 175;
		thisAngle = Mathf.Clamp(thisAngle, -180, 90);

			
			Vector2 pos = new Vector2 (91, 91); // rotatepoint in texture plus x/y coordinates. our needle is at 16/16. Texture is 128/128. Makes middle 64 plus 16 = 80
			
			arrowUI.transform.rotation = Quaternion.Euler(new Vector3(0f,0f,-thisAngle));

			if (gearst > 0 && BikeScript.speed > 1) {
				gearstUI.text = gearst.ToString();
			} else if (BikeScript.speed > 1) {
				gearstUI.text = "R";
			} else {
				gearstUI.text = "N";
			}
			
			speedUI.text = ((int)BikeScript.speed).ToString ();
	}

//	void OnGUIxxxx()
//	{
//		gearst = BikeScript.currentGear;
//
//		thisAngle = (BikeScript.motorRPM / 20) - 175;
//		thisAngle = Mathf.Clamp(thisAngle, -180, 90);
//
//		if (Game.isRunning) {
//
//			GUI.color = new Color (1, 1, 1, 0.5f);
//			GUI.DrawTexture (new Rect (Screen.width / 2 - 74, Screen.height - 70, 148, 50), shiftGUI);
//			GUI.color = Color.white;
//			GUI.DrawTexture (new Rect (Screen.width / 2 - 74 + 36, Screen.height - 49, BikeScript.powerShift, 25), barShiftGUI);
//			GUI.BeginGroup (new Rect (Screen.width - 150, 50, 800, 600));
//			GUI.Label (new Rect (16, 16, 150, 150), tachoMeter);	// x position, y position, size x, size y
//
//			Matrix4x4 matrixBackup = GUI.matrix; //Here comes the tachoneedle rotation
//
//			Vector2 pos = new Vector2 (91, 91); // rotatepoint in texture plus x/y coordinates. our needle is at 16/16. Texture is 128/128. Makes middle 64 plus 16 = 80
//
//			GUIUtility.RotateAroundPivot (thisAngle, pos);
//			 
//			arrowUI.transform.rotation = Quaternion.Euler(new Vector3(0f,0f,-thisAngle));
//
//			Rect thisRect = new Rect (16, 16, 150, 150); //x position, y position, size x, size y
//
//			GUI.DrawTexture (thisRect, arrow);
//			GUI.matrix = matrixBackup;
//			GUI.skin = TachometerSkin;
//
//			if (gearst > 0 && BikeScript.speed > 1) {
//				GUI.color = Color.green;
//				GUI.Label (new Rect (109, 116, 250, 250), "" + gearst);
//				gearstUI.text = gearst.ToString();
//			} else if (BikeScript.speed > 1) {
//				GUI.color = Color.red;
//				GUI.Label (new Rect (109, 116, 250, 250), "R");
//				gearstUI.text = "R";
//			} else {
//				GUI.Label (new Rect (109, 116, 250, 250), "N");
//				gearstUI.text = "N";
//			}
//
//			GUI.skin.label.fontSize = 14;
//
//			GUI.color = new Color (0.0f, 0.0f, 0.0f, 0.5f);
//			GUI.Label (new Rect (75, 80, 100, 30), "000");
//
//			GUI.color = Color.yellow;
//			GUI.Label (new Rect (75, 80, 100, 30), ((int)BikeScript.speed).ToString ());
//
//			speedUI.text = ((int)BikeScript.speed).ToString ();
//
//			GUI.skin.label.fontSize = 8;
//			GUI.Label (new Rect (85, 95, 100, 30), "KM/H");
//
//			GUI.EndGroup ();
//		}
//	}
}
