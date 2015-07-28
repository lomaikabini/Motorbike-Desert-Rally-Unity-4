/* This script moves a character along several splines using a Character Controller
 * 
 * Note: This is an example how to use Curvy for a Railrunner game. The controller is by no means perfect (the collision handling is crappy!), but you'll
 *       get the idea how to use Curvy for such a game.
 *       This example uses the Curvy API to get the ground information (height, orientation) to move the character around and limit
 *       it's y-position to stay on top of the spline. The benefit of this method is it's speed.
 *       Other possible methods:
 *       - write a script that adds a series of colliders following the spline and let the physics engine drive the
 *         character over the ground.
 *       - Use CurvySpline.GetNearestPointTF() and drag the controller towards this point. While you will get a lot of
 *         freedom using this method, GetNearestPointTF isn't the fastest method of the world.
 */
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public CurvySplineBase Spline;
    
    public float Speed = 20;
    public float JumpSpeed = 20;
    public float JumpDuration = 0.5f;
    public float Gravity = 15;
    public float TF;

    CharacterController mController;
    Transform mTransform;

    float mLastCurveY; // stores the y of the last curve position
    float mJumpDurationLeft; // seconds left to apply jump
    bool mStopMoving; // stop moving if we collide from the side
	
	IEnumerator Start () {
        mController = GetComponent<CharacterController>();
        mController.stepOffset = 0;
        
        mTransform = transform;
        // setup physics to ignore CharacterController vs. Head collision
        var headCollider=GetComponentInChildren<CapsuleCollider>();
        Physics.IgnoreCollision(headCollider, mController.collider);

        if (Spline){
            while (!Spline.IsInitialized)
                yield return new WaitForEndOfFrame();

            Init();
        }
	}

    void Update()
    {
        Vector3 moveDelta = Vector3.zero;
        Vector3 oldPos = mTransform.position; // store old position
        float oldTF = TF; // store old tf
        float minY = mLastCurveY; // store old minimum height

        float moveaxis = Input.GetAxis("Horizontal");
        bool jump = Input.GetButton("Jump");
        
        // Handle Left/Right movement
        if (moveaxis != 0) {
            // Calculate new spline position, setting movement to x/z and storing minimum height
            int dir = (moveaxis > 0) ? -1 : 1;
            int newdir = dir;

            Vector3 newPos = Spline.MoveBy(ref TF, ref newdir, Mathf.Abs(moveaxis) * Speed * Time.smoothDeltaTime, CurvyClamping.Loop);
            
            // y-position needs extra handling, so just store x/z in moveDelta
            moveDelta.x = newPos.x - oldPos.x;
            moveDelta.z = newPos.z - oldPos.z;
            minY = newPos.y;
            
        }
        // Jumping (Y++)
        if (jump && mJumpDurationLeft>0) {
                moveDelta += new Vector3(0, JumpSpeed * Time.smoothDeltaTime, 0);
                mJumpDurationLeft -= Time.deltaTime;
        }
        else  // Gravity (Y--)
            moveDelta += new Vector3(0, -Gravity * Time.smoothDeltaTime, 0);
        
        // If we would move below the spline, restrict movement to stay above it
        if (oldPos.y + moveDelta.y < minY) {
            moveDelta.y = minY - oldPos.y;
            mJumpDurationLeft = JumpDuration;
        }
        
        // The actual moving
        if (moveDelta != Vector3.zero) {
            // Move and handle collision
            if (mController.Move(moveDelta) != CollisionFlags.None) {
                // If we're not on top of a collider => Halt and reset to last "valid" position
                if (mStopMoving) {
                    mTransform.position = oldPos;
                    TF = oldTF;
                    minY = mLastCurveY;
                }
            } else
                mStopMoving = false;
            // Align rotation to spline
            mTransform.rotation = Spline.GetOrientationFast(TF);
        }
        mLastCurveY = minY;

    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // if we run straight into an obstacle, stop!
        mStopMoving = (hit.moveDirection.x != 0);
    }


    void Init()
    {
        if (!Spline) return;
        mTransform.position = Spline.Interpolate(TF);
        mTransform.rotation = Spline.GetOrientationFast(TF);
        mLastCurveY = mTransform.position.y;
    }
}
