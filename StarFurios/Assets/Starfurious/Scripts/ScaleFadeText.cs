using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
/// <summary>
/// Scale up and fade out UI text - exectures auto-magically when the game object is set to active / enabled
/// </summary>
[ExecuteInEditMode()]
public class ScaleFadeText : MonoBehaviour 
{
	[SerializeField]
	float MaxScale = 5f;

	Text text;
	float scaleMult = 1.0f;

	// Use this for initialization
	void Start () 
	{
		Init();
	}

	void OnEnable()
	{
		Init();
	}


	void Init()
	{
		text = GetComponent<Text>();
		text.color = Color.white;
		transform.localScale = Vector3.one;
		scaleMult = 1.0f;
		gameObject.SetActive( true );
	}
	
	// Update is called once per frame
	void Update () 
	{
		if( text != null )
		{
			scaleMult += Time.deltaTime * 10;
			if( scaleMult <= MaxScale )
			{
				Vector3 scale = Vector3.one * scaleMult;
				transform.localScale = scale;
			}
			if( scaleMult > (MaxScale * 0.75f) )
			{
				Color c = text.color;
				c.a -= Time.deltaTime * 0.75f;

				if( c.a <= 0 )
				{
					c.a = 0;
					gameObject.SetActive( false );
				}

				text.color = c;
			}


		}
	}
}
