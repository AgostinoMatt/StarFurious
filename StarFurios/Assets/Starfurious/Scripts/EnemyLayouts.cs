using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Class that handles the logic of adding enemies to the Enemy Continer, row by row.
/// This component also sets the initial delay time of each enemy unit, based on its location
/// </summary>
public class EnemyLayouts : MonoBehaviour 
{

	delegate void MoveDelayFunc(GameObject GameObject);

	[SerializeField]	GameObject EnemyA = null;
	[SerializeField]	GameObject EnemyB = null;
	[SerializeField]	GameObject EnemyC = null;
	[SerializeField]	Transform EnemyContainer = null;


	// Use this for initialization
	void Start () 
	{

	}
	// Update is called once per frame
	void Update () 
	{

	}


	/// <summary>
	/// Layout a single row
	/// </summary>
	/// <param name="Enemy">Enemy Gameobject to instance</param>
	/// <param name="maxcnt">Max count of enemies in the given row</param>
	/// <param name="yVal">Y of row to place units</param>
	/// <param name="moveDelayFunc">Reference to the delay function</param>
	void LayoutRow(GameObject Enemy, int maxcnt, float yVal, MoveDelayFunc moveDelayFunc)
	{
		// Start of center screen
		float startX = 0;


		if( maxcnt % 2 == 0 )
		{
			startX = -(maxcnt / 2) + 0.5f; // Offset by 0.5f;
		}
		else
		{
			startX = -((maxcnt - 1) / 2);
		}


		for( int cnt = 0; cnt < maxcnt; cnt++ )
		{
			GameObject go = Instantiate( Enemy ) as GameObject;

			go.transform.position = new Vector3( startX, yVal, 0 );
			go.transform.SetParent( EnemyContainer, false );

			if( moveDelayFunc != null )		moveDelayFunc( go );

			startX += 1.0f;
		}
	}

	/// <summary>
	/// Layout ships for basic level
	/// </summary>
	public void Level1Layout()
	{

		LayoutRow( EnemyC, 8,  6, MoveDelayC );		// Top most row

		LayoutRow( EnemyB, 10, 5, MoveDelayB );		// middle rows
		LayoutRow( EnemyB, 10, 4, MoveDelayB );

		LayoutRow( EnemyA, 12, 3, MoveDelayA );		// bottom two rows, closest to player
		LayoutRow( EnemyA, 12, 2, MoveDelayA );

	}

	/// <summary>
	/// Moves delay function A.
	/// </summary>
	/// <param name="gobject">Enemy Game Object</param>
	void MoveDelayA(GameObject gobject)
	{
		EnemyLogicController enemyLogic = gobject.GetComponent<EnemyLogicController>();
		if( enemyLogic != null )
		{
			
			Vector3 pos = gobject.transform.position;
			float absx = Mathf.Abs( pos.x );

			if( pos.x > 0 ) enemyLogic.MirrorX = true;

			float waitStart = Mathf.Floor(7 - absx );

			waitStart *= 3;

			enemyLogic.MoveDelay = waitStart;
			
		}
	}

	/// <summary>
	/// Moves the delay b.
	/// </summary>
	/// <param name="gobject">Enemy Game Object</param>
	void MoveDelayB(GameObject gobject)
	{
		EnemyLogicController enemyLogic = gobject.GetComponent<EnemyLogicController>();
		if( enemyLogic != null )
		{
			Vector3 pos = gobject.transform.position;
			if( pos.x < 0 ) enemyLogic.MirrorX = true;
			float waitStart = Mathf.Floor( Mathf.Abs( pos.x ) );
			if( waitStart % 2 != 0 ) waitStart -= 1;

			enemyLogic.MoveDelay = (waitStart + ( Math.Abs(pos.y) )) ;

		}
	}

	/// <summary>
	/// Moves the delay c.
	/// </summary>
	/// <param name="gobject">Enemy Game Object</param>
	void MoveDelayC(GameObject gobject)
	{
		EnemyLogicController enemyLogic = gobject.GetComponent<EnemyLogicController>();
		if( enemyLogic != null )
		{
			Vector3 pos = gobject.transform.position;
			if( pos.x < 0 ) enemyLogic.MirrorX = true;
			float waitStart = Mathf.Floor( Mathf.Abs( pos.x ) );
			if( waitStart % 2 != 0 ) waitStart -= 1;

			enemyLogic.MoveDelay = (waitStart + ( Math.Abs(pos.y) * 3 ));

		}

	}


	
}
