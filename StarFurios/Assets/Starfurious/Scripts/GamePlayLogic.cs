using UnityEngine;
using System;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

using RFLib;

/// <summary>
/// Core game play logic manager.  Handles upadating the players score and managing the
/// overall general game state.  Also handles interactions between missiles, players, and enemies.
/// 
/// </summary>
public class GamePlayLogic : SingletonMonoBehaviour<GamePlayLogic>
{
	enum GAME_MODE
	{
		WAVE_START,
		WAVE_ACTIVE,
		GAME_OVER,
		COLLECING_SCORE,
		RESTARTABLE			// Restartable means the game has gotten into a point in time where it can be "restarted" (nav back to initial scene)

	}

	[SerializeField]
	bool GodMode = false;


	[SerializeField]
	Transform EnemyContainer = null;			// Hold the enemies; move the transform to put them into position at start of wave

	[SerializeField]
	EnemyContainerLogic EnemiesContainerLogic = null; // Additional behavior for moving the enemy container horizontally

	
	[SerializeField]
	RFLabelValueIndicator ScoreLabelValue = null;

	[SerializeField]
	RFLabelValueIndicator WaveLabelValue = null;

	[SerializeField]
	RFLabelValueIndicator HighScoreLabelValue = null;


	[SerializeField]
	Transform PlayerShipsLeftContainer = null;		// Reference to the UI container that has the player's ships remaining

	[SerializeField]
	RFHighScoreViewer highScoreViewer = null;

	[SerializeField]
	int	StartingWave = 1;

	[SerializeField]
	GameObject ContinueText = null;	

	[SerializeField]
	GameObject GameOverText = null;	



	int highestScore = 0;		
	int score 		= 0;		// The player's current score
	int wave  		= 1;		// The wave the player is on
	int ship_count 	= 3;		// Remaining Player Ship

	GameObject	player;			// Player Game Object
	PlayerLogicController playerLogic;	// Script for player logic
	float dropVel = 2.0f;

	EnemyLayouts enemyLayouts;
	float enemyDropTime = 0;

	/*
	float enemyContainerSpeed = 0.5f;
	float enemyContainerMinX = -2.0f;
	float enemyContainerMaxX = 2.0f;
	*/

	GAME_MODE gameMode;

	/// <summary>
	/// Getter for current wave
	/// </summary>
	/// <value>The wave.</value>
	public int Wave { get { return wave; } }


	// Use this for initialization
	void Start () 
	{
		// Force our target framerate to 60 (at best)
		Application.targetFrameRate = 60;


		player = GameObject.Find( "Player" );
		if( player == null )
		{
			Debug.Log( "Player object need to be in scene!" );
		}

		playerLogic = player.GetComponent<PlayerLogicController>();
		if( playerLogic == null )
		{
			Debug.Log( "Player Object has no Logic Controller!" );
		}

		enemyLayouts = GetComponent<EnemyLayouts>();
		if( StartingWave < 1 ) StartingWave = 1;

		highScoreViewer.LoadScores();
		highScoreViewer.gameObject.SetActive( false );

		player.SetActive( true );
		ship_count = 3;
		score = 0;
		wave = StartingWave;
		highestScore = highScoreViewer.HighScoresManager.GetHighestScore();
		HighScoreLabelValue.SetValue( highestScore, true );
		WaveLabelValue.SetValue( wave );
		ScoreLabelValue.SetValue( score );
		showCurrentShipsLeft();
		StartWave();

	}

	/// <summary>
	/// Starts the next wave
	/// Increment the wave counter by 1, start the wave
	/// </summary>
	void StartNextWave()
	{
		wave += 1;
		WaveLabelValue.SetValue( wave );
		StartWave();
	}

	/// <summary>
	/// Start the wave - sets up enemy configuration, calls co-routine to drop them into view
	/// </summary>
	void StartWave()
	{
		dropVel = 2.0f;
		enemyDropTime = 0;
		EnemiesContainerLogic.Reset();
		enemyLayouts.Level1Layout();
		gameMode = GAME_MODE.WAVE_START;
		EnemiesContainerLogic.InitContainerValues();
		StartCoroutine( DropEnemiesToView() );



	}

	/// <summary>
	/// Moves the "Enemy Container" into game view, then sets the Wave Active state
	/// </summary>
	IEnumerator DropEnemiesToView()
	{
		// While the game is in "startup mode", move the enemies into view from their
		// original position
		while( enemyDropTime <= 1.0 )
		{
			EnemyContainer.position = Vector3.Lerp( new Vector3( 0, 7, 0 ), Vector3.zero, enemyDropTime );
			enemyDropTime += Time.deltaTime * dropVel;
			if(dropVel > 0.95)
				dropVel *= 0.95f;

			yield return null;
		}

		// Activate all enemies!
		EnemiesContainerLogic.ActivateEnemies(wave);
		gameMode = GAME_MODE.WAVE_ACTIVE;


		// Test end game
//		score = 5000;
//		ship_count = 1;
//		PlayerHit();


	}


	// Update is called once per frame
	void Update () 
	{
		if( gameMode == GAME_MODE.RESTARTABLE )
		{
			if( CrossPlatformInputManager.GetAxis( "Fire1" ) != 0 )
				ReturnToStartScene();
		}
		// Move the Enemy Container back and forth
		else if( gameMode == GAME_MODE.WAVE_ACTIVE )
		{
			EnemiesContainerLogic.UpdateXPosition();
		}
	}

