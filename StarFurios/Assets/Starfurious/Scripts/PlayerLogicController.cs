using UnityEngine;
using System;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
/// <summary>
/// Player logic controller  - manages the fire  input for the player object.
///  For "movement", see PlayerMover script
/// </summary>
public class PlayerLogicController : MonoBehaviour 
{

	public bool IsInvincible = false; 	// Invincible! Generally happens post-missile/enemy hit. 

	[SerializeField]
	GameObject PlayerMissile = null;			// Reference to player missile to instantiate upon firing

	[SerializeField]
	float FireDelay = 0.25f;			// Amount of time to wait before allowing another fire

	float currFireTime;					// Current fire wait time
	bool isDead = false;

	public bool Dead
	{
		get { return isDead; }
	}

	// Use this for initialization
	void Start () 
	{
		currFireTime = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{
		currFireTime -= Time.deltaTime;

		/*
		if( currFireTime < 0 && ! isDead)
		{
			currFireTime = -1;
			if( CrossPlatformInputManager.GetAxis( "Fire1" ) != 0 )
			{
				currFireTime = Time.time + FireDelay;
				FireMissile();
			}
		}
		*/

		if (Time.time > currFireTime && ! isDead)
        {
			currFireTime = Time.time + FireDelay;
			FireMissile();
        }
	}

	/// <summary>
	/// Fires the missile
	/// </summary>
	void FireMissile()
	{
		if( PlayerMissile != null && GamePlayLogic.Instance.IsWaveActive())
		{
			GameObject go = Instantiate( PlayerMissile ) as GameObject;
			Vector3 pos = transform.position;
			pos.y += 0.5f; // Offset the missile a bit
			go.transform.position = pos;

		}
	}

	/// <summary>
	/// Handle 2d collisions
	/// </summary>
	/// <param name="collider">Collider.</param>
	public void OnTriggerEnter2D(Collider2D collider)
	{
		// Collided with an ememy. DOH!
		if( collider.gameObject.tag == "Enemy" )
		{
			GamePlayLogic.Instance.PlayerHit( collider.gameObject );
		}
	}

	/// <summary>
	/// Player died. Called by the Game Play Logic manager
	/// Creates an explosion, and sets the player "invincible" for 3 seconds
	/// </summary>
	public void PlayerDied()
	{
		if( IsInvincible ) return;
		isDead = true;
		GetComponent<Explodable>().SpawnExplosion( transform.position);
		StartRespawn();
	}
	/// <summary>
	/// Respawn Player
	/// </summary>
	public void StartRespawn()
	{
		StartCoroutine( DoInvincible() );
	}

	IEnumerator DoInvincible()
	{
		IsInvincible = true;

		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		sr.enabled = false;
		yield return new WaitForSeconds( 1.0f ); // 1 second respawn time
		sr.enabled = true;
		isDead = false;

		float countdown = Time.time + 3; // 3 second invincibility


		while( Time.time < countdown )
		{
			// Blink!
			sr.enabled = !sr.enabled;
			yield return new WaitForSeconds( 0.1f );
		}

		sr.enabled = true;
		IsInvincible = false;
	}
}