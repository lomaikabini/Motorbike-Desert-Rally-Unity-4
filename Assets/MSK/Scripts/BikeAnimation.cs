using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Animator))]

public class BikeAnimation : MonoBehaviour
{



    protected Animator animator;

	public float crashDistance = 1f;
	public int bikeId;

    public bool ikActive = false;
    public float RestTime = 5;

    public Transform myBike;

    public AudioSource crashSound;

    public Transform player, playerRoot;
    public Transform eventPoint;

    public Transform rightHandObj, leftHandObj = null;
    public Transform rightFootObj, leftFootObj = null;


    public Texture2D RestGUI;
    private float AlphaColor = 0.0f;



    private BikeControl BikeScript;



    private Vector3 myPosition;
    private Quaternion myRotation;
    private float timer;



    private float steer = 0.0f;
    private float speed = 0.0f;
    private float groundedTime = 0.0f;
    private bool grounded = true;

	GameData data;
    void Start()
    {
		data = GameData.Get ();
        BikeScript = myBike.GetComponent<BikeControl>();
        animator = player.GetComponent<Animator>();


        myPosition = player.localPosition;
        myRotation = player.localRotation;
        DisableRagdoll(true);
    }






    void OnGUIxxx()
    {

        if (timer == 0 && AlphaColor!=0.0f)
        {
            AlphaColor = Mathf.MoveTowards(AlphaColor, 0.0f, Time.deltaTime);
        }
        else
        {
            AlphaColor = Mathf.MoveTowards(AlphaColor, 1.0f, Time.deltaTime);
        }


        GUI.color = new Color(1, 1, 1, AlphaColor);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), RestGUI);

    }

    void Update()
    {


        Vector3 dir;



        if (timer!=0.0f)
        timer = Mathf.MoveTowards(timer, 0.0f, 0.02f);



        if (BikeScript.grounded)
        {

            dir = eventPoint.TransformDirection(Vector3.forward);

        }
        else
        {
            dir = eventPoint.TransformDirection(0, -0.25f, 1);
        }





        Debug.DrawRay(eventPoint.position, dir,Color.red);


        RaycastHit hit;

		var layerMask = ~(1 << LayerMask.NameToLayer("Ramps"));
		layerMask = ~(1 << LayerMask.NameToLayer("Bike"));
		//layerMask = ~layerMask;

        if (Physics.Raycast(eventPoint.position, dir, out hit, crashDistance, layerMask) && BikeScript.speed > 1400f)
        {
			//TODO:rewrite if need to crash bike
			string l = LayerMask.LayerToName(hit.transform.gameObject.layer);
			if(l == "Ramps" || l == "Bike")
				goto azaza;
            if (player.parent != null)
            {
				if(data.sfx)
                	crashSound.audio.Play();
                player.parent = null;
            }

			//Debug.Log(hit.transform.name+" "+);
			Game.instance.ShowFallDownMsg();
            DisableRagdoll(true);
            player.GetComponent<Animator>().enabled = false;

            BikeScript.crash = true;
            timer = RestTime;
        }

		azaza:

        if (timer == 0.0f)
        {

            player.GetComponent<Animator>().enabled = true;
            DisableRagdoll(false);

            player.parent = myBike;

            player.localPosition = myPosition;
            player.localRotation = myRotation;

            BikeScript.crash = false;

			if(Game.instance != null)
				Game.instance.HideFallDownMsg();

        }




        if (player.GetComponent<Animator>().enabled != true) return;



        if (BikeScript.speed > 50 && grounded)
        {
            steer = BikeScript.steer;
        }
        else
        {
            steer = Mathf.MoveTowards(steer, 0.0f, Time.deltaTime * 10.0f);


        }



        if (BikeScript.grounded)
        {
            grounded = true;
            groundedTime = 2.0f;
        }
        else
        {
            groundedTime = Mathf.MoveTowards(groundedTime, 0.0f, Time.deltaTime * 10.0f);

            if (groundedTime == 0)
                grounded = false;
        }









        if (BikeScript.currentGear > 0)
        {
            speed = BikeScript.speed;
        }
        else
        {
            speed = -BikeScript.speed;
        }





        animator.SetFloat("speed", speed);

        animator.SetFloat("right", steer);

        animator.SetBool("grounded", grounded);


    }






    void DisableRagdoll(bool active)
    {


        Component[] transforms = playerRoot.GetComponentsInChildren(typeof(Rigidbody));

        foreach (Rigidbody t in transforms)
        {
            t.isKinematic = !active;
        }


        Component[] transforms2 = playerRoot.GetComponentsInChildren(typeof(Collider));

        foreach (Collider t in transforms2)
        {
            t.enabled = active;
        }



    }






    //a callback for calculating IK
    void OnAnimatorIK()
    {




        if (player.GetComponent<Animator>().enabled != true) return;




        if (animator)
        {

            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive)
            {


                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);


                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1.0f);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1.0f);

                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1.0f);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1.0f);




                if (leftHandObj != null)
                {
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandObj.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObj.rotation);
                }


                if (speed > -1)
                {

                    //set the position and the rotation of the right hand where the external object is
                    if (rightHandObj != null)
                    {
                        animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
                        animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
                    }



                    //set the position and the rotation of the right hand where the external object is
                    if (rightFootObj != null)
                    {
                        animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootObj.position);
                        animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootObj.rotation);
                    }

                    if (leftFootObj != null && BikeScript.speed > 30.0f)
                    {

                        animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootObj.position);
                        animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootObj.rotation);
                    }


                }



            }

                //if the IK is not active, set the position and rotation of the hand back to the original position
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            }
        }
    }
}