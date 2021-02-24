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

	public class HandPick
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		public enum EVENT { START, STOP }

		public delegate void OnEvent( HandPick handPick, EVENT evt );

		public delegate GPS.Coord LatFrom3DCoord( Vector3 p );

		public delegate GPS.Coord LngFrom3DCoord( Vector3 p );

		static public GPS.Coord DefaultLatFrom3DCoord( Vector3 p ) { return new GPS.Coord( GPS.TYPE.LATITUDE,  0.0f ); }

		static public GPS.Coord DefaultLngFrom3DCoord( Vector3 p ) { return new GPS.Coord( GPS.TYPE.LONGITUDE, 0.0f ); }

		//************************************************************************************************
		//
		//************************************************************************************************

		private OnEvent			m_handler        = null;

		private LatFrom3DCoord  m_latFrom3DCoord = DefaultLatFrom3DCoord;

		private LngFrom3DCoord	m_lngFrom3DCoord = DefaultLngFrom3DCoord;

		private Camera			m_camera         = null;

		private bool			m_enabled        = false;
		
		private CORE.Plane		m_plane          = null;

		private CORE.Sphere		m_sphere         = null;

		private Vector3			m_point          = new Vector3  ( 0.0f, 0.0f, 0.0f );

		GPS.Coord				m_latitude       = new GPS.Coord( GPS.TYPE.LATITUDE,  0 );

		GPS.Coord				m_longitude      = new GPS.Coord( GPS.TYPE.LONGITUDE, 0 );

		private bool			m_valid          = false;

		//************************************************************************************************
		//
		//************************************************************************************************

		public bool      IsActive  { get { return m_enabled == true; } }

		public Vector3   Point     { get { return m_point;           } }

		public GPS.Coord Latitude  { get { return m_latitude;        } }

		public GPS.Coord Longitude { get { return m_longitude;       } }

		public bool      IsValid   { get { return m_valid;           } }

		//************************************************************************************************
		//
		//************************************************************************************************

		public void Setup( Camera camera, Plane plane, OnEvent handler, LatFrom3DCoord latFrom3DCoord = null, LngFrom3DCoord lngFrom3DCoord = null )
		{
			Enable( false );

			m_camera         = camera;

			m_plane          = plane;

			m_sphere         = null;

			m_handler        = handler;

			m_latFrom3DCoord = latFrom3DCoord != null ? latFrom3DCoord : DefaultLatFrom3DCoord;

			m_lngFrom3DCoord = lngFrom3DCoord != null ? lngFrom3DCoord : DefaultLngFrom3DCoord;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public void Setup( Camera camera, CORE.Sphere sphere, OnEvent handler, LatFrom3DCoord latFrom3DCoord = null, LngFrom3DCoord lngFrom3DCoord = null )
		{
			Enable( false );

			m_camera         = camera;

			m_plane          = null;

			m_sphere         = sphere;

			m_handler        = handler;

			m_latFrom3DCoord = latFrom3DCoord != null ? latFrom3DCoord : DefaultLatFrom3DCoord;

			m_lngFrom3DCoord = lngFrom3DCoord != null ? lngFrom3DCoord : DefaultLngFrom3DCoord;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public void Enable( bool enable )
		{
			if( m_enabled != enable )
			{
				m_enabled  = enable;

				if( enable ) { if( m_handler != null ) m_handler( this, EVENT.START ); }

				else         { if( m_handler != null ) m_handler( this, EVENT.STOP  ); }
			}
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public void Update()
		{
			m_valid = false;

			if( m_enabled == false )                          return;
		
			if( m_camera  == null  )                          return;

			if( ( m_plane == null ) && ( m_sphere == null ) ) return;


			Vector3 cursorPos = Input.mousePosition;

			if( ( cursorPos.x < 0.0f ) || ( cursorPos.x > Screen.width  ) ) return;

			if( ( cursorPos.y < 0.0f ) || ( cursorPos.y > Screen.height ) ) return;

			Ray  ray = m_camera.ScreenPointToRay( cursorPos );


			if( m_plane != null )
			{
				float t = float.MinValue;

				float r = 1.0e3f;

				m_valid = CORE.RayCast.Intersect( ray, 1.0e3f, m_plane, ref t );

				if( m_valid )
				{
					m_point     = ray.origin + ( ray.direction * r * t );

					m_latitude  = m_latFrom3DCoord( m_point );

					m_longitude = m_lngFrom3DCoord( m_point );
				}
			}
			else
			{
				float t1 = float.MaxValue;

				float t2 = float.MaxValue;

				m_valid = CORE.RayCast.Intersect( ray, m_sphere, ref t1, ref t2 );

				if( m_valid )
				{
					m_point     = ray.origin + ( ray.direction * t1 );

					m_latitude  = m_latFrom3DCoord( ( m_point - m_sphere.center ).normalized );

					m_longitude = m_lngFrom3DCoord( ( new Vector3( m_point.x, 0.0f, m_point.z ) - m_sphere.center ).normalized );
				}
			}
		}
	}
}