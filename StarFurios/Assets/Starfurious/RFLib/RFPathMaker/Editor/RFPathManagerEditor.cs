using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using RFLib;

namespace RFLibEditor
{

	[CustomEditor (typeof(RFPathManager))]
	public class RFPathManagerEditor : Editor 
	{


		float ENTPOINT_SIZE = 0.3f;	// Size of endpoint handles
		float SEGPOINT_SIZE = 0.2f; // Size of segment step handles


		bool firstPointPlaced = false;
		Vector3 LastPlacedPoint  ;	// Point Placed;
		Vector3 CurrentSelectedPoint; // Point selected 

		RFPathManager rfPathManager;

		List<Vector3> points = new List<Vector3>();

		bool editMode = false;	// Edit Mode allows addition/removal of segment points


		bool addToTail = true;	// How to add to the pathway -- to the head or to the tail


		string lastLoadFromPath = "Assets/";

		RFPathSegment selectedSegment = null;

		#region Menu Item
		// Menu item for creating a path
		[MenuItem("GameObject/RFLib/Create Path Manager")]
		public static void CreateRFPathManager()
		{
			if( GameObject.FindObjectOfType<RFPathManager>() == null )
			{
				GameObject go = new GameObject( "RFPathManager" );
				go.AddComponent<RFPathManager>();

			}
			else
			{
				Debug.LogError( "RFPathManager already exists." );
			}
		}
		#endregion


		void OnEnable()
		{



			rfPathManager = target as RFPathManager;
			rfPathManager.Init();

			Repaint();


		}
		void OnDisable()
		{

		}


		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			RFPathData pathData = rfPathManager.CurrentPathData;

			EditorGUILayout.BeginHorizontal();
			// Clear out the current path
			if( GUILayout.Button( "Clear", GUILayout.Width( 100 ) ) )
			{
				pathData.ClearAllSegments();
				points.Clear();
				firstPointPlaced = false;
			}

			if( GUILayout.Button( "Create New", GUILayout.Width( 100 ) ) )
				CreateNewCurrentPath();
			if( GUILayout.Button( "Save", GUILayout.Width( 100 ) ) )
				SaveRFPathAsset();
			if( GUILayout.Button( "Load", GUILayout.Width( 100 ) ) )
				LoadRFPathAsset();

			EditorGUILayout.EndVertical();

			// Add To Section; option to select head or tail to add to
			EditorGUILayout.BeginHorizontal();


			editMode = GUILayout.Toggle( editMode, "Edit Mode",GUILayout.Width(75) );
			Color oldC = GUI.color;
			if( !editMode )
				GUI.color = Color.gray;


			GUILayout.Label( "Add to: ", GUILayout.Width(50) );

			bool addHead = !addToTail;
			bool addTail = addToTail;

			addTail = GUILayout.Toggle( addTail, "Tail", GUILayout.Width(50) );
			addHead   = GUILayout.Toggle( addHead, "Head", GUILayout.Width(50) );
			if( (addHead && addToTail) || (addTail && !addToTail) )
			{
				addToTail = !addToTail;
				// depending on where to add, need to grab the "last placed point"
				if( pathData.SegmentCount > 0 )
				{
					if( addToTail )
						LastPlacedPoint = pathData.GetLastSegment().segmentEndPoint;
					else
						LastPlacedPoint = pathData.GetFirstSegment().segmentStartPoint;
				}
			}
			GUI.color = oldC;

			EditorGUILayout.EndHorizontal();



			ShowSegmentGUI();
			Repaint();
			SceneView.RepaintAll();

		}

