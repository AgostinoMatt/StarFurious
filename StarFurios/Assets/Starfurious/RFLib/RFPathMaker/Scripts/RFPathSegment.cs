using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RFLib
{
	/// <summary>
	/// A path segment consists of a start point and an endpoint.  Points
	/// between are derived based on the segement type: curve, straight, etc
	/// Segement 'points' are generated / returned when GetSegmentPoints is called
	/// </summary>
	[Serializable]
	public class RFPathSegment
	{
		static float CP_MULT	= 1f;

		public enum RFPathSegmentType
		{
			FIXED,				// Non-algorthimic segment; created externally, then stored in this structure
			STRAIGHT,			// Segment follows a straight light
			BEZIER_CURVE, 		// Segment is genereted using a bezier curve algorithm
			CATMULL_ROM_SPLINE, // Segement generated using catmull-rom spling algorithm
		}


		public RFPathSegmentType segmentType;
		public List<Vector3> segmentPoints;	// Generated or from storage

		public Vector3 segmentStartPoint;
		public Vector3 segmentEndPoint;

		public Vector3 controlPoint1;			// CatmulRom/Beziers use control points	
		public Vector3 controlPoint2;

		public float segmentSteps;				// num points to generate
		public float alpha;				
		public float cp_multiplier = CP_MULT;				// Control point mult ( no longer used? )


		public void SwapEndpoints()
		{
			Vector3 oldStart = segmentStartPoint;
			segmentStartPoint = segmentEndPoint;
			segmentEndPoint = oldStart;
		}

		public void SetAsFixed(List<Vector3> points)
		{
			segmentType 	= RFPathSegmentType.FIXED;
			segmentPoints 	= new List<Vector3>( points );
		}

		public void SetAsStraight(Vector3 startPoint, Vector3 endPoint, float steps)
		{
			segmentType 		= RFPathSegmentType.STRAIGHT;
			segmentSteps 		= steps;
			segmentStartPoint 	= startPoint;
			segmentEndPoint 	= endPoint;
		}

		public void SetAsBezier2D( Vector3 segStart, Vector3 cp1, Vector3 cp2, Vector3 segEnd, float steps)
		{
			segmentType 		= RFPathSegmentType.BEZIER_CURVE;
			segmentSteps		= steps;
			segmentStartPoint	= segStart;
			segmentEndPoint 	= segEnd;
			controlPoint1 		= cp1;
			controlPoint2		= cp2;
		}

		public void SetAsCatmulRom( Vector3 cp1, Vector3 segStart, Vector3 segEnd, Vector3 cp2, float steps, float talpha )
		{
			segmentType 		= RFPathSegmentType.CATMULL_ROM_SPLINE;
			segmentSteps		= steps;
			segmentStartPoint	= segStart;
			segmentEndPoint 	= segEnd;
			controlPoint1 		= cp1;
			controlPoint2		= cp2;
			alpha 				= talpha;

		}

		/// <summary>
		/// Sets segment options based on lastSegment
		/// </summary>
		/// <param name="newPoint">New Endpoint for the segment </param>
		/// <param name="lastSegment">Segment to base startpoint and other options from</param>
		public void CreateNextFromOther(Vector3 newPoint, RFPathSegment lastSegment)
		{
			segmentType 		= lastSegment.segmentType;
			segmentSteps		= lastSegment.segmentSteps;
			segmentStartPoint	= lastSegment.segmentEndPoint;
			segmentEndPoint 	= newPoint;

			ResetControlPoints( true );

			alpha 				= lastSegment.alpha;
		}
		/// <summary>
		/// Sets segment end points based LastSegment
		/// </summary>
		/// <param name="newPoint">Starting Segment point.</param>
		/// <param name="lastSegment">Segment to base endpoint and other options from</param>
		public void CreatePrevFromOther(Vector3 newPoint, RFPathSegment lastSegment)
		{
			segmentType 		= lastSegment.segmentType;
			segmentSteps		= lastSegment.segmentSteps;

			segmentStartPoint	= newPoint;
			segmentEndPoint 	= lastSegment.segmentStartPoint;

			ResetControlPoints( true );
			alpha 				= lastSegment.alpha;
		}

		/// <summary>
		/// Determines whether this segment instance is a curve.
		/// </summary>
		/// <returns><c>true</c> if this instance is a curve; otherwise, <c>false</c>.</returns>
		public bool IsCurve()
		{
			return segmentType == RFPathSegmentType.BEZIER_CURVE || segmentType == RFPathSegmentType.CATMULL_ROM_SPLINE;
		}

		public bool IsNearStart(Vector3 point, float maxDistance) 	{	return Vector3.Distance( point, segmentStartPoint ) < maxDistance;		}
		public bool IsNearEnd(Vector3 point, float maxDistance) 	{	return Vector3.Distance( point, segmentEndPoint ) < maxDistance;		}
		public bool IsNearCP1(Vector3 point, float maxDistance) 	{	return Vector3.Distance( point, controlPoint1 ) < maxDistance && IsCurve();	}
		public bool IsNearCP2(Vector3 point, float maxDistance) 	{	return Vector3.Distance( point, controlPoint2 ) < maxDistance && IsCurve();	}

		/// <summary>
		/// Generate a list of points between the endpoints of the segment
		/// See the specific implementations of each path type in RFSplineUtils
		/// </summary>
		/// <returns>The segment steps.</returns>
		public List<Vector3> GetSegmentSteps()
		{

			if( segmentType == RFPathSegmentType.STRAIGHT )
			{
				return RFSplineUtils.GetStraightLine( segmentStartPoint, segmentEndPoint, segmentSteps );
			}
			else if( segmentType == RFPathSegmentType.CATMULL_ROM_SPLINE )
			{
				return RFSplineUtils.CatmulRomSpline2D( controlPoint1 , segmentStartPoint, segmentEndPoint, controlPoint2 , segmentSteps, alpha );
			}
			else if( segmentType == RFPathSegmentType.BEZIER_CURVE )
			{
				return RFSplineUtils.Bezier2D( segmentStartPoint, controlPoint1, controlPoint2, segmentEndPoint, segmentSteps );
			}
			else if( segmentType == RFPathSegmentType.FIXED )
			{
				if( segmentPoints == null ) segmentPoints = new List<Vector3>();
				return segmentPoints;

			}
			else
			{
				return new List<Vector3>();
			}
		}
		/// <summary>
		/// Updates the segment start point.
		/// </summary>
		/// <param name="newLoc">New location.</param>
		public void UpdateStartPoint(Vector3 newLoc)
		{
			if( IsCurve() )
			{
				Vector3 diff =  newLoc-segmentStartPoint;
				controlPoint1 += diff;
			}
			segmentStartPoint = newLoc;
		}

		/// <summary>
		/// Updates the segment end point.
		/// </summary>
		/// <param name="newLoc">New location.</param>
		public void UpdateEndPoint(Vector3 newLoc)
		{
			if( IsCurve() )
			{
				Vector3 diff =  newLoc - segmentEndPoint;
				controlPoint2 += diff;
			}
			segmentEndPoint = newLoc;
		}

		/// <summary>
		/// Resets the control points for a curve
		/// </summary>
		public void ResetControlPoints(bool force=false)
		{
			if( IsCurve() || force)
			{
				controlPoint1 		= segmentStartPoint;
				controlPoint1.x -= 1;
				controlPoint2		= segmentEndPoint;
				controlPoint2.x += 1;
			}
		}

		public bool PoinNearSteps(Vector3 point, float maxDistance)
		{
			List<Vector3> points = this.GetSegmentSteps();
			for( int cnt = 0; cnt < points.Count; cnt++ )
			{
				if( Vector3.Distance( point, points[ cnt ] ) < maxDistance )
					return true;
			}
			return false;
		}


	}
}