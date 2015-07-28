using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
    public Transform Character;
    public float Distance=10;
    public float Height = 2;
    Transform mTransform;

	// Use this for initialization
	void Start () {
        mTransform = transform;
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 center = new Vector3(0, Character.position.y, 0);
        Vector3 charPos=Character.position;
        Ray R = new Ray(center,charPos-center);
        Vector3 camPos = R.GetPoint((charPos-center).magnitude + Distance) + new Vector3(0, Height, 0);
        // Damping
        mTransform.position = new Vector3(Mathf.Lerp(mTransform.position.x, camPos.x, 0.08f),
                                          Mathf.Lerp(mTransform.position.y, camPos.y, 0.01f),
                                          Mathf.Lerp(mTransform.position.z, camPos.z, 0.08f));  
            
        mTransform.LookAt(center);
	}
    
}