		void OnSceneGUI()
		{
			
			// Necessary so we don't lose focus during point change up
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			HandleUtility.AddDefaultControl(controlID);

			Event evt = Event.current;
			// Mouse button was pressed... figure out where
			if (evt.type == EventType.MouseDown)
			{
				if( evt.button == 0 && !evt.alt && !evt.command && !evt.control )
				{
					Vector2 mousePosition = evt.mousePosition;

					Ray wRay = HandleUtility.GUIPointToWorldRay( mousePosition );
					Vector3 pathPoint = wRay.GetPoint( 0 );
					pathPoint.z = 0;

					// If the first point is already placed, figure out if the player clicked on (near) a current
					// Segment point (or control point); If not, add a point or segment. Adding a new segment is based
					// in the LAST segment added
					if( firstPointPlaced )
					{

						RFPathSegment tseg = rfPathManager.CurrentPathData.GetSegmentFromStepPoint( 0.2f, pathPoint, true );
						if( tseg != null )
						{
							selectedSegment = tseg;
						//	CurrentSelectedPoint = tseg.segmentStartPoint;
						}

						if(editMode)
						{


							// If the current path doesn't have a control point/endpoint where mose clicked...
							if( !rfPathManager.CurrentPathData.DoesPointControlSegment( 0.5f, pathPoint ) )
							{
								// add click point - Depending what's what, either add it to the head or add it to the tail
								if( addToTail )
								{
									if( rfPathManager.CurrentPathData.AddPointFromLastSegment( pathPoint ) == null )
										rfPathManager.CurrentPathData.AddStraightSegment( LastPlacedPoint, pathPoint, 5 );
								}
								else
								{
									if( rfPathManager.CurrentPathData.AddPointFromFirstSegment( pathPoint ) == null )
										rfPathManager.CurrentPathData.AddStraightSegment( LastPlacedPoint, pathPoint, 5, true );
								}
							}
							CurrentSelectedPoint = pathPoint;
							LastPlacedPoint = pathPoint;
						}


					}


					if( firstPointPlaced == false ) firstPointPlaced = true;

					Repaint();
					SceneView.RepaintAll();

				}
				else if( evt.button == 1 && editMode)
				{
					Vector2 mousePosition = evt.mousePosition;
					Ray wRay = HandleUtility.GUIPointToWorldRay( mousePosition );
					Vector3 pathPoint = wRay.GetPoint( 0 );
					pathPoint.z = 0;

					// If the current path doesn't have a control point/endpoint where mose clicked...
					if(rfPathManager.CurrentPathData.DoesPointControlSegment( 0.5f, pathPoint ) )
					{
						CurrentSelectedPoint = pathPoint;
						deleteSelectedPoint();

					}
					
				}
			}

			DrawPointHandles();

		}


		/// <summary>
		/// Remove a point from the path data
		/// </summary>
		void deleteSelectedPoint()
		{
			RFPathData pathData = rfPathManager.CurrentPathData;
			pathData.DeletePoint( CurrentSelectedPoint );

			Repaint();
			SceneView.RepaintAll();
			if( pathData.SegmentCount == 0 )
				firstPointPlaced = false;

			selectedSegment = null;
		}

		/// <summary>
		/// Draw the target Data Segments information within the Custom Editor Window
		/// </summary>
		void ShowSegmentGUI()
		{
			RFPathData pathData = rfPathManager.CurrentPathData;
			if( pathData == null || pathData.PathSegments == null ) return;


			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField( "Select", GUILayout.Width(40) );

			EditorGUILayout.LabelField( "Seg Type", GUILayout.Width(100) );
			EditorGUILayout.LabelField( "Steps", GUILayout.Width(50) );
			EditorGUILayout.LabelField( "Alpha", GUILayout.Width(50) );

			EditorGUILayout.EndHorizontal();

			Color oldColor = GUI.color;


			for( int cnt = 0; cnt < pathData.PathSegments.Count; cnt++ )
			{
				RFPathSegment pathSeg = pathData.PathSegments[ cnt ];

				// Highlight the selected segment data in the Editor Window
				if( pathSeg == selectedSegment && selectedSegment != null )
					GUI.color = Color.cyan;
				else
					GUI.color = oldColor;
				
				EditorGUILayout.BeginHorizontal();

				GUILayout.Space( 5 );
				if(GUILayout.Button("S", GUILayout.Width(20)) )
					selectedSegment = pathSeg;
				GUILayout.Space( 20 );


				EditorGUI.BeginChangeCheck();
				RFPathSegment.RFPathSegmentType newType = (RFPathSegment.RFPathSegmentType) EditorGUILayout.EnumPopup( pathSeg.segmentType, GUILayout.Width(100) ) ; 
				if( EditorGUI.EndChangeCheck()  )
				{
					Undo.RecordObject( pathData, "Segment Type Change" );
					pathSeg.segmentType = newType;
					pathSeg.ResetControlPoints();

				}

				EditorGUI.BeginChangeCheck();
				float segsteps = EditorGUILayout.FloatField( pathSeg.segmentSteps, GUILayout.Width(50) );
				if( EditorGUI.EndChangeCheck() )
				{
					Undo.RecordObject( pathData, "Segment Step Change" );
					pathSeg.segmentSteps = segsteps;
				}


				if( pathSeg.segmentType != RFPathSegment.RFPathSegmentType.CATMULL_ROM_SPLINE )
				{
					EditorGUILayout.LabelField( "-", GUILayout.Width(50));

				}
				else
				{
					EditorGUI.BeginChangeCheck();
					float catmulalpha = EditorGUILayout.FloatField( pathSeg.alpha, GUILayout.Width(50) );
					if( EditorGUI.EndChangeCheck() )
					{
						Undo.RecordObject( pathData, "CatMul Alpha Change" );
						pathSeg.alpha = catmulalpha;
					}
				}

				EditorGUILayout.EndHorizontal();
			}



		}



