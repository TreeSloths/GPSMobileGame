using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class NavigationModeMap : Mode< ApplicationMain >
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	private bool                      m_exit              = false;

	private WebMap                    m_map               = null;

	private UnityEngine.UI.InputField m_zoomInput         = null;

	private UnityEngine.UI.Dropdown   m_mapSources        = null;

	private float                     m_lastZoomDisplayed = float.MinValue;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void SetupMap()
	{
		UI         ui       = ( o != null ) ? o.ui       : null;

		GameObject worldMap = ( o != null ) ? o.worldMap : null;


		m_map = ( worldMap != null ) ? worldMap.GetComponent< WebMap >() : null;

		if( m_map != null )
		{
			o.mapHandpick.Setup( o.cameraMap, new CORE.Plane( Vector3.zero, Vector3.up ), null, m_map.GetLatitudeFrom3DCoord, m_map.GetLongitudeFrom3DCoord );

			m_map.Setup( o.mapHandpick, 12, 12, 256.0f, OnMapExitRequested );

			m_map.notice = ( ui != null ) ? ui.mapNotice : null;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void SetupZoomInput()
	{
		m_zoomInput = ( o.ui != null ) ? o.ui.zoomInput : null;

		if( m_zoomInput != null )
		{
			m_zoomInput.onEndEdit.RemoveAllListeners();

			m_zoomInput.onEndEdit.AddListener( delegate { OnZoomValueEndEdit(); } );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void SetupMapSources()
	{
		m_mapSources = ( o.ui != null ) ? o.ui.mapSources : null;

		if( m_mapSources != null )
		{
			m_mapSources.ClearOptions();

			int nbSources = System.Enum.GetValues( typeof( WebMap.SOURCE ) ).Length;

			for( int source = 0; source < nbSources; ++source )
			{
				m_mapSources.options.Add( new UnityEngine.UI.Dropdown.OptionData( ( ( WebMap.SOURCE )source ).ToString() ) );
			}


			m_mapSources.onValueChanged.RemoveAllListeners();

			m_mapSources.onValueChanged.AddListener( delegate { OnMapSourceSelected(); } );

			m_mapSources.value = ( int )( ( m_map != null ) ? m_map.source : WebMap.DEFAULT_SOURCE );

			m_mapSources.RefreshShownValue();
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void Init()
	{
		SetupMap();

		SetupZoomInput ();

		SetupMapSources();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnZoomValueEndEdit()
	{
		if( m_zoomInput == null ) return;

		if( m_map       == null ) return;


		float value = 0.0f;

		if( float.TryParse( m_zoomInput.text, out value ) )
		{
			float clamp = Mathf.Clamp( value, ( float )WebMap.ZOOM_MIN, ( float )( WebMap.ZOOM_MAX + 0.99f ) );

			if( clamp != value ) m_zoomInput.text = string.Format( "{0:F2}", clamp );

			m_map.zoom         = ( int )clamp;

			m_map.analogicZoom = clamp - m_map.zoom;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnMapSourceSelected()
	{
		if( m_map != null )
		{
			m_map.source = ( WebMap.SOURCE )m_mapSources.value;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void Enter()
	{
		m_exit = false;

		if( m_map != null )
		{
			m_map.Enter( o.coords.cur.latitude.deg, o.coords.cur.longitude.deg );

			o.mapHandpick.Enable( true );
		}

		if( ( o.ui != null ) && ( o.ui.nameplates != null ) )
		{
			o.ui.nameplates.gameObject.SetActive( true );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void Exit()
	{
		if( m_map != null )
		{
			m_map.Exit();

			o.mapHandpick.Enable( false );

			if( ( o.ui != null ) && ( o.ui.nameplates != null ) )
			{
				o.ui.nameplates.gameObject.SetActive( false );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnMapExitRequested()
	{
		m_exit = true;

		o.TeleportTo( m_map.centerCoords.latitude.deg, m_map.centerCoords.longitude.deg, ApplicationMain.MAP_SWITCH_DIST_EXIT );

		o.UpdateNavigationMode();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateZoomSelection( float zoom )
	{
		if( m_zoomInput != null )
		{
			m_zoomInput.text = string.Format( "{0:F2}", zoom );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateCoordSelection()
	{
		o.mapHandpick.Update();

		o.UpdateCoordSelection( o.mapHandpick.Latitude, o.mapHandpick.Longitude );

		if( m_lastZoomDisplayed != m_map.zoomDisplayValue )
		{
			m_lastZoomDisplayed  = m_map.zoomDisplayValue;

			UpdateZoomSelection  ( m_map.zoomDisplayValue );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void MoveTo( float latitude, float longitude )
	{
		if( m_map != null )
		{
			m_map.MoveTo( latitude, longitude, WebMap.FORCE_UPDATE.NO );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void Update ()
	{
		if( m_exit == false )
		{
			if( Focusable.popup == false )
			{
				UpdateCoordSelection();
			}

			if( m_map != null )
			{
				m_map.Update();
			}
		}
	}
}
