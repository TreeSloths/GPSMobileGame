using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class NavigationModeGlobe : Mode< ApplicationMain >
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public struct Click
	{
		public float m_lat;

		public float m_lng;

		public float m_t;

		public void Reset() { m_lat = m_lng = m_t = 0.0f; }
	}

	private Click m_click;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private MapCursor m_cursor = new MapCursor();

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void Init()
	{
		o.globeHandpick.Setup( o.cameraGlobe, new Sphere(), null, GPS.Coord.GetLatitudeFrom3DCoord, GPS.Coord.GetLongitudeFrom3DCoord );

		m_cursor.Setup( "Earth", "Globe.Cursor.Spot", 64, 0.04f, "Materials/Band" );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void Enter()
	{
		m_click.Reset();

		o.globeHandpick.Enable( true );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void Exit()
	{
		m_click.Reset();

		o.globeHandpick.Enable( false );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateCoordSelection()
	{
		o.globeHandpick.Update();

		m_cursor.Update       ( o.globeHandpick.Latitude, o.globeHandpick.Longitude, o.globeHandpick.Point );

		o.UpdateCoordSelection( o.globeHandpick.Latitude, o.globeHandpick.Longitude );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool MouseClick()
	{
		if(	Input.GetMouseButtonDown( 0 ) )
		{
			m_click.m_lat = o.globeHandpick.Latitude.deg;

			m_click.m_lng = o.globeHandpick.Longitude.deg;

			m_click.m_t   = Time.time;
		}
		else if( Input.GetMouseButtonUp( 0 ) )
		{
			float diffLat  = Mathf.Abs( m_click.m_lat - o.globeHandpick.Latitude.deg  );

			float diffLng  = Mathf.Abs( m_click.m_lng - o.globeHandpick.Longitude.deg );

			float diffTime = Time.time - m_click.m_t;

			bool click     = ( diffLat < 0.1f ) && ( diffLng < 0.1f ) && ( diffTime < 0.25f );

			if( click )
			{
				if( o.globeHandpick.IsValid )
				{
					o.StartTransition( o.globeHandpick.Latitude.deg, o.globeHandpick.Longitude.deg, o.coords.cur.altitude );
				}
			}

			m_click.m_t = 0.0f;

			return click;
		}

		return false;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool MouseDrag()
	{
		Vector3 sensivity   = new Vector3( 2.0f, 2.0f, 2.0f ); 

		Vector3 deltas      = new Vector3( 0.0f, 0.0f, 0.0f );
		
		bool    mouseButton = ( Input.GetMouseButton( 0 ) == true );


		deltas.x = mouseButton ? ( sensivity.x * Input.GetAxis( "Mouse X" ) ) : 0.0f;

		deltas.y = mouseButton ? ( sensivity.y * Input.GetAxis( "Mouse Y" ) ) : 0.0f;
		
		deltas.z = sensivity.z * ( Input.GetAxis( "Mouse ScrollWheel" ) );


		if( ( deltas.x != 0.0f ) || ( deltas.y != 0.0f ) || ( deltas.z != 0.0f ) )
		{
			o.TeleportTo( o.coords.cur.latitude.deg - deltas.y, o.coords.cur.longitude.deg - deltas.x, o.coords.cur.altitude - deltas.z );

			return true;
		}

		return false;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void Update ()
	{
		if( Focusable.any )
		{
			return;
		}

		UpdateCoordSelection();

		if( MouseClick() == false )
		{
			if( MouseDrag() )
			{
				return;
			}
		}

		if( Input.anyKey )
		{
			MapCoord coords = o.coords.req;

			if     ( Input.GetKeyDown( KeyCode.KeypadPlus  ) ) o.StartTransition( coords.latitude.deg,			coords.longitude.deg,			coords.altitude  - 1.0f );

			else if( Input.GetKeyDown( KeyCode.KeypadMinus ) ) o.StartTransition( coords.latitude.deg,			coords.longitude.deg,			coords.altitude  + 1.0f );

			if     ( Input.GetKeyDown( KeyCode.UpArrow     ) ) o.StartTransition( coords.latitude.deg + 45.0f,	coords.longitude.deg,			coords.altitude );

			else if( Input.GetKeyDown( KeyCode.DownArrow   ) ) o.StartTransition( coords.latitude.deg - 45.0f,	coords.longitude.deg,			coords.altitude );

			if     ( Input.GetKeyDown( KeyCode.RightArrow  ) ) o.StartTransition( coords.latitude.deg,			coords.longitude.deg + 45.0f,	coords.altitude );

			else if( Input.GetKeyDown( KeyCode.LeftArrow   ) ) o.StartTransition( coords.latitude.deg,			coords.longitude.deg - 45.0f,	coords.altitude );
		}
	}
}
