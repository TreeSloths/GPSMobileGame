using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class HUDSites : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	private enum BUTTON
	{
		REFRESH			,

		GOTO_SELECTION	,

		EDIT_SELECTION	,

		EXPAND_ALL		,

		COLLAPSE_ALL	,

		SORT_ASCENDING  ,

		SORT_DESCENDING ,

		NB_BUTTONS
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private class UIContextBackup
	{
		public long         selection = -1L;

		public List< long > expandeds = new List< long >();

		public void Clear() { selection = -1L; expandeds.Clear(); }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private const float FADE_IN_DURATION = 0.3f;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private HUDSites m_instance = null;

	static public  HUDSites Instance { get { return m_instance; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private UnityEngine.UI.RawImage      m_previewBG         = null;

	private UnityEngine.UI.RawImage      m_preview           = null;

	private GameObject                   m_noPic             = null;

	private UnityEngine.UI.Text          m_noPicLabel        = null;

	private CORE.UIButton[]              m_buttons           = new CORE.UIButton[ ( int )BUTTON.NB_BUTTONS ];

	private UIItemList< UISiteListItem > m_items             = null;

	private UIContextBackup              m_context           = new UIContextBackup();

	private bool                         m_rebuild           = false;

	private bool                         m_updatePreview     = false;

	private HTTPReq                      m_lastPicReq        = null;

	private bool                         m_initDefaultSelection = true;

	public  UIItemList< UISiteListItem > items { get { return m_items; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Awake()
	{
		if( m_instance == null ) m_instance = this;

		GameObject header    = CORE.HIERARCHY.Find( gameObject, "HEADER"    );

		GameObject hierarchy = CORE.HIERARCHY.Find( gameObject, "HIERARCHY" );

		m_previewBG          = CORE.HIERARCHY.FindComp< UnityEngine.UI.RawImage >( gameObject, "PREVIEW_BG" );

		m_preview            = CORE.HIERARCHY.FindComp< UnityEngine.UI.RawImage >( gameObject, "PREVIEW"    );

		m_noPic              = CORE.HIERARCHY.Find( m_preview.gameObject, "NO_PIC_AVAILABLE" );

		m_noPicLabel         = ( m_noPic != null ) ? m_noPic.GetComponent< UnityEngine.UI.Text >() : null;

		m_items              = new UIItemList< UISiteListItem >( gameObject, "Scroll View", "Viewport", "Content", "Scrollbar Vertical", "2D/UI/SiteListItem", OnListItemSelection );


		m_buttons[ ( int )BUTTON.REFRESH         ] = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( header,    "REFRESH"         ) );

		m_buttons[ ( int )BUTTON.GOTO_SELECTION  ] = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( header,    "GO_TO_SELECTION" ) );

		m_buttons[ ( int )BUTTON.EDIT_SELECTION  ] = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( header,    "EDIT_SELECTION"  ) );

		m_buttons[ ( int )BUTTON.EXPAND_ALL      ] = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( hierarchy, "EXPAND_ALL"      ) );

		m_buttons[ ( int )BUTTON.COLLAPSE_ALL    ] = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( hierarchy, "COLLAPSE_ALL"    ) );

		m_buttons[ ( int )BUTTON.SORT_ASCENDING  ] = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( hierarchy, "SORT_ASCENDING"  ) );

		m_buttons[ ( int )BUTTON.SORT_DESCENDING ] = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( hierarchy, "SORT_DESCENDING" ) );


		m_buttons[ ( int )BUTTON.REFRESH         ].SetListener( delegate { UICmd.Set( UICMD.DB_REFRESH               ); } );

		m_buttons[ ( int )BUTTON.GOTO_SELECTION  ].SetListener( delegate { UICmd.Set( UICMD.GO_TO_SELECTION          ); } );

		m_buttons[ ( int )BUTTON.EDIT_SELECTION  ].SetListener( delegate { UICmd.Set( UICMD.EDIT_SELECTION           ); } );

		m_buttons[ ( int )BUTTON.EXPAND_ALL      ].SetListener( delegate { UICmd.Set( UICMD.UI_SITES_EXPAND_ALL      ); } );

		m_buttons[ ( int )BUTTON.COLLAPSE_ALL    ].SetListener( delegate { UICmd.Set( UICMD.UI_SITES_COLLAPSE_ALL    ); } );

		m_buttons[ ( int )BUTTON.SORT_ASCENDING  ].SetListener( delegate { UICmd.Set( UICMD.UI_SITES_SORT_ASCENDING  ); } );

		m_buttons[ ( int )BUTTON.SORT_DESCENDING ].SetListener( delegate { UICmd.Set( UICMD.UI_SITES_SORT_DESCENDING ); } );


		UpdatePreview();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Start()
	{
		if( DBObjects.instance != null ) DBObjects.instance.listeners.Add( OnDBEvent );

		UICmd.SetUniqueHandler( UICMD.UI_SITES_EXPAND_ALL,	    ProcessUICmd );

		UICmd.SetUniqueHandler( UICMD.UI_SITES_COLLAPSE_ALL,    ProcessUICmd );

		UICmd.SetUniqueHandler( UICMD.UI_SITES_SORT_ASCENDING,  ProcessUICmd );

		UICmd.SetUniqueHandler( UICMD.UI_SITES_SORT_DESCENDING, ProcessUICmd );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnDestroy()
	{
		if( m_instance == this ) m_instance = null;

		if( DBObjects.instance != null ) DBObjects.instance.listeners.Remove( OnDBEvent );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnDBEvent( DBObjects.EVT evt, params object[] paramsList )
	{
		if     ( evt == DBObjects.EVT.EVT_REFRESH_BEGIN  ) { BackupUIContext  (); }

		else if( evt == DBObjects.EVT.EVT_REFRESH_END    ) { SetShouldRebuild (); }

		else if( evt == DBObjects.EVT.EVT_EDIT_BEGIN     ) { BackupUIContext  (); }

		else if( evt == DBObjects.EVT.EVT_EDIT_END       ) { SetShouldRebuild (); }

		else if( evt == DBObjects.EVT.EVT_EDIT_CANCEL    ) {}

		else if( evt == DBObjects.EVT.EVT_OBJECT_CREATED )
		{
			Localizable localizable = CORE.Params.Get< Localizable >( 0, paramsList );

			if( ( localizable != null ) && ( m_items != null ) )
			{
				BackupUISelection( localizable.id );
			}

			SetShouldRebuild();
		}
		else if( evt == DBObjects.EVT.EVT_OBJECT_DELETED )
		{
			Localizable localizable = CORE.Params.Get< Localizable >( 0, paramsList );

			if( ( localizable != null ) && ( m_items != null ) && ( m_items.selection == localizable ) )
			{
				BackupUISelection( -1L );
			}

			SetShouldRebuild();
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void SetItemsListDirty     () { if( m_items != null ) { m_items.dirty = true; } }

	private void SetShouldRebuild      () { m_rebuild       = true; }

	private void SetShouldUpdatePreview() { m_updatePreview = true; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void BackupUISelection( long id )
	{
		m_context.selection = id;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void BackupUIContext()
	{
		if( m_items == null ) return;

		m_context.selection = ( m_items.selection != null ) ? ( m_items.selection as Localizable ).id : -1L;

		Localizable root  = DBObjects.instance.root;

		if( root != null )
		{
			List< Localizable > scope = new List< Localizable >( root.m_childs );

			for( int index = 0; index < scope.Count; ++index )
			{
				scope.AddRange( scope[ index ].m_childs );

				if( ( scope[ index ].m_ListItemDatas != null ) && ( scope[ index ].m_ListItemDatas.expanded ) )
				{
					m_context.expandeds.Add( scope[ index ].id );
				}
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void RestoreUIContext()
	{
		if( m_items == null ) return;

		if( ( m_initDefaultSelection ) && ( m_context.selection == -1L ) )
		{
			if( ( DBObjects.instance != null ) && ( DBObjects.instance.sites.Count > 0 ) )
			{
				m_items.SelectItemUserDatas( DBObjects.instance.sites[ 0 ] );

				m_initDefaultSelection = false;
			}
		}
		else
		{
			object selection = Localizable.Get( m_context.selection );

			if( selection != null )
			{
				m_items.SelectItemUserDatas( selection );
			}
		}

		for( int expanded = 0; expanded < m_context.expandeds.Count; ++expanded )
		{
			Localizable localizable = Localizable.Get( m_context.expandeds[ expanded ] );

			if( ( localizable != null ) && ( localizable.m_ListItemDatas != null ) )
			{
				localizable.m_ListItemDatas.Expand( true, UIListItemData.RECURSE.NO );
			}
		}

		m_context.Clear();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void RebuildListItemDatasList()
	{
		if( DBObjects.instance == null ) return;

		m_rebuild = false;

		if( m_items != null )
		{
			Localizable root = DBObjects.instance.root;

			m_items.SetItemsDatas( root != null ? root.m_ListItemDatas : null );

			RestoreUIContext();

		    SetShouldUpdatePreview();
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ExpandAll()
	{
		Localizable root = DBObjects.instance.root;

		if( root != null )
		{
			root.m_ListItemDatas.Expand( true, UIListItemData.RECURSE.YES );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void CollapseAll()
	{
		Localizable root = DBObjects.instance.root;

		if( root != null )
		{
			root.m_ListItemDatas.Expand( false, UIListItemData.RECURSE.YES );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void SortAscending()
	{
		if( m_items != null ) m_items.Sort( SitesItemDatasComparison.CompareAscending );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void SortDescending()
	{
		if( m_items != null ) m_items.Sort( SitesItemDatasComparison.CompareDescending );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void UpdatePreviewLabel()
	{
		if( m_noPic != null )
		{
			m_noPic.SetActive( ( m_preview == null ) || ( m_preview.texture == null ) );

			if( ( m_noPic.activeSelf ) && ( m_noPicLabel != null ) )
			{
				m_noPic.transform.localScale = XForm.LocalScaleForWorldScale( Vector3.one, m_noPic.transform );

				object  selection = ( m_items   != null ) ? m_items.selection : null;

				m_noPicLabel.text = ( selection != null ) ? "No picture available" : "No selection";
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void UpdatePreviewPicture( string picture )
	{
		if( m_preview != null )
		{
			if( ( string.IsNullOrEmpty( picture ) == false ) && ( picture.CompareTo( "NONE" ) != 0 ) )
			{
				m_lastPicReq = new HTTPReq( "https://" + ApplicationMain.instance.options.DBInfos.server_adr + "/uploads/pictures/" + picture, HTTP_METHOD.GET, HTTP_CACHE_POLICY.IMAGE, null );

				m_lastPicReq.SubmitAsync( new HTTPReqDelegate( OnPicReqCompletion, null ) );
			}
			else
			{
				m_preview.texture = null;

				m_preview.color   = ( m_previewBG != null ) ? m_previewBG.color : Color.black;

				m_preview.rectTransform.localScale = Vector3.one;
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


			if( req.result == QUERY_RESULT.SUCCESS )
			{
				m_preview.color   = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

				m_preview.texture = CORE.UI.ClampTexture( req.texture, FilterMode.Bilinear );

				CORE.UI.Constrain(m_preview.rectTransform, req.texture, CORE.UI.PRESERVE_ASPECT.W );

				UpdatePreviewLabel();
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void UpdatePreview()
	{
		m_updatePreview         = false;

		object      selection   = ( m_items != null ) ? m_items.selection : null;

		Localizable localizable = ( ( selection != null ) && ( selection is Localizable ) ) ? selection as Localizable : null;

		UpdatePreviewPicture( ( localizable != null ) ? localizable.m_pic : null );

		UpdatePreviewLabel  ();
	}
		
	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnListItemSelection( object userDatas )
	{
		SetShouldUpdatePreview();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnEnable () { SetShouldRebuild(); }

	private void OnDisable() {}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void Update()
	{
		if( m_rebuild       ) RebuildListItemDatasList();
		
		if( m_updatePreview ) UpdatePreview();

		m_items.Update();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ProcessUICmd( UICMD cmd )
	{
		if( ( DBObjects.instance != null ) && ( DBObjects.instance.busy ) ) return;

		if     ( cmd == UICMD.UI_SITES_EXPAND_ALL	   ) { ExpandAll();      }

		else if( cmd == UICMD.UI_SITES_COLLAPSE_ALL	   ) { CollapseAll();    }

		else if( cmd == UICMD.UI_SITES_SORT_ASCENDING  ) { SortAscending();  }

		else if( cmd == UICMD.UI_SITES_SORT_DESCENDING ) { SortDescending(); }
	}
}
