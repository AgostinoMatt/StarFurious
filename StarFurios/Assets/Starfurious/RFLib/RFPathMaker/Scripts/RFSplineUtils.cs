using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Catmul-Rom Spline: https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline
// Bezier Pathing: http://devmag.org.za/2011/04/05/bzier-curves-a-tutorial/
// Lines: http://math.stackexchange.com/questions/175896/finding-a-point-along-a-line-a-certain-distance-away-from-another-point

namespace RFLib
{
	public class RFSplineUtils 
	{
		/// <summary>
		/// Generate a Catmul-Rom Spline
		/// </summary>
		/// <returns>Return a list of points that make up the spline path</returns>
		/// <param name="point_count">Point count in curve</param>
		public static List<Vector3> CatmulRomSpline2D( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float point_count, float alpha=0.5f)
		{
			List<Vector3> points = new List<Vector3>();

			float t0 = 0.0f;
			float t1 = GetCatmulT(t0, p0, p1, alpha);
			float t2 = GetCatmulT(t1, p1, p2, alpha);
			float t3 = GetCatmulT(t2, p2, p3, alpha);

			for(float t=t1; t<t2; t+=((t2-t1) / point_count))
			{
				Vector3 A1 = (t1-t)/(t1-t0)*p0 + (t-t0)/(t1-t0)*p1;
				Vector3 A2 = (t2-t)/(t2-t1)*p1 + (t-t1)/(t2-t1)*p2;
				Vector3 A3 = (t3-t)/(t3-t2)*p2 + (t-t2)/(t3-t2)*p3;

				Vector3 B1 = (t2-t)/(t2-t0)*A1 + (t-t0)/(t2-t0)*A2;
				Vector3 B2 = (t3-t)/(t3-t1)*A2 + (t-t1)/(t3-t1)*A3;

				Vector3 C = (t2-t)/(t2-t1)*B1 + (t-t1)/(t2-t1)*B2;

				points.Add(C);
			}

			return points;
		}
		public static float GetCatmulT(float t, Vector3 p0, Vector3 p1, float alpha)
		{
			float a = Mathf.Pow((p1.x-p0.x), 2.0f) + Mathf.Pow((p1.y-p0.y), 2.0f) + Mathf.Pow((p1.z-p0.z), 2.0f);
			float b = Mathf.Pow(a, 0.5f);
			float c = Mathf.Pow(b, alpha);

			return (c + t);
		}




		public static List<Vector3> Bezier2D(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float point_count)
		{
			List<Vector3> points = new List<Vector3>();

			for( float cnt = 0; cnt < point_count; cnt++ )
			{
				float t = cnt / point_count;
				points.Add( GetBezierPoint(t, p0, p1, p2, p3));
			}

			return points;
		}

		public static Vector3 GetBezierPoint( float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
		{
			 float u = 1 - t;
			 float tt = t*t;
			 float uu = u*u;
			 float uuu = uu * u;
			 float ttt = tt * t;

			 Vector3 p = uuu * p0; //first term
			 p += 3 * uu * t * p1; //second term
			 p += 3 * u * tt * p2; //third term
			 p += ttt * p3; //fourth term
			 
			 return p;
		}

		public static List<Vector3> GetStraightLine(Vector3 p0, Vector3 p1, float point_count)
		{
			List<Vector3> points = new List<Vector3>();
			float dist = Vector3.Distance( p0, p1 );

			float distChunk = dist/point_count;

			float x;
			float y;
			float z;

			for( float cnt = 0; cnt < point_count; cnt++ )
			{
				float t = (distChunk * cnt) / dist;
				x =  ((1 - t) * p0.x) + (t * p1.x);
				y =  ((1 - t) * p0.y) +  (t * p1.y);
				z =  ((1 - t) * p0.z) +  (t * p1.z);

				Vector3 v = new Vector3(x, y, z );
				points.Add( v );
			}

			return points;
		}



	}
}