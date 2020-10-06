using UnityEngine;
using System;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;

/// <summary>
///  Reacts to input, and moves the player! 
/// See also PalyerLogicController for additional player related logic
/// </summary>
public class PlayerMover : MonoBehaviour 
{

	[SerializeField]
	float MaxX = 6.9f;		// Max X Move Constraint

	[SerializeField]
	float MinX = -6.9f;    // Min X Move Constraint

	[SerializeField]
	float MaxY = 5.0f;		// Max Y Move Constraint

	[SerializeField]
	float MinY = -6.6f;		// Min Y Move Constraint

	[SerializeField]
	float VelYMult = 5.0f;	// Y Move speed multiplier

	[SerializeField]
	float VelXMult = 5.0f;	// X Move speed multiplier

	Vector3 translateVector = Vector3.zero;		
	PlayerLogicController player;

    public Slider slider;
    private float sliderValue;

    // Use this for initialization
    void Start () 
	{
		player = GetComponent<PlayerLogicController>();
		transform.position = new Vector3( 0, MinY, 0 );
	}
	
	// Update is called once per frame
	void Update () 
	{
		if( player.Dead ) return; // No movement while dead.

		float xvel = 0;
		float yvel = 0;

		xvel = CrossPlatformInputManager.GetAxis( InputHelper.HORIZONTAL );  //Input.GetAxis( InputHelper.HORIZONTAL );
		yvel = CrossPlatformInputManager.GetAxis( InputHelper.VERTICAL ); //Input.GetAxis( InputHelper.VERTICAL );

		translateVector.x = xvel * Time.smoothDeltaTime * VelXMult;
		translateVector.y = yvel * Time.smoothDeltaTime * VelYMult;


		//Vector3 pos = transform.position;

		//pos.x += translateVector.x;
		//pos.y += translateVector.y;

        sliderValue = slider.value;
        Vector3 temp = transform.position;
        temp.x = sliderValue;
        transform.localPosition = temp;

        //if ( pos.x > MaxX ) pos.x = MaxX;
		//else if( pos.x < MinX ) pos.x = MinX;
		//if( pos.y < MinY  ) pos.y = MinY  ;
		//else if( pos.y > MaxY  ) pos.y = MaxY ;

		//transform.position = pos;

	}
}
