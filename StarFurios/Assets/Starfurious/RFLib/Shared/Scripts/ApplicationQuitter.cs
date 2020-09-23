using UnityEngine;
using System;
using UnityStandardAssets.CrossPlatformInput;
#if UNITY_EDITOR || UNITY_EDITOR_64
using UnityEditor;
#endif

/*  Quit Script -- simple input handling script to exit the program
 * 
 *  Requires QuitAxisName value to be setup in the input manager
 *  
 *  If AllowEscapeToQuit is set to True, then allow the Keyboard ESCAPE key to also function
 *  as an application exit trigger.
 * 
 */ 

namespace RFLib
{
	
public class ApplicationQuitter : MonoBehaviour 
{
	public bool 	AllowEscapeToQuit 	= false;
	public string 	QuitAxisName 		= "Exit";  // Static name of the axis we want to check input; this should be setup in the Input Manager

	bool 			axisIsDefined 		= false;	// Set to true on start, if the axis is defined.  Checked during Update



	// Use this for initialization
	void Start () 
	{
	
		try
		{
			if( !string.IsNullOrEmpty(QuitAxisName ))
			{
				CrossPlatformInputManager.GetAxis(QuitAxisName);	
				axisIsDefined = true;
			}
		}
		catch(ArgumentException e)
		{
			Debug.LogWarning(e.Message);
				axisIsDefined = false;
		}

		if(!axisIsDefined && !AllowEscapeToQuit)
		{
			Debug.LogWarning(string.Format("ApplicationQuitter Warning: The {0} input axis is not defined and AllowEscapeToExit is also false. \nPlayers will not be able to exit the application using ApplicationQuitter", QuitAxisName));
		}

	}
	
	// Update is called once per frame
	void Update () 
	{
			if( (axisIsDefined && CrossPlatformInputManager.GetAxis(QuitAxisName) != 0) || (AllowEscapeToQuit && Input.GetKeyDown(KeyCode.Escape)) )
		{
			DoQuit();
		}
	}


	// Do Quit 
	public void DoQuit()
	{
		// If we turned the cursor off, make sure it's back on!
		if(Cursor.visible == false)	Cursor.visible = true;

#if UNITY_EDITOR || UNITY_EDITOR_64
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif


	}
}

}