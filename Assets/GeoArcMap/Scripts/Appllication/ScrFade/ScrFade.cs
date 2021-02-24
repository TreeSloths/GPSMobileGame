using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class ScrFade : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private UnityEngine.UI.RawImage m_img   = null;

	static private CORE.Fade               m_fade  = new CORE.Fade();

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private ScrFade		m_instance = null;

	static public  ScrFade		Instance        { get { return m_instance;             } }

	static public  FADE_TYPE	type			{ get { return m_fade.type;            } }

	static public  bool			finished        { get { return m_fade.finished;        } }

	static public  bool			finishedFadeOut { get { return m_fade.finishedFadeOut; } }

	static public  bool			finishedFadeIn  { get { return m_fade.finishedFadeIn;  } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public bool Begin( FADE_TYPE type, float duration = 1.0f, CORE.Fade.OnFadeFinished evtHandler = null )
	{
		if( m_fade.Begin( type, duration, evtHandler ) )
		{
			m_instance.enabled = true;

			m_instance.Update();

			return true;
		}

		return false;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public bool Cut( CORE.Fade.OnFadeFinished evtHandler = null )
	{
		return m_fade.Cut( evtHandler );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void Awake    () { if( m_instance == null ) m_instance = this; }

	private void OnDestroy() { if( m_instance == this ) m_instance = null; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void Start()
	{
		if( m_instance == this )
		{
			if( m_img == null )
			{
				GameObject o = GameObject.Find( "Fade" );

				m_img = ( o != null ) ? o.GetComponent< UnityEngine.UI.RawImage >() : null;
			}

			if( m_img != null )
			{
				m_img.color = new Color( 0.0f, 0.0f, 0.0f, ( type == FADE_TYPE.FADE_IN ) ? 1.0f : 0.0f );
			}

			Begin( FADE_TYPE.FADE_IN, 1.0f );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void Update()
	{
		m_fade.Update();

		if( m_img  !=  null ) m_img.color = new Color( 0.0f, 0.0f, 0.0f, m_fade.alpha );

		if( m_fade.finished ) enabled = false;
	}
}

