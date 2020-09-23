using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// Simple label / value display manager for text items
/// If SetValue with an integer is called, and "IntIncrement" is != 0; then the actual displayed
/// score is incremented over time by IntIncrement, until it reaches the actual target value set.
/// </summary>
public class RFLabelValueIndicator : MonoBehaviour 
{
	[SerializeField]
	Text LabelText = null;			// Reference to the uGui Label Element

	[SerializeField]
	Text ValueText = null;			// The value text field...

	[SerializeField]
	int IntIncrement = 0;		// Increment the value, if an integer, by this amount;

	int currIntValue 	= 0;		// Holder for our current int value
	int targetIntValue 	= 0;		// Target value; if IntIncrement is != 0, then currIntValue is modified by IntIncrement, until it reaches targetIntValue


	/// <summary>
	/// Sets the label text display
	/// </summary>
	/// <param name="labelVal">Label value.</param>
	public void SetLabelDisplay(string labelVal)
	{
		if( LabelText != null ) LabelText.text = labelVal;
	}

	/// <summary>
	/// Set the value text display
	/// </summary>
	/// <param name="newValue">New value.</param>
	public void SetValue(int newValue, bool immediate = false)
	{
		targetIntValue = newValue;
		if( IntIncrement == 0 || newValue == 0 || immediate)
		{
			currIntValue = targetIntValue;
			SetValue( currIntValue.ToString() );
		}
	}
	/// <summary>
	/// Set the value text display
	/// </summary>
	/// <param name="newValue">New value.</param>
	public void SetValue(string newValue)
	{
		if( ValueText != null ) ValueText.text = newValue;
	}



	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Handle the Incremental update to targetint value if that is how the value is getting set.
		if( targetIntValue != currIntValue )
		{
			currIntValue += IntIncrement;
			if( IntIncrement < 0 )
			{
				if( currIntValue < targetIntValue ) currIntValue = targetIntValue;
			}
			else if( IntIncrement > 0 )
			{
				if( currIntValue > targetIntValue ) currIntValue = targetIntValue;
			}
			SetValue( currIntValue.ToString() );
		}
	}
}
