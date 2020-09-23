using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using RFLib;

namespace RFLibEditor
{
	[CustomEditor( typeof(RFPathTraveller) ) ]
	public class RFPathTravellerEditor : Editor
	{

		bool viewPath = true;


		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.BeginHorizontal();
			viewPath = EditorGUILayout.Toggle( "View Path", viewPath );
			EditorGUILayout.EndHorizontal();

		}

		void OnSceneGUI()
		{
			if( viewPath)
			{
				RFPathTraveller traveller = target as RFPathTraveller;
				if( traveller )
				{
					List<Vector3> currPath = traveller.CurrentPath;
					if( currPath != null )
					{
						Handles.color = Color.gray;
						for( int cnt = 0; cnt < currPath.Count; cnt++ )
						{
							
							Handles.SphereHandleCap( 0, currPath[ cnt ], Quaternion.identity, 0.05f, EventType.Repaint );
						}
					}
				}
			}


		}


	}

}