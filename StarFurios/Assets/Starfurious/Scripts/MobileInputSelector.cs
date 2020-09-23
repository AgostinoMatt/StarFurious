using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Simple component to allow which mobile input to use: Button direction pad or Mobile Joy Stick 
/// 
/// </summary>
public class MobileInputSelector : MonoBehaviour 
{

	[SerializeField]
	GameObject MobileStick;

	[SerializeField]
	GameObject DirPad;


	public void SelectJoystick()
	{
#if MOBILE_INPUT		
		MobileStick.SetActive( true );
		DirPad.SetActive( false );
#endif
	}
	public void SelectDirPad()
	{
#if MOBILE_INPUT		
		MobileStick.SetActive( false );
		DirPad.SetActive( true );
#endif
	}

}
