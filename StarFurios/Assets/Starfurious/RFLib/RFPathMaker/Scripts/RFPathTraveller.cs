using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RFLib
{
	// What to do when the endpoint is reached
	public enum RFPathTravellerEndpointBehaviors
	{
		STOP,				// Stop following the path
		REVERSE,			// Reverse course and travel back to the other endpoint (will bounce between endpoints until changed)
		RETURN_TO_OPPOSITE	// Return to the opposite endpoint; if moveing from start to end, will return to start; if moving
							//    from end to start (reversed) will move to end
	}

	public delegate void RFPathTravellerEndpointCallback(RFPathTraveller traveller);

	public class RFPathTraveller : MonoBehaviour 
	{
		[SerializeField]
		RFPathData StartPath = null;					// Paths to potentially travel


		public float MoveSpeed      = 1.0f;			// movement speed between steps
		public bool AutoStart		= false;			// Start moving on the first path;

		public RFPathTravellerEndpointBehaviors EndpointBehavior = RFPathTravellerEndpointBehaviors.RETURN_TO_OPPOSITE;
		public RFPathTravellerEndpointCallback endpointCallback;	// Function to call when this traveller reaches an endpoint


		int				currentPathIndex;		// Index reference to Paths
		int				currentPathIndexAdd = 1; // used to increase currentPathIndex

		List<Vector3>	currentPath;			// Current Path Points to follow
		int 			currentStepInPath;		// Path set; index into current path


		bool 	isMoving = false;				// Is this object currently following a path?

		float 	startTime		   = 0;			// time leaving the current step
		float 	distanceToNextStep = 0;			// Distance to the next step
		Vector3	destinationStep;
		Vector3 currentStep;

		Quaternion	startRotation;				// Rotation at start of travel (between steps)
		Quaternion	destRotation;				// Rotation at end of step travel
		float rotationFrac = 0;					// Rotation Frac is used to lerp rotation; makes rotation a little smoother



		bool mirrorX = false;
		bool mirrorY = false;


		Vector3	initialStartPosition;			// Position of object when START is run; used to "offset" a path to normalize to
												// transform's position


		public List<Vector3> CurrentPath	
		{		
			get { return currentPath; }		
			set { currentPath = value; }
		}

		public bool IsMoving
		{
			get{ return isMoving; }
		}

		public void CacheStartPosition()
		{
			initialStartPosition = transform.position;
		}


		// Use this for initialization
		void Start () 
		{		



			if( AutoStart && StartPath != null )
			{
				CacheStartPosition();
				Play( StartPath );
			}

		}
		
		// Update is called once per frame
		void Update () 
		{


			if( isMoving )
			{

				float distanceTraveled = (Time.time - startTime) * MoveSpeed;  // d = s*t
				float fracAmount = distanceTraveled / distanceToNextStep;

				rotationFrac += Time.deltaTime * 10;

				transform.position = Vector3.Lerp( currentStep, destinationStep, fracAmount );
				transform.rotation = Quaternion.Lerp( startRotation, destRotation,rotationFrac);
				if( fracAmount >= 1 )
				{
					this.moveToNextStep();
				}
			}
		
		}

		/// <summary>
		/// Mirror path axis
		/// </summary>
		/// <param name="x">If set to <c>true</c> x.</param>
		/// <param name="y">If set to <c>true</c> y.</param>
		public void SetPathMirror(bool x, bool y)
		{
			mirrorX = x;
			mirrorY = y;

		}


		/// <summary>
		/// Travel a path from the start
		/// </summary>
		public void Play(RFPathData newPath = null)
		{

			CacheStartPosition();

			// If a path was passed in, use that
			if( newPath != null )
				SetPath( newPath );

			// If no path is to play, but the Start Path has been set, use it
			else if( currentPath == null && StartPath != null )
				SetPath( StartPath );
			// If the current path is not null, Update the current path based on the cached start position
			else if( currentPath != null )
				updateCurrentPath();	
			else if( currentPath == null && StartPath == null )
			{
				Debug.LogError( "RFPathTraveller has no path to follow!" );
			}

			currentStepInPath 	= 0;
			moveToNextStep();
			isMoving = true;
		}
		/// <summary>
		/// Resume following a path from where it was last stopped
		/// </summary>
		public void Resume()
		{
			isMoving = true;
		}
		/// <summary>
		/// Stop following a path
		/// </summary>
		public void Stop()
		{
			isMoving = false;
		}
		public void ResetToStartPoint()
		{
			currentStepInPath = 0;
		}

		/// <summary>
		/// Sets the path to follow
		/// </summary>
		/// <param name="path">Path data</param>
		public void SetPath(RFPathData path)
		{
			currentPath 		= path.GetAllPathPoints();
			updateCurrentPath();
			currentStepInPath 	= 0;
		}

		/// <summary>
		/// Upates the current path, based on the initialstart position
		/// </summary>
		void updateCurrentPath()
		{
			Vector3 firstPoint = currentPath[ 0 ];
			if( mirrorX ) firstPoint.x = -firstPoint.x;
			if( mirrorY ) firstPoint.y = -firstPoint.y;


			// Normalize the path the the START position
			Vector3 step0 =  Vector3.zero-firstPoint;
			for( int cnt = 0; cnt < currentPath.Count; cnt++ )
			{
				Vector3 pathStep = currentPath[ cnt ];

				if( mirrorX ) pathStep.x = -pathStep.x;
				if( mirrorY ) pathStep.y = -pathStep.y;


				pathStep.x =  initialStartPosition.x  + pathStep.x + step0.x;
				pathStep.y =  step0.y + pathStep.y+initialStartPosition.y;


				currentPath[ cnt ] = pathStep;
			}
		}


		/// <summary>
		/// Move to the next step in the current path
		/// If an endpoint is reached, perform the endpoint behavior
		/// </summary>
		void moveToNextStep()
		{
			if( currentPath == null )
				return;
			
			int nextStepNdx = currentStepInPath + currentPathIndexAdd;

			startTime = Time.time;

			// We've Reached the end of the path, now what?
			if( nextStepNdx >= currentPath.Count || nextStepNdx < 0 )
			{
				if( EndpointBehavior == RFPathTravellerEndpointBehaviors.RETURN_TO_OPPOSITE )
				{
					if( currentPathIndexAdd > 0 )
						nextStepNdx = 0;
					else if( nextStepNdx < 0 )
						nextStepNdx = currentPath.Count - 1;
				
				}
				else if( EndpointBehavior == RFPathTravellerEndpointBehaviors.STOP )
				{
					Stop();
					if( endpointCallback != null )	endpointCallback( this );
					return;
				}
				else if( EndpointBehavior == RFPathTravellerEndpointBehaviors.REVERSE )
				{
					currentPathIndexAdd = -currentPathIndexAdd;
					nextStepNdx = currentStepInPath + currentPathIndexAdd;
				}

				if( endpointCallback != null )	endpointCallback( this );

			}

			destinationStep = currentPath[ nextStepNdx ];	
			currentStep = currentPath[ currentStepInPath ];



			//		if( mirrorX )
			//			currWP.x = -currWP.x;
			//			if(mirrorX)
			//				destinationWP.x = -destinationWP.x;


			// Update destination rotation data
			Vector3 lookatPos = destinationStep - currentStep;
			float destZAngle  = ( Mathf.Atan2(lookatPos.y, lookatPos.x)* Mathf.Rad2Deg ) - 90 ;
			destRotation = Quaternion.AngleAxis( destZAngle, Vector3.forward );
			startRotation = transform.localRotation;
			// If we are at the start, force a look toward the destination point
			if( currentStepInPath == 0 )
			{
				startRotation = destRotation;
				transform.localRotation = destRotation;
			}

			distanceToNextStep = Vector3.Distance(currentStep, destinationStep);
			rotationFrac = 0;



			currentStepInPath = nextStepNdx;
		}

	}
}