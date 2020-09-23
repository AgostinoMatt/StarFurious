using UnityEngine;
using System;
using System.Collections;

using RFLib;

public class EnemyLogicController : MonoBehaviour 
{

	public static float GLOBAL_NEXT_FIRE = 0;


	[SerializeField]
	GameObject EnemyMissile = null;			// Reference to teh enemy missile prefab 

	[SerializeField]
	float FireDelay = 7.0f;				// Minimum time to wait before firing again

	[SerializeField]
	int LifePoints	= 1;				// The number of times the enemy can get hit

	public int ScoreValue = 100;		// How much is this enemy woth when killed by a player?

	public float MoveDelay;				// Amount of time to "wait" before beginning the path travel
	public bool MirrorX = false;		// MirrorX - Mirror the travel path x cordinates

	RFPathTraveller PathTraveller;		// Path Traveller component reference
	Explodable explodable;			// Reference to Explodable Component

	Vector3 startLocalPosition;			
	Vector3 startPosition;	
	Quaternion startRotation;

	float fireTime = 2.0f;

	float wiggleMagnitude = 0;			// WiggleVel - more or less the "magnitude" of the wiggle
	float wiggletime = 5;			// Random time multiplier for frameTime;
	float frameTime = 0; 			// Used handle the wiggle sin/cos values

	float escape_speed = 1;			// Escape speed -- ultimately set to the PathTraveller speed; escape speed refers to how quickly
									// the ship leaves the screen when a player is all out of lives

	float wave = 1;					// Current Wave ; updated in BeginLogic

	bool isGameOver = false;		// Game Over state variable
	bool isDead = false;
	bool moveToLocal = false;		// Set to true when the end of path is reached; this flag is used to indicate that the update function should 
									// move the enemy BACK to origin..

	/// <summary>
	/// Gets the start position 
	/// </summary>
	/// <value>The start position.</value>
	public Vector3 StartPosition
	{
		get { return startPosition; }
	}

	void Awake()
	{
		PathTraveller = GetComponent<RFPathTraveller>();
		PathTraveller.endpointCallback = ReachedEndpoint;		

	}

	// Use this for initialization
	void Start () 
	{
		// Ensure some minumums for score and lifepoints
		if( ScoreValue < 1 ) ScoreValue = 1;
		if( LifePoints < 1 ) LifePoints = 1;

		explodable = GetComponent<Explodable>();

		// Set up some "wiggle" values; when not following a path, just wiggle a bit for
		//  a more organic look
		wiggleMagnitude = UnityEngine.Random.Range( 0.0005f, 0.001f );
		wiggletime =UnityEngine.Random.Range( 2, 5 );
		if( UnityEngine.Random.Range( 0, 100 ) < 50 )
			wiggleMagnitude = -wiggleMagnitude;

		startRotation = transform.localRotation;
		startPosition = transform.position;
		startLocalPosition = transform.localPosition;

		PathTraveller.SetPathMirror( MirrorX, false );

	}

	/// <summary>
	/// Start the enemy's basic movement logic
	/// inwave determines the movement speed and initial delay of this enemy;
	/// Short delays and higher movement speed = high difficulty
	/// </summary>
	/// <param name="inwave">Inwave.</param>
	public void BeginLogic(int inwave)
	{
		if( PathTraveller == null )
		{
			PathTraveller = GetComponent<RFPathTraveller>();
			PathTraveller.endpointCallback = ReachedEndpoint;		
		}


		wave = inwave;
		float travelSpeedMult = 1.0f;

		for(int cnt = 1; cnt < wave; cnt ++)
		{
	
			MoveDelay *= 0.75f;			// Decrease move delay 25% per wave
			FireDelay *= 0.9f;   		// Decrease fire delay 10% per wave
			travelSpeedMult *= 1.1f; 	// Increase move speed 10% per wave
		}
				
		isGameOver = false;
		PathTraveller.CacheStartPosition();
		PathTraveller.MoveSpeed *= travelSpeedMult;

		escape_speed = PathTraveller.MoveSpeed;

		StartCoroutine(StartMovement());

	}

	/// <summary>
	/// Begin following the denoted travel path
	/// </summary>
	IEnumerator StartMovement()
	{
		yield return new WaitForSeconds( MoveDelay );
		if( !isGameOver )
			PathTraveller.Play();
	}

