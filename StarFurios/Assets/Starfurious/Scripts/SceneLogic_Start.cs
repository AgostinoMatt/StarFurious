using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using RFLib;

/// <summary>
/// Simple logic to control the switch moving from the "start" scene to the "main" play scene
/// Reacts to the press of the firebutton.
/// Crossfades between the highscores panel and the enemy target information
/// </summary>
public class SceneLogic_Start : MonoBehaviour 
{
	[SerializeField]
	CanvasGroup HighScores = null;

	[SerializeField]
	CanvasGroup EnemyInfo = null;

	// Use this for initialization
	void Start () 
	{
		StartCoroutine( FadeInfoChange() );
	}
	
	// Update is called once per frame
	void Update () 
	{
		if( CrossPlatformInputManager.GetAxis( "Fire1" ) != 0 )
		{

			if( !RFEasySceneLoader.Instance.isLoading )
				RFEasySceneLoader.Instance.LoadScene();

			Input.ResetInputAxes();
		}
	}

    /// <summary>
    /// Crossfade between the Enemy Information and the current High Scores scoreboard
    /// </summary>
    /// <returns></returns>
	IEnumerator FadeInfoChange()
	{
		yield return new WaitForSeconds( 2.5f );
		float total = 1.0f;
        // Fade out Enemy Info, fade in high scores
		while( total >= 0 )
		{
			EnemyInfo.alpha = total;
			HighScores.alpha = 1 - total;
			total -= Time.deltaTime;
			yield return null;
		}
        EnemyInfo.alpha = 0;
        HighScores.alpha = 1.0f;


        // Fade out High Scores, Fade In Enemy Info
        yield return new WaitForSeconds( 4.0f );
		total = 1.0f;
		while( total >= 0 )
		{
			EnemyInfo.alpha = 1 - total;
			HighScores.alpha =  total;
			total -= Time.deltaTime;
			yield return null;
		}
        EnemyInfo.alpha = 1.0f;
        HighScores.alpha = 0;


        StartCoroutine( FadeInfoChange() );
	}


}