		/// <summary>
		/// Draw point / control points on the screen for easy visualization / manipulation
		/// </summary>


		/// <summary>
		/// Draw point / control points on the screen for easy visualization / manipulation
		/// </summary>
		void DrawPointHandles()
		{
			RFPathData pathData = rfPathManager.CurrentPathData;
			if( pathData == null || pathData.PathSegments == null ) return;
			if( !firstPointPlaced && pathData.SegmentCount == 0 ) return;

			points.Clear();


			Vector3 oldPoint;
			Vector3 changedPoint;

			bool doRepaint = false;

			if( pathData.PathSegments.Count == 0 )
			{
				EditorGUI.BeginChangeCheck();
				oldPoint = 	Handles.FreeMoveHandle( LastPlacedPoint, Quaternion.identity, ENTPOINT_SIZE * 1.1f, Vector3.zero, Handles.SphereHandleCap );
				if( EditorGUI.EndChangeCheck() )
				{
					Undo.RecordObject( pathData, "End Point Move" );
					LastPlacedPoint = oldPoint;
					doRepaint = true;

				}


			}
			else
			{
				for( int cnt = 0; cnt < pathData.PathSegments.Count; cnt++ )
				{
					RFPathSegment seg = pathData.PathSegments[ cnt ];

					EditorGUI.BeginChangeCheck();
					changedPoint = Handles.FreeMoveHandle( seg.segmentStartPoint, Quaternion.identity, ENTPOINT_SIZE * 1.1f, Vector3.zero, Handles.SphereHandleCap );
					if( EditorGUI.EndChangeCheck() )
					{
						Undo.RecordObject( pathData, "End Point Move" );		
						seg.UpdateStartPoint( changedPoint );
						if( cnt > 0 && cnt < pathData.PathSegments.Count )
						{
							
							RFPathSegment seg2 = pathData.PathSegments[ cnt-1 ];
							seg2.UpdateEndPoint( changedPoint );
							if( selectedSegment != seg && selectedSegment != seg2 )
								selectedSegment = null;
						}

						doRepaint = true;
					}

					if( cnt == pathData.PathSegments.Count - 1 )
					{
						EditorGUI.BeginChangeCheck();
						changedPoint = Handles.FreeMoveHandle( seg.segmentEndPoint, Quaternion.identity, ENTPOINT_SIZE * 1.1f, Vector3.zero, Handles.SphereHandleCap );
						if(EditorGUI.EndChangeCheck())
						{
							Undo.RecordObject( pathData, "End Point Move" );		
							seg.UpdateEndPoint( changedPoint );
							doRepaint = true;
							if( selectedSegment != seg )
								selectedSegment = null;
						}
					}
				}

			}

			showSegmentPoints();

			if( doRepaint )			Repaint();

		}