	/// <summary>
	/// Reached the endpoint of the travel path
	/// </summary>
	/// <param name="traveller">Script that called this method</param>
	public void ReachedEndpoint(RFPathTraveller traveller)
	{

		float speed = 7;

		moveToLocal = true;
		MoveToLocation mv = GetComponent<MoveToLocation>();
		mv.StartMove( transform.localPosition, startLocalPosition, true, speed );

	}

	void OnMoveToLocationComplete()
	{
		moveToLocal = false;
		transform.localPosition = startLocalPosition;
		transform.localRotation = startRotation;
		StartCoroutine(StartMovement());
	}

	/// <summary>
	/// Resets the next fire time.  Fire Time is random,
	/// </summary>
	void resetNextFireTime()
	{
		fireTime = UnityEngine.Random.Range( FireDelay / 2, FireDelay );	
	}

	// Update is called once per frame
	void Update () 
	{
		if( isDead ) return;
		if( moveToLocal ) return;

		Vector3 pos = transform.position;


		// Not game over, so randomly perform a firing action and the "wiggle"
		if( !isGameOver )
		{

			// Only fire when definitely visible
			if( pos.x > -7.5f && pos.x < 7.5f )
			{
				fireTime -= Time.deltaTime;
				if( fireTime < 0 && Time.time > GLOBAL_NEXT_FIRE )
				{
					float fireChance = 1;
					if( PathTraveller.IsMoving )
						fireChance = 7;

					if( UnityEngine.Random.Range( 0, 100 ) < fireChance )
					{
						GameObject go = Instantiate( EnemyMissile ) as GameObject;
						go.transform.position = pos;
						resetNextFireTime();

						// Global timer - so there are not a ton of enemies firing at once
						GLOBAL_NEXT_FIRE = Time.time + UnityEngine.Random.Range( 0.1f, 0.5f );
					}
				}
			}

			// If not travelling, then perform a little wiggle
			if( !PathTraveller.IsMoving )
			{
                if (Time.timeScale > 0)
                {
                    frameTime += Time.deltaTime;
                    //	Debug.Log( Mathf.Sin( Time.time ) );
                    pos.y += (Mathf.Sin(frameTime * wiggletime) * wiggleMagnitude);
                    pos.x += (Mathf.Cos(frameTime * wiggletime) * wiggleMagnitude);
                    transform.position = pos;
                }
			}
		}

		// Game Over movement
		else
		{
			if( !PathTraveller.IsMoving )
			{
				transform.Translate( Vector3.up * escape_speed * Time.deltaTime );
				// We want to increase the speed a small percent each frame, making it appear the enemy flies out with increasing speed
				escape_speed *= 1.01f;
			}

			// If this enemy has reached the screen borders, just kill it.
			if( pos.x < -10 || pos.x > 10 || pos.y < -10 || pos.y > 10 )
				Destroy( gameObject );
		}
	}

	/// <summary>
	/// Flit away. Get out of the play screen.  Called by the game logic manager when its GAME OVER!
	/// </summary>
	public void FlitAway()
	{
		isGameOver = true;
		if( isDead ) return;

		if( PathTraveller.IsMoving )
		{
			PathTraveller.Stop();
		}
		else
		{
			PathTraveller.Play();
			StartCoroutine( FlitStart() );
		}

	}
	/// <summary>
	/// In order to "flit away", we want the enemies to travel their path for a random amount of time
	/// then go back to stopping and exitng the play screen
	/// </summary>
	IEnumerator FlitStart()
	{
		yield return new WaitForSeconds( UnityEngine.Random.Range( 0.5f, 2.5f ) );
		FlitAway();
	}


	// Perform death cleanup
	void DoDie()
	{
		if( isDead ) return;

		isDead = true;
		transform.SetParent( null );
		explodable.SpawnExplosion( transform.position, 0.075f );


	}

	/// <summary>
	/// Reduces the life of an enemy
	/// </summary>
	/// <returns><c>true</c>, if dead, <c>false</c> otherwise.</returns>
	public bool ReduceLife()
	{
		this.LifePoints -= 1;
		if( LifePoints <= 0 )
		{
			DoDie();
		}

		return LifePoints <= 0;
	}

}
