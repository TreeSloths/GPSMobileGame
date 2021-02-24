using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class POPUPCsvConfig : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum BUTON      { RESET, CANCEL, OK }

	public enum BUTON_FLAG { F_NONE = 0X0, F_CANCEL = 0X1, F_OK = 0X2, F_ALL = int.MaxValue }

	public delegate void OnButon ( POPUPCsvConfig popup, BUTON buton );

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private const float FADE_IN_DURATION = 0.3f;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private POPUPCsvConfig m_instance = null;

	static public  POPUPCsvConfig Instance { get { return m_instance; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public  CSV.Options                 m_options       = new CSV.Options();

	private string[]                    m_header        = null;

	private CanvasGroup					m_group         = null;

	private UnityEngine.UI.Dropdown     m_coordsSys     = null;

	private UnityEngine.UI.Dropdown[]   m_dropDowns     = new UnityEngine.UI.Dropdown[ ( int )CSV.COL.NB ];

	private CORE.UIButton               m_RESET         = null;

	private CORE.UIButton               m_CANCEL        = null;

	private CORE.UIButton               m_OK            = null;

	private CORE.Fade					m_fade          = new CORE.Fade();

	private OnButon                     m_onButon       = null;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Awake()
	{
		if( m_instance == null ) m_instance = this;

		m_group            = GetComponent< CanvasGroup >();

		GameObject root    = gameObject;
		
		GameObject content = CORE.HIERARCHY.Find( root, "Content" );

		GameObject buts    = CORE.HIERARCHY.Find( root, "Buttons" );

		m_coordsSys        = CORE.HIERARCHY.FindComp< UnityEngine.UI.Dropdown >( content, "COORDS_SYS" );

		m_RESET            = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( buts, "RESET"  ) );

		m_CANCEL           = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( buts, "CANCEL" ) );

		m_OK               = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( buts, "OK"     ) );



		if( m_coordsSys != null )
		{
			m_coordsSys.onValueChanged.RemoveAllListeners();

			m_coordsSys.onValueChanged.AddListener( delegate { OnValueChanged( m_coordsSys ); } );
		}

		for( int col = 0; col < ( int )CSV.COL.NB; ++col )
		{
			int dropDown = col;

			m_dropDowns[ col ] = CORE.HIERARCHY.FindComp< UnityEngine.UI.Dropdown >( content, ( ( CSV.COL )col ).ToString() );

			if( m_dropDowns[ col ] != null )
			{
				m_dropDowns[ col ].onValueChanged.RemoveAllListeners();

				m_dropDowns[ col ].onValueChanged.AddListener( delegate { OnValueChanged( dropDown ); } );
			}
		}


		m_RESET.SetListener ( delegate { OnRESET (); } );

		m_CANCEL.SetListener( delegate { OnCANCEL(); } );

		m_OK.SetListener    ( delegate { OnOK    (); } );


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

	public void EnableButons( BUTON_FLAG selection )
	{
		if( m_CANCEL != null ) m_CANCEL.interactable = ( ( selection & BUTON_FLAG.F_CANCEL ) != 0 );

		if( m_OK     != null ) m_OK.interactable     = ( ( selection & BUTON_FLAG.F_OK     ) != 0 );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnFadeFinished( FADE_TYPE type )
	{
		if( type == FADE_TYPE.FADE_IN )
		{
			EnableButons( BUTON_FLAG.F_CANCEL );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ConfigureDropDownLists()
	{
		if( m_coordsSys != null )
		{
			m_coordsSys.ClearOptions();

			m_coordsSys.options.Add( new UnityEngine.UI.Dropdown.OptionData( "WGS_84"     ) );

			m_coordsSys.options.Add( new UnityEngine.UI.Dropdown.OptionData( "LAMBERT_93" ) );

			m_coordsSys.value = 0;

			m_coordsSys.RefreshShownValue();
		}


		for( int row = 0; row < ( int )CSV.COL.NB; ++row )
		{
			UnityEngine.UI.Dropdown d = m_dropDowns[ row ];
			
			if( d == null ) continue;

			d.ClearOptions();

			d.options.Add( new UnityEngine.UI.Dropdown.OptionData( "SELECT" ) );

			for( int col = 0; col < m_header.Length; ++col ) { d.options.Add( new UnityEngine.UI.Dropdown.OptionData( m_header[ col ] ) ); }

			d.value = 0;

			d.RefreshShownValue();
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool Show( OnButon onButon, string[] header )
	{
		if( ( gameObject.activeSelf == false ) && ( header != null ) )
		{
			m_header        = header;

			m_onButon       = onButon;

			m_options.ResetToDefault();

			ConfigureDropDownLists  ();

			ReflectOptions          ();


			m_fade.Begin( FADE_TYPE.FADE_IN, FADE_IN_DURATION, OnFadeFinished );

			Update();


			gameObject.SetActive( true );

			EnableButons( BUTON_FLAG.F_NONE );

			return true;
		}

		return false;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void ReflectOptions()
	{
		if( m_coordsSys != null )
		{
			m_coordsSys.value = ( int )m_options.coords;

			m_coordsSys.RefreshShownValue();
		}


		for( int row = 0; row < ( int )CSV.COL.NB; ++row )
		{
			UnityEngine.UI.Dropdown d = m_dropDowns[ row ];
			
			if( d == null ) continue;

			d.value = ( row < m_header.Length ) ? ( m_options.cols[ row ] + 1 ) : 0;

			d.RefreshShownValue();
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void UpdateOptions()
	{
		m_options.coords = ( m_coordsSys != null ) ? ( CSV.COORDS )m_coordsSys.value : CSV.COORDS.WGS84;

		for( int col = 0; col < ( int )CSV.COL.NB; ++col )
		{
			m_options.cols[ col ] = ( m_dropDowns[ col ] != null ) ? m_dropDowns[ col ].value - 1 : -1;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void ResetOptions()
	{
		m_options.ResetToDefault();

		ReflectOptions();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnEnable()  { }

	private void OnDisable() { m_fade.Begin( FADE_TYPE.FADE_OUT, 0.0f, null ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public  void OnRESET()   { ResetOptions(); }

	public  void OnCANCEL()  { if( m_onButon != null ) { m_onButon( this, BUTON.CANCEL ); m_onButon = null; } gameObject.SetActive( false ); }

	public  void OnOK()      { if( m_onButon != null ) { m_onButon( this, BUTON.OK     ); m_onButon = null; } gameObject.SetActive( false ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnValueChanged( UnityEngine.UI.Dropdown dropDown )
	{
		m_options.coords = ( CSV.COORDS )dropDown.value;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnValueChanged( int dropDown )
	{
		m_options.cols[ dropDown ] = m_dropDowns[ dropDown ].value - 1;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool Validate()
	{
		if( m_options.cols[ ( int )CSV.COL.LONGITUDE ] == -1 ) return false;

		if( m_options.cols[ ( int )CSV.COL.LATITUDE ] == -1 ) return false;

		return true;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void Update()
	{
		m_fade.Update();

		if( m_group != null ) { if( ( m_group.alpha = ( 1.0f - m_fade.alpha ) ) < 1.0f ) return; }

		if( m_OK != null ) m_OK.interactable = Validate();
	}
}
