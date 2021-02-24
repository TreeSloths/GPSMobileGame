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

	public static class EARTH
	{
		public const float RADIUS_POLE = 12713510f * 0.5f;

		public const float RADIUS_EQUT = 12756280f * 0.5f;

		public const float RADIUS      = ( RADIUS_POLE + RADIUS_EQUT ) * 0.5f;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class MapCoord
	{
		private float     m_altitude  = 0.0f;

		private GPS.Coord m_latitude  = new GPS.Coord( GPS.TYPE.LATITUDE,  0.0f );

		private GPS.Coord m_longitude = new GPS.Coord( GPS.TYPE.LONGITUDE, 0.0f );


		public  float     altitude  { get { return m_altitude;  } set { m_altitude = value;       } }

		public  GPS.Coord latitude  { get { return m_latitude;  } set { m_latitude.Set ( value ); } }

		public  GPS.Coord longitude { get { return m_longitude; } set { m_longitude.Set( value ); } }


		public bool Equals( MapCoord coord )
		{
			if( altitude  != coord.altitude  )                 return false;

			if( latitude.Equals ( coord.latitude  ) == false ) return false;

			if( longitude.Equals( coord.longitude ) == false ) return false;

			return true;
		}

		public void Set( MapCoord coord )
		{
			altitude  = coord.altitude;

			latitude  = coord.latitude;

			longitude = coord.longitude;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class MapCoords
	{
		private MapCoord m_cur = new MapCoord();

		private MapCoord m_req = new MapCoord();


		public  MapCoord cur { get { return m_cur; } set { m_cur.Set( value ); } }

		public  MapCoord req { get { return m_req; } set { m_req.Set( value ); } }


		public void SyncOnReq() { cur.Set( req ); }

		public void SyncOnCur() { req.Set( cur ); }

		public bool IsSync   () { return cur.Equals( req ); }
	}
}
