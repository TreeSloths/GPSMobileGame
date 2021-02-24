using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public interface IMapNotice
{
	string text { get; set; }
}

//********************************************************************************************************
//
//********************************************************************************************************

public partial class WebMap : DragDropComponent
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum SOURCE        { ESRI, GOOGLE }

	public enum FORCE_UPDATE  { YES, NO }

	public enum DISPLAY_MODE  { VERTICAL, INCLINED }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public const SOURCE DEFAULT_SOURCE       = SOURCE.ESRI;

	public const int	ZOOM_MIN             = 5;

	public const int	ZOOM_MAX             = 19;

	public const int	MAP_ZOOM_VALUE_ENTER = ZOOM_MIN;

	public const int	MAP_ZOOM_VALUE_EXIT  = ZOOM_MIN - 1;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public WebMapProvider[] m_providers = new WebMapProvider[ 2 ] 
	{
		new WebMapProvider( "ESRI",   256.0f, 156543.033928f, ZOOM_MAX, new WGS_84_WEB(), "DigitalGlobe, Microsoft | Esri, HERE, Delorme, iPC" ),

		new WebMapProvider( "GOOGLE", 256.0f, 156543.033928f, ZOOM_MAX, new WGS_84_WEB(), "Imagery ©2017 NASA, TerraMetrics | GOOGLE" )
	};

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private bool           m_refresh  = false;

	private SOURCE         m_source   = DEFAULT_SOURCE;

	private bool           m_mobile   = true;

	private IMapNotice     m_notice   = null;

	private WebMapProvider m_provider = m_providers[ ( int )DEFAULT_SOURCE ];


	public SOURCE          source   { get { return m_source;   } set { if( m_source != value ) { m_source = value; m_provider = m_providers[ ( int )m_source ]; m_refresh = true; } } }

	public bool            mobile   { get { return m_mobile;   } set { m_mobile = value; } }

	public IMapNotice      notice   { get { return m_notice;   } set { m_notice = value; } }

	public WebMapProvider  provider { get { return m_provider; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public delegate void OnExitRequested();

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private HandPick    m_handpick        = null;

	private GameObject	m_tilesNode       = null;

	private Camera		m_camera          = null;

	private float		m_analogicZoom    = 0.0f;

	private XForm		m_initialCamXForm = new XForm();

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private int	  m_zoom        = 0;

	private float m_zoomTime    = 0.0f;

	private int	  m_nbCols      = 0;

	private int	  m_nbRows      = 0;

	private int	  m_nbSlots     = 0;

	private int	  m_nbColsUsed  = 0;

	private int	  m_nbRowsUsed  = 0;

	private int	  m_nbSlotsUsed = 0;

	private int	  m_nbTiles     = 1;

	private float m_tilesSize   = 512.0f;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private Vector2			  m_aperture         = new Vector2 ( 0.0f, 0.0f );

	private MapCoord		  m_centerCoords     = new MapCoord();

	private float			  m_scrAspect        = float.MinValue;

	private ScreenOrientation m_scrOrient        = ScreenOrientation.Unknown;

	private DISPLAY_MODE      m_displayMode      = DISPLAY_MODE.INCLINED;

	private Vector3			  m_virtualPos       = Vector3.zero;

	private Vector3			  m_virtualPosSnaped = Vector3.zero;

	private Vector3           m_TLCorner         = Vector3.zero;

	private Vector3           m_BRCorner         = Vector3.zero;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private WebMapLoader	  m_loader          = null;

	private WebMapTilePool	  m_pool            = null;

	private WebMapTileSlot[]  m_slots           = new WebMapTileSlot[ 0 ];

	private WebMapTileSlot[]  m_slotsUsed       = new WebMapTileSlot[ 0 ];

	private WebMapTile    []  m_tilesRelaxed    = new WebMapTile    [ 0 ];

	private int[]             m_loadSeq         = new int           [ 0 ];

	private LoadingIcon       m_loadingIcon     = new LoadingIcon();

	private OnExitRequested   m_onExitRequested = null;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public HandPick handpick     { get { return m_handpick;     } }

	public Vector2  aperture     { get { return m_aperture;     } }

	public MapCoord centerCoords { get { return m_centerCoords; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Start()
	{
		if( DBObjects.instance != null ) DBObjects.instance.listeners.Add( OnDBEvent );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnDestroy()
	{
		if( DBObjects.instance != null ) DBObjects.instance.listeners.Remove( OnDBEvent );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public int zoom
	{
		get { return m_zoom; }

		set
		{
			if( ( Time.time - m_zoomTime ) > 1.0f )
			{
				int val = Mathf.Max( MAP_ZOOM_VALUE_EXIT, Mathf.Min( value, ZOOM_MAX ) );

				if( m_zoom != val ) { m_zoom = val; OnZoomChanged(); }
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public float analogicZoom
	{
		get { return m_analogicZoom; }

		set
		{
			float Value = Mathf.Clamp( value, 0.0f, 1.0f );

			if( m_analogicZoom != Value )
			{
				m_analogicZoom  = Value;
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public float zoomDisplayValue
	{
		get { return m_zoom + m_analogicZoom; }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public float LatToY( float lat )
	{
		return m_provider.LatToY( m_zoom, m_tilesSize, lat );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public float YToLat( float y )
	{
		return m_provider.YToLat( m_zoom, m_tilesSize, y );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public GPS.Coord GetLatitudeFrom3DCoord( Vector3 p )
	{
		float lat = YToLat( p.z + m_virtualPos.z );

		lat = Angle.Normalize( lat, Angle.UNIT.DEG, Angle.NORM.NEG, GPS.TYPE.LATITUDE );

		return new GPS.Coord( GPS.TYPE.LATITUDE, lat );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public GPS.Coord GetLongitudeFrom3DCoord( Vector3 p )
	{
		float lng = ( ( p.x / m_tilesSize ) * aperture.x ) + m_centerCoords.longitude.deg;

		lng = Angle.Normalize( lng, Angle.UNIT.DEG, Angle.NORM.NEG, GPS.TYPE.LONGITUDE );

		return new GPS.Coord( GPS.TYPE.LONGITUDE, lng );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Vector3 GetGeoCoordsFromPosition( Vector3 pos )
	{
		return new Vector3( GetLongitudeFrom3DCoord( pos ).deg, GetLatitudeFrom3DCoord( pos ).deg, m_zoom );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Vector3 GetPositionFromGeoCoords( float lng, float lat )
	{
		Vector3 pos = new Vector3( ( lng / m_aperture.x ) * m_tilesSize, 0.0f, LatToY( lat ) );

		return  pos;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Vector3 GetRelativePositionFromGeoCoords( Vector2 bounds, float lng, float lat )
	{
		Vector3 pos  = GetPositionFromGeoCoords( lng, lat );

		        pos -= m_virtualPos;

		if( pos.x < m_TLCorner.x ) pos.x += bounds.x; else if( pos.x > m_BRCorner.x ) pos.x -= bounds.x;

		if( pos.z < m_BRCorner.z ) pos.z += bounds.y; else if( pos.z > m_TLCorner.z ) pos.x -= bounds.y;

		return pos;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public GridCoords GetGridCoordsFromRelativePosition( Vector3 pos )
	{
		GridCoords coords       = new GridCoords();

		Vector3    gridLocalPos = ( pos + m_virtualPos ) - m_TLCorner;

		coords.X = ( int )(  gridLocalPos.x / m_tilesSize ); if( gridLocalPos.x < 0 ) coords.X += ( 1 << zoom ) - 1;

		coords.Y = ( int )( -gridLocalPos.z / m_tilesSize ); if( gridLocalPos.z > 0 ) coords.Y += ( 1 << zoom ) - 1;

		coords.Clamp( 1 << zoom, 1 << zoom );

		return coords;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Setup( HandPick @handpick, int nbCols, int nbRows, float tilesSize, OnExitRequested onExitRequested )
	{
		UICmd.SetUniqueHandler( new UICMD[] { UICMD.MAP_ZOOM_INC,     UICMD.MAP_ZOOM_DEC     }, ProcessUICmd );

		UICmd.SetUniqueHandler( new UICMD[] { UICMD.SWITCH_TO_2D_MAP, UICMD.SWITCH_TO_3D_MAP }, ProcessUICmd );

		UICmd.SetUniqueHandler( UICMD.MAP_ADD_FLAG, ContentProcessUICmd );

		UICmd.SetUniqueHandler( UICMD.MAP_ADD_PIN,  ContentProcessUICmd );


		m_handpick    = @handpick;

		m_nbCols      = nbCols > 0 ? Alignement.Align( nbCols, 2 ) : 2;

		m_nbRows      = nbRows > 0 ? Alignement.Align( nbRows, 2 ) : 2;

		m_nbSlots     = m_nbRows * m_nbCols;

		m_nbColsUsed  = 0;

		m_nbRowsUsed  = 0;

		m_nbSlotsUsed = 0;

		m_nbTiles     = m_nbSlots << 1;

		m_tilesSize   = tilesSize >= 1.0f ? tilesSize : 1.0f;

		m_pool        = new WebMapTilePool( m_nbTiles );

		System.Array.Resize( ref m_slots,        m_nbSlots );

		System.Array.Resize( ref m_slotsUsed,    m_nbSlots );

		System.Array.Resize( ref m_tilesRelaxed, m_nbSlots );

		System.Array.Resize( ref m_loadSeq,      m_nbSlots );

		m_onExitRequested = onExitRequested;

		CreateLayout();

		CreateContentDespotitNode();

		CreatePinSelectionHalo();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Show( bool show )
	{
		gameObject.SetActive( show );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool ResolveDependencies()
	{
		m_loader = GetComponent < WebMapLoader >();

		m_camera = GetComponentInChildren< Camera >();

		if( m_loader != null )
		{
			m_loader.map       = this;

			m_loader.indicator = HUDDLoad.Instance;
		}

		if( m_camera != null )
		{
			m_initialCamXForm.FromTransform( m_camera.transform, XForm.SPACE.LOCAL );
		}

		return ( m_camera != null ) && ( m_loader != null );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public WebMapTile CreateTile( string name, GameObject mdl, Quaternion q, Transform parent, float size )
	{
		GameObject obj  = ( mdl != null ) ? GameObject.Instantiate( mdl, Vector3.zero, q, ( parent != null ) ? parent.transform : null ) as GameObject : null;

		WebMapTile tile = ( obj != null ) ? obj.GetComponent< WebMapTile >() : null;

		if( tile != null ) tile.Setup( name, size );

		return tile;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public GameObject CreateObjInstance( string name, GameObject mdl, Quaternion q, Transform parent )
	{
		GameObject obj  = ( mdl != null ) ? GameObject.Instantiate( mdl, Vector3.zero, q, ( parent != null ) ? parent.transform : null ) as GameObject : null;

		if( obj != null ) obj.name = name;

		return obj;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void CreateLayout()
	{
		if( ResolveDependencies() )
		{
			m_loadingIcon.Setup( "2D/MapTile/TileLoading" );

			m_tilesNode = new GameObject( "Tiles" );

			m_tilesNode.transform.parent = transform;

			GameObject mdl = Resources.Load< GameObject >( "3D/MapTile/MapTile" );

			Quaternion q   = Quaternion.AngleAxis( 90.0f, Vector3.right );

			if( mdl != null )
			{
				for( int tile = 0; tile < m_nbTiles; ++tile )
				{
					m_pool.Release( CreateTile( string.Format( "Tile{0}", tile ), mdl, q, m_tilesNode.transform, m_tilesSize ) );
				}
			}

			for( int slot = 0; slot < m_nbSlots; ++slot )
			{
				m_slots[ slot ] = new WebMapTileSlot();
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ResetLayout()
	{
		float    startX = ( -( m_nbColsUsed >> 1 ) + 0.5f ) * m_tilesSize; if( ( m_nbColsUsed & 1 ) != 0 ) startX -= 0.5f * m_tilesSize;

		float    startZ =  ( ( m_nbRowsUsed >> 1 ) - 0.5f ) * m_tilesSize; if( ( m_nbRowsUsed & 1 ) != 0 ) startZ += 0.5f * m_tilesSize;

		for( int row = 0; row < m_nbRowsUsed; ++row )
		{
			for( int col = 0; col < m_nbColsUsed; ++col )
			{
				WebMapTileSlot Slot = m_slotsUsed[ ( row * m_nbColsUsed ) + col ];

				Slot.layoutPos  = new Vector3( startX + ( col * m_tilesSize ), 0.0f, startZ - ( row * m_tilesSize ) ); 

				Slot.position   = Slot.layoutPos;

				Slot.coordGeo   = GetGeoCoordsFromPosition( Slot.position );

				Slot.coordGrid  = new GridCoords( int.MaxValue, int.MaxValue );
			}
		}

		UpdateLoadSequence();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateLoadSequence()
	{
		int[] cells = new int[ m_nbRowsUsed ];

		System.Array.Clear( cells, 0, m_nbRowsUsed );


		int L        = ( m_nbColsUsed >> 1 ) - 1;

		int R        = L + ( ( m_nbColsUsed + 1 ) & 1 );

		int T        = ( m_nbRowsUsed >> 1 ) - 1;

		int B        = T + ( ( m_nbRowsUsed + 1 ) & 1 );

		int slot     = 0;

		while( slot < m_nbSlotsUsed )
		{
			for( int col = L;     col <= R; ++col ) if( ( cells[ T   ] & ( 1 << col ) ) == 0 ) { m_loadSeq[ slot++ ] = ( T   * m_nbColsUsed ) + col; cells[ T   ] |= ( 1 << col ); }

			for( int row = T + 1; row <  B; ++row ) if( ( cells[ row ] & ( 1 << R   ) ) == 0 ) { m_loadSeq[ slot++ ] = ( row * m_nbColsUsed ) + R;   cells[ row ] |= ( 1 << R   ); }

			for( int col = R;     col >= L; --col ) if( ( cells[ B   ] & ( 1 << col ) ) == 0 ) { m_loadSeq[ slot++ ] = ( B   * m_nbColsUsed ) + col; cells[ B   ] |= ( 1 << col ); }

			for( int row = B - 1; row >  T; --row ) if( ( cells[ row ] & ( 1 << L   ) ) == 0 ) { m_loadSeq[ slot++ ] = ( row * m_nbColsUsed ) + L;   cells[ row ] |= ( 1 << L   ); }

			if( L > 0 ) { --L; ++R; }

			if( T > 0 ) { --T; ++B; }
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void MoveTo( float lat, float lng, FORCE_UPDATE forceUpdate )
	{
		if( ( forceUpdate == FORCE_UPDATE.NO ) && ( ( m_centerCoords.latitude.deg == lat ) && ( m_centerCoords.longitude.deg == lng ) ) )
		{
			return;
		}

		m_centerCoords.latitude.FromAngle ( lat, GPS.UNIT.DD );

		m_centerCoords.longitude.FromAngle( lng, GPS.UNIT.DD );

		UpdateMapPosition();

		UpdateMapObjectsPosition();

		UpdateTilesPosition();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateMapPosition()
	{
		float   lng        = m_centerCoords.longitude.deg;

		float   lat        = m_centerCoords.latitude.deg;

		bool    extendX    = ( ( m_nbColsUsed > 1 ) && ( m_nbColsUsed & 1 ) != 0 );

		bool    extendZ    = ( ( m_nbRowsUsed > 1 ) && ( m_nbRowsUsed & 1 ) != 0 );

		Vector3 extent     = new Vector3( extendX ? m_tilesSize * 0.5f : 0.0f, 0.0f, extendZ ? m_tilesSize * 0.5f : 0.0f );
		
		m_TLCorner         = new Vector3( ( ( 1 << zoom ) >> 1 ) * -m_tilesSize, 0.0f, ( ( 1 << zoom ) >> 1 ) * m_tilesSize ) + extent;

		m_BRCorner         = m_TLCorner * -1.0f;

		m_virtualPos       = GetPositionFromGeoCoords( lng, lat );

		m_virtualPosSnaped = new Vector3( ( m_virtualPos.x % m_tilesSize ), 0.0f, ( m_virtualPos.z % m_tilesSize ) );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateTilesPosition()
	{
		int nbTilesRelaxed = 0;

		for( int slot = 0; slot < m_nbSlotsUsed; ++slot )
		{
			WebMapTileSlot Slot = m_slotsUsed[ slot ];

			Slot.position  = Slot.layoutPos - m_virtualPosSnaped;

			Slot.coordGeo  = GetGeoCoordsFromPosition         ( Slot.position );

			Slot.coordGrid = GetGridCoordsFromRelativePosition( Slot.position );


			if( Slot.tile != null )
			{
				if( Slot.tile.location.coordGrid != Slot.coordGrid )
				{
					m_tilesRelaxed[ nbTilesRelaxed++ ] = Slot.tile;

					m_pool.Release( Slot.tile );
				}
			}
		}

		for( int slot = 0; slot < m_nbSlotsUsed; ++slot )
		{
			WebMapTileSlot Slot = m_slotsUsed[ slot ];

			if( Slot.tile == null )
			{
				m_pool.Grab( Slot );

				if( Slot.tile != null )
				{
					Slot.tile.location.coordGrid = Slot.coordGrid;

					Slot.tile.location.coordGeo  = Slot.coordGeo;
				}
			}

			if( Slot.tile != null ) Slot.tile.position = Slot.position;
		}

		for( int tile = 0; tile < nbTilesRelaxed; ++tile )
		{
			if( m_tilesRelaxed[ tile ].slot == null )
			{
				m_tilesRelaxed[ tile ].CancelLoadingRequest();
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void LoadMaps()
	{
		UnityEngine.Plane[] planes = GeometryUtility.CalculateFrustumPlanes( m_camera );

		Bounds              bounds = new Bounds( Vector3.zero, new Vector3( m_tilesSize, 0.0f, m_tilesSize ) );

		for( int slot = 0; slot < m_nbSlotsUsed; ++slot )
		{
			WebMapTile Tile = m_slotsUsed[ m_loadSeq[ slot ] ].tile;

			if( Tile != null )
			{
				bounds.center = Tile.position;

				if( GeometryUtility.TestPlanesAABB( planes, bounds ) )
				{
					Tile.SubmitLoadingRequest( m_loader, m_loadingIcon );
				}
				else
				{
					Tile.CancelLoadingRequest();
				}
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Enter( float latitude, float longitude )
	{
		if( m_camera != null ) m_camera.gameObject.SetActive( true );

		if( m_notice != null ) m_notice.text = ( m_provider != null ) ? m_provider.copyrights : string.Empty;

		zoom = MAP_ZOOM_VALUE_ENTER;

		MoveTo( latitude, longitude, FORCE_UPDATE.YES );

		Show( true );

		ReflectDB();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Exit()
	{
		if( m_notice != null ) m_notice.text = string.Empty;

		if( m_camera != null ) m_camera.gameObject.SetActive( false );

		Show( false );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void FlushTextures()
	{
		if( m_loader != null ) { m_loader.Cancel(); m_loader.FlushCache(); }

		for( int entry = 0; entry < m_pool.size;   ++entry ) { WebMapTile tile = m_pool     [ entry ];      if( tile != null ) tile.FlushTexture(); }

		for( int slot  = 0; slot  < m_nbSlotsUsed; ++slot  ) { WebMapTile tile = m_slotsUsed[ slot  ].tile; if( tile != null ) tile.FlushTexture(); }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void MonitorScreenChanges()
	{
		float             scrAspect = ( float )Screen.width / ( float )Screen.height;

		ScreenOrientation scrOrient = Screen.orientation;

		if( ( m_scrAspect != scrAspect ) || ( m_scrOrient != scrOrient ) )
		{
			m_scrAspect        = scrAspect;

			m_scrOrient        = scrOrient;

			bool doubleW       = ( ( scrOrient <= ScreenOrientation.PortraitUpsideDown ) && ( scrAspect > 1.33f ) ) || ( ( scrOrient > ScreenOrientation.PortraitUpsideDown ) && ( scrAspect <  0.8f ) );

			bool doubleH       = ( ( scrOrient <= ScreenOrientation.PortraitUpsideDown ) && ( scrAspect <  0.8f ) ) || ( ( scrOrient > ScreenOrientation.PortraitUpsideDown ) && ( scrAspect > 1.33f ) );

			int  nbColsNeeded  = doubleW ? m_nbCols : m_nbCols >> 1;

			int  nbRowsNeeded  = doubleH ? m_nbRows : m_nbRows >> 1;


			if( ( m_nbColsUsed != nbColsNeeded ) || ( m_nbRowsUsed != nbRowsNeeded ) )
			{
				int nbSlotsNeeded = nbColsNeeded * nbRowsNeeded;

				if( nbSlotsNeeded < m_nbSlotsUsed )
				{
					for( int slot = nbSlotsNeeded - 1; slot < m_nbSlotsUsed; ++slot )
					{
						m_pool.Release( m_slotsUsed[ slot ].tile );
					}
				}
				else
				{
					for( int slot = m_nbSlotsUsed; slot < nbSlotsNeeded; ++slot )
					{
						m_slotsUsed[ slot ] = m_slots[ slot ];
					}
				}

				m_nbColsUsed  = nbColsNeeded;

				m_nbRowsUsed  = nbRowsNeeded;

				m_nbSlotsUsed = nbSlotsNeeded;

				ResetLayout();

				MoveTo( m_centerCoords.latitude.deg, m_centerCoords.longitude.deg, FORCE_UPDATE.YES );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateDisplayModeTransition()
	{
		float cur = m_camera.transform.rotation.eulerAngles.x;

		float trg = ( m_displayMode == DISPLAY_MODE.VERTICAL ) ? 90.0f : m_initialCamXForm.m_quat.eulerAngles.x;

		if( cur != trg )
		{
			m_camera.transform.rotation = Quaternion.AngleAxis( CORE.Damp.Value( cur, trg, 0.5f ), Vector3.right );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateCameraPosition()
	{
		UpdateDisplayModeTransition();

		if( m_camera != null )
		{
			float offset = m_initialCamXForm.pos.z * ( 1.0f - ( ( 90.0f - m_camera.transform.rotation.eulerAngles.x ) / ( 90.0f - 65.0f ) ) );

			m_camera.transform.position = m_initialCamXForm.pos - ( Vector3.forward * offset );

			m_camera.transform.position = m_camera.transform.position * ( 1.0f - ( m_analogicZoom * 0.5f ) );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnZoomChanged()
	{
		if( zoom <= MAP_ZOOM_VALUE_EXIT )
		{
			if( m_onExitRequested != null )
			{
				m_onExitRequested();
			}
		} 
		else
		{
			Refresh();
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Refresh()
	{
		m_refresh      = false;

		m_zoomTime     = Time.time;

		m_aperture     = new Vector2( 360.0f / ( 1 << zoom ), ( 180.0f * 180.0f / 152.0f ) / ( 1 << zoom ) );

		m_analogicZoom = 0.0f;

		FlushTextures();

		ResetLayout  ();

		MoveTo( m_centerCoords.latitude.deg, m_centerCoords.longitude.deg, FORCE_UPDATE.YES );

		if( m_notice != null ) m_notice.text = ( m_provider != null ) ? m_provider.copyrights : string.Empty;

		if( m_camera != null ) m_camera.transform.position = m_initialCamXForm.m_pos;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateNavigationInputs()
	{
        if( ( Time.time - m_zoomTime ) > 1.0f )
		{
			int zoomStep = 0; 

			if     ( Input.GetKeyDown( KeyCode.KeypadMinus ) ) { zoomStep = -1; }

			else if( Input.GetKeyDown( KeyCode.KeypadPlus  ) ) { zoomStep =  1; }

			else
			{
				m_analogicZoom += 0.125f * Input.GetAxis( "Mouse ScrollWheel" );

				if     ( m_analogicZoom < 0.0f ) { zoomStep = -1; }

				else if( m_analogicZoom > 1.0f ) { zoomStep =  1; }
			}

			if( zoomStep != 0 ) { m_analogicZoom = 0.0f; zoom += zoomStep; return; }
		}



		Vector2 offset      = new Vector3( 0.0f, 0.0f, 0.0f );
		
		bool    mouseButton = Input.GetMouseButton( 0 );
		
		float dt = Time.deltaTime;

		offset.x = mouseButton ? ( dt * aperture.x * 4.0f * -Input.GetAxis( "Mouse X" ) ) : 0.0f;

		offset.y = mouseButton ? ( dt * aperture.y * 4.0f * -Input.GetAxis( "Mouse Y" ) ) : 0.0f;



		if( offset != Vector2.zero )
		{
			float lat = m_centerCoords.latitude.deg  + offset.y;

			float lng = m_centerCoords.longitude.deg + offset.x; 

			lat = Angle.Normalize( lat, Angle.UNIT.DEG, Angle.NORM.NEG, GPS.TYPE.LATITUDE  );

			lng = Angle.Normalize( lng, Angle.UNIT.DEG, Angle.NORM.NEG, GPS.TYPE.LONGITUDE );

			lat = Mathf.Clamp( lat, -90.0f + aperture.y, 90.0f - aperture.y );

			MoveTo( lat, lng, FORCE_UPDATE.NO );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateInputs()
	{
		if( Focusable.any ) return;

		if( DragDropOperation.pending )
		{
			UpdateDragDropInputs();
		}
		else
		{
			UpdateNavigationInputs();
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Update()
	{
		if( zoom <= MAP_ZOOM_VALUE_EXIT ) { return;      }

		if( m_refresh )                   { Refresh();   }

		if( m_shouldReflectDB )           { ReflectDB(); }


		m_loadingIcon.Rotate();

		MonitorScreenChanges();

		UpdateInputs();

		UpdateCameraPosition();

		LoadMaps();

		UpdateMapObjectsSelection();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ProcessUICmd( UICMD cmd )
	{
		if     ( cmd == UICMD.MAP_ZOOM_INC     ) ++zoom;

		else if( cmd == UICMD.MAP_ZOOM_DEC     ) --zoom;

		else if( cmd == UICMD.SWITCH_TO_2D_MAP ) m_displayMode = DISPLAY_MODE.VERTICAL;

		else if( cmd == UICMD.SWITCH_TO_3D_MAP ) m_displayMode = DISPLAY_MODE.INCLINED;
	}
}



