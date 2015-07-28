using UnityEngine;
using System.Collections;

public class BikeCamera : MonoBehaviour
{
	public GameObject cameraCollDetector;
    public Transform target;
    public float smooth = 0.3f;
    public float distance = 5.0f;
    public float haight = 5.0f;
    public float Angle = 20;
    public Transform[] cameraSwitchView;
    public GUISkin GUISkin;

    private float yVelocity = 0.0f;
    private float xVelocity = 0.0f;
    private int Switch;
    private float backAngle = 0;
	[HideInInspector]
	public BikeControl BikeScript;
	private float tolleranceAngle = 10f;
	[HideInInspector]
	public bool underGround =false;
	[HideInInspector]
	public bool underRamps = false;

	public bool isForShop =false;
	float bikeAngle = 0f;

	float valueForAngle =0f;
	public LayerMask lineOfSightMask = 0;
	void Start()
	{
		lineOfSightMask = LayerMask.NameToLayer("Bike");
		BikeScript = (BikeControl)target.GetComponent<BikeControl>();
	}

    void Update()
    {
		if(underGround)
		{
			bikeAngle +=Time.deltaTime*120f;
			bikeAngle = Mathf.Min(bikeAngle,35f);
		}
		else if(underRamps)
		{
			bikeAngle +=Time.deltaTime*120f;
			bikeAngle = Mathf.Min(bikeAngle,20f);
		}
		else if (!underGround && !underRamps )
		{
			float value = Time.deltaTime *50f;
			if(bikeAngle - value>0)
				bikeAngle -= value;
			else
				bikeAngle = 0f;
			value = Time.deltaTime * 130f;
			if(valueForAngle  -value >0 )
				valueForAngle -=value;
			else
				valueForAngle = 0f;
		}
						
		if(BikeScript != null)
        camera.fieldOfView = Mathf.Clamp(BikeScript.speed / 10.0f + 60.0f, 60, 90.0f);

//        if (BikeScript.curTorque == BikeScript.bikeSetting.shiftPower)
//        {
//            transform.GetComponent<MotionBlur>().blurAmount = Mathf.Lerp(transform.GetComponent<MotionBlur>().blurAmount, 1.0f, Time.deltaTime * 5);
//        }
//        else
//        {
//            transform.GetComponent<MotionBlur>().blurAmount = Mathf.Lerp(transform.GetComponent<MotionBlur>().blurAmount, 0.0f, Time.deltaTime);
//        }

		
		
		if (Input.GetKeyDown(KeyCode.C))
        {
            Switch++;
            if (Switch > cameraSwitchView.Length) { Switch = 0; }
        }

        if (Switch == 0)
        {
            if (BikeScript == null || BikeScript.currentGear == 0 && BikeScript.speed > 2)
            {
                backAngle = 180;

            }
            else
            {
                backAngle = 0;
            }


            float yAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y,
            target.eulerAngles.y + backAngle, ref yVelocity, smooth);

            /*
            float xAngle = Mathf.SmoothDampAngle(transform.eulerAngles.x,
            target.eulerAngles.x + Angle, ref xVelocity, smooth);
            */


            // Position at the target
            Vector3 position = target.position;
            // Then offset by distance behind the new angle
            position += Quaternion.Euler(0, yAngle, 0) * new Vector3(0, 0, -distance);
            // Apply the position
            //  transform.position = position;

            // Look at the target
            //transform.eulerAngles = new Vector3(Angle, yAngle, 0);

			if(underGround || underRamps)
			{
				var d = transform.rotation * Vector3.down;
				float dist = AdjustLineOfSight(transform.position,d);
				if(dist < 0.25f)
					valueForAngle +=Time.deltaTime * 130f;
			}
			//for colision detector
			cameraCollDetector.transform.eulerAngles = new Vector3(Angle, yAngle, 0);
			
			var direction1 = cameraCollDetector.transform.rotation * -Vector3.forward;
			var targetDistance1 = AdjustLineOfSight(target.position + new Vector3(0, haight, 0), direction1);

			cameraCollDetector.transform.position = target.position + new Vector3(0, haight, 0)+ direction1 * targetDistance1;
			
			//for camera
			transform.eulerAngles = new Vector3(Angle+bikeAngle+valueForAngle, yAngle, 0);
			var direction = Vector3.zero;
			if(!isForShop)
            	direction = transform.rotation * -Vector3.forward;
			else
				direction = transform.rotation * -Vector3.right;
            var targetDistance = AdjustLineOfSight(target.position + new Vector3(0, haight, 0), direction);
			
			transform.position = target.position + new Vector3(0, haight, 0)+ direction * targetDistance;
//			Vector3 rot = transform.eulerAngles;
//			rot.x -=bikeAngle;
//			transform.rotation = Quaternion.Euler(rot);
			//cameraCollDetector.transform.position = transform.position;
        }
        else
        {

            transform.position = cameraSwitchView[Switch - 1].position;
            transform.rotation = Quaternion.Lerp(transform.rotation, cameraSwitchView[Switch - 1].rotation, Time.deltaTime * 10.0f);

        }

    }


	public void switchCamera()
	{
		AppSoundManager.Get ().PlaySfx (Sfx.Type.sfx_click);
		var BikeScript = (BikeControl)target.GetComponent<BikeControl>();
		camera.fieldOfView = Mathf.Clamp(BikeScript.speed / 10.0f + 60.0f, 60, 90.0f);

		if (BikeScript.curTorque == BikeScript.bikeSetting.shiftPower)
		{
			//transform.GetComponent<MotionBlur>().blurAmount = Mathf.Lerp(transform.GetComponent<MotionBlur>().blurAmount, 1.0f, Time.deltaTime * 5);
		}
		else
		{
			//transform.GetComponent<MotionBlur>().blurAmount = Mathf.Lerp(transform.GetComponent<MotionBlur>().blurAmount, 0.0f, Time.deltaTime);
		}
		Switch++;
		if (Switch > cameraSwitchView.Length) { Switch = 0; }
		if (Switch == 0)
		{
			if (BikeScript.currentGear == 0 && BikeScript.speed > 2)
			{
				backAngle = 180;
				
			}
			else
			{
				backAngle = 0;
			}
			
			
			float yAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y,
			                                     target.eulerAngles.y + backAngle, ref yVelocity, smooth);
			
			/*
            float xAngle = Mathf.SmoothDampAngle(transform.eulerAngles.x,
            target.eulerAngles.x + Angle, ref xVelocity, smooth);
            */
			
			
			// Position at the target
			Vector3 position = target.position;
			// Then offset by distance behind the new angle
			position += Quaternion.Euler(0, yAngle, 0) * new Vector3(0, 0, -distance);
			// Apply the position
			//  transform.position = position;
			
			// Look at the target
			transform.eulerAngles = new Vector3(Angle, yAngle, 0);
			
			var direction = transform.rotation * -Vector3.forward;
			var targetDistance = AdjustLineOfSight(target.position + new Vector3(0, haight, 0), direction);

			
			
			transform.position = target.position + new Vector3(0, haight, 0) + direction * targetDistance;
			
			
		}
		else
		{
			
			transform.position = cameraSwitchView[Switch - 1].position;
			transform.rotation = Quaternion.Lerp(transform.rotation, cameraSwitchView[Switch - 1].rotation, Time.deltaTime * 10.0f);
			
		}
	}



    float AdjustLineOfSight(Vector3 target, Vector3 direction)
    {

        RaycastHit hit;

        if (Physics.Raycast(target, direction, out hit, distance, lineOfSightMask))
            return hit.distance;
        else
            return distance;

    }








    /////////// for test Only //////////////////////////////// 


    public void OnGUIxxxxxxxxxxxxxxxxxxxx()
    {


        GUI.skin = GUISkin;


        GUI.Box(new Rect(5, 5, 280, 250), "");

        GUI.Label(new Rect(10, 10, 200, 50), "MSK (keys to control the bike)");

        GUI.Label(new Rect(10, 50, 200, 50), "'C' key to change view (camera)");

        GUI.Label(new Rect(10, 100, 250, 50), "'ARROWS' keys or 'WASD' keys to drive the bike");

        GUI.Label(new Rect(10, 150, 250, 50), "'Shift' key to shift bike");

        GUI.Label(new Rect(10, 200, 200, 50), "'R' key to rest scene");

        if (Input.GetKeyDown(KeyCode.R))
        {
            Application.LoadLevel(Application.loadedLevel);
        }

    }

//	void OnTriggerEnter(Collider other) {
//		if (other.name == "Terrain")
//		{
//			Debug.Log("zawlo");
//				underGround = true;
//		}
//	}
//
//	void OnTriggerExit(Collider other) {
//		if (other.name == "Terrain")
//		{
//			Debug.Log("viwlo");
//			underGround = false;
//		}
//	}
	
	
}
