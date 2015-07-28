using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BgAdjuster : MonoBehaviour {

	public enum Anchor
	{
		TOP_LEFT,
		TOP_CENTER,
		TOP_RIGHT,
		MIDDLE_LEFT,
		MIDDLE_CENTER,
		MIDDLE_RIGHT,
		BOTTOM_LEFT,
		BOTTOM_CENTER,
		BOTTOM_RIGHT
	}

    public Vector2 originalSize = new Vector2(13650, 7680);
    public float zDepth = 10;

	public Anchor baseAnchor = Anchor.BOTTOM_RIGHT;

    private Camera gameCam;
    private float originalAspect;
	// Use this for initialization

	void Start () {
        gameCam = Camera.main;
        originalAspect = (originalSize.x) / (originalSize.y);
        adjust();
	}
	
	// Update is called once per frame
	void Update () {
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            adjust();
        }
	}

    void adjust()
    {
        float scaleX = 2 * originalAspect;
        transform.localScale = new Vector3(scaleX*gameCam.orthographicSize, 2*gameCam.orthographicSize, 1);
        float camAspect = gameCam.aspect;
        float camWorldWidth = 2 * camAspect;
        Vector3 camPos = gameCam.transform.position;
		switch (baseAnchor)
		{
			//TODO: Finish this
		case Anchor.BOTTOM_RIGHT:
			camPos.x -= (scaleX - camWorldWidth) * 0.5f;
			break;
		
		}

        camPos.z = zDepth;
        transform.position = camPos;
    }
}
