using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class POPUPOptions : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum BUTON  { RESET, CANCEL, OK }

	public delegate void OnButon ( POPUPOptions popup, BUTON buton );

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private const float FADE_IN_DURATION = 0.3f;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private POPUPOptions m_instance = null;

	static public  POPUPOptions Instance { get { return m_instance; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private CanvasGroup					m_group       = null;

	private UnityEngine.UI.Text         m_title       = null;

	private UnityEngine.UI.InputField	m_server      = null;

	private CORE.UIButton				m_RESET       = null;

	private CORE.UIButton				m_CANCEL      = null;

	private CORE.UIButton				m_OK          = null;

	private CORE.Fade					m_fade        = new CORE.Fade();

	private Options					    m_options     = null;

	private OnButon                     m_onButon     = null;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Options options { get { return m_options; } }

	public string  title   { get { return ( m_title  != null ) ? m_title.text    : string.Empty; } set { if( m_title  != null ) { m_title.text  = value; } } }

	public string  server  { get { return ( m_server != null ) ? m_server.text   : string.Empty; } set { if( m_server != null ) { m_server.text = value; } } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Awake()
	{
		if( m_instance == null ) m_instance = this;

		m_group         = GetComponent< CanvasGroup >();
		
		GameObject root = GameObject.Find( "POPUPOptions" );

		GameObject buts = CORE.HIERARCHY.Find( root, "Butons" );

		m_title         = CORE.HIERARCHY.FindComp< UnityEngine.UI.Text       >( root, "Title"     );

		m_server        = CORE.HIERARCHY.FindComp< UnityEngine.UI.InputField >( root, "ServerAdr" );

		m_RESET         = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( buts, "RESET"  ) );

		m_CANCEL        = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( buts, "CANCEL" ) );

		m_OK            = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( buts, "OK"     ) );


		m_RESET.SetListener ( delegate { OnRESET();  } );

		m_CANCEL.SetListener( delegate { OnCANCEL(); } );

		m_OK.SetListener    ( delegate { OnOK();     } );


		if( m_title != null ) m_title.text = "OPTIONS";

		m_fade.Begin( FADE_TYPE.FADE_OUT, 0.0f );

		gameObject.SetActive( false );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnDestroy() { if( m_instance == this ) m_instance = null; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void EnableButons( int selection )
	{
		if( m_RESET  != null ) m_RESET.interactable  = ( ( selection & ( 1 << ( int )BUTON.RESET  ) ) != 0 );

		if( m_CANCEL != null ) m_CANCEL.interactable = ( ( selection & ( 1 << ( int )BUTON.CANCEL ) ) != 0 );

		if( m_OK     != null ) m_OK.interactable     = ( ( selection & ( 1 << ( int )BUTON.OK     ) ) != 0 );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnFadeFinished( FADE_TYPE type )
	{
		if( type == FADE_TYPE.FADE_IN )
		{
			EnableButons( ( 1 << ( int )BUTON.RESET  ) | ( 1 << ( int )BUTON.CANCEL ) );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool Show( OnButon onButon, Options options )
	{
		if( ( gameObject.activeSelf == false ) && ( options != null ) )
		{
			m_onButon = onButon;

			ReflectOptions( options );


			m_fade.Begin( FADE_TYPE.FADE_IN, FADE_IN_DURATION, OnFadeFinished );

			Update();


			gameObject.SetActive( true );

			EnableButons( 0 );

			return true;
		}

		return false;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void ReflectOptions( Options opts )
	{
		if( ReferenceEquals( m_options, opts ) == false )
		{
			m_options = ( opts != null ) ? opts.byval : new Options();
		}

		server = m_options.DBInfos.server_adr;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void UpdateOptions()
	{
		m_options.DBInfos.server_adr = server;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void ResetOptions()
	{
		m_options.ResetToDefault();

		ReflectOptions( m_options );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnEnable () {}

	private void OnDisable() { m_fade.Begin( FADE_TYPE.FADE_OUT, 0.0f, null ); m_options = null; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public  void OnColorSelect() { POPUPColor.Instance.Show( OnPOPUPColorButon, m_options.m_colorBgnds ); }

	public  void OnRESET()       { ResetOptions();  if( m_onButon != null ) { m_onButon( this, BUTON.RESET  ); } }

	public  void OnCANCEL()      {                  if( m_onButon != null ) { m_onButon( this, BUTON.CANCEL ); m_onButon = null; } gameObject.SetActive( false ); }

	public  void OnOK()          { UpdateOptions(); if( m_onButon != null ) { m_onButon( this, BUTON.OK     ); m_onButon = null; } gameObject.SetActive( false ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnPOPUPColorButon( POPUPColor popup, POPUPColor.BUTON buton )
	{
		if( buton == POPUPColor.BUTON.OK )
		{
			if( m_options != null ) m_options.m_colorBgnds = popup.color;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private bool ValidateContent()
	{
		if( ( m_server == null ) || ( m_server.text.Length <= 0 ) )
		{
			// Nobody will have a server up and running after they downloaded this free application exemple

			// return false;
		}

		return true;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void Update()
	{
		m_fade.Update();

		if( m_group != null ) { if( ( m_group.alpha = ( 1.0f - m_fade.alpha ) ) < 1.0f ) return; }

		if( m_OK    != null ) m_OK.interactable = ValidateContent();
	}
}
