using UnityEngine;
using System.Collections;

/// <summary>
/// Enemy container logic - Handle the basic logic / behavior for the enemy container
///  The GamePlayLogic controller interfaces with this component in order for it to
///  perform certain behaviors based on in-game events
/// </summary>
public class EnemyContainerLogic : MonoBehaviour 
{

	public float horizontalSpeed = 0.5f;			// Speed to "move" the container horizontally
	public float horizontalSpeedMult = 1.0f;
	public float horizontalMinX = -2.0f;			// Min horizontal location before invtering speed
	public float horizontalMaxX = 2.0f;			// Max horizontal location before invtering speed
	int startChildCount = 0;				// Number of enemies contained at initialization call


	/// <summary>
	/// Reset the component values to "game startup" values
	/// </summary>
	public void Reset()
	{
		transform.position = new Vector3( 0, 7, 0 );
	}

	public void UpdateXPosition()
	{
		Vector3 p = transform.position;
		p.x += horizontalSpeed * Time.deltaTime * horizontalSpeedMult;
		p.x = Mathf.Clamp( p.x, horizontalMinX, horizontalMaxX );
		transform.position = p;
		// If constraints are hit, update constraints and direction
		if( p.x >= horizontalMaxX || p.x <= horizontalMinX )
		{
			UpdateContainerValues(); // Tick Tock! Update speed and other min/max constraints
			horizontalSpeedMult = -horizontalSpeedMult;
		}
	}

	/// <summary>
	/// Activates the enemies.
	/// * Called by GamePlayLogic
	/// </summary>
	/// <param name="wave">Current Wave</param>
	public void ActivateEnemies(int wave)
	{
		// Activate all enemies!
		for( int cnt = 0; cnt < transform.childCount; cnt++ )
		{
			Transform t = transform.GetChild( cnt );
			EnemyLogicController elc = t.GetComponent<EnemyLogicController>();
			if( elc )
			{
				elc.BeginLogic(wave);
				elc.enabled = true;
			}
		}
	}


	/// <summary>
	/// When an enemy dies, update the min/max extents and speed in which the 
	/// container moves left / right
	/// </summary>
	/// <param name="speedOnly">If set to <c>true</c> speed only.</param>
	public void UpdateContainerValues(bool speedOnly=false)
	{

		if( !speedOnly )
		{
			float minX = 0;
			float maxX = 0;

			// Update MinX / MaxX to then figure out horizontal changes
			for( int cnt = 0; cnt < transform.childCount; cnt++ )
			{
				Transform t = transform.GetChild( cnt );
				EnemyLogicController elc = t.GetComponent<EnemyLogicController>();
				if( elc )
				{
					Vector3 pos = elc.StartPosition;
					if( pos.x < minX ) minX = pos.x;
					if( pos.x > maxX ) maxX = pos.x;

				}
			}
			horizontalMinX = -7.5f - minX;
			horizontalMaxX = 7.5f - maxX;
		}

		float childCountDeltaPct = 0.95f - ((float)transform.childCount) / ((float)startChildCount) ;
		horizontalSpeed = 0.5f + (childCountDeltaPct * 0.5f);


//		Debug.Log( "Min/Max X: " + minX.ToString() + " " + maxX.ToString() );

	}

	/// <summary>
	/// Initializes the container values based on number of enemies and their positions in relation to the screen
	/// bounds
	/// </summary>
	public void InitContainerValues()
	{
		// Always work from the base values
		horizontalSpeed = 0.5f;
		horizontalMinX = -2.0f;
		horizontalMaxX = 2.0f;

		startChildCount = transform.childCount;

		float minX = 0;
		float maxX = 0;


		// Update MinX / MaxX to then figure out horizontal changes
		for( int cnt = 0; cnt < transform.childCount; cnt++ )
		{
			Transform t = transform.GetChild( cnt );
			if( t.position.x < minX ) minX = t.position.x;
			if( t.position.x > maxX ) maxX = t.position.x;
		}

		horizontalMinX = -7.5f - minX;
		horizontalMaxX =  7.5f - maxX;

	}


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
