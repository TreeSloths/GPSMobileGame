using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class POPUPFileBrowser : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum INTENT { LOAD, SAVE }

	public enum BUTON  { CANCEL, OK }

	public delegate void OnButon ( POPUPFileBrowser popup, BUTON buton );

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private const float FADE_IN_DURATION = 0.3f;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private POPUPFileBrowser m_instance = null;

	static public  POPUPFileBrowser Instance { get { return m_instance; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private CanvasGroup               m_group          = null;

	private UnityEngine.UI.Text       m_title          = null;

	private UnityEngine.UI.InputField m_inputFilter    = null;

	private UnityEngine.UI.InputField m_inputDirectory = null;

	private UnityEngine.UI.InputField m_inputFilename  = null;

	private UnityEngine.UI.RawImage   m_lockFilter     = null;

	private UnityEngine.UI.RawImage   m_lockDirectory  = null;

	private UnityEngine.UI.RawImage   m_lockFilename   = null;

	private CORE.UIButton             m_CANCEL         = null;

	private CORE.UIButton             m_OK             = null;

	private CORE.Fade                 m_fade           = new CORE.Fade();

	private string                    m_rootPath       = null;

	private string                    m_curFilter      = string.Empty;

	private OnButon                   m_onButon        = null;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private UIItemList< UIFileListItem > m_items = null;

	public  UIItemList< UIFileListItem > items     { get { return m_items; } }

	public string                        title     { get { return ( m_title != null ) ? m_title.text : string.Empty; } set { if( m_title != null ) { m_title.text = value; } } }

	public INTENT                        intent    { get; set; }

	public FSFilter                      filter    { get; set; }

	public string                        path      { get; set; }

	public string                        directory { get; set; }

	public string                        filename  { get; set; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Awake()
	{
		if( m_instance == null ) m_instance = this;

		m_group                 = GetComponent< CanvasGroup >();
		
		GameObject root         = gameObject;

		GameObject buts         = CORE.HIERARCHY.Find( root, "Buttons"      );

		GameObject GRPFilter    = CORE.HIERARCHY.Find( root, "GRPFilter"    );

		GameObject GRPDirectory = CORE.HIERARCHY.Find( root, "GRPDirectory" );

		GameObject GRPFilename  = CORE.HIERARCHY.Find( root, "GRPFilename"  );


		m_title           = CORE.HIERARCHY.FindComp< UnityEngine.UI.Text       >( root,         "Title"     );

		m_inputFilter     = CORE.HIERARCHY.FindComp< UnityEngine.UI.InputField >( GRPFilter,    "Filter"    );

		m_inputDirectory  = CORE.HIERARCHY.FindComp< UnityEngine.UI.InputField >( GRPDirectory, "Directory" );

		m_inputFilename   = CORE.HIERARCHY.FindComp< UnityEngine.UI.InputField >( GRPFilename,  "Filename"  );

		m_lockFilter      = CORE.HIERARCHY.FindComp< UnityEngine.UI.RawImage   >( GRPFilter,    "LockIcon"  );

		m_lockDirectory   = CORE.HIERARCHY.FindComp< UnityEngine.UI.RawImage   >( GRPDirectory, "LockIcon"  );

		m_lockFilename    = CORE.HIERARCHY.FindComp< UnityEngine.UI.RawImage   >( GRPFilename,  "LockIcon"  );

		m_CANCEL          = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( buts, "CANCEL" ) );

		m_OK              = new CORE.UIButton( CORE.HIERARCHY.FindComp< UnityEngine.UI.Button >( buts, "OK"     ) );

		m_items           = new UIItemList< UIFileListItem >( gameObject, "Scroll View", "Viewport", "Content", "Scrollbar Vertical", "2D/UI/FileListItem", OnListItemSelection );


		m_CANCEL.SetListener( delegate { OnCANCEL(); } );

		m_OK.SetListener    ( delegate { OnOK();     } );


		if( m_inputFilter    != null ) { m_inputFilter.onEndEdit.RemoveAllListeners();    m_inputFilter.onEndEdit.AddListener   ( delegate { OnEndEditFilter();    } ); }

		if( m_inputDirectory != null ) { m_inputDirectory.onEndEdit.RemoveAllListeners(); m_inputDirectory.onEndEdit.AddListener( delegate { OnEndEditDirectory(); } ); }

		if( m_inputFilename  != null ) { m_inputFilename.onEndEdit.RemoveAllListeners();  m_inputFilename.onEndEdit.AddListener ( delegate { OnEndEditFilename();  } ); }


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

	private void EnableButons( bool enable )
	{
		if( m_CANCEL != null ) m_CANCEL.interactable = enable;

		if( m_OK     != null ) m_OK.interactable     = enable;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnFadeFinished( FADE_TYPE type )
	{
		if( type == FADE_TYPE.FADE_IN )
		{
			if( m_CANCEL != null ) m_CANCEL.interactable = true;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void InitInputFields()
	{
		if( m_inputFilter    != null ) { m_inputFilter.text    = filter.showFiles ? filter.pattern  : "*"; m_inputFilter.interactable    = filter.allowFileSelection; }

		if( m_inputDirectory != null ) { m_inputDirectory.text = string.Empty;                             m_inputDirectory.interactable = filter.allowDirSelection;  }

		if( m_inputFilename  != null ) { m_inputFilename.text  = string.Empty;                             m_inputFilename.interactable  = filter.allowFileSelection; }


		bool lockedFilter    = ( m_inputFilter   != null ) && ( m_inputFilter.interactable    == false );

		bool lockedDirectory = ( m_lockDirectory != null ) && ( m_inputDirectory.interactable == false );

		bool lockedFilename  = ( m_lockFilename  != null ) && ( m_inputFilename.interactable  == false );

		if( m_lockFilter    != null ) { m_lockFilter.uvRect    = new Rect( new Vector2( lockedFilter    ? 0.5f : 0.0f, 0.0f ), new Vector2( 0.5f, 1.0f ) ); }

		if( m_lockDirectory != null ) { m_lockDirectory.uvRect = new Rect( new Vector2( lockedDirectory ? 0.5f : 0.0f, 0.0f ), new Vector2( 0.5f, 1.0f ) ); }

		if( m_lockFilename  != null ) { m_lockFilename.uvRect  = new Rect( new Vector2( lockedFilename  ? 0.5f : 0.0f, 0.0f ), new Vector2( 0.5f, 1.0f ) ); }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool Show( string rootPath, INTENT paramIntent, FSFilter paramFilter, OnButon onButon )
	{
		if( gameObject.activeSelf == false )
		{
			m_rootPath  = rootPath;

			m_onButon   = onButon;

			intent      = paramIntent;

			filter      = ( paramFilter != null ) ? paramFilter.byval : new FSFilter( FSFilter.LOD.FILE, FSFilter.LOD.FILE, "*" );

			m_curFilter = filter.pattern;

			InitInputFields();

			UpdateTitle();


			m_fade.Begin( FADE_TYPE.FADE_IN, FADE_IN_DURATION, OnFadeFinished );

			Update();


			gameObject.SetActive( true  );

			EnableButons        ( false );

			return true;
		}

		return false;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void UpdateTitle()
	{
		title = FSFilter.AllowFileSelection( filter ) ? "FILES" : "DIRECTORIES";
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void RebuildListItemDatasList()
	{
		if( m_items != null )
		{
			m_items.Clear();

			FileSystemEntry root = new FileSystemEntry( null, filter );

			root.BuildFilesList  ( m_rootPath );

			m_items.SetItemsDatas( root );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void UpdatePathFromInputs()
	{
		if( m_inputDirectory == null )                      return;

		if( m_inputFilename  == null )                      return;

		if( string.IsNullOrEmpty( m_inputDirectory.text ) ) return;

		if( string.IsNullOrEmpty( m_inputFilename.text ) )  return;


		string d = m_inputDirectory.text;

		string f = m_inputFilename.text;

		string p = System.IO.Path.Combine( d, f );


		if( filter.allowDirSelection )
		{
			if( ( new DirectoryInfo( d ) ).Exists )
			{
				directory = d;

				filename  = f;

				path      = d;

				return;
			}
		}
		else
		{
			if( ( intent == INTENT.SAVE ) || ( ( new FileInfo( p ) ).Exists ) )
			{
				directory = d;

				filename  = f;

				path      = p;

				return;
			}
		}

		m_inputDirectory.text = directory;

		m_inputFilename.text  = filename;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnEndEditFilter()
	{
		filter.pattern = m_inputFilter.text;

		if( m_curFilter.CompareTo( filter.pattern ) != 0 )
		{
			m_curFilter       = filter.pattern;

			m_OK.interactable = false;

			RebuildListItemDatasList();
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnEndEditDirectory()
	{
		UpdatePathFromInputs();

		UpdateButtonOKState ();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnEndEditFilename()
	{
		UpdatePathFromInputs();

		UpdateButtonOKState ();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void UpdateButtonOKState()
	{
		bool validDirectory = ( string.IsNullOrEmpty( directory ) == false );

		bool validFilename  = ( string.IsNullOrEmpty( filename  ) == false );


		if( m_OK != null )
		{
			m_OK.interactable = ( ( ( validDirectory ) && ( filter.allowDirSelection  ) ) || 
				
				                  ( ( validFilename ) && ( filter.allowFileSelection ) ) );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnListItemSelection( object userDatas )
	{
		FileSystemEntry entry = ( userDatas != null ) && ( userDatas is FileSystemEntry ) ? userDatas as FileSystemEntry : null;

		path      = ( entry != null ) ? entry.path : string.Empty;

		directory = ( entry != null ) ? ( entry.dir ? path         : System.IO.Path.GetDirectoryName( path ) ) : string.Empty;

		filename  = ( entry != null ) ? ( entry.dir ? string.Empty : System.IO.Path.GetFileName     ( path ) ) : string.Empty;

		if( m_inputDirectory != null ) m_inputDirectory.text = directory;

		if( m_inputFilename  != null ) m_inputFilename.text  = filename;

		UpdateButtonOKState();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnEnable () { RebuildListItemDatasList(); }

	private void OnDisable() { m_fade.Begin( FADE_TYPE.FADE_OUT, 0.0f, null ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnCANCEL() { if( m_onButon != null ) { m_onButon( this, BUTON.CANCEL ); m_onButon = null; } gameObject.SetActive( false ); }

	public void OnOK()     { if( m_onButon != null ) { m_onButon( this, BUTON.OK     ); m_onButon = null; } gameObject.SetActive( false ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void Update()
	{
		m_fade.Update();

		if( m_group != null ) { if( ( m_group.alpha = ( 1.0f - m_fade.alpha ) ) < 1.0f ) return; }

		m_items.Update();
	}
}
