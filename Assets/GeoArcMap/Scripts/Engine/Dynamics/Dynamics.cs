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

	public class TransitionFunc
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		public delegate float Func    ( float t );

		static public   float Identity( float t ) { return t; }

		//************************************************************************************************
		//
		//************************************************************************************************

		private Vector2 m_interval = new Vector2( 0.0f, 1.0f );

		private Vector2 m_domain   = new Vector2( 0.0f, 1.0f );

		private Func    m_func     = Identity;

		//************************************************************************************************
		//
		//************************************************************************************************

		public TransitionFunc( Vector2 interval, Func func )
		{
			m_func     = ( func != null ) ? func : Identity;

			m_interval = interval;

			m_domain.x = m_func( m_interval.x );

			m_domain.y = m_func( m_interval.y );
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public float Execute( float time )
		{
			if( m_domain.y > m_domain.x )
			{
				float  t = CORE.Math.Clamp( time );

				float  x = m_interval.x + ( t * ( m_interval.y - m_interval.x ) );

				float  y = ( m_func( x ) - m_domain.x ) / ( m_domain.y - m_domain.x );

				return y;
			}

			return 1.0f;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class Transition
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		static public float FuncLog   ( float x ) { return Mathf.Log( x ); }

		static public float FuncInvSqr( float x ) { return 1.0f - ( 1.0f / ( Mathf.Pow( x, 2 ) ) ); }

		//************************************************************************************************
		//
		//************************************************************************************************

		public enum TYPE { LOG, INV_SQR }

		static private TransitionFunc   m_log   = new TransitionFunc( new Vector2(  1.0e-2f, 10.0f ), FuncLog    );

		static private TransitionFunc   m_isq   = new TransitionFunc( new Vector2(    1.0f,   7.0f ), FuncInvSqr );

		static private TransitionFunc[] m_funcs = new TransitionFunc[] { m_log, m_isq };

		//************************************************************************************************
		//
		//************************************************************************************************

		private TYPE  m_type     = TYPE.LOG;

		private float m_duration = 0.0f;

		private float m_time     = 0.0f;

		private float m_prc      = 1.0f;

		//************************************************************************************************
		//
		//************************************************************************************************

		public void Start( TYPE type, float duration )
		{
			m_type     = type;

			m_duration = ( duration > 0.0f ) ? duration : 0.0f;

			m_prc      = ( duration > 0.0f ) ? 0.0f     : 1.0f;

			m_time     = 0.0f;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public float Update()
		{
			if( m_time < m_duration )
			{
				m_time = Mathf.Min( m_time + Time.deltaTime, m_duration );

				m_prc  = CORE.Math.Clamp( m_funcs[ ( int )m_type ].Execute( m_time / m_duration ) );
			}

			return m_prc;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class FixedTimeDamp
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		public enum TYPE { VALUE, ANGLE }

		//************************************************************************************************
		//
		//************************************************************************************************

		Transition m_transition = new Transition();

		TYPE       m_type       = TYPE.VALUE;

		float      m_elasticity = 0.0f;

		float	   m_refValue   = 0.0f;

		float	   m_trgValue   = 0.0f;

		float	   m_curValue   = 0.0f;

		//************************************************************************************************
		//
		//************************************************************************************************

		public FixedTimeDamp( TYPE type, float value, float elasticity )
		{
			m_type       = type;

			m_elasticity = CORE.Math.Clamp( elasticity, 0.0f, ( type == TYPE.VALUE ) ? float.MaxValue : 180.0f );

			m_refValue   = value;

			m_trgValue   = value;

			m_curValue   = value;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public float Update( float trgValue )
		{
			if( m_type == TYPE.VALUE ) return UpdateValue( trgValue );

			else                       return UpdateAngle( trgValue );
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public void Reset( float trgValue )
		{
			m_refValue   = trgValue;

			m_trgValue   = trgValue;

			m_curValue   = trgValue;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public float UpdateValue( float trgValue )
		{
			if( m_trgValue != trgValue )
			{
				float diff  = trgValue - m_curValue;

				m_refValue  = ( Mathf.Abs( diff ) > m_elasticity ) ? trgValue - ( Mathf.Sign( diff ) * m_elasticity ) : m_curValue;

				m_trgValue  = trgValue;

				m_curValue  = m_refValue;

				if( m_elasticity > 0.0f ) m_transition.Start( Transition.TYPE.INV_SQR, Mathf.Abs( diff ) / m_elasticity );
			}

			if( m_curValue != m_trgValue )
			{
				m_curValue  = m_refValue + ( m_transition.Update() * ( m_trgValue - m_refValue ) );
			}

			return m_curValue;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public float UpdateAngle( float trgValue )
		{
			if( m_trgValue != trgValue )
			{
				float diff  = ( trgValue - m_curValue );

				float shrt  = ( diff > 180.0f ) ? ( diff - 360.0f ) : ( ( diff < -180.0f ) ? ( diff + 360.0f ) : diff );

				m_refValue  = ( Mathf.Abs( shrt ) > m_elasticity ) ? trgValue - ( Mathf.Sign( shrt ) * m_elasticity ) : m_curValue;

				m_refValue  = CORE.Angle.Normalize( m_refValue, Angle.UNIT.DEG, Angle.NORM.NEG );

				m_trgValue  = trgValue;

				m_curValue  = m_refValue;

				if( m_elasticity > 0.0f ) m_transition.Start( Transition.TYPE.INV_SQR, Mathf.Abs( shrt ) / m_elasticity );
			}

			if( m_curValue != m_trgValue )
			{
				float diff  = ( m_trgValue - m_refValue );

				float shrt  = ( diff > 180.0f ) ? ( diff - 360.0f ) : ( ( diff < -180.0f ) ? ( diff + 360.0f ) : diff );

				m_curValue  = CORE.Angle.Normalize( m_refValue + ( m_transition.Update() * shrt ), Angle.UNIT.DEG, Angle.NORM.NEG );
			}

			return m_curValue;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public static class Damp
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		static public float Value( float cur, float trg, float speedFactor = 1.0f )
		{
			if( cur != trg )
			{
				float diff = trg - cur;

				float abs  = Mathf.Abs  ( diff );

				float sign = Mathf.Sign ( diff );

				float min  = sign;

				float max  = diff * 10.0f;

				float vel  = Mathf.Clamp( sign * 200.0f * Mathf.Pow( abs, 3 ), ( sign > 0.0f ) ? min : max, ( sign > 0.0f ) ? max : min );


				cur = cur + ( vel * speedFactor * Time.deltaTime );

				if     ( ( diff < 0 ) && ( cur < trg ) ) cur = trg;

				else if( ( diff > 0 ) && ( cur > trg ) ) cur = trg;
			}

			return cur;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static public float Angle( float cur, float trg )
		{
			if( cur != trg )
			{
				float diff = trg - cur;

				float shrt = ( diff > 180.0f ) ? ( diff - 360.0f ) : ( ( diff <= -180.0f ) ? ( diff + 360.0f ) : diff );

				float abs  = Mathf.Abs  ( shrt );

				float sign = Mathf.Sign ( shrt );

				float min  = sign;

				float max  = shrt * 10.0f;

				float vel  = Mathf.Clamp( sign * 200.0f * Mathf.Pow( abs, 3 ), ( sign > 0.0f ) ? min : max, ( sign > 0.0f ) ? max : min );


				cur = cur + ( vel * Time.deltaTime );

				if     ( ( diff < 0 ) && ( cur < trg ) ) cur = trg;

				else if( ( diff > 0 ) && ( cur > trg ) ) cur = trg;

				cur = CORE.Angle.Normalize( cur, CORE.Angle.UNIT.DEG, CORE.Angle.NORM.NEG );
			}

			return cur;
		}
	}
}