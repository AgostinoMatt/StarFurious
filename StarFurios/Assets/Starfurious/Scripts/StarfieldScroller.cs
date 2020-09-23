using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Starfield scroller: Upates the texture offset for the associated game object 
/// </summary>
public class StarfieldScroller : MonoBehaviour 
{
	[SerializeField]
	float ScrollSpeed = 0.05f;

	Vector2 offset = Vector2.zero;
	Renderer renderComp;

	// Use this for initialization
	void Start () 
	{
		renderComp = GetComponent<Renderer>();
		offset.x = 0.5f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if( renderComp != null )
		{	
			offset.y = Time.time * ScrollSpeed;
			renderComp.material.SetTextureOffset( "_MainTex", offset );
		}
	}
}
