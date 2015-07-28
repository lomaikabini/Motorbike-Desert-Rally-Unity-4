
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarAI : MonoBehaviour {
	
	public GameObject buttons;
	public CurvySpline spline;
	private Transform target;
	private float rotateSpeed = 1f;
	//	private float movementSpeed = 1000f;
	//	private float maxVelocity = 12f;
	public float movementSpeed = 400f;
	private float maxVelocity = 12f;
	private List<Transform> points;
	private int currentIndx = 0;

	public WheelCollider[] wheelColliders;


	private bool isRide = false;
	public static Transform motik;
	BoxCollider collider;
	void Start () 
	{
		//Transform centerOfmass = transform.Find("centerMass");
		//		gameObject.rigidbody.centerOfMass = centerOfmass.localPosition;
		points = new List<Transform> ();
		int count = spline.transform.childCount;
		for (int i = 0; i<count; i++)
			points.Add (spline.transform.GetChild(i));
		//motik = BikeManager.instance.cam.target; //GameObject.Find ("Motorbike 1").transform;
		collider = gameObject.GetComponent<BoxCollider> ();
	}

	void FixedUpdate () 
	{
		//TODO: zamytit't ety wtyky, kogda ne pokazani knopki ne davat' ta4kam exat'
		if(buttons != null && !isRide)
		{
			isRide = buttons.activeSelf;
			return;
		}

		if(motik != null && Vector3.Distance(motik.position,transform.position) < 10f)
		{
			collider.enabled = true;
		}
		else
		{
			collider.enabled = false;
		}
		setTarget ();
		Vector3 targetDir = target.transform.position - transform.position;
		targetDir.y = 0f;
		var rotation = Quaternion.LookRotation(targetDir);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotateSpeed); 
		//		float step = rotateSpeed * Time.deltaTime;
		//		Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
		//		transform.rotation = Quaternion.LookRotation(newDir);
		
		Vector3 angles = gameObject.transform.eulerAngles;
		Vector3 vel  = gameObject.rigidbody.velocity;
		if(!(angles.z > 328f || angles.z < 32f))
		{
			if(angles.z < 180f)
				angles.z = 32f;
			else
				angles.z  = 328f;
		}
		
		gameObject.transform.eulerAngles = angles;
		
		if (Mathf.Abs(gameObject.rigidbody.velocity.x) > maxVelocity)
		{
			if(vel.x<0f)
				vel.x = -maxVelocity;
			else
				vel.x = maxVelocity;
			
			targetDir.x = 0f;
		}
		if (Mathf.Abs (gameObject.rigidbody.velocity.z) > maxVelocity)
		{
			if(vel.z<0f)
				vel.z = -maxVelocity;
			else
				vel.z = maxVelocity;
			targetDir.z = 0f;
		}

		Vector3 additionalForce = Vector3.zero;
		bool addAdditionalForce = true;
		for(int i = 0; i < wheelColliders.Length;i++)
		{
			if(wheelColliders[i].isGrounded)
			{
				addAdditionalForce = false;
				break;
			}
		}
		if(addAdditionalForce)
		{
			additionalForce = new Vector3(0f,-100f,0f);
		}

		gameObject.rigidbody.velocity = vel;
		gameObject.rigidbody.AddForce((targetDir.normalized+additionalForce) * movementSpeed);
	}
	
	void setTarget ()
	{
		Vector3 targetPos = points [currentIndx].position;
		targetPos.y = gameObject.transform.position.y;
		if(Vector3.Distance(gameObject.transform.position,targetPos) < 10f)
		{
			if(currentIndx+1< points.Count)
				currentIndx++;
			else
				currentIndx = 0;
		}
		//Debug.Log (Vector3.Distance(gameObject.transform.position,points[currentIndx].position));
		target = points [currentIndx];
	}
}
