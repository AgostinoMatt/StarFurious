using UnityEngine;
using System;
using System.Collections;

public class Explodable : MonoBehaviour 
{
	[SerializeField]
	GameObject Explosion = null;		// Game Object to spawn, representing the explosion.  Typically a particle system

	[SerializeField]
	bool AutoDestroy = false;	// Automatically destroy the associated game object 



	Vector3 spawnPosition;
	float spawnDelay = 0;

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	/// <summary>
	/// Spawns the explosion.
	/// </summary>
	/// <param name="position">Position.</param>
	/// <param name="delay">Delay to spawn, if 0 spawns immediately.</param>
	public void SpawnExplosion(Vector3 position, float delay=0)
	{
		spawnPosition = position;
		if( delay == 0 )
			createExplosion();
		else
		{
			spawnDelay = delay;
			StartCoroutine( delaySpawn() );
		}
	}

	/// <summary>
	/// Spawn on a delay
	/// </summary>
	IEnumerator delaySpawn()
	{
		yield return new WaitForSeconds( spawnDelay );
		createExplosion();
	}

	/// <summary>
	/// Creates the explosion.
	/// </summary>
	void createExplosion()
	{
		Instantiate( Explosion, spawnPosition, Quaternion.identity );

		if( AutoDestroy )
			Destroy( gameObject );
	}
}
