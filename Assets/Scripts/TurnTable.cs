using UnityEngine;
using System.Collections;

public class TurnTable : MonoBehaviour {

	float speed = 3f;

	void Update () 
	{
		transform.Rotate (new Vector3(0f,0f,-1f) * Time.deltaTime * speed);
	}
}
