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

    public static class Alignement 
    {
	    public static int  Align       ( int value, int align ) { return ( ( ( value ) + ( ( align ) - 1 ) ) & ( ~( ( align ) - 1 ) ) ); }

        public static bool IsAligned   ( int value, int align ) { return ( ( ( value ) & ~( ( align ) - 1 ) ) == 0 ); }

        public static bool IsPowerOfTwo( int value )            { return( ( value & ( value - 1 ) ) == 0 ); }
    }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public class Math
	{
		static public float Clamp( float value, float min, float max ) { return Mathf.Max( min,  Mathf.Min(  max, value ) ); }

		static public float Clamp( float value )                       { return Mathf.Max( 0.0f, Mathf.Min( 1.0f, value ) ); }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class Angle
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		public enum UNIT { RAD, DEG }

		public enum NORM { POS, NEG }

		private delegate float NormalizeDelegate( float angle, NORM norm, GPS.TYPE type = GPS.TYPE.LONGITUDE );

		private static NormalizeDelegate[] NormalizeFunc = new NormalizeDelegate[ 2 ] { NormalizeRad, NormalizeDeg };

		//************************************************************************************************
		//
		//************************************************************************************************

		public const float PI_DIV2 = Mathf.PI * 0.5f;

		public const float PI      = Mathf.PI;

		public const float PI_34   = Mathf.PI * 1.5f;

		public const float PI_MUL2 = Mathf.PI * 2.0f;

		//************************************************************************************************
		//
		//************************************************************************************************

		private float m_value;

		private UNIT  m_unit;

		//************************************************************************************************
		//
		//************************************************************************************************

		public       Angle ( float value, UNIT unit ) { m_value = value; m_unit = unit; }

		public       Angle ( Angle angle ) { m_value = angle.m_value; m_unit = angle.m_unit; }

		public float As    ( UNIT  unit  ) { if( m_unit != unit ) { return ( m_unit == UNIT.RAD ) ? ( m_value * Mathf.Rad2Deg ) : ( m_value * Mathf.Rad2Deg ); } return m_value; }

		public float Value { get { return m_value;        } set { m_value = value;                    } }

		public float Rad   { get { return As( UNIT.RAD ); } set { m_value = value; m_unit = UNIT.RAD; } }

		public float Deg   { get { return As( UNIT.DEG ); } set { m_value = value; m_unit = UNIT.DEG; } }

		//************************************************************************************************
		//
		//************************************************************************************************

		static public float NormalizeDeg( float angle, NORM norm, GPS.TYPE type = GPS.TYPE.LONGITUDE )
		{
			int  revs = ( int ) ( angle / 360.0f );

			angle    -= ( ( float )revs * 360.0f );

			if( angle < 0.0f )   angle += 360.0f;


			if( ( norm == NORM.NEG ) && ( angle > 180.0f ) ) angle -= 360.0f;

			if( type == GPS.TYPE.LATITUDE )
			{
				if     ( angle > 270.0f ) angle = -360.0f + angle;

				else if( angle > 180.0f ) angle = -180.0f + angle;

				else if( angle >  90.0f ) angle =  180.0f - angle;

				else if( angle < -90.0f ) angle = -180.0f - angle;
			}

			return angle;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static public float NormalizeRad( float angle, NORM norm, GPS.TYPE type = GPS.TYPE.LONGITUDE )
		{
			int  revs = ( int ) ( angle / PI_MUL2 );

			angle    -= ( ( float )revs * PI_MUL2 );

			if( angle < 0.0f )   angle += PI_MUL2;


			if( ( norm == NORM.NEG ) && ( angle > PI ) ) angle -= PI_MUL2;

			if( type == GPS.TYPE.LATITUDE )
			{
				if     ( angle > PI_34    ) angle = -PI_MUL2 + angle;

				else if( angle > PI       ) angle = -PI      + angle;

				else if( angle >  PI_DIV2 ) angle =  PI      - angle;

				else if( angle < -PI_DIV2 ) angle = -PI      - angle;
			}

			return angle;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static public float Normalize( float angle, UNIT unit, NORM norm, GPS.TYPE type = GPS.TYPE.LONGITUDE )
		{
			return NormalizeFunc[ ( int )unit ]( angle, norm, type );
		}
	}
}