	/// <summary>
	/// Update the ship count display
	/// </summary>
	void showCurrentShipsLeft()
	{
		for( int cnt = 0; cnt < PlayerShipsLeftContainer.childCount; cnt++ )
		{
			PlayerShipsLeftContainer.GetChild( cnt ).gameObject.SetActive( cnt < (ship_count-1) );
		}

	}

	/// <summary>
	/// Missile Hit something - an enemy or a player
	/// If the the game over condition exists, just return
	/// </summary>
	public void MissileHit( GameObject hitObject)
	{
		if( IsGameOver() ) return;

		if( hitObject.tag == "Player" )
		{
			PlayerHit();
		}
		else if( hitObject.tag == "Enemy" || gameMode == GAME_MODE.WAVE_ACTIVE)
		{

			EnemyLogicController elc = hitObject.GetComponent<EnemyLogicController>();
			if( elc && elc.isActiveAndEnabled )
			{
				// Check to see if the enemy is dead
				if( elc.ReduceLife() )
				{
					score += elc.ScoreValue;
					if(score > highestScore)
						HighScoreLabelValue.SetValue( score );
					ScoreLabelValue.SetValue( score );
					EnemiesContainerLogic.UpdateContainerValues(true);

				}
			}

			if( EnemyContainer.childCount <= 0 )
			{
				StartNextWave();
			}

		}
	}


	/// <summary>
	/// Player was hit by a missile or an enemy
	/// </summary>
	public void PlayerHit(GameObject hitBy = null)
	{
		// Perform the same logic as if an enemy was hit by a missile
		// when player is hit by an enemy
		if(hitBy != null && hitBy.tag == "Enemy")
			MissileHit( hitBy );
		
		if( playerLogic.IsInvincible ) return;

		if(!GodMode)
			ship_count -= 1;


		playerLogic.PlayerDied();

		showCurrentShipsLeft();
		if( ship_count == 0 )
		{
			gameMode = GAME_MODE.GAME_OVER;
			StartCoroutine( DoGameOver() );
		}
	}

	/// <summary>
	/// Perform game over logic.
	///  - Clean up enemies on screen (send them offscreen to be destroyed)
	///  - Check to see if the player got a high score; if so, go into score collect
	///    mode
	///  - otherwise, wait a couple sends, then set state to provide "restart" of game
	/// </summary>

	IEnumerator DoGameOver()
	{
		player.SetActive( false );

		for( int cnt = 0; cnt < EnemyContainer.childCount; cnt++ )
		{
			EnemyLogicController elc = EnemyContainer.GetChild( cnt ).GetComponent<EnemyLogicController>();
			if( elc ) elc.FlitAway();
		}

		//Activate game over text; (will fade and disable itself)
		GameOverText.SetActive( true );

		while( EnemyContainer.childCount > 0 )
			yield return null;


		highScoreViewer.gameObject.SetActive( true );
		yield return new WaitForEndOfFrame();

		// If it is a high score..
		if( highScoreViewer.AddScore( score ) )
		{
			gameMode = GAME_MODE.COLLECING_SCORE;
		}
		else
		{
			// Force display of scores for a few seconds
			yield return new WaitForSeconds( 2.0f );
			StartCoroutine( AllowOrAutoRestart() );
		}
		yield return null;
	}
	/// <summary>
	/// Indicate whether or not the player has run out of lives!
	/// </summary>
	/// <returns><c>true</c> if game over; otherwise, <c>false</c>.</returns>
	public bool IsGameOver()
	{
		return ship_count <= 0;
	}

	/// <summary>
	/// Determine if the game is  in play mode
	/// </summary>
	public bool IsWaveActive()
	{
		return gameMode == GAME_MODE.WAVE_ACTIVE;
	}

	/// <summary>
	/// Handles RFHighScore events.
	/// </summary>
	/// <param name="highScoreEvent">High score event.</param>
	public void HandleRFHighScoreEvent(RFHighScoreEventData highScoreEvent)
	{
		switch( highScoreEvent.EventType )
		{
			case RFHighScoreEventType.RFHIGH_SCORE_EDIT_DONE:
			{
				StartCoroutine( AllowOrAutoRestart() );
				break;
			}
			case RFHighScoreEventType.RFHIGH_SCORE_NEW:
			{
				break;
			}
			case RFHighScoreEventType.RFHIGH_SCORE_EDIT_UPDATE:
			{
				break;
			}

		}
	}

	/// <summary>
	/// Allows the or auto restarts the game
	/// </summary>
	IEnumerator AllowOrAutoRestart()
	{
		ContinueText.SetActive( true );
		gameMode = GAME_MODE.RESTARTABLE;
		// Auto restart in 5 seconds
		yield return new WaitForSeconds( 5.0f );
		ReturnToStartScene();

	}

	/// <summary>
	/// Returns to start scene
	/// </summary>
	void ReturnToStartScene()
	{
		if(!RFEasySceneLoader.Instance.isLoading)	RFEasySceneLoader.Instance.LoadScene();
	}


}
