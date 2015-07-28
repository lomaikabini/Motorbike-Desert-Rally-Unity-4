using UnityEngine;
using System.Collections;

public class ItemRotator : MonoBehaviour {

	public bool rotate;
	[HideInInspector]
	public Transform target;

	void Update () 
	{
		if(rotate)
		{
			for(int i = 0; i < transform.childCount; i++)
			{
				Transform tr = transform.GetChild(i);
				if(Vector3.Distance(tr.position,target.position) > 5f)
					transform.GetChild(i).LookAt(target.position);
			}
		}
	}
}
