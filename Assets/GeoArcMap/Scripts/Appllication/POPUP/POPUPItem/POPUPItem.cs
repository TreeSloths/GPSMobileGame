using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class POPUPItem : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum BUTON { DELETE, CANCEL, OK }

	public delegate void OnButon ( POPUPItem popup, BUTON buton );

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private const float FADE_IN_DURATION = 0.3f;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private POPUPItem m_instance = null;

	static public  POPUPItem Instance { get { return m_instance; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private CanvasGroup					m_group       = null;

	private GameObject                  m_content     = null;

	private UnityEngine.UI.Text         m_title       = null;

	private UnityEngine.UI.InputField	m_name        = null;

	private UnityEngine.UI.InputField	m_lat         = null;

	private UnityEngine.UI.InputField	m_lng         = null;

	private UnityEngine.UI.InputField	m_desc        = null;

	private UnityEngine.UI.Dropdown     m_sites       = null;

	private UnityEngine.UI.RawImage	    m_pic         = null;

	private CORE.UIButton               m_DELETE      = null;

	private CORE.UIButton               m_CANCEL      = null;

	private CORE.UIButton               m_OK          = null;

	private CORE.Fade					m_fade        = new CORE.Fade();

	private Localizable		            m_target      = null;

	private OnButon                     m_onButon     = null;

	private Texture                     m_picNotFound = null;

	private string                      m_pictureName = null;

	private HTTPReq                     m_lastPicReq  = null;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public string			title    { get { return ( m_title != null ) ? m_title.text : string.Empty; } set { if( m_title != null ) { m_title.text = value; } } }

	public string			itemName { get { return ( m_name  != null ) ? m_name.text  : string.Empty; } set { if( m_name  != null ) { m_name.text  = value; } } }

	public string			itemLat  { get { return ( m_lat   != null ) ? m_lat.text   : string.Empty; } set { if( m_lat   != null ) { m_lat.text   = value; } } }

	public string			itemLng  { get { return ( m_lng   != null ) ? m_lng.text   : string.Empty; } set { if( m_lng   != null ) { m_lng.text   = value; } } }

	public string			itemDesc { get { return ( m_desc  != null ) ? m_desc.text  : string.Empty; } set { if( m_desc  != null ) { m_desc.text  = value; } } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Awake()
	{
		if( m_instance == null ) m_instance = this;

		m_group         = GetComponent< CanvasGroup >();
		
		GameObject root = gameObject;

		GameObject buts = CORE.HIERARCHY.Find( root, "Buttons" );

		m_content       = CORE.HIERARCHY.Find( root, "Content" );

		m_title         = CORE.HIERARCHY.FindComp< UnityEngine.UI.Text       >( root,      "Title"     );

		m_name          = CORE.HIERARCHY.FindComp< UnityEngine.UI.InputField >( m_content, "Name"      );

		m_lat           = CORE.HIERARCHY.FindComp< UnityEngine.UI.InputField >( m_content, "Lat"       );

		m_lng           = CORE.HIERARCHY.FindComp< UnityEngine.UI.InputField >( m_content, "Lng"       );

		m_desc          = CORE.HIERARCHY.FindComp< UnityEngine.UI.InputField >( m_content, "Desc"      );

		m_sites         = CORE.HIERARCHY.FindComp< UnityEngine.UI.Dropdown   >( m_content, "Site"      );

		m_pic           = CORE.HIERARCHY.FindComp< UnityEngine.UI.RawImage   >( m_content, "Picture"   );

		m_CANCEL        = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( buts, "CANCEL" ) );

		m_DELETE        = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( buts, "DELETE" ) );

		m_OK            = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( buts, "OK"     ) );

		m_picNotFound   = Resources.Load< Texture >( "2D/UI/picture_not_found" );


		m_CANCEL.SetListener( delegate { OnCANCEL(); } );

		m_DELETE.SetListener( delegate { OnDELETE(); } );

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

	public void EnableButons( int selection )
	{
		if( m_DELETE != null ) m_DELETE.interactable = ( ( selection & ( 1 << ( int )BUTON.DELETE ) ) != 0 );

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
			EnableButons( ( 1 << ( int )BUTON.DELETE  ) | ( 1 << ( int )BUTON.CANCEL ) );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool Show( OnButon onButon, Localizable target )
	{
		if( ( gameObject.activeSelf == false ) && ( target != null ) )
		{
			m_onButon = onButon;

			BuildSitesList( target );

			ReflectTarget ( target );


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

	private void BuildSitesList( Localizable target )
	{
		if( m_sites == null ) return;


		Site site = ( target is Site ) ? target as Site : null;

		Item item = ( target is Item ) ? target as Item : null;

		m_sites.ClearOptions();

		m_sites.value        = 0;

		m_sites.interactable = ( item != null ) && ( DBObjects.instance != null );



		if( m_sites.interactable )
		{
			m_sites.options.Add( new UnityEngine.UI.Dropdown.OptionData( "NONE" ) );

			for( int siteIndex = 0; siteIndex < DBObjects.instance.sites.Count; ++siteIndex )
			{
				m_sites.options.Add( new UnityEngine.UI.Dropdown.OptionData( DBObjects.instance.sites[ siteIndex ].name ) );

				if( item.parentID == DBObjects.instance.sites[ siteIndex ].id )
				{
					m_sites.value = siteIndex + 1;
				}
			}

			m_sites.RefreshShownValue();
		}
		else
		{
			m_sites.captionText.text = ( site != null ) ? site.name : string.Empty;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void ReflectPicture( string picture )
	{
		m_pictureName = ( string.IsNullOrEmpty( picture ) == false ) && ( picture.CompareTo( "NONE" ) != 0 ) ? picture : string.Empty;

		if( m_pic != null )
		{
			if( string.IsNullOrEmpty( m_pictureName ) == false )
			{
				m_lastPicReq = new HTTPReq( "https://" + ApplicationMain.instance.options.DBInfos.server_adr + "/uploads/pictures/" + m_pictureName, HTTP_METHOD.GET, HTTP_CACHE_POLICY.IMAGE, m_picNotFound );

				m_lastPicReq.SubmitAsync( new HTTPReqDelegate( OnPicReqCompletion, null ) );
			}
			else
			{
				m_pic.texture = null;

				m_pic.enabled = false;
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnPicReqCompletion( HTTPReq req, params object[] paramsList )
	{
		if( ( m_lastPicReq == null ) || ( req.started >= m_lastPicReq.started ) )
		{
			m_lastPicReq = null; 


			bool  success = ( req.result == QUERY_RESULT.SUCCESS );

			m_pic.texture = CORE.UI.ClampTexture( success ? req.texture : m_picNotFound, FilterMode.Bilinear );

			m_pic.enabled = ( m_pic.texture != null );

			CORE.UI.Constrain( m_pic.rectTransform, m_pic.texture );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void ReflectTarget( Localizable target )
	{
		m_target  = target;

		title     = ( ( target is Site ) ? "SITE" : "ITEM" ) + " EDITION";

		itemName  = target.name;

		itemDesc  = target.m_desc;

		itemLat   = target.m_coord.latitude.As ( GPS.UNIT.DD ).ToString();

		itemLng   = target.m_coord.longitude.As( GPS.UNIT.DD ).ToString();

		ReflectPicture( target.m_pic );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void UpdateMapObject()
	{
		if( DBObjects.instance == null ) return;

		if( m_target           == null ) return;


		object session = DBObjects.instance.BeginEdit();

		if( session != null )
		{
			float LAT = 0.0f; float.TryParse( itemLat.TrimEnd( '°' ), out LAT );

			float LNG = 0.0f; float.TryParse( itemLng.TrimEnd( '°' ), out LNG );

			m_target.name   = itemName;	

			m_target.m_desc = itemDesc;

			m_target.m_pic  = m_pictureName;

			m_target.m_coord.latitude.FromAngle ( LAT, GPS.UNIT.DD );

			m_target.m_coord.longitude.FromAngle( LNG, GPS.UNIT.DD );


			if( m_sites != null )
			{
				Item item = ( m_target is Item ) ? m_target as Item : null;

				if( item != null )
				{
					if( ( m_sites.value > 0 ) && ( m_sites.value < ( DBObjects.instance.sites.Count + 1 ) ) )
					{
						long siteID = ( DBObjects.instance.sites[ m_sites.value - 1 ] as Site ).id;

						item.SetParent( siteID );
					}
					else
					{
						item.SetParent( DBObjects.instance.root );
					}
				}
			}


			m_target.Async_DBPush( null );

			DBObjects.instance.EndEdit( session );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void DeleteTarget()
	{
		if( DBObjects.instance == null ) return;

		if( m_target           == null ) return;


		object session = DBObjects.instance.BeginEdit();

		if( session != null )
		{
			DBObjects.instance.Delete( m_target, null, session );

			m_target = null;

			DBObjects.instance.EndEdit( session );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnPictureSELECT()
	{
		POPUPPicture.Instance.Show( OnPopupPictureButon );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnPopupPictureButon( POPUPPicture popup, POPUPPicture.ACTION action )
	{
		if( action == POPUPPicture.ACTION.SELECT )
		{
			if( string.IsNullOrEmpty( popup.picture ) == false )
			{
				ReflectPicture( popup.picture );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnEnable()  { if( m_content != null ) m_content.transform.localPosition = new Vector3( m_content.transform.localPosition.x, 0.0f , m_content.transform.localPosition.z ); }

	private void OnDisable() { m_fade.Begin( FADE_TYPE.FADE_OUT, 0.0f, null ); m_target = null; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public  void OnDELETE()  { DeleteTarget();    if( m_onButon != null ) { m_onButon( this, BUTON.DELETE ); m_onButon = null; } gameObject.SetActive( false ); }

	public  void OnCANCEL()  {                    if( m_onButon != null ) { m_onButon( this, BUTON.CANCEL ); m_onButon = null; } gameObject.SetActive( false ); }

	public  void OnOK()      { UpdateMapObject(); if( m_onButon != null ) { m_onButon( this, BUTON.OK     ); m_onButon = null; } gameObject.SetActive( false ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private bool ValidateContent()
	{
		if( ( m_name == null ) || ( m_name.text.Length <= 0 ) ) return false;

		if( ( m_lat == null  ) || ( m_lat.text.Length  <= 0 ) ) return false;

		if( ( m_lng == null  ) || ( m_lng.text.Length  <= 0 ) ) return false;

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
