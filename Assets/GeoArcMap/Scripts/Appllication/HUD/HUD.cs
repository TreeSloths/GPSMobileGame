using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class UI
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	[ System.Flags ] public enum ASSETS { NONE = 0X0, COORDS = 0X1, MAP_DISPLAY_MODE = 0X2, MAP_ZOOM = 0X4 }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private GameObject                m_UI             = null;

	private GameObject                m_coords         = null;

	private GameObject                m_mapDisplatMode = null;

	private GameObject                m_mapZoom        = null;

	private GameObject                m_mapButtons     = null;

	private GameObject                m_mapSourceIcon  = null;

	private GameObject                m_mapSource      = null;

	private MapNotice                 m_mapNotice      = null;

	private Nameplates                m_nameplates     = null;

	private UnityEngine.UI.Dropdown   m_mapSources     = null;

	private UnityEngine.UI.Text       m_latitude       = null;

	private UnityEngine.UI.Text       m_longitude      = null;

	private UnityEngine.UI.InputField m_mapZoomValue   = null;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public UnityEngine.UI.Dropdown   mapSources { get { return m_mapSources;   } }

	public MapNotice                 mapNotice  { get { return m_mapNotice;    } }               

	public Nameplates                nameplates { get { return m_nameplates;   } }

	public UnityEngine.UI.InputField zoomInput  { get { return m_mapZoomValue; } }

	public string                    latitude   { set { if( m_latitude     != null ) m_latitude.text     = value; } }

	public string                    longitude  { set { if( m_longitude    != null ) m_longitude.text    = value; } }

	public string                    zoom       { set { if( m_mapZoomValue != null ) m_mapZoomValue.text = value; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ResolveDependencies()
	{
		m_UI             = GameObject.Find( "UI" );

		GameObject dockT = CORE.HIERARCHY.Resolve( m_UI, "CANVAS.HUDS.DOCK_T" );

		GameObject dockB = CORE.HIERARCHY.Resolve( m_UI, "CANVAS.HUDS.DOCK_B" );

		GameObject dockR = CORE.HIERARCHY.Resolve( m_UI, "CANVAS.HUDS.DOCK_R" );

		m_coords         = CORE.HIERARCHY.Find   ( dockT, "HUDCoords"         );

		m_mapSourceIcon  = CORE.HIERARCHY.Find   ( dockT, "ICON_SOURCE"       );

		m_mapSource      = CORE.HIERARCHY.Find   ( dockT, "SOURCE"            );

		m_mapDisplatMode = CORE.HIERARCHY.Find   ( dockR, "HUDMapDisplayMode" );

		m_mapZoom        = CORE.HIERARCHY.Find   ( dockR, "HUDZoom"           );

		m_mapButtons     = CORE.HIERARCHY.Find   ( dockR, "HUDMapButtons"     );

		m_latitude       = CORE.HIERARCHY.FindComp< UnityEngine.UI.Text       >( m_coords,  "HUDLatitude"  );

		m_longitude      = CORE.HIERARCHY.FindComp< UnityEngine.UI.Text       >( m_coords,  "HUDLongitude" );

		m_mapZoomValue   = CORE.HIERARCHY.FindComp< UnityEngine.UI.InputField >( m_mapZoom, "HUDZoomValue" );


		m_mapSources = ( m_mapSource != null ) ? m_mapSource.GetComponent< UnityEngine.UI.Dropdown >() : null;

		m_mapNotice  = ( dockB != null ) ? dockB.GetComponent< MapNotice >() : null;

		if( m_mapNotice != null ) m_mapNotice.text = string.Empty;


		GameObject o = CORE.HIERARCHY.Resolve( m_UI, "CANVAS.NAMEPLATES" ); 

		m_nameplates = ( o != null ) ? o.GetComponent< Nameplates >() : null;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Create()
	{
		ResolveDependencies();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ShowNavModeAssets( ApplicationMain.NAVIGATION_MODE mode )
	{
		ASSETS assets = ASSETS.COORDS;

		if( mode == ApplicationMain.NAVIGATION_MODE.MAP ) assets |= ASSETS.MAP_DISPLAY_MODE;

		if( mode == ApplicationMain.NAVIGATION_MODE.MAP ) assets |= ASSETS.MAP_ZOOM;

		if( m_UI != null )
		{
			m_UI.SetActive( assets != 0 );

			if( m_UI.activeInHierarchy )
			{
				if( m_coords         != null ) m_coords.SetActive        ( ( assets & ASSETS.COORDS           ) != 0 );

				if( m_mapDisplatMode != null ) m_mapDisplatMode.SetActive( ( assets & ASSETS.MAP_DISPLAY_MODE ) != 0 );

				if( m_mapZoom        != null ) m_mapZoom.SetActive       ( ( assets & ASSETS.MAP_ZOOM         ) != 0 );

				if( m_mapSourceIcon  != null ) m_mapSourceIcon.SetActive ( ( assets & ASSETS.MAP_DISPLAY_MODE ) != 0 );

				if( m_mapSource      != null ) m_mapSource.SetActive     ( ( assets & ASSETS.MAP_DISPLAY_MODE ) != 0 );

				if( m_mapButtons     != null ) m_mapButtons.SetActive    ( ( assets & ASSETS.MAP_DISPLAY_MODE ) != 0 );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Show( bool show )
	{
		if( m_UI != null ) m_UI.SetActive( show );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Destroy()
	{
	}
}
