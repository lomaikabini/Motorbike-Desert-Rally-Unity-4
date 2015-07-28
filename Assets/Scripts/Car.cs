using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Car : MonoBehaviour {

	public int checkPathEveryNFrame = 10;
	public float pathPointTolerance = 0.5f;
	private NavMeshAgent navAgent;
	private List<Vector3> routePath;
	private int controlpointsCounter;
	private bool isRouteInitialized;
	public Vector3 currentDestination;
	public CurvySpline spline;
	private int count = 0;
	private Quaternion _lookRotation;
	private Vector3 _direction;
	public List<Vector3> RoutePath
	{
		set
		{
			routePath = value;
		}
	}
	
	void Awake()
	{
		routePath = new List<Vector3> ();
		navAgent = GetComponent(typeof(NavMeshAgent)) as NavMeshAgent;

	}
	void Start()
	{
//		Debug.Log ("segments = " + spline.Segments.Count);
//		foreach(CurvySplineSegment cr in spline.Segments)
//		{
//			routePath.Add(cr.transform.position);
//		}
//		Debug.Log (routePath.Count);
	}

	void Update () 
	{
		if (routePath.Count ==0)
		{

			foreach(CurvySplineSegment cr in spline.Segments)
			{
				//routePath.Add(cr.transform.position);
				Vector3[] apr = cr.Approximation;
				for(int i =0;i<apr.Length;i++)
				{

					routePath.Add(apr[i]);
				}
			}
			isRouteInitialized = true;
		}
		//updateWalkToTarget ();
	}
	void FixedUpdate()
	{
		if (count + 1 < routePath.Count) 
		{
			gameObject.transform.position = routePath [count];
			gameObject.transform.LookAt(routePath[count+1]);
			count++;
		} 
		else
			count = 0;
	}
	
	void updateWalkToTarget()
	{
		if (isRouteInitialized && (Time.frameCount % checkPathEveryNFrame) == 0)
		{
			moveThroughCheckPoints();
		}
	}
	private void routePathInitialization()
	{
		controlpointsCounter = 0;
		if (routePath != null && routePath.Count > controlpointsCounter)
		{
			currentDestination = routePath[controlpointsCounter];
		}
		
		isRouteInitialized = true;
		
		navAgent.SetDestination(currentDestination);
		
		//playmakerFsm.SendEvent("set_target");
	}
	
	/// <summary>
	/// moves bot thru series of check-points that was previously set in routePath varaible
	/// </summary>
	private void moveThroughCheckPoints()
	{
		if (routePath != null && navAgent != null && navAgent.enabled)
		{
			var dist = navAgent.remainingDistance;
//			if (navAgent.pathStatus == NavMeshPathStatus.PathComplete && dist <= pathPointTolerance && controlpointsCounter >= routePath.Count)
//			{
//				// done
//				// time to move next state
//				//setState(State.ROTATE_TOWARD_TARGET);
//				return;
//			}
			if (navAgent.pathStatus == NavMeshPathStatus.PathComplete && dist <= pathPointTolerance && (controlpointsCounter) < routePath.Count)
			{
				currentDestination = routePath[controlpointsCounter];
				navAgent.SetDestination(currentDestination);
				//Debug.Log(controlpointsCounter);
				if(controlpointsCounter+1 == routePath.Count)
				{
					controlpointsCounter = 0;
				}
				else
				controlpointsCounter++;
			}
		}
	}
}
