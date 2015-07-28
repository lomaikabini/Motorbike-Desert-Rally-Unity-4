/* This script demonstrates how to create an endless/lane runner type controller.
 * 
 */
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class EndlessRunnerPlayer : MonoBehaviour {
    public float Speed=8f; // Forward speed
    public float SwitchSpeed = 2f; // Lane switching speed
    public float Gravity = 6f; // Falling speed
    public float JumpTime = 1; // Duration the jumpcurve will be applied
    public AnimationCurve JumpCurve = AnimationCurve.EaseInOut(0, 1,1,0.3f); // Jump Curve
    public CurvySplineBase Spline; // Starting Spline or SplineGroup
    public float TF; // Starting TF

    Transform mTransform;
    bool mInAir;
    float mJumpDelta;
    
    bool Jumping { get { return mJumpDelta > 0; } }
    
    
    float mStartTF;

    Vector3 mLastPosOnSpline;

    
    const int LAYER_LANE = 21;

    void Awake()
    {
        mTransform = transform;
        mStartTF = TF;
    }

	// Use this for initialization
	IEnumerator Start () {
        if (Spline) {
            while (!Spline.IsInitialized)
                yield return 0;
            StartOver();
        }
	}

	void Update () 
    {
        if (!Spline || !Spline.IsInitialized)
            return;
        
        // *** Place Player in editor ***
        if (!Application.isPlaying) {
            if (Spline.Interpolate(TF)!=mTransform.position)
                mTransform.position = Spline.Interpolate(TF);
            return;
        }

        int dir=1;
        
        // advance on lane. We use PingPong clamping to detect when we reach the end of a spline (in that case dir changes!)
        Vector3 newPosOnSpline=Spline.MoveBy(ref TF, ref dir, Speed*Time.deltaTime, CurvyClamping.PingPong);
        Vector3 newTangent = Spline.GetTangent(TF);

        // Advance by spline curvation delta. We don't use newPosOnSpline directly because we don't want a snap-effect when in air or switching lanes
        mTransform.position += newPosOnSpline - mLastPosOnSpline;

        // *** Switch lanes? ***
        if (Input.GetButtonDown("Horizontal")) {
            if (TrySwitchLane(Input.GetAxis("Horizontal") < 0 ? Vector3.left : Vector3.right)) {
                newPosOnSpline = Spline.Interpolate(TF);
            }
        }        
        // *** Jump? ***
        if (!mInAir && !Jumping && Input.GetKey(KeyCode.Space)) {
            StartCoroutine(Jump());
        }
       
        // Set orientation
        mTransform.forward = newTangent;

        // Oops! We've reached end of spline. Check if we can fall down onto another spline...
        if (dir != 1) {
            CurvySplineBase newSpline;
            float newTF;
            if (RaycastForFollowUpSpline(Vector3.down, out newSpline, out newTF)) {
                // we found a new spline underneath us. Let's use it from now on
                Spline = newSpline;
                TF = newTF;
                newPosOnSpline = Spline.Interpolate(TF);
                mInAir = (newPosOnSpline - mTransform.position).sqrMagnitude > 0.001f;
            }
            else {
                StartCoroutine(Die()); // No Spline found. Time to die...
            }
        }


        // When in air or switching lanes, our position isn't where it should be. So we drag the player toward the lane position
        if (!Jumping) {
            Vector3 offset = mTransform.position - newPosOnSpline;
            if (offset.sqrMagnitude > 0.001f) {
                if (mInAir)
                    mTransform.position -= offset.normalized * Gravity * Time.deltaTime;
                else
                    mTransform.position -= offset.normalized * SwitchSpeed * Time.deltaTime;
            }
            else
                mInAir = false;
        }
        else { // Perform a jump
            mTransform.Translate(0, mJumpDelta * Time.deltaTime,0, Space.Self);
        }
        
        

        mLastPosOnSpline = newPosOnSpline;
	}

    // Die when we collide
    void OnTriggerEnter(Collider other)
    {
        StartCoroutine(Die());
    }

    // Reset to start conditions
    void StartOver()
    {
        TF = mStartTF;
        mTransform.position = Spline.Interpolate(TF);
        mLastPosOnSpline = mTransform.position;
    }

    // Die: Wait a second, then start over
    IEnumerator Die()
    {
        float tmp = Speed;
        Speed = 0;
        yield return new WaitForSeconds(1.0f);
        Speed = tmp;
        StartOver();
    }

    // Initiate jumping
    IEnumerator Jump()
    {
        mInAir = true;
        float tend = Time.time + JumpTime;
        while (Time.time <= tend) {
            mJumpDelta = JumpCurve.Evaluate(Time.time/tend);
            yield return new WaitForEndOfFrame();
        }
        mJumpDelta=0;
    }

    // Look for another lane to the left or right
    bool TrySwitchLane(Vector3 dir)
    {
        CurvySplineBase followUpSpline;
        float newTF;
        if (RaycastForFollowUpSpline(dir, out followUpSpline, out newTF)) {
            Spline=followUpSpline;
            TF = newTF;
            return true;
        }
        return false;
    }

    // Raycast a direction for another spline and get the nearest position on it
    bool RaycastForFollowUpSpline(Vector3 dir, out CurvySplineBase newSpline, out float newTF)
    {
        newSpline = null;
        newTF = 0;
        Ray R=new Ray(mTransform.position,dir);
        RaycastHit hitInfo;
        if (Physics.Raycast(R, out hitInfo,LAYER_LANE)) {
            newSpline = hitInfo.collider.transform.parent.GetComponent<CurvySplineBase>();
            if (newSpline) {
                newTF = newSpline.GetNearestPointTF(hitInfo.point);
                return true;
            }
        }
        return false;
    }

    
}
