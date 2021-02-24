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

	public static class GPS
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		public enum TYPE { LONGITUDE, LATITUDE }

		public enum UNIT { DD, DDM, DMS }

		//************************************************************************************************
		//
		//************************************************************************************************

		public class Coord
		{
			//********************************************************************************************
			//
			//********************************************************************************************

			private float m_deg  = 0.0f;

			private float m_min  = 0.0f;

			private float m_sec  = 0.0f;

			private TYPE  m_type = TYPE.LONGITUDE;

			private UNIT  m_unit = UNIT.DMS;

			//********************************************************************************************
			//
			//********************************************************************************************

			public Coord( TYPE type, float deg )							{ m_type = type; m_deg = deg; m_min = 0.0f; m_sec = 0.0f; m_unit = UNIT.DD;  }

			public Coord( TYPE type, float deg, float min )					{ m_type = type; m_deg = deg; m_min = min;  m_sec = 0.0f; m_unit = UNIT.DDM; }

			public Coord( TYPE type, float deg, float min, float sec )		{ m_type = type; m_deg = deg; m_min = min;  m_sec = sec;  m_unit = UNIT.DMS; }

			public Coord( TYPE type, float ang, UNIT unit )					{ m_type = type; FromAngle( ang, unit ); }

			public Coord( Coord coord )										{ m_type = coord.m_type; m_deg = coord.m_deg; m_min = coord.m_min; m_sec = coord.m_sec; m_unit = coord.m_unit; }

			//********************************************************************************************
			//
			//********************************************************************************************

			public void Set( TYPE type, float deg )							{ m_type = type; m_deg = deg; m_min = 0.0f; m_sec = 0.0f; m_unit = UNIT.DD;  }

			public void Set( TYPE type, float deg, float min )				{ m_type = type; m_deg = deg; m_min = min;  m_sec = 0.0f; m_unit = UNIT.DDM; }

			public void Set( TYPE type, float deg, float min, float sec )	{ m_type = type; m_deg = deg; m_min = min;  m_sec = sec;  m_unit = UNIT.DMS; }

			public void Set( TYPE type, float ang, UNIT unit )				{ m_type = type; FromAngle( ang, unit ); }

			public void Set( Coord coord )									{ m_type = coord.m_type; m_deg = coord.m_deg; m_min = coord.m_min; m_sec = coord.m_sec; m_unit = coord.m_unit; }

			//********************************************************************************************
			//
			//********************************************************************************************

			public float deg { get {  return m_deg; } }

			public float min { get {  return m_min; } }

			public float sec { get {  return m_sec; } }

			//********************************************************************************************
			//
			//********************************************************************************************

			public Coord FromAngle( float angle, UNIT unit )
			{
				switch( unit )
				{
					case UNIT.DD:
					{
						m_deg  = angle;

						m_min  = m_sec = 0.0f;

						break;
					}
					case UNIT.DDM:
					{
						m_deg  = ( int )angle;

						m_min  = ( angle - ( int )m_deg ) * 60.0f;

						m_sec  = 0.0f;

						break;
					}
					case UNIT.DMS:
					{
						m_deg  = ( int )angle;

						m_min  = ( angle - ( int )m_deg ) * 60.0f;

						m_sec  = ( m_min - ( int )m_min ) * 60.0f;

						m_min  = ( int )m_min;
					
						m_sec  = ( int )( m_sec * 100.0f ) * 0.01f;

						break;
					}
				}

				m_unit = unit;

				return   this;
			}

			//********************************************************************************************
			//
			//********************************************************************************************

			public Coord ToDD()
			{
				if( m_unit == UNIT.DD  ) return this;

				if( m_unit >= UNIT.DDM ) m_deg += ( m_min / 60.0f   );

				if( m_unit >= UNIT.DMS ) m_deg += ( m_sec / 3600.0f );

				m_min  = 0.0f;

				m_sec  = 0.0f;

				m_unit = UNIT.DD;

				return this;
			}

			//********************************************************************************************
			//
			//********************************************************************************************

			public Coord ConvertTo( UNIT unit )
			{
				if( m_unit != unit ) FromAngle( ToDD().m_deg, unit );

				return this;
			}

			//********************************************************************************************
			//
			//********************************************************************************************

			public Coord As( UNIT unit )
			{
				Coord coord = new Coord( this );

				return  coord.ConvertTo( unit );
			}

			//********************************************************************************************
			//
			//********************************************************************************************

			static public Coord GetLatitudeFrom3DCoord ( Vector3 p ) { return new Coord( TYPE.LATITUDE,  Mathf.Rad2Deg * Mathf.Asin(  p.y ) ); }

			static public Coord GetLongitudeFrom3DCoord( Vector3 p ) { return new Coord( TYPE.LONGITUDE, Mathf.Rad2Deg * Mathf.Acos( -p.z ) * Mathf.Sign( p.x ) ); }

			//********************************************************************************************
			//
			//********************************************************************************************

			public override string ToString() 
			{
				if( m_unit == UNIT.DMS ) return string.Format( "{0:F0}° {1:F0}' {2:F0}''", m_deg, Mathf.Abs( m_min ), Mathf.Abs( m_sec ) );

				if( m_unit == UNIT.DDM ) return string.Format( "{0:F0}° {1:F3}'",          m_deg, Mathf.Abs( m_min ) );

				                         return string.Format( "{0:F6}°",                  m_deg );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public class CONVERT
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		public static float Tanh ( float x ) { return ( float )System.Math.Tanh( x ); }

		public static float ATanh( float x ) { return 0.5f * ( Mathf.Log( 1 + x ) - Mathf.Log( 1 - x ) ); }

		//************************************************************************************************
		//
		//************************************************************************************************

		static public Vector2 LambertToLngLat( Vector2 lamberts )
		{
			float       x  = lamberts.x;

			float       y  = lamberts.y;

			const float e  = 0.0818191910428158f;	// first ellispsoid eccentric

			const float c  = 11754255.426096f;		// projection constant

			const float n  = 0.725607765053267f;	// projection exponent

			const float xs = 700000f;				// projection coordinate at the pole

			const float ys = 12655612.049876f;		// projection coordinate at the pole


			float dx2 = Mathf.Pow ( ( x - xs ), 2 );

			float dy2 = Mathf.Pow ( ( y - ys ), 2 );

			float D   = Mathf.Sqrt( dx2 + dy2 );

			float a   = Mathf.Log ( c / D ) / n;

			float E   = e * Mathf.Sin( 1.0f );

			      E   = Tanh( a + e * ATanh( E ) );

			      E   = Tanh( a + e * ATanh( E ) );

			      E   = Tanh( a + e * ATanh( E ) );

			      E   = Tanh( a + e * ATanh( E ) );

			      E   = Tanh( a + e * ATanh( E ) );

			      E   = Tanh( a + e * ATanh( E ) );

			      E   = Tanh( a + e * ATanh( E ) );

			float lat = ( Mathf.Asin( E ) * 180.0f ) / Mathf.PI;

			float lng = ( ( Mathf.Atan( -( x - xs ) / ( y - ys ) ) ) / n + 3 / 180.0f * Mathf.PI ) / Mathf.PI * 180.0f;

			return new Vector2( lng, lat );
		}
	}
}
