using UnityEngine;
using System.Collections;

/// <summary>
/// Move the associated game object to a new location
/// The component starts "disabled". Use the StartMove method, which will auto-enable the component
/// Once the destination is reached, this component uses SendMessage (OnMoveToLocationComplete) to notify
/// the game object of completion, then this component auto-disables itself.
/// </summary>
public class MoveToLocation : MonoBehaviour 
{
	bool useLocalPosition = true;	// Set to TRUE to use the local position or false to use World Position

	Vector3 startLocation;
	Vector3 destination;	// Destination x,y,z
	float startTime = 0;	// time when starting move
	float moveSpeed = 1.0f;
	float totalDistance;
	Quaternion startRotation;
	Quaternion destinationRotation;

	void Awake()
	{
		enabled = false;
	}

	/// <summary>
	/// Starts the move. Auto-Enables this component so update is called.
	/// </summary>
	/// <param name="startLoc">Start location - location to move from</param>
	/// <param name="newDest">New destination - destination to move to</param>
	/// <param name="useLocal">If set to <c>true</c> update transform.localPostion, otherwise update transform.Position.</param>
	/// <param name="speed">Speed - speed to move toward the destination</param>
	public void StartMove(Vector3 startLoc, Vector3 newDest, bool useLocal=true, float speed = 1.0f)
	{
		useLocalPosition = useLocal;
		moveSpeed = speed;
		destination = newDest;
		startLocation = startLoc;
		startRotation = transform.localRotation;

		Vector3 lookatPos = destination - startLocation;
		float destZAngle = (Mathf.Atan2( lookatPos.y, lookatPos.x ) * Mathf.Rad2Deg) - 90;
		destinationRotation = Quaternion.AngleAxis( destZAngle, Vector3.forward );
		totalDistance = Vector3.Distance( startLoc, newDest );

		this.enabled = true;	
		startTime = Time.time;

	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		float distanceTraveled;
		float fracAmount;

		distanceTraveled = (Time.time - startTime) * moveSpeed;
		fracAmount = distanceTraveled / totalDistance;

		Vector3 newPos = Vector3.Lerp( startLocation, destination, fracAmount );
		transform.localRotation = Quaternion.Lerp( startRotation, destinationRotation, fracAmount );
		if( useLocalPosition )
			transform.localPosition = newPos;
		else
			transform.position = newPos;

		if( fracAmount >= 1 )
		{
			// Once the destination is reached, the notify the game object
			SendMessage( "OnMoveToLocationComplete", null, SendMessageOptions.RequireReceiver );
			enabled = false;
				
		}

	}
}
