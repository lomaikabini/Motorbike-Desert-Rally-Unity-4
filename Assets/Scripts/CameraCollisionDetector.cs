using UnityEngine;
using System.Collections;

public class CameraCollisionDetector : MonoBehaviour {

	public BikeCamera cam;

	int ramps = 0;

		void OnTriggerEnter(Collider other) {
			if (other.gameObject.layer ==LayerMask.NameToLayer("Terrain"))
			{
				cam.underGround = true;
			}
			else if(other.gameObject.layer ==LayerMask.NameToLayer("Ramps"))
			{
				cam.underRamps = true;
				ramps++;
			}
		}

		void OnTriggerExit(Collider other) 
		{
			if (other.gameObject.layer ==LayerMask.NameToLayer("Terrain"))
				cam.underGround = false;
			else if(other.gameObject.layer == LayerMask.NameToLayer("Ramps"))
			{
				ramps --;
				if(ramps == 0)
				cam.underRamps = false;
			}
		}

}
