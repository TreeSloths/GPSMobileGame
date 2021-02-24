using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public enum FADE_TYPE { FADE_IN, FADE_OUT }

//********************************************************************************************************
//
//********************************************************************************************************

namespace CORE
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class Fade
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		public delegate void OnFadeFinished( FADE_TYPE type );

		//************************************************************************************************
		//
		//************************************************************************************************

		private float          m_duration   = 1.0f;

		private float          m_start      = 0.0f;

		private float          m_progress   = 1.0f;

		private FADE_TYPE      m_type       = FADE_TYPE.FADE_OUT;

		private OnFadeFinished m_evtHandler = null;

		//************************************************************************************************
		//
		//************************************************************************************************

		public  FADE_TYPE	type            { get { return m_type;             } }

		public  float		progress        { get { return m_progress;         } }

		public  bool		finished        { get { return m_progress >= 1.0f; } }

		public  bool		finishedFadeOut { get { return ( m_type == FADE_TYPE.FADE_OUT ) && ( m_progress >= 1.0f ); } }

		public  bool		finishedFadeIn  { get { return ( m_type == FADE_TYPE.FADE_IN  ) && ( m_progress >= 1.0f ); } }

		public float        alpha           { get { return ( m_type == FADE_TYPE.FADE_IN ) ? ( 1.0f - m_progress ) : m_progress; } }

		//************************************************************************************************
		//
		//************************************************************************************************

		public bool Begin( FADE_TYPE type, float duration, OnFadeFinished evtHandler = null )
		{
			if( m_type != type )
			{
				m_type       = type;

				m_start      = Time.time;

				m_progress   = ( duration > 0.0f ) ? 1.0f - m_progress : 1.0f;

				m_duration   = duration;

				m_evtHandler = evtHandler;

				return true;
			}
			else 
			{
				if( duration < m_duration )
				{
					m_progress   = ( duration > 0.0f ) ? m_progress * ( m_duration / duration ) : 1.0f;

					m_duration   = duration;

					m_evtHandler = evtHandler;

					return true;
				}
			}

			return false;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public bool Cut( OnFadeFinished evtHandler = null )
		{
			return Begin( m_type, 0.0f, evtHandler );
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public void Update()
		{
			if( m_progress < 1.0f  )
			{
				m_progress = ( m_duration > 0.0f ) ? CORE.Math.Clamp( ( Time.time - m_start ) / m_duration ) : 1.0f;

				if( m_progress >= 1.0f )
				{
					if( m_evtHandler != null ) m_evtHandler( m_type );
				}
			}
		}
	}
}
