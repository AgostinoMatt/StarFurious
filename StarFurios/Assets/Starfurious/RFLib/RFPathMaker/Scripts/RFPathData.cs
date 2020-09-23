using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace RFLib
{
	/// <summary>
	/// RFPathData is a collection of path segments.
	/// </summary>
	public class RFPathData :ScriptableObject
	{
		[SerializeField]
		public List<RFPathSegment> PathSegments = new List<RFPathSegment>();	// Collection of path segments


		void Awake()
		{
		}

		/// <summary>
		/// Gets all path points.
		/// </summary>
		/// <returns>List of path points</returns>
		public List<Vector3> GetAllPathPoints()
		{
			List<Vector3> allPoints = new List<Vector3>();

			for( int cnt = 0; cnt < PathSegments.Count; cnt++ )
			{
				RFPathSegment seg = PathSegments[ cnt ];

				// Add the start POint
				if(cnt == 0)
					allPoints.Add( seg.segmentStartPoint );
				else if( allPoints[ allPoints.Count - 1 ] != seg.segmentStartPoint )
					allPoints.Add( seg.segmentStartPoint );
					

				// Step through segment steps; add steps of not already added
				List<Vector3> segSteps = seg.GetSegmentSteps();
				for( int segCnt = 0; segCnt < segSteps.Count; segCnt++ )
				{
					Vector3 segStepPoint = segSteps[ segCnt ];
					bool okayToAdd = true;
					if( segCnt == 0 )
						okayToAdd = segStepPoint != allPoints[ allPoints.Count - 1 ];

					if(okayToAdd )
						allPoints.Add( segStepPoint );
				}

				// Add endpoint, if not in collection
				if( allPoints[ allPoints.Count - 1 ] != seg.segmentEndPoint )
					allPoints.Add( seg.segmentEndPoint );

			}

			return allPoints;
		}

		public void ClearAllSegments()
		{
			PathSegments.Clear();
		}

		public bool DoesPointControlSegment(float maxDistance, Vector3 point, bool checkControlPoints = true)
		{
			for( int cnt = 0; cnt < PathSegments.Count; cnt++ )
			{
				RFPathSegment seg = PathSegments[ cnt ];
				if( Vector3.Distance( point, seg.segmentStartPoint ) <= maxDistance ||
					Vector3.Distance( point, seg.segmentEndPoint ) <= maxDistance ) 
					return true;
				
				// Check control points (if requested, and if it's a curve!)
				if( checkControlPoints && seg.IsCurve()  )
				{
					if( Vector3.Distance( point, seg.controlPoint1 ) <= maxDistance ||
						Vector3.Distance( point, seg.controlPoint2 ) <= maxDistance ) 
						return true;
				}

			}
			return false;
		}

		/// <summary>
		/// Return a segment based on a point within MaxDistance to a step within the segment
		/// </summary>
		/// <param name="maxDistance">Max distance POINT can be from a step.</param>
		/// <param name="point">Point to check.</param>
		/// <param name="checkCloseToEnds">If true, dont return the segment if the check point is within MaxDistance to an endpoint</param>
		public RFPathSegment GetSegmentFromStepPoint(float maxDistance, Vector3 point, bool checkCloseToEnds=false)
		{
			for( int cnt = 0; cnt < PathSegments.Count; cnt++ )
			{
				RFPathSegment seg = PathSegments[ cnt ];

				if( checkCloseToEnds == true )
				{
					if( seg.IsNearEnd( point, maxDistance ) ) return null;
					if( seg.IsNearStart( point, maxDistance ) ) return null;
				}
					

				if( seg.PoinNearSteps( point, maxDistance ) ) return seg;
			}
			return null;
		}

		// 2 segments share a point. Segments must be adjacent; 
		// "distance" is the proximity to the point to check for
		// distance of 0 means the points must exactly match; otherwisel try to see if it's close
		public List<RFPathSegment> GetSegmentsSharingPoint(Vector3 point, float distance=0)
		{
			List<RFPathSegment> segs = new List<RFPathSegment>();
			int segCnt = PathSegments.Count;
			// No elements in the segment list
			if( segCnt == 0 ) return segs;

			RFPathSegment seg1;
			RFPathSegment seg2;

			// There is only one element in the segment list; see if this one contains a point
			if( segCnt == 1 )
			{
				seg1 = PathSegments[ 0 ];
				if( seg1.IsNearStart( point, distance ) || seg1.IsNearEnd( point, distance ) )
				{
					segs.Add( seg1 );
					return segs;
				}
					
			}

			for( int cnt = 0; cnt < PathSegments.Count-1; cnt++ )
			{
				seg1 = PathSegments[ cnt 	 ];
				seg2 = PathSegments[ cnt + 1 ];
				bool added = false;

				if( seg1.IsNearEnd( point, distance ) )
				{
					segs.Add( seg1 );
					added = true;
				}
				if( seg2.IsNearStart( point, distance ) )
				{
					segs.Add( seg2 );
					added = true;
				}

				if( added ) return segs;

			}

			// If we made it here, no midpoints are shared; check to see if the last segment is 
			// holding a point
			seg1 = PathSegments[ PathSegments.Count-1 ];
			if( seg1.IsNearStart( point, distance ) || seg1.IsNearEnd( point, distance ) )
			{
				segs.Add( seg1 );
				return segs;
			}
			// Did we simply click on the starting segment?
			seg1 = PathSegments[ 0 ];
			if( seg1.IsNearStart( point, distance ) || seg1.IsNearEnd( point, distance ) )
			{
				segs.Add( seg1 );
				return segs;
			}
			return segs;

		}

		// Delete a point from a segment/segments;  auto-joins flanking segments if they share the point
		// Only care about endpoints (not control points)
		public bool DeletePoint(Vector3 point)
		{

			// nothing to delete!
			if( PathSegments.Count == 0 ) return false;

			List<RFPathSegment> segs = GetSegmentsSharingPoint( point, 0.5f );
			if( segs.Count == 1 )
			{
				PathSegments.Remove( segs[ 0 ] );

				return true;
			}
			else if( segs.Count == 2 )
			{
				segs[ 0 ].segmentEndPoint = segs[ 1 ].segmentEndPoint;
				PathSegments.Remove( segs[ 1 ] );

				return true;
			}

				
			// No point found
			return false;
		}

		public int SegmentCount
		{
			get{ return PathSegments.Count; }
		}

		public RFPathSegment GetLastSegment()
		{
			if( PathSegments.Count > 0 )
				return PathSegments[ PathSegments.Count - 1 ];
			return null;
		}

		public RFPathSegment GetFirstSegment()
		{
			if( PathSegments.Count > 0 )
				return PathSegments[ 0 ];
			return null;

		}

		public RFPathSegment GetSegment(int ndx)
		{
			if( ndx >= 0 && ndx < PathSegments.Count )
				return PathSegments[ ndx ];
			return null;
		}

		public RFPathSegment AddSegment(RFPathSegment segment, bool toHead = false)
		{
			if( toHead )
			{
				PathSegments.Insert( 0, segment );
				if( PathSegments.Count == 1 )
				{
					segment.SwapEndpoints();
				}
			}
			else
			{
				PathSegments.Add( segment );
			}

			return segment;

		}

		public RFPathSegment AddFixedSegment(List<Vector3> points, bool toHead = false)
		{
			RFPathSegment seg = new RFPathSegment();
			seg.SetAsFixed( points );
			return AddSegment( seg, toHead );
		}

		public RFPathSegment AddStraightSegment(Vector3 startPoint, Vector3 endPoint, float steps, bool toHead = false)
		{
			RFPathSegment seg = new RFPathSegment();
			seg.SetAsStraight( startPoint, endPoint, steps );
			return AddSegment( seg, toHead );
		}
		public RFPathSegment AddCatMulSegment(Vector3 startPoint, Vector3 endPoint, Vector3 cp1, Vector3 cp2, float steps, float alpha, bool toHead = false)
		{
			RFPathSegment seg = new RFPathSegment();
			seg.SetAsCatmulRom( cp1, startPoint, endPoint, cp2, steps, alpha );
			return AddSegment( seg, toHead );
		}

		public RFPathSegment AddBezierSegment(Vector3 startPoint, Vector3 endPoint, Vector3 cp1, Vector3 cp2, float steps, bool toHead = false)
		{
			RFPathSegment seg = new RFPathSegment();
			seg.SetAsBezier2D( cp1, startPoint, endPoint, cp2, steps);
			return AddSegment( seg, toHead );
		}

		/// <summary>
		/// Create a new segment, based off the last segment in the list 
		/// </summary>
		/// <returns>Newly created segment, or null of no segments currently exist</returns>
		/// <param name="newPoint">End point of segment to create</param>
		public RFPathSegment AddPointFromLastSegment(Vector3 newPoint)
		{

			RFPathSegment lastSeg = GetLastSegment();
			if( lastSeg == null )
			{
				Debug.Log( "No segments..." );
				return null;
			}

			RFPathSegment seg = new RFPathSegment();
			seg.CreateNextFromOther( newPoint, lastSeg );
			return AddSegment( seg );
		}

		/// <summary>
		/// Create a new segment, based on the first segment of the list
		/// </summary>
		/// <returns>Newly created segment</returns>
		/// <param name="newPoint">Starting point of new segment.</param>
		public RFPathSegment AddPointFromFirstSegment(Vector3 newPoint)
		{
			RFPathSegment seg = null;
			if( PathSegments.Count > 0 )
			{
				RFPathSegment lastSeg = PathSegments[ 0 ];
				if( lastSeg != null )
				{
					seg = new RFPathSegment();
					seg.CreatePrevFromOther( newPoint, lastSeg );
					seg = AddSegment( seg, true );
				}
			}

			if( seg == null )
			{
				Debug.Log( "No First segment?" );
			}

			return seg;
		}
	
	}
}