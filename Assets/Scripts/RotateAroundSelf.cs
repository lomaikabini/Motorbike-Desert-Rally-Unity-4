using UnityEngine;
using System.Collections;

public class RotateAroundSelf : MonoBehaviour {

	public Vector3 dir;
	public float speed;

	void Update () 
	{
			transform.Rotate (dir * speed);
	}
}