		/// <summary>
		/// Shows the segment points - segments will "render" their points
		/// </summary>
		void showSegmentPoints()
		{
			RFPathData pathData = rfPathManager.CurrentPathData;
			for( int cnt = 0; cnt < pathData.PathSegments.Count; cnt++ )
			{
				RFPathSegment seg = pathData.PathSegments[ cnt ];
				List<Vector3> vlist = seg.GetSegmentSteps();
	

				// Draw the segment steps; Selected Segment shows green
				Color c = Handles.color;
				if( seg == selectedSegment )
					Handles.color = Color.green;
				else
					Handles.color = Color.red;
				// Iterate through and draw the segment step points. IF a point is close enough to
				// the start or end points for the segment, don't draw them.
				for( int vcnt = 0; vcnt < vlist.Count; vcnt += 1 )
				{
					Vector3 loc = vlist[ vcnt ];
					if( !seg.IsNearEnd(loc, 0.15f) && !seg.IsNearStart(loc, 0.15f))
						Handles.SphereHandleCap( 0, loc, Quaternion.identity, SEGPOINT_SIZE, EventType.Repaint );
				}

				// Draw the Control Points if the selected segment is a curve
				Handles.color = c;
				if( seg.IsCurve() && ( seg==selectedSegment ) )
				{

					c = Handles.color;
					Handles.color = Color.blue;

					// Draw handle for Control Point 1
					Vector3 oldPoint = seg.controlPoint1;
					Vector3 newpoint = oldPoint;
					EditorGUI.BeginChangeCheck();
					newpoint= Handles.FreeMoveHandle( oldPoint, Quaternion.identity, ENTPOINT_SIZE, Vector3.zero, Handles.SphereHandleCap);
					if( EditorGUI.EndChangeCheck()) 
					{
						Undo.RecordObject(pathData, "Control Point Move");
						seg.controlPoint1 = newpoint;
					}

					drawHandleLine( newpoint, seg.segmentStartPoint, Color.gray );

					// Draw Handle to Control Point 2
					oldPoint = seg.controlPoint2;
					newpoint = oldPoint;
					EditorGUI.BeginChangeCheck();
					newpoint= Handles.FreeMoveHandle( oldPoint, Quaternion.identity, ENTPOINT_SIZE, Vector3.zero,Handles.SphereHandleCap);
					if( EditorGUI.EndChangeCheck()) 
					{
						Undo.RecordObject(pathData, "Control Point Move");
						seg.controlPoint2 = newpoint;
					}
					Handles.color = c;

					drawHandleLine( newpoint, seg.segmentEndPoint, Color.gray );
				}

			}
		}



		/// <summary>
		/// Draws the handle line; color gray
		/// </summary>
		/// <param name="p1">P1.</param>
		/// <param name="p2">P2.</param>
		void drawHandleLine(Vector3 p1, Vector3 p2, Color lineColor)
		{
			Color c = Handles.color;
			Handles.color = lineColor;
			Handles.DrawLine(p1, p2);
			Handles.color = c;

		}



		// Create a new asset
		void CreateNewCurrentPath()
		{

			string assetPath = EditorUtility.SaveFilePanelInProject("Create New RFPathData", "rf_path", "asset", "");

			if (string.IsNullOrEmpty(assetPath))		return;

			assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

			int startIndex = assetPath.LastIndexOf("/") + 1;
			int length = assetPath.LastIndexOf(".") - startIndex;
			string assetName = assetPath.Substring(startIndex, length);

			RFPathData pd = ScriptableObject.CreateInstance<RFPathData>();

			rfPathManager.CurrentPathData = pd;
			pd.name = assetName;

			AssetDatabase.CreateAsset(pd, assetPath);
			AssetDatabase.SaveAssets();

		}

		void LoadRFPathAsset()
		{
			string strAssetPath = EditorUtility.OpenFilePanel("Load Path", lastLoadFromPath, "asset");
			if (string.IsNullOrEmpty(strAssetPath))		return ;
			strAssetPath = strAssetPath.Substring(strAssetPath.IndexOf("Assets/"));



			rfPathManager.CurrentPathData = (RFPathData)AssetDatabase.LoadAssetAtPath(strAssetPath, typeof(RFPathData));
			if( rfPathManager.CurrentPathData == null )
			{
				Debug.Log("Bad Path Data?");
				return;
					
			}

			int startIndex = strAssetPath.LastIndexOf("/") + 1;
			int length = strAssetPath.LastIndexOf(".") - startIndex;
			string strAssetName = strAssetPath.Substring(startIndex, length);

			lastLoadFromPath = strAssetPath.Substring( 0, startIndex );

			rfPathManager.CurrentPathData.name = strAssetName;

		}

		void SaveRFPathAsset()
		{
			EditorUtility.SetDirty(rfPathManager.CurrentPathData);
			AssetDatabase.SaveAssets();
		}



	}



}

	


