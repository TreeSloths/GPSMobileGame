using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class Options
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public const int    m_version = 0;  

	public Color        m_colorBgnds;

	public GPS.UNIT     m_coordUnit;

	private DBInfos     m_dbInfos    = new DBInfos();

	private CSV.Options m_CSVOptions = new CSV.Options();

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public  GPS.UNIT coordUnit { get { return m_coordUnit; } }

	public  DBInfos  DBInfos   { get { return m_dbInfos;   } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Options()
	{
		ResetToDefault();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ResetToDefault()
	{
		m_colorBgnds          = new Color( 0.0f, 0.0f, 0.0f, 1.0f );

		m_coordUnit           = GPS.UNIT.DMS;

		m_dbInfos.server_adr  = string.Empty;

		m_dbInfos.api_version = 2;

		m_CSVOptions.ResetToDefault();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Options byval
	{
		get
		{
			Options o = new Options();

			o.m_colorBgnds = m_colorBgnds;

			o.m_coordUnit  = m_coordUnit;

			o.m_dbInfos    = m_dbInfos.byval;

			o.m_CSVOptions = m_CSVOptions.byval;

			return o;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Load()
	{
		if( PlayerPrefs.HasKey( "version" ) == false )
		{
			ResetToDefault();

			Save();

			return;
		}

		m_colorBgnds       = COLOR.FromInt( PlayerPrefs.GetInt( "colorBGs"   ) );

		m_coordUnit        = ( GPS.UNIT )PlayerPrefs.GetInt   ( "coordUnit"  );

		DBInfos.server_adr = PlayerPrefs.GetString            ( "server_adr" );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Save()
	{
		PlayerPrefs.SetInt   ( "version",    m_version );

		PlayerPrefs.SetInt   ( "colorBGs",   COLOR.ToInt( m_colorBgnds ) );

		PlayerPrefs.SetInt   ( "coordUnit",  ( int )m_coordUnit );

		PlayerPrefs.SetString( "server_adr", DBInfos.server_adr );

		PlayerPrefs.Save();
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class ApplicationMain : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public const float MAP_SWITCH_DIST           = 1.25f;

	public const float MAP_SWITCH_DIST_THRESHOLD = 0.1f;

	public const float MAP_SWITCH_DIST_ENTER     = MAP_SWITCH_DIST - MAP_SWITCH_DIST_THRESHOLD;

	public const float MAP_SWITCH_DIST_EXIT      = MAP_SWITCH_DIST + MAP_SWITCH_DIST_THRESHOLD;

	public const float MIN_DIST                  = 500.0f / EARTH.RADIUS;

	public const float MAX_DIST                  = MAP_SWITCH_DIST + 1.0f;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private ApplicationMain m_instance = null;

	static public  ApplicationMain instance { get { return m_instance; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private Options m_options = new Options();

	public  Options options { get { return m_options; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum NAVIGATION_MODE { GLOBE, MAP }

	private Modes< ApplicationMain > m_navModes = null;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	[ SerializeField ]	private Camera     m_cameraGlobe = null;

	[ SerializeField ]	private Camera     m_cameraMap   = null;

	[ SerializeField ]	private GameObject m_globe       = null;

	[ SerializeField ]	private GameObject m_worldMap    = null;

	public Camera     cameraGlobe { get { return m_cameraGlobe; } }

	public Camera     cameraMap   { get { return m_cameraMap;   } }

	public GameObject globe       { get { return m_globe;       } }

	public GameObject worldMap    { get { return m_worldMap;    } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private  MapCoords m_coords        = new MapCoords();

	private  UI        m_UI            = new UI();

	private HandPick   m_globeHandpick = new HandPick();

	private HandPick   m_mapHandpick   = new HandPick();

	public   MapCoords coords        { get { return m_coords;        } }

	public   UI        ui            { get { return m_UI;            } }

	public   HandPick  globeHandpick { get { return m_globeHandpick; } }

	public   HandPick  mapHandpick   { get { return m_mapHandpick;   } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ApplyOptions()
	{
		ColorPrefs colorPrefs = GetComponent< ColorPrefs >();

		if( colorPrefs != null )
		{
			colorPrefs.m_matBgnds.color = m_options.m_colorBgnds;
		}

		if( DBObjects.instance != null )
		{
			DBObjects.instance.disabled = string.IsNullOrEmpty( m_options.DBInfos.server_adr );
		}

		m_options.Save();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnPOPUPOptionsButon( POPUPOptions popup, POPUPOptions.BUTON buton )
	{
		if( buton != POPUPOptions.BUTON.CANCEL )
		{
			m_options = popup.options.byval;

			ApplyOptions();
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Awake()
	{
		if( m_instance == null ) m_instance = this;
	} 

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnDestroy()
	{
		options.Save();

		if( m_instance == this ) m_instance = null;

		m_UI.Destroy();

		if( m_navModes != null ) { m_navModes.Destroy(); m_navModes = null; }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Start()
	{
		options.Load();

		ApplyOptions();

		UICmd.SetUniqueHandler( UICMD.SWITCH_TO_GLOBE,		ProcessUICmd );

		UICmd.SetUniqueHandler( UICMD.SWITCH_TO_MAP,		ProcessUICmd );

		UICmd.SetUniqueHandler( UICMD.SWITCH_TO_OPTIONS,	ProcessUICmd );

		UICmd.SetUniqueHandler( UICMD.DB_REFRESH,			ProcessUICmd );

		UICmd.SetUniqueHandler( UICMD.GO_TO_SELECTION,		ProcessUICmd );

		UICmd.SetUniqueHandler( UICMD.EDIT_SELECTION,		ProcessUICmd );

		UICmd.SetUniqueHandler( UICMD.IMPORT_CSV,			ProcessUICmd );

		UICmd.SetUniqueHandler( UICMD.EXPORT_CSV,			ProcessUICmd );

		UICmd.SetUniqueHandler( UICMD.APPLICATION_EXIT,		ProcessUICmd );

		m_UI.Create();

		m_UI.Show  ( false );

		if( m_navModes == null )
		{
			m_navModes = new Modes< ApplicationMain >( this );

			m_navModes.Create< NavigationModeGlobe >();

			m_navModes.Create< NavigationModeMap   >();
		}

		if( m_worldMap != null ) m_worldMap.SetActive( false );

		SelectNavigationMode( NAVIGATION_MODE.GLOBE );

		TeleportTo( 0.0f, 0.0f, MAP_SWITCH_DIST_EXIT );
	}
	
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void SelectNavigationMode( NAVIGATION_MODE mode )
	{
		if( m_navModes.ActiveMode != ( int )mode )
		{
			TeleportTo( m_coords.cur.latitude.deg, m_coords.cur.longitude.deg, ( mode == NAVIGATION_MODE.MAP ) ? MAP_SWITCH_DIST_ENTER : MAP_SWITCH_DIST_EXIT );

			ShowNavModeAssets( mode ); 

			m_navModes.Select( ( int )mode );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void SelectPreviousNavigationMode()
	{
		SelectNavigationMode( ( NAVIGATION_MODE )m_navModes.PreviousMode );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateNavigationMode()
	{
		bool showMap = ( ( m_coords.cur.altitude <= MAP_SWITCH_DIST_ENTER ) || ( ( m_navModes.ActiveMode == ( int )NAVIGATION_MODE.MAP ) && ( m_coords.cur.altitude < MAP_SWITCH_DIST_EXIT ) ) );


		NAVIGATION_MODE navMode = ( NAVIGATION_MODE )m_navModes.ActiveMode;

		if( showMap )   navMode = NAVIGATION_MODE.MAP;

		else            navMode = NAVIGATION_MODE.GLOBE;

		if( m_navModes.ActiveMode != ( int )navMode ) ScrFade.Begin( FADE_TYPE.FADE_OUT, 0.5f );

		else                                          ScrFade.Begin( FADE_TYPE.FADE_IN,  0.7f );


		if     ( ScrFade.finishedFadeOut ) { SelectNavigationMode( navMode ); }

		else if( ScrFade.finishedFadeIn  ) { if( m_navModes != null ) m_navModes.Update(); }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ShowNavModeAssets( NAVIGATION_MODE mode )
	{
		if( m_globe    != null ) m_globe.SetActive   ( mode == NAVIGATION_MODE.GLOBE );

		if( m_worldMap != null ) m_worldMap.SetActive( mode == NAVIGATION_MODE.MAP   );

		m_UI.ShowNavModeAssets( mode );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void LookAt( float latitude, float longitude, float dist )
	{
		if( m_globe       == null ) return;

		if( m_cameraGlobe == null ) return;


		latitude  = CORE.Angle.Normalize( latitude,  Angle.UNIT.DEG , Angle.NORM.NEG );

		longitude = CORE.Angle.Normalize( longitude, Angle.UNIT.DEG , Angle.NORM.NEG );

		dist      = CORE.Math.Clamp( dist, MIN_DIST, MAX_DIST );


		Vector3    trgPos     = m_globe.transform.position;

		Quaternion qLatitude  = Quaternion.AngleAxis(  latitude, Vector3.right );

		Quaternion qLongitude = Quaternion.AngleAxis( -longitude, Vector3.up    );

		Quaternion qRotation  = qLongitude * qLatitude;
		 
		Vector3    newPos     = trgPos + ( qRotation * new Vector3( 0.0f, 0.0f, -( 1.0f + dist ) ) );


		m_cameraGlobe.transform.position = newPos;

		m_cameraGlobe.transform.LookAt( trgPos, Vector3.up );


		m_coords.cur.altitude = dist;

		m_coords.cur.latitude.FromAngle ( latitude,  GPS.UNIT.DD );

		m_coords.cur.longitude.FromAngle( longitude, GPS.UNIT.DD );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void StartTransition( float latitude, float longitude, float dist )
	{
		latitude  = CORE.Angle.Normalize( latitude,  Angle.UNIT.DEG , Angle.NORM.NEG );

		longitude = CORE.Angle.Normalize( longitude, Angle.UNIT.DEG , Angle.NORM.NEG );

		dist      = CORE.Math.Clamp( dist, MIN_DIST, MAX_DIST );


		m_coords.req.altitude = dist;

		m_coords.req.latitude.FromAngle ( latitude,  GPS.UNIT.DD );

		m_coords.req.longitude.FromAngle( longitude, GPS.UNIT.DD );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateTransition()
	{
		if( m_coords.IsSync() == false )
		{
			float dist = CORE.Damp.Value( m_coords.cur.altitude,      m_coords.req.altitude      );

			float lat  = CORE.Damp.Angle( m_coords.cur.latitude.deg,  m_coords.req.latitude.deg  );

			float lng  = CORE.Damp.Angle( m_coords.cur.longitude.deg, m_coords.req.longitude.deg );

			LookAt( lat, lng, dist );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void StopTransition()
	{
		m_coords.SyncOnCur();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void TeleportTo( float latitude, float longitude, float dist ) 
	{
		LookAt( latitude, longitude, dist );

		StopTransition();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateCoordSelection( GPS.Coord latitude, GPS.Coord longitude )
	{
		m_UI.latitude  = string.Format( "{0}N", latitude.As ( m_options.coordUnit ).ToString() );

		m_UI.longitude = string.Format( "{0}E", longitude.As( m_options.coordUnit ).ToString() );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void SetCoordUnitPrefs( int unit )
	{
		GPS.UNIT pref = ( unit < 3 ) ? ( GPS.UNIT ) unit : GPS.UNIT.DMS;

		if( m_options.m_coordUnit != pref )
		{
			m_options.m_coordUnit  = pref;

			m_options.Save();
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void GoToSelection()
	{
		if( ( DBObjects.instance != null ) && ( DBObjects.instance.busy ) ) return;

		if( HUDSites.Instance != null )
		{
			object            selection   = HUDSites.Instance.items.selection;

			Localizable       localizable = ( ( selection != null ) && ( selection is Localizable ) ) ? selection as Localizable : null;

			NavigationModeMap navModeMap  = m_navModes[ 1 ] as NavigationModeMap;

			if( ( localizable != null ) && ( navModeMap != null ) )
			{
				ScrFade.Begin       ( FADE_TYPE.FADE_OUT, 0.0f );

				TeleportTo          ( localizable.m_coord.latitude.deg, localizable.m_coord.longitude.deg, localizable.m_coord.altitude );

				navModeMap.MoveTo   ( localizable.m_coord.latitude.deg, localizable.m_coord.longitude.deg );

				SelectNavigationMode( NAVIGATION_MODE.MAP );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void EditSelection()
	{
		if( ( DBObjects.instance != null ) && ( DBObjects.instance.busy ) ) return;

		if( HUDSites.Instance != null )
		{
			object      selection   = HUDSites.Instance.items.selection;

			Localizable localizable = ( ( selection != null ) && ( selection is Localizable ) ) ? selection as Localizable : null;

			if( ( localizable != null ) && ( POPUPItem.Instance != null ) )
			{
				POPUPItem.Instance.Show( null, localizable );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ImportCSV()
	{
		if( ( DBObjects.instance != null ) && ( DBObjects.instance.busy ) ) return;

		if( POPUPFileBrowser.Instance != null )
		{
			FSFilter filter = new FSFilter( FSFilter.LOD.FILE, "*.*" );

			POPUPFileBrowser.Instance.Show( Application.dataPath, POPUPFileBrowser.INTENT.LOAD, filter, ( POPUPFileBrowser popup, POPUPFileBrowser.BUTON but ) => 
			{
				if( but == POPUPFileBrowser.BUTON.OK )
				{
					CSV.File csv = new CSV.File();

					csv.Import( popup.path ); // to do: fix Lambert_( 93 ? ) - WGS84 latitude conversion
				}
			} );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ExportCSV()
	{
		if( POPUPFileBrowser.Instance != null )
		{
			FSFilter filter = new FSFilter( FSFilter.LOD.FILE, "*.*" );

			POPUPFileBrowser.Instance.Show( Application.dataPath, POPUPFileBrowser.INTENT.SAVE, filter, ( POPUPFileBrowser popup, POPUPFileBrowser.BUTON but ) => 
			{
				if( but == POPUPFileBrowser.BUTON.OK )
				{
					CSV.File csv = new CSV.File();

					csv.Export( popup.path );
				}
			} );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ProcessUICmd( UICMD cmd )
	{
		if( ScrFade.finishedFadeIn )
		{
			if     ( cmd == UICMD.SWITCH_TO_GLOBE		) { ScrFade.Begin( FADE_TYPE.FADE_OUT, 0.0f ); SelectNavigationMode( NAVIGATION_MODE.GLOBE ); }

			else if( cmd == UICMD.SWITCH_TO_MAP			) { ScrFade.Begin( FADE_TYPE.FADE_OUT, 0.0f ); SelectNavigationMode( NAVIGATION_MODE.MAP   ); }

			else if( cmd == UICMD.SWITCH_TO_OPTIONS		) { POPUPOptions.Instance.Show( OnPOPUPOptionsButon, m_options );       }

			else if( cmd == UICMD.DB_REFRESH			) { if( DBObjects.instance != null ) DBObjects.instance.UpdateFromDB(); }

			else if( cmd == UICMD.GO_TO_SELECTION		) { GoToSelection();    }

			else if( cmd == UICMD.EDIT_SELECTION		) { EditSelection();    }

			else if( cmd == UICMD.IMPORT_CSV	        ) { ImportCSV();        }

			else if( cmd == UICMD.EXPORT_CSV	        ) { ExportCSV();        }

			else if( cmd == UICMD.APPLICATION_EXIT		) { Application.Quit(); }
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Update()
	{
		if( ScrFade.finished )
		{
			UpdateTransition    ();

			UpdateNavigationMode();

			DragDropOperation.Update();
		}
	}
}

