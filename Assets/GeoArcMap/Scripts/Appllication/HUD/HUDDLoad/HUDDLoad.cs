using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class HUDDLoad : MonoBehaviour, IProgressIndicator
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	private const float MAX_INACTIVITY_DURATION = 1.0f;

	private const float FADE_IN_DURATION        = 0.5f;

	private const float FADE_OUT_DURATION       = 0.5f;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private HUDDLoad m_instance = null;

	static public  HUDDLoad Instance { get { return m_instance; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private CanvasGroup             m_group        = null;

	private UnityEngine.UI.Text     m_text         = null;

	private UnityEngine.UI.RawImage m_bar          = null;

	private RectTransform           m_barTransform = null;

	private float                   m_touchTime    = 0.0f;

	private CORE.Fade				m_fade         = new CORE.Fade();

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Awake()
	{
		if( m_instance == null ) m_instance = this;

		m_group         = GetComponent< CanvasGroup >();

		GameObject text = CORE.HIERARCHY.Find( gameObject, "HUDDLoadText" );

		GameObject bar  = CORE.HIERARCHY.Find( gameObject, "HUDDLoadBar"  );

		if( text  != null ) m_text = text.GetComponent< UnityEngine.UI.Text    >();

		if( bar   != null ) m_bar  = bar.GetComponent< UnityEngine.UI.RawImage >();

		if( m_bar != null ) m_barTransform = m_bar.GetComponent< RectTransform >();

		m_fade.Begin( FADE_TYPE.FADE_OUT, FADE_OUT_DURATION );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnDestroy() { if( m_instance == this ) m_instance = null; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public string text
	{
		get { return ( m_text != null ) ? m_text.text : string.Empty; }

		set
		{
			if( m_text != null )
			{
				m_text.text = value;

				m_touchTime = Time.time;

				m_fade.Begin( FADE_TYPE.FADE_IN, FADE_IN_DURATION );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public float prc
	{
		get { return ( m_barTransform != null ) ? m_barTransform.localScale.x : 0.0f; }

		set
		{
			if( m_barTransform != null )
			{
				Vector3 scale = m_barTransform.localScale;

				scale.x = Mathf.Clamp( value, 0.0f, 1.0f );

				m_barTransform.localScale = scale;

				m_touchTime = Time.time;

				m_fade.Begin( FADE_TYPE.FADE_IN, FADE_IN_DURATION );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnEnable () { m_fade.Begin( FADE_TYPE.FADE_IN,  FADE_IN_DURATION,  null ); }

	public void OnDisable() { m_fade.Begin( FADE_TYPE.FADE_OUT, FADE_OUT_DURATION, null ); }

	public void Start    () { m_fade.Begin( FADE_TYPE.FADE_OUT, 0.0f ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void Update()
	{
		m_fade.Update();

		if( ( prc >= 1.0f ) && ( Time.time - m_touchTime ) > MAX_INACTIVITY_DURATION )
		{
			m_fade.Begin( FADE_TYPE.FADE_OUT, FADE_OUT_DURATION );
		}

		if( m_group != null )
		{
			m_group.alpha = ( 1.0f - m_fade.alpha );
		}
	}
}
