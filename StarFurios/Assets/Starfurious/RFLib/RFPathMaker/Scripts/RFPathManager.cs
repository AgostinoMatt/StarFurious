using UnityEngine;
using System;
using System.Collections;


namespace RFLib
{
	/// <summary>
	/// RFPathManager - object to provide for GUI building and management of paths
	/// </summary>

	[ExecuteInEditMode]
	public class RFPathManager : MonoBehaviour 
	{


		public RFPathData CurrentPathData;		// Current path set to modify


		public void Init()
		{
			if( CurrentPathData == null )
			{
				CurrentPathData = ScriptableObject.CreateInstance<RFPathData>();
				CurrentPathData.name = "new_path";
			}
		}


		void Awake()
		{
			Init();
		}
		void OnEnable()
		{
			Init();
		}

		// Use this for initialization
		void Start () 
		{
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}
	}
}