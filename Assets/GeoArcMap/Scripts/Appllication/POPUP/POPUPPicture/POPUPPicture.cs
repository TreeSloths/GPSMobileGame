using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class POPUPPicture : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum BUTTON
	{
		BT00  ,

		BT01  ,

		BT02  ,

		BT03  ,

		NB_BUTTONS
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum ACTION
	{
		DELETE,

		UPLOAD,

		CANCEL, 

		SELECT,

		NB_ACTIONS
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	[ System.Flags ] public enum ACTION_FLAG
	{
		NONE   = 0x0,

		DELETE = 1 << ACTION.DELETE,

		UPLOAD = 1 << ACTION.UPLOAD,

		CANCEL = 1 << ACTION.CANCEL,

		SELECT = 1 << ACTION.SELECT,

		ALL    = int.MaxValue
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public delegate void OnAction( POPUPPicture popup, ACTION action );

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private const float FADE_IN_DURATION = 0.3f;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private POPUPPicture m_instance = null;

	static public  POPUPPicture Instance { get { return m_instance; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private CanvasGroup					m_group             = null;

	private UnityEngine.UI.Text			m_title             = null;

	private CORE.UIButton[]				m_buttons           = new CORE.UIButton[ ( int )BUTTON.NB_BUTTONS ];

	private ACTION[]                    m_actions           = new ACTION       [ ( int )BUTTON.NB_BUTTONS ];

	private CORE.Fade					m_fade              = new CORE.Fade();

	private UIItemList< UIPicListItem >	m_items             = null;

	private string						m_picture           = "NONE";

	private ACTION_FLAG                 m_possibleActions   = ACTION_FLAG.NONE;

	private OnAction					m_onAction          = null;

	private HTTPReq                     m_httpReq           = null;

	private UIItemListDeferredSelection m_deferredSelection = new UIItemListDeferredSelection();

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private HTTPReq httpReq
	{
		get { return m_httpReq; }

		set
		{
			if( m_httpReq != value )
			{
				m_httpReq  = value;

				EnableActions( ( m_httpReq != null ) ? ACTION_FLAG.NONE : m_possibleActions );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public string  title   { get { return ( m_title != null ) ? m_title.text : string.Empty; } set { if( m_title != null ) { m_title.text = value; } } }

	public string  picture { get { return m_picture; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Awake()
	{
		if( m_instance == null ) m_instance = this;

		m_group         = GetComponent< CanvasGroup >();
		
		GameObject root = gameObject;

		GameObject buts = CORE.HIERARCHY.Find( root, "Buttons" );

		m_title         = CORE.HIERARCHY.FindComp< UnityEngine.UI.Text >( root, "Title"   );

		m_items         = new UIItemList< UIPicListItem >( root, "Scroll View", "Viewport", "Content", "Scrollbar Vertical", "2D/UI/Picture", OnSelectListItem );


		for( int but = 0; but < ( int )BUTTON.NB_BUTTONS; ++but )
		{
			BUTTON button = ( BUTTON )but;

			m_buttons[ ( int )but ] = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( buts, string.Format( "BT{0:D2}", but ) ) );

			m_buttons[ ( int )but ].SetListener( delegate { OnButton( button ); } );
		}


		MapActionOnButton( BUTTON.BT00, ACTION.DELETE );

		MapActionOnButton( BUTTON.BT01, ACTION.UPLOAD );

		MapActionOnButton( BUTTON.BT02, ACTION.CANCEL );

		MapActionOnButton( BUTTON.BT03, ACTION.SELECT );


		httpReq = null;

		m_fade.Begin( FADE_TYPE.FADE_OUT, 0.0f );

		gameObject.SetActive( false );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnDestroy()
	{
		if( m_instance == this ) m_instance = null;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void MapActionOnButton( BUTTON but, ACTION act )
	{
		m_actions[ ( int )but ] = act;

		if( m_buttons[ ( int )but ].txt != null )
		{
			m_buttons[ ( int )but ].txt.text        = act.ToString();

			m_buttons[ ( int )but ].enabledTxtColor = ( act != ACTION.DELETE ) ? Color.white : new Color( 1.0f, 0.2f, 0.2f, 1.0f );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private ACTION GetActionForButton( BUTTON but )
	{
		return m_actions[ ( int )but ];
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void NotifyActionListener( ACTION act )
	{
		if( m_onAction != null )
		{
			m_onAction( this, act );

			m_onAction = null;
		}

		gameObject.SetActive( false );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void EnableActions( ACTION_FLAG selection )
	{
		for( int but = 0; but < ( int )BUTTON.NB_BUTTONS; ++but )
		{
			ACTION action = GetActionForButton( ( BUTTON )but );

			m_buttons[ but ].interactable = ( ( selection & ( ( ACTION_FLAG )( 1 << ( int )action ) ) ) != 0 );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private ACTION_FLAG GetActionsForSelection()
	{
		ACTION_FLAG actions = ( ACTION_FLAG.UPLOAD | ACTION_FLAG.CANCEL );

		if( string.IsNullOrEmpty( m_picture ) == false )
		{
			actions |= ACTION_FLAG.SELECT;

			if( m_picture.CompareTo( "NONE" ) != 0 ) actions |= ACTION_FLAG.DELETE;
		}

		return actions;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void CacheSelection()
	{
		m_deferredSelection.Consume();

		m_picture         = ( m_items != null ) && ( m_items.selection != null ) ? m_items.selection as string : string.Empty;

		m_possibleActions = GetActionsForSelection();

		EnableActions( m_possibleActions );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool Show( OnAction onButon )
	{
		if( gameObject.activeSelf == false )
		{
			m_onAction = onButon;

			gameObject.SetActive( true );

			return true;
		}

		return false;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void UpdatePicturesList()
	{
		if( httpReq == null )
		{
			if( m_items != null )
			{
				m_items.Clear();

				httpReq = new HTTPReq( "https://" + ApplicationMain.instance.options.DBInfos.server_adr + "/picture_list.php", HTTP_METHOD.GET );

				httpReq.SubmitAsync( new HTTPReqDelegate( OnPicListReqCompletion, null ) );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnPicListReqCompletion( HTTPReq req, params object[] paramsList )
	{
		if( ( gameObject.activeInHierarchy ) && ( req.result == QUERY_RESULT.SUCCESS ) )
		{
			string[] files = req.response.Split( new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries );

			m_items.datas.Add( new UIListItemData( null, "NONE" ) );

			for( int file  = 0; file < files.Length; ++file )
			{
				m_items.datas.Add( new UIListItemData( null, files[ file ].TrimEnd( ',' ) ) );
			}

			m_items.SetDirty();
		}

		httpReq = null;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void PictureUPLOAD()
	{
		Application.OpenURL( "https://" + ApplicationMain.instance.options.DBInfos.server_adr + "/picture_upload.html" ); 
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void PictureDELETE()
	{
		if( httpReq == null )
		{
			httpReq = new HTTPReq( "https://" + ApplicationMain.instance.options.DBInfos.server_adr + "/picture_delete.php", HTTP_METHOD.POST );

			httpReq.AddParameter( "filename", m_picture, QUERY_PARAM_TYPE.STRING );

			httpReq.SubmitAsync( new HTTPReqDelegate( OnPicDeleteReqCompletion, null ) );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnPicDeleteReqCompletion( HTTPReq req, params object[] paramsList )
	{
		if( ( gameObject.activeInHierarchy ) && ( req.result == QUERY_RESULT.SUCCESS ) )
		{
			NotifyActionListener( ACTION.DELETE );

			gameObject.SetActive( false );
		}

		httpReq = null;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnEnable()
	{
		CacheSelection    ();

		UpdatePicturesList();

		m_fade.Begin( FADE_TYPE.FADE_IN, FADE_IN_DURATION, null );

		Update();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnDisable()
	{
		httpReq = null;

		m_fade.Begin( FADE_TYPE.FADE_OUT, 0.0f, null );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnSelectListItem( object userDatas )
	{
		if( httpReq != null ) { m_deferredSelection.Value = userDatas; }

		else                  { CacheSelection(); }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnButton( BUTTON but )
	{
		if( httpReq != null ) return;

		ACTION  action = GetActionForButton( but );

		switch( action )
		{
			case ACTION.DELETE: { PictureDELETE(); break; }

			case ACTION.UPLOAD: { PictureUPLOAD(); NotifyActionListener( action ); break; }

			case ACTION.SELECT: {                  NotifyActionListener( action ); break; }

			case ACTION.CANCEL: {                  NotifyActionListener( action ); break; }

			default: break;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void Update()
	{
		if( m_deferredSelection.Active ) CacheSelection();

		m_fade.Update();

		if( m_group != null ) { if( ( m_group.alpha = ( 1.0f - m_fade.alpha ) ) < 1.0f ) return; }

		m_items.Update();
	}
}
