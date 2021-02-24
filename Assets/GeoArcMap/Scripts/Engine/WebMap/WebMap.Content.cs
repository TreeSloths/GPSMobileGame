using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public partial class WebMap
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum CREATE_MODE { CREATE, CREATE_DRAG }

	public enum OBJECT_TYPE { FLAG, PIN }

	static private string[]  m_mapObjectModel = new string[ 2 ] { "3D/Imports/Flag/Flag", "3D/Imports/Pin/Pin" };

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private GameObject               m_deposit         = null;

	private GameObject               m_objSelection    = null;

	private List< WebMapObjectBase > m_flags           = new List< WebMapObjectBase >();

	private List< WebMapObjectBase > m_pins            = new List< WebMapObjectBase >();

	private List< WebMapObjectBase > m_selectables     = new List< WebMapObjectBase >( 2048 );

	private bool                     m_shouldReflectDB = false;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void CreateContentDespotitNode()
	{
		m_deposit = new GameObject( "Content" );

		m_deposit.transform.parent = transform;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void CreatePinSelectionHalo()
	{
		if( m_deposit != null )
		{
			GameObject mdl = Resources.Load< GameObject >( "3D/PinSelection/PinSelection" );

			Quaternion q   = Quaternion.AngleAxis( 90.0f, Vector3.right );

			m_objSelection = ( mdl != null ) ? GameObject.Instantiate( mdl, Vector3.up * 0.1f, q, m_deposit.transform ) as GameObject : null;

			if( m_objSelection != null )
			{
				m_objSelection.SetActive( false );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnDBEvent( DBObjects.EVT evt, params object[] paramsList )
	{
		if     ( evt == DBObjects.EVT.EVT_REFRESH_END    ) { SetShouldReflectDB(); }

		else if( evt == DBObjects.EVT.EVT_EDIT_BEGIN     ) {}

		else if( evt == DBObjects.EVT.EVT_EDIT_END       ) { SetShouldReflectDB(); }

		else if( evt == DBObjects.EVT.EVT_EDIT_CANCEL    ) {}

		else if( evt == DBObjects.EVT.EVT_OBJECT_CREATED ) { SetShouldReflectDB(); }

		else if( evt == DBObjects.EVT.EVT_OBJECT_DELETED ) { SetShouldReflectDB(); }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public WebMapObjectBase ReuseOrCreateMapObject( List< WebMapObjectBase > pool, List< WebMapObjectBase > list, OBJECT_TYPE type, CREATE_MODE mode, Localizable localizable )
	{
		WebMapObjectBase mapObj = null;

		if( ( pool != null ) && ( pool.Count > 0 ) )
		{
			mapObj = pool[ pool.Count - 1 ];

			pool.RemoveAt( pool.Count - 1 );

			if( mapObj != null )
			{
				mapObj.localizable = localizable;

				if( list != null ) list.Add( mapObj );
			}
		}
		else
		{
			mapObj = CreateMapObject( type, mode, localizable ); 
		}

		return mapObj;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void SetShouldReflectDB()
	{
		m_shouldReflectDB = true;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ReflectDB()
	{
		m_shouldReflectDB = false;

		List< WebMapObjectBase > reusableFlags = m_flags; 

		List< WebMapObjectBase > reusablePins  = m_pins;  

		m_flags = new List< WebMapObjectBase >();

		m_pins  = new List< WebMapObjectBase >();


		if( DBObjects.instance != null )
		{
			List< Localizable > sites  = DBObjects.instance.sites;

			List< Localizable > items  = DBObjects.instance.items;

			for( int site = 0; site < sites.Count; ++site ) ReuseOrCreateMapObject( reusableFlags, m_flags, OBJECT_TYPE.FLAG, CREATE_MODE.CREATE, sites[ site ] );

			for( int item = 0; item < items.Count; ++item ) ReuseOrCreateMapObject( reusablePins,  m_pins,  OBJECT_TYPE.PIN,  CREATE_MODE.CREATE, items[ item ] );
		}


		for( int unused = 0; unused < reusableFlags.Count; ++unused ) { GameObject.Destroy( reusableFlags[ unused ].gameObject ); }

		for( int unused = 0; unused < reusablePins.Count;  ++unused ) { GameObject.Destroy( reusablePins [ unused ].gameObject ); }

		reusableFlags.Clear();

		reusablePins.Clear ();

		UpdateMapObjectsPosition();


		Nameplates nameplates = ( ApplicationMain.instance != null ) && ( ApplicationMain.instance.ui != null ) ? ApplicationMain.instance.ui.nameplates : null;

		if( nameplates != null ) nameplates.Update();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateMapObjectPosition( Vector2 bounds, WebMapObjectBase mapObject )
	{
		Localizable loc = ( mapObject != null ) ? mapObject.localizable : null;

		if( loc != null )
		{
			Vector3 pos = GetRelativePositionFromGeoCoords( bounds, loc.m_coord.longitude.deg, loc.m_coord.latitude.deg );

			mapObject.transform.position = new Vector3( pos.x, 0.0f, pos.z );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateMapObjectsPosition()
	{
		UnityEngine.Plane[] planes = GeometryUtility.CalculateFrustumPlanes( m_camera );

		Vector2             bounds = new Vector2( m_BRCorner.x - m_TLCorner.x, m_TLCorner.z - m_BRCorner.z );

		WebMapObjectBase    mapObj = null;


		m_selectables.Clear();

		for( int flag = 0; flag < m_flags.Count; ++flag )
		{
			mapObj = m_flags[ flag ];

			UpdateMapObjectPosition( bounds, mapObj );

			mapObj.UpdateVisibilityStatus( planes );
		}

		for( int pin  = 0; pin  < m_pins.Count;  ++pin  )
		{
			mapObj = m_pins[ pin ];

			UpdateMapObjectPosition( bounds, mapObj );

			mapObj.UpdateVisibilityStatus( planes );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateMapObjectsSelection()
	{
		if( Focusable.any )
		{
			return;
		}

		if( DragDropOperation.pending )
		{
			if( m_objSelection != null ) m_objSelection.SetActive( false );

			return;
		}


		m_selectables.Clear();

		for( int flag = 0; flag < m_flags.Count; ++flag ) { m_flags[ flag ].UpdateSelectablesList( m_handpick.Point, m_selectables ); }

		for( int pin  = 0; pin  < m_pins.Count;  ++pin  ) { m_pins [ pin  ].UpdateSelectablesList( m_handpick.Point, m_selectables ); }


		float sqrMinDist = float.MaxValue;

		WebMapObjectBase selection = null;

		for( int sel = 0; sel < m_selectables.Count; ++sel )
		{
			if( sqrMinDist > m_selectables[ sel ].sqrDistFromCursor )
			{
				sqrMinDist = m_selectables[ sel ].sqrDistFromCursor;

				selection  = m_selectables[ sel ];
			}
		}


		if( m_objSelection != null )
		{
			m_objSelection.SetActive( selection != null );

			if( m_objSelection.activeInHierarchy )
			{
				float   selectionScaleModifier      = 0.7f + ( 0.3f * Mathf.Sin( Mathf.PI * ( ( Time.time % 0.250f ) / 0.250f ) ) );

				Vector3 selectionAnchorPos          = selection.transform.position;

				m_objSelection.transform.position   = new Vector3( selectionAnchorPos.x, 2.0f, selectionAnchorPos.z ); 

				m_objSelection.transform.localScale = XForm.LocalScaleForWorldScale( ( Vector3.one * selection.bounds.size.x ) * selectionScaleModifier, m_objSelection.transform );
			}
		}


		if( selection != null )
		{
			if( Input.GetMouseButtonDown( 0 ) )
			{
				if( HUDSites.Instance != null ) HUDSites.Instance.items.SelectItemUserDatas( selection.localizable );

				POPUPItem.Instance.Show( null, selection.localizable );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Localizable CreateMapObjectLocalizable( OBJECT_TYPE type )
	{
		Localizable localizable = null;

		object      session     = ( DBObjects.instance != null ) ? DBObjects.instance.BeginEdit() : null;


		if( session != null )
		{
			Localizable parent = DBObjects.instance.root;

			if( type == OBJECT_TYPE.PIN )
			{
				Localizable selection    = ( HUDSites.Instance != null ) && ( HUDSites.Instance.items.selection is Localizable ) ? HUDSites.Instance.items.selection as Localizable : null;

				Site        selectedSite = ( selection != null ) && ( selection is Site ) ? selection as Site : null;

				Item        selectedItem = ( selection != null ) && ( selection is Item ) ? selection as Item : null;
			
				if     ( selectedSite != null ) parent = selectedSite;

				else if( selectedItem != null ) parent = selectedItem.m_parent;
			}
		
			localizable = ( type == OBJECT_TYPE.FLAG ) ? DBObjects.instance.Create< Site >( null, null, session ) : DBObjects.instance.Create< Item >( parent, null, session );

			DBObjects.instance.EndEdit( session );
		}
		
		return localizable;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public WebMapObjectBase CreateMapObject( OBJECT_TYPE type, CREATE_MODE mode, Localizable localizable )
	{
		GameObject mdl = Resources.Load< GameObject >( m_mapObjectModel[ ( int )type ] );

		Quaternion q   = Quaternion.AngleAxis( -90.0f, Vector3.right );

		GameObject Obj = ( mdl != null ) ? GameObject.Instantiate( mdl, Vector3.up * 32.0f, q, m_deposit.transform ) as GameObject : null;

		if( Obj != null )
		{
			WebMapObjectBase mapObj = Obj.GetComponent< WebMapObjectBase >();

			if( mapObj != null )
			{
				mapObj.localizable = localizable;

				if( mode == CREATE_MODE.CREATE_DRAG )
				{
					if( DragDropOperation.Begin( this, mapObj ) == false )
					{
						DragDropOperation.Cancel();
					}
					else
					{
						UpdateDrag( mapObj );

						return    ( mapObj );
					}
				}
				else
				{
					List< WebMapObjectBase > list = ( type == OBJECT_TYPE.FLAG ) ? m_flags : m_pins;

					list.Add( mapObj );

					return  ( mapObj );
				}
			}
		}

		return null;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void DeleteMapObject( WebMapObjectBase mapObject )
	{
		if( mapObject != null )
		{
			List< WebMapObjectBase > list = ( mapObject.GetType() == typeof( WebMapFlag ) ) ? m_flags : m_pins;

			list.Remove       ( mapObject );

			GameObject.Destroy( mapObject.gameObject );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void DeleteMapObjects()
	{
		for( int flag = 0; flag < m_flags.Count; ++flag ) { GameObject.Destroy( m_flags[ flag ].gameObject ); }

		for( int pin  = 0; pin  < m_pins.Count;  ++pin  ) { GameObject.Destroy( m_pins [ pin  ].gameObject ); }

		m_flags.Clear();

		m_pins.Clear();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override bool BeginDrag( IDragDrop drag )
	{
		return ( drag is WebMapObjectBase );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void CancelDrag( IDragDrop drag )
	{
		if( drag is WebMapObjectBase )
		{
			DeleteMapObject( drag as WebMapObjectBase );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void UpdateDrag( IDragDrop drag )
	{
		if( drag is WebMapObjectBase )
		{
			( drag as WebMapObjectBase ).transform.position = m_handpick.Point + ( Vector3.up * 40.0f );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override bool AcceptDrop( IDragDrop drop )
	{
		if( drop is WebMapObjectBase )
		{
			WebMapObjectBase mapObject   = drop as WebMapObjectBase;

			Localizable      localizable = CreateMapObjectLocalizable( ( drop is WebMapFlag ) ? OBJECT_TYPE.FLAG : OBJECT_TYPE.PIN );

			if( localizable != null )
			{
				localizable.m_coord.latitude  = GetLatitudeFrom3DCoord ( mapObject.transform.position );

				localizable.m_coord.longitude = GetLongitudeFrom3DCoord( mapObject.transform.position );

				localizable.m_coord.altitude  = 0.0f;

				localizable.DBPush();
			}

			DeleteMapObject( mapObject );

			return true;
		}

		return false;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateDragDropInputs()
	{
		if( DragDropOperation.pending == false ) return;

		if     ( Input.GetKeyDown( KeyCode.Escape ) ) { DragDropOperation.Cancel();     }

		else if( Input.GetMouseButtonDown( 0 )      ) { DragDropOperation.Drop( this ); }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ContentProcessUICmd( UICMD cmd )
	{
		if     ( cmd == UICMD.MAP_ADD_FLAG ) { if( ( DBObjects.instance == null ) || ( DBObjects.instance.busy == false ) ) CreateMapObject( OBJECT_TYPE.FLAG, CREATE_MODE.CREATE_DRAG, null ); }

		else if( cmd == UICMD.MAP_ADD_PIN  ) { if( ( DBObjects.instance == null ) || ( DBObjects.instance.busy == false ) ) CreateMapObject( OBJECT_TYPE.PIN,  CREATE_MODE.CREATE_DRAG, null ); }
	}
}