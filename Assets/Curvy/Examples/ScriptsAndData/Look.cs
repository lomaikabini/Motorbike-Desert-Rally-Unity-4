using UnityEngine;
using System.Collections;

public class Look : MonoBehaviour {
    public Transform Target;

    Transform mTransform;

	// Use this for initialization
	void Start () {
        mTransform = transform;
	}
	
	
	void LateUpdate () {
        if (Target)
            mTransform.LookAt(Target);
	}
}
