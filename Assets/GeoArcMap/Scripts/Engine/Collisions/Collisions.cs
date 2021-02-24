using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

namespace CORE
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class Plane
	{
		public Vector3 p = Vector3.zero;

		public Vector3 n = Vector3.up;

		public Plane( Vector3 P, Vector3 N ) { p = P; n = N; }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class Sphere
	{
		public Vector3 center = Vector3.zero;

		public float   radius = 1.0f;

		public Sphere() {}

		public Sphere( Vector3 C, float R ) { center = C; radius = R; }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public static class RayCast
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		static public bool Intersect( Ray ray, float range, CORE.Plane plane, ref float t )
		{
			Vector3 prj  = ray.direction * range;

			float   prjD = Vector3.Dot( prj, plane.n ); if( prjD >= 0.0f ) return false;

			Vector3 sep  = plane.p - ray.origin;

			float   sepD = Vector3.Dot( sep, plane.n ); if( sepD < prjD  ) return false;

			t = sepD / prjD;

			return true;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static public bool Intersect( Ray ray, Sphere sphere, ref float t1, ref float t2 )
		{
			Vector3 sep  = sphere.center - ray.origin;

			float   lead = Vector3.Dot( sep, ray.direction );

			float   dsqr = Vector3.Dot( sep, sep ) - ( lead * lead );

			float   rsqr = sphere.radius * sphere.radius;

			if( dsqr > rsqr ) { t1 = t2 = float.MaxValue; return false; }


			float   t   = Mathf.Sqrt( rsqr - dsqr );

			        t1  = lead - t;

					t2  = lead + t;

			return true;
		}
	}
}