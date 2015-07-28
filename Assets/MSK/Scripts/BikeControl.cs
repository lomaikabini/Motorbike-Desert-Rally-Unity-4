using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BikeControl : MonoBehaviour
{
	public ParticleSystem wheelParticle;
	[HideInInspector]
	public bool showBrakeParticles = false;
	bool useTilt = false;

	bool moveUp = false;
	bool moveDown = false;
	bool moveLeft = false;
	bool moveRight = false;
	bool nitro = false;
	float moveLeftValue = 0f;
	float moveRightValue = 0f;
	float moveUpValue = 0f;
	float moveDownValue = 0f;
	float stepForValue = 0.1f;//0.01f;
	
    // Wheels Setting /////////////////////////////////

    public BikeWheels bikeWheels;


    [System.Serializable]
    public class BikeWheels
    {

        public ConnectWheel wheels;
        public WheelSetting setting;
    }
	
    [System.Serializable]
    public class ConnectWheel
    {
        public Transform wheelFront; // connect to Front Right Wheel transform
        public Transform wheelBack; // connect to Front Left Wheel transform

        public Transform AxleFront; // connect to Back Right Wheel transform
        public Transform AxleBack; // connect to Back Left Wheel transform
    }
	
    [System.Serializable]
    public class WheelSetting
    {
        public bool AutomaticRadius = true;
        public float Radius = 0.25f; // the radius of the wheels
        public float Weight = 3f; // the weight of a wheel
        public float Distance = 0.2f;
    }
	
    // Lights Setting /////////////////////////////////

    public BikeLights bikeLights;

    [System.Serializable]
    public class BikeLights
    {
        public Light[] brakeLights;

    }
	
    // Bike sounds /////////////////////////////////

    public BikeSounds bikeSounds;

    [System.Serializable]
    public class BikeSounds
    {
        public AudioSource nitro;
        public AudioSource switchGear;

    }

    // Bike Particle /////////////////////////////////

    public BikeParticles bikeParticles;

    [System.Serializable]
    public class BikeParticles
    {
        public ParticleSystem shiftParticle1, shiftParticle2;
    }
	
    // Bike Engine Setting /////////////////////////////////

    public BikeSetting bikeSetting;

    [System.Serializable]
    public class BikeSetting
    {
        public bool showNormalGizmos = false;

        public Transform bikeSteer;

        public float springs = 7000.0f;
        public float dampers = 10.0f;

        public float bikePower = 70;
        public float shiftPower = 100;
        public float brakePower = 500;

        public Vector3 shiftCentre = new Vector3(0.0f, -0.6f, 0.0f); // offset of centre of mass
        public Vector3 shiftSpeed = new Vector3(0.0f, -0.5f, -0.8f); // offset of centre of mass

        public float maxSteerAngle = 30.0f; // max angle of steering wheels
        public float maxTurn = 1.5f;

        public float shiftDownRPM = 1500.0f; // rpm script will shift gear down
        public float shiftUpRPM = 4000.0f; // rpm script will shift gear up
        public float idleRPM = 700.0f; // idle rpm

        public float stiffness = 0.1f; // for wheels, determines slip

        public bool automatic = true;  // automatic, if true bike shifts auto

        public float[] gears = { -10f, 9f, 6f, 4.5f, 3f, 2.5f }; // gear ratios (index 0 is reverse)
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
    private Quaternion SteerRotation;
	
    [HideInInspector]
    public bool grounded = true;

    private float MotorRotation;

    [HideInInspector]
    public bool crash;

    [HideInInspector]
    public float steer = 0; // steering -1.0 .. 1.0

    [HideInInspector]
    public float steer2;

    private float accel = 0.0f; // accelerating -1.0 .. 1.0
	
    private bool shifmotor;

    [HideInInspector]
    public float curTorque = 100f;

    [HideInInspector]
    public float powerShift = 100;

    [HideInInspector]
    public bool shift;

    private float torque = 100f; // the base power of the engine (per wheel, and before gears)
	
    [HideInInspector]
    public float speed = 0.0f;

    private float lastSpeed = -15.0f;

    // table of efficiency at certain RPM, in tableStep RPM increases, 1.0f is 100% efficient
    // at the given RPM, current table has 100% at around 2000RPM
    float[] efficiencyTable = { 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 1.0f, 1.0f, 0.95f, 0.80f, 0.70f, 0.60f, 0.5f, 0.45f, 0.40f, 0.36f, 0.33f, 0.30f, 0.20f, 0.10f, 0.05f };

    // the scale of the indices in table, so with 250f, 750RPM translates to efficiencyTable[3].
    float efficiencyTableStep = 250.0f;
	[HideInInspector]
    public float shiftDelay = 0.0f;

    // shortcut to the component audiosource (engine sound).
    private AudioSource audioSource;

    [HideInInspector]
    public int currentGear = 1;

    [HideInInspector]
    public float motorRPM = 0.0f;

    private float wantedRPM = 0.0f;
    private float w_rotate;
	
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private WheelComponent[] wheels;

    private class WheelComponent
    {
        public Transform wheel;
        public Transform axle;
        public WheelCollider collider;
        public Vector3 startPos;
        public float rotation = 0.0f;
        public float maxSteer;
        public bool drive;
        public float pos_y = 0.0f;
    }

    private WheelComponent SetWheelComponent(Transform wheel, Transform axle, bool drive, float maxSteer, float pos_y)
    {

        WheelComponent result = new WheelComponent();
        GameObject wheelCol = new GameObject(wheel.name + "WheelCollider");

        wheelCol.transform.parent = transform;
        wheelCol.transform.position = wheel.position;
        wheelCol.transform.eulerAngles = transform.eulerAngles;
        pos_y = wheelCol.transform.localPosition.y;

        wheel.gameObject.AddComponent<WheelCollider>();
        WheelCollider col = (WheelCollider)wheelCol.AddComponent(typeof(WheelCollider));
		//TODO: rewrite if we got some problem with wheels size
        //col.transform.localScale = wheel.localScale;
		float cof;
		if(wheel.parent.localScale.x == 1f)
			cof = wheel.localScale.x;
		else
			cof = wheel.parent.localScale.x;

        col.radius = wheel.GetComponent<WheelCollider>().radius * cof;
        Destroy(wheel.transform.GetComponent<WheelCollider>());

        result.drive = drive;
        result.wheel = wheel;
        result.axle = axle;
        result.collider = wheelCol.GetComponent<WheelCollider>();
        result.pos_y = pos_y;
        result.maxSteer = maxSteer;
        result.startPos = wheelCol.transform.localPosition;

        return result;
    }
	
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnTiltPress()
	{
		useTilt = true;
		GameObject.Find("game").GetComponent<Game>().StartGame(true);
	}

	public void OnArrowsPress()
	{
		useTilt = false;
		GameObject.Find("game").GetComponent<Game>().StartGame(false);
	}

	public void PressNitroBtn()
	{
		nitro = true;
	}
	public void ReleaseNitroBtn()
	{
		nitro = false;
	}
	
	public void PressMoveUpBtn()
	{
		moveUp = true;
	}
	public void ReleaseMoveUpBtn()
	{
		moveUpValue = 0f;
		moveUp = false;
	}
	
	public void PressMoveDownBtn()
	{
		moveDown = true;
	}
	public void ReleaseMoveDownBtn()
	{
		moveDownValue = 0f;
		moveDown = false;
	}
	
	public void PressMoveLeftBtn()
	{
		moveLeft = true;
	}
	public void ReleaseMoveLeftBtn()
	{
		moveLeftValue = 0f;
		moveLeft = false;
	}

	public void PressMoveRightBtn()
	{
		moveRight = true;
	}
	public void ReleaseMoveRightBtn()
	{
		moveRightValue = 0f;
		moveRight = false;
	}

	public void ToggleControlTypeBtn() {
		useTilt = !useTilt;
		moveRightValue = 0f;
		moveRight = false;
		moveLeftValue = 0f;
		moveLeft = false;
		GameObject.Find("game").GetComponent<Game>().ShowLeftRightButtons(!useTilt);
	}

    void Start()
    {
		wheelParticle.enableEmission = false;

        SteerRotation = bikeSetting.bikeSteer.localRotation;
        wheels = new WheelComponent[2];

        wheels[0] = SetWheelComponent(bikeWheels.wheels.wheelFront, bikeWheels.wheels.AxleFront, false, bikeSetting.maxSteerAngle, bikeWheels.wheels.AxleFront.localPosition.y);
        wheels[1] = SetWheelComponent(bikeWheels.wheels.wheelBack, bikeWheels.wheels.AxleBack, true, 0, bikeWheels.wheels.AxleBack.localPosition.y);

        foreach (WheelComponent w in wheels)
        {
            WheelCollider col = w.collider;
            col.suspensionDistance = bikeWheels.setting.Distance;
            JointSpring js = col.suspensionSpring;

            js.spring = bikeSetting.springs;
            js.damper = bikeSetting.dampers;
            col.suspensionSpring = js;

            if (!bikeWheels.setting.AutomaticRadius)
                col.radius = bikeWheels.setting.Radius;


            col.mass = bikeWheels.setting.Weight;


            WheelFrictionCurve fc = col.forwardFriction;

            fc.asymptoteValue = 5000.0f;
            fc.extremumSlip = 2.0f;
            fc.asymptoteSlip = 20.0f;
			fc.stiffness = bikeSetting.stiffness;
            col.forwardFriction = fc;
            fc = col.sidewaysFriction;
            fc.asymptoteValue = 7500.0f;
            fc.asymptoteSlip = 2.0f;
			fc.stiffness =bikeSetting.stiffness;
            col.sidewaysFriction = fc;

        }


        audioSource = (AudioSource)GetComponent(typeof(AudioSource));
        if (audioSource == null)
        {
            Debug.Log("No audio please add one");
        }

    }

    // handle shifting a gear up
    public void ShiftUp()
    {
        float now = Time.timeSinceLevelLoad;

        // check if we have waited long enough to shift
        if (now < shiftDelay) return;

        // check if we can shift up
        if (currentGear < bikeSetting.gears.Length - 1)
        {

			if (!bikeSounds.switchGear.isPlaying && !AppSoundManager.MuteSfx) {
                bikeSounds.switchGear.audio.Play();
			}

            currentGear++;

            // we delay the next shift with 1s. (sorry, hardcoded)
            shiftDelay = now + 1.0f;
        }
    }
	
    // handle shifting a gear down
    public void ShiftDown()
    {
        float now = Time.timeSinceLevelLoad;

        if (now < shiftDelay) return;
        // check if we can shift down (note gear 0 is reverse)
        if (currentGear > 0)
        {

			if (!bikeSounds.switchGear.isPlaying && !AppSoundManager.MuteSfx)
                bikeSounds.switchGear.audio.Play();


            currentGear--;

            // we delay the next shift with 1/10s. (sorry, hardcoded)
            shiftDelay = now + 0.1f;
        }
    }
	
    void Update()
    {
		if(moveUp)
		{
			if(moveUpValue + stepForValue <=1f)
				moveUpValue +=stepForValue;
			else
				moveUpValue = 1f;
		}
		if(moveDown)
		{
			if(moveDownValue - stepForValue >=-1f)
				moveDownValue -=stepForValue;
			else
				moveDownValue = -1f;
		}
		if (!useTilt) 
		{
			if (moveRight) 
			{
				if (moveRightValue + stepForValue <= 1f)
						moveRightValue += stepForValue;
				else
						moveRightValue = 1f;
			}
			if (moveLeft) 
			{
				if (moveLeftValue - stepForValue >= -1f)
						moveLeftValue -= stepForValue;
				else
						moveLeftValue = -1f;
			}
		}
		else
		{
			float valueLR = Input.acceleration.x;
			if(valueLR > 0f)
			{
				if(moveLeft)
				{
					moveLeft = false;
					moveLeftValue = 0f;
				}

				moveRight = true;
				moveRightValue = valueLR;
//				if (moveRightValue + valueLR <= 1f)
//					moveRightValue += valueLR;
//				else
//					moveRightValue = 1f;
			}
			else if (valueLR < 0f)
			{
				if(moveRight)
				{
					moveRight = false;
					moveRightValue = 0f;
				}

				moveLeft = true;
				moveLeftValue = valueLR;
//				if (moveLeftValue - valueLR >= -1f)
//					moveLeftValue -= valueLR;
//				else
//					moveLeftValue = -1f;
			}
			else
			{
				moveLeft = false;
				moveRight = false;
				moveLeftValue = 0f;
				moveRightValue = 0f;
			}
		}


        speed = rigidbody.velocity.magnitude * 3.6f;


        float delta = Time.fixedDeltaTime;
        rigidbody.centerOfMass = bikeSetting.shiftCentre;

        float accel = 0.0f; // accelerating -1.0 .. 1.0

		if (Application.platform == RuntimePlatform.Android)
		{
			if (moveUp && !moveDown)
				accel = moveUpValue;
			else if (moveDown && !moveUp)
				accel = moveDownValue;
			else
				accel = 0f;

			if(moveRight && !moveLeft)
				steer = Mathf.MoveTowards(steer,moveRightValue, Time.deltaTime * 2.5f);
			else if (moveLeft && !moveRight)
				steer = Mathf.MoveTowards(steer,moveLeftValue, Time.deltaTime * 2.5f);
			else
				steer = Mathf.MoveTowards(steer, 0f, Time.deltaTime * 2.5f);
			shift = nitro;
		}

		#if UNITY_EDITOR
		if (!crash)
		{
			
			steer = Mathf.MoveTowards(steer, Input.GetAxis("Horizontal"), Time.deltaTime * 2.5f);
			accel = Input.GetAxis("Vertical");
			shift = Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift);
		}
		else
		{
			steer = 0;
		}
		#endif
        foreach (Light brakeLight in bikeLights.brakeLights)
        {
			if(/*accel < 0 &&*/ speed > 11f && showBrakeParticles && wheels[1].collider.isGrounded)
			{
				wheelParticle.enableEmission = true;
			}
			else
				wheelParticle.enableEmission = false;


            if (accel < 0 || speed < 1.0f)
            {
                brakeLight.intensity = Mathf.Lerp(brakeLight.intensity, 8, 0.1f);
            }
            else
            {
                brakeLight.intensity = Mathf.Lerp(brakeLight.intensity, 0, 0.1f);
            }
        }




        steer2 = Mathf.LerpAngle(steer2, steer * -bikeSetting.maxSteerAngle, Time.deltaTime * 10.0f);

        MotorRotation = Mathf.LerpAngle(MotorRotation, steer2 * bikeSetting.maxTurn * (Mathf.Clamp(speed / 200, 0.0f, 1.0f)), Time.deltaTime * 5.0f);

		//TODO: pofiksit' eott kof
        if (bikeSetting.bikeSteer)
            bikeSetting.bikeSteer.localRotation = SteerRotation * Quaternion.Euler(0, (wheels[0].collider.steerAngle*1f), 0); // this is 90 degrees around y axis

        if (!crash)
        {

            Quaternion deltaRotation = Quaternion.Euler(0, 0, -transform.localEulerAngles.z + (MotorRotation));
            rigidbody.MoveRotation(rigidbody.rotation * deltaRotation);

        }
        // handle automatic shifting
        if (bikeSetting.automatic && (currentGear == 1) && (accel < 0.0f))
        {
            if (speed < 1.0f)
                ShiftDown(); // reverse


        }
        else if (bikeSetting.automatic && (currentGear == 0) && (accel > 0.0f))
        {
            if (speed < 5.0f)
                ShiftUp(); // go from reverse to first gear

        }
		else if (bikeSetting.automatic && (motorRPM > bikeSetting.shiftUpRPM) && (accel > 0.0f))
        {
            // if (speed > 20)
            ShiftUp(); // shift up

        }
        else if (bikeSetting.automatic && (motorRPM < bikeSetting.shiftDownRPM) && (currentGear > 1))
        {
            ShiftDown(); // shift down
        }


        if ((currentGear == 0))
        {
            bikeSetting.shiftCentre.z = -accel / 5.0f;
            if (speed < bikeSetting.gears[0] * -10)
                accel = -accel; // in automatic mode we need to hold arrow down for reverse
        }
        else
        {

            bikeSetting.shiftCentre.z = -(accel / currentGear) / 3.0f;
        }







        // the RPM we try to achieve.
        wantedRPM = (5500.0f * accel) * 0.1f + wantedRPM * 0.9f;

        float rpm = 0.0f;
        int motorizedWheels = 0;
        bool floorContact = false;
        int currentWheel = 0;
        // calc rpm from current wheel speed and do some updating





        foreach (WheelComponent w in wheels)
        {
            WheelHit hit;
            WheelCollider col = w.collider;



            // only calculate rpm on wheels that are connected to engine
            if (w.drive)
            {
                rpm += col.rpm;
                motorizedWheels++;
            }




            if (accel < 0.0f)
            {

                wantedRPM = 0.0f;
                col.brakeTorque = bikeSetting.brakePower;

            }
            else
            {

                col.brakeTorque = accel == 0 ? col.brakeTorque = 200 : col.brakeTorque = 1;
            }

			if (shift && speed > 5 && shifmotor && Mathf.Abs(steer) < 0.6f/*&& steer == 0*/)//TODO:do it for allowing nitro on tilt control
            {
                if (transform.localEulerAngles.x > 320)
                {
                    bikeSetting.shiftCentre.y = bikeSetting.shiftSpeed.y;
                    bikeSetting.shiftCentre.z = bikeSetting.shiftSpeed.z;
                }
                else
                {
                    bikeSetting.shiftCentre = bikeSetting.shiftCentre;
                }



                if (powerShift == 0) { shifmotor = false; }

                powerShift = Mathf.MoveTowards(powerShift, 0.0f, Time.deltaTime * 10.0f);

                bikeSounds.nitro.volume = Mathf.Lerp(bikeSounds.nitro.volume, 1.0f, Time.deltaTime * 10.0f);

                if (!bikeSounds.nitro.isPlaying && !AppSoundManager.MuteSfx)
                {
                    bikeSounds.nitro.audio.Play();

                }

                curTorque = powerShift > 0 ? bikeSetting.shiftPower : bikeSetting.bikePower;
                bikeParticles.shiftParticle1.emissionRate = Mathf.Lerp(bikeParticles.shiftParticle1.emissionRate, powerShift > 0 ? 50 : 0, Time.deltaTime * 10.0f);
                bikeParticles.shiftParticle2.emissionRate = Mathf.Lerp(bikeParticles.shiftParticle2.emissionRate, powerShift > 0 ? 50 : 0, Time.deltaTime * 10.0f);



            }
            else
            {


                bikeSetting.shiftCentre = bikeSetting.shiftCentre;


                if (powerShift > 20)
                {
                    shifmotor = true;
                }

                bikeSounds.nitro.volume = Mathf.MoveTowards(bikeSounds.nitro.volume, 0.0f, Time.deltaTime * 2.0f);

                if (bikeSounds.nitro.volume == 0)
                    bikeSounds.nitro.Stop();

                powerShift = Mathf.MoveTowards(powerShift, 100.0f, Time.deltaTime * 5.0f);
                curTorque = bikeSetting.bikePower;
                bikeParticles.shiftParticle1.emissionRate = Mathf.Lerp(bikeParticles.shiftParticle1.emissionRate, 0, Time.deltaTime * 10.0f);
                bikeParticles.shiftParticle2.emissionRate = Mathf.Lerp(bikeParticles.shiftParticle2.emissionRate, 0, Time.deltaTime * 10.0f);

            }

            w.rotation = Mathf.Repeat(w.rotation + delta * col.rpm * 360.0f / 60.0f, 360.0f);
            // w.transform.localRotation = Quaternion.Euler(w.rotation, col.steerAngle, 0.0f);
            w.wheel.localRotation = Quaternion.Euler(w.rotation / 2.0f, 0.0f, 0.0f);

            // let the wheels contact the ground, if no groundhit extend max suspension distance
            Vector3 lp = w.axle.localPosition;

            if (col.GetGroundHit(out hit))
            {

                lp.y -= Vector3.Dot(w.wheel.position - hit.point, Vector3.up / transform.lossyScale.x) - (col.radius);
                lp.y = Mathf.Clamp(lp.y, -10.0f, w.pos_y);
                floorContact = floorContact || (w.drive);

                if (!crash)
                {
					//TODO: ybrat' elsi bydyt gorbywki, prejnee zna4enie 10.0
                    rigidbody.angularDrag = 30.0f;
                }
                else
                {
                    rigidbody.angularDrag = 0.0f;


                }
                grounded = true;


            }
            else
            {
                grounded = false;

                rigidbody.angularDrag = 0.0f;

				//TODO: ymenwit' 4islo esli nado yveli4it zavisanie
                rigidbody.AddForce(0, -1000, 0);

                lp.y = w.startPos.y - bikeWheels.setting.Distance;
            }

            currentWheel++;
			//TODO:rewrite if need be add dumpers
           // w.axle.localPosition = lp;

        }





        // calculate the actual motor rpm from the wheels connected to the engine
        // note we haven't corrected for gear yet.
        if (motorizedWheels > 1)
        {
            rpm = rpm / motorizedWheels;
        }

        // we do some delay of the change (should take delta instead of just 95% of
        // previous rpm, and also adjust or gears.
        motorRPM = 0.95f * motorRPM + 0.05f * Mathf.Abs(rpm * bikeSetting.gears[currentGear]);
        if (motorRPM > 5500.0f) motorRPM = 5200.0f;

        // calculate the 'efficiency' (low or high rpm have lower efficiency then the
        // ideal efficiency, say 2000RPM, see table
        int index = (int)(motorRPM / efficiencyTableStep);
        if (index >= efficiencyTable.Length) index = efficiencyTable.Length - 1;
        if (index < 0) index = 0;

        // calculate torque using gears and efficiency table
        float newTorque = curTorque * bikeSetting.gears[currentGear] * efficiencyTable[index];

        // go set torque to the wheels
        foreach (WheelComponent w in wheels)
        {
            WheelCollider col = w.collider;

            // of course, only the wheels connected to the engine can get engine torque
            if (w.drive)
            {
                // only set torque if wheel goes slower than the expected speed
				//TODO: zdes' skorost' reversa
                if (Mathf.Abs(col.rpm) > Mathf.Abs(wantedRPM) || (currentGear == 0 && speed > 15))
                {
                    // wheel goes too fast, set torque to 0
                    col.motorTorque = 0;
                }
                else
                {
                    // 
                    float curTorqueCol = col.motorTorque;


                    col.motorTorque = curTorqueCol * 0.9f + newTorque * 0.1f;


                }
            }



            float SteerAngle = Mathf.Clamp((speed / transform.lossyScale.x) / bikeSetting.maxSteerAngle, 1.0f, bikeSetting.maxSteerAngle);
            col.steerAngle = steer * (w.maxSteer / SteerAngle);





        }



        // if we have an audiosource (motorsound) adjust pitch using rpm        
		if (audioSource != null && ! AppSoundManager.MuteSfx)
        {
            // calculate pitch (keep it within reasonable bounds)
            float pitch = Mathf.Clamp(1.0f + ((motorRPM - bikeSetting.idleRPM) / (bikeSetting.shiftUpRPM - bikeSetting.idleRPM)), 1.0f, 10.0f);
            audioSource.pitch = pitch;
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, 1.0f, 0.02f);
        }
		else
		{
			audioSource.pitch = 0f;
			audioSource.volume = 0f;
		}


        if (crash)
        {
            transform.rigidbody.centerOfMass = Vector3.zero;
        }


    }







    /////////////// Show Normal Gizmos ////////////////////////////


    void OnDrawGizmos()
    {

        if (!bikeSetting.showNormalGizmos || Application.isPlaying) return;

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

        Gizmos.matrix = rotationMatrix;
        Gizmos.color = new Color(1, 0, 0, 0.5f);

        Gizmos.DrawCube(Vector3.zero, new Vector3(0.5f, 1.0f, 2.5f));
        Gizmos.DrawSphere(bikeSetting.shiftCentre / transform.lossyScale.x, 0.2f);

    }



}