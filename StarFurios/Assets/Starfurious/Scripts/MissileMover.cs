using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Missile mover  - basically moves a missle up or down the screen; if the missile collides
///  with an object who's tag matches the killtag value, then notify the game logic manager
/// </summary>
public class MissileMover : MonoBehaviour 
{


	[SerializeField]
	float ExplosionYOffset = 0.5f;	// Offset the explosion slightly;

	[SerializeField]
	float Speed = 5;		// Movement speed of the missile, negative moves DOWN SCREEN


	[SerializeField]
	string KillTag = "";			// Tag of object that this missile will damage


	Explodable explodable;

	// Use this for initialization
	void Start () 
	{
		explodable = GetComponent<Explodable>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		Vector3 pos = transform.position;
		pos.y += Speed * Time.deltaTime;
		transform.position = pos;
		if( pos.y < -10 || pos.y > 10 )
			Destroy( gameObject );

	}

	/// <summary>
	/// Kills the missile and spawn an explosion
	/// </summary>
	public void KillMissile()
	{
		
		Vector3 pos = transform.position;
		pos.y += ExplosionYOffset;

		// Spawn the explosion (also kills this game object)
		explodable.SpawnExplosion( pos );
	}

	/// <summary>
	/// Handles the collision with a game object.  If the kill tag matches the game object's tag, let
	/// the Game Play Logic manager know.
	/// </summary>
	/// <param name="collider">Collider.</param>
	public void OnTriggerEnter2D(Collider2D collider)
	{
		
		if( collider.gameObject.tag == KillTag )
		{
			GamePlayLogic.Instance.MissileHit( collider.gameObject );
			KillMissile();
		}
		
	}
}
