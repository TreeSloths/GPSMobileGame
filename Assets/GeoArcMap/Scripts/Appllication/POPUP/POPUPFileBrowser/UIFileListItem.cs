using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class FSFilter
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum LOD { DIRECTORY, FILE }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private LOD    m_selectionLevel = LOD.FILE;

	private LOD    m_displayLevel   = LOD.FILE;

	private string m_pattern        = "*";

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public FSFilter( LOD paramSelectionLevel = LOD.FILE, LOD paramDisplayLevel = LOD.FILE, string paramPattern = "*" )
	{
		selectionLevel = paramSelectionLevel;

		displayLevel   = paramDisplayLevel;

		pattern        = paramPattern;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public FSFilter( LOD paramSelectionLevel, string paramPattern = "*" )
	{
		selectionLevel = paramSelectionLevel;

		displayLevel   = LOD.FILE;

		pattern        = paramPattern;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public FSFilter( FSFilter o )
	{
		m_selectionLevel = o.m_selectionLevel;

		m_displayLevel   = o.m_displayLevel;

		m_pattern        = o.m_pattern;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public LOD selectionLevel
	{
		get { return m_selectionLevel;  }

		set { m_selectionLevel = value; if( m_displayLevel < m_selectionLevel ) m_displayLevel = m_selectionLevel; }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public LOD displayLevel
	{
		get { return m_displayLevel; }

		set
		{
			m_displayLevel = ( value < m_selectionLevel ) ? m_selectionLevel : value;

			if( m_displayLevel == LOD.DIRECTORY ) m_pattern = "*";
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public string pattern
	{
		get { return m_pattern; }

		set { if( m_displayLevel == LOD.FILE ) { string Value = value.Trim(); m_pattern = string.IsNullOrEmpty( Value ) ? "*" : Value; } }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public        bool allowDirSelection                     { get { return m_selectionLevel == LOD.DIRECTORY; } }

	public        bool allowFileSelection                    { get { return m_selectionLevel == LOD.FILE;      } }

	static public bool AllowDirSelection ( FSFilter filter ) { return ( filter != null ) && ( filter.allowDirSelection  ); }

	static public bool AllowFileSelection( FSFilter filter ) { return ( filter == null ) || ( filter.allowFileSelection ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public        bool hideFiles                             { get { return m_displayLevel <  LOD.FILE; } }

	public        bool showFiles                             { get { return m_displayLevel >= LOD.FILE; } }

	static public bool HideFiles( FSFilter filter )          { return ( filter != null ) && ( filter.hideFiles ); }

	static public bool ShowFiles( FSFilter filter )          { return ( filter == null ) || ( filter.showFiles ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public FSFilter byval { get { FSFilter o = new FSFilter( this ); return o; } }
}

//********************************************************************************************************
//
//********************************************************************************************************

public class FileSystemEntry : UIListItemData
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public FileSystemEntry( UIListItemData parent, FSFilter paramFilter = null ) : base( parent, null )
	{
		userDatas     = this;

		dir           = false;

		listDirs      = false;

		listFiles     = false;

		name          = string.Empty;

		path          = string.Empty;

		displayFilter = paramFilter;
	}

	public bool          dir           { get; set; }

	public string        name          { get; set; }

	public string        path          { get; set; }

	private bool         listDirs      { get; set; }

	private bool         listFiles     { get; set; }

	public FSFilter      displayFilter { get; set; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void AddEntry( FileSystemEntry parent, FileSystemInfo info, FSFilter paramFilter )
	{
		DirectoryInfo   directory = info is DirectoryInfo ? info as DirectoryInfo : null;

		FileSystemEntry entry     = new FileSystemEntry( parent, paramFilter );


		entry.dir  = ( directory != null );

		entry.path = info.FullName;

		entry.name = info.Name;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void AddDirectories()
	{
		if( listDirs == false )
		{
			listDirs = true;

			DirectoryInfo   dirInfo  = new DirectoryInfo( path );

			DirectoryInfo[] dirInfos = dirInfo.GetDirectories( "*", SearchOption.TopDirectoryOnly );

			for( int directory = 0; directory < dirInfos.Length; ++directory )
			{
				AddEntry( this, dirInfos[ directory ], displayFilter );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void AddFiles()
	{
		if( FSFilter.HideFiles( displayFilter ) ) return;

		if( listFiles == false )
		{
			listFiles = true;

			DirectoryInfo dirInfo   = new DirectoryInfo( path );

			FileInfo[]    fileInfos = ( displayFilter != null ) ? dirInfo.GetFiles( displayFilter.pattern, SearchOption.TopDirectoryOnly ) : dirInfo.GetFiles();

			for( int file = 0; file < fileInfos.Length; ++file )
			{
				AddEntry( this, fileInfos[ file ], displayFilter );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void RemoveFiles()
	{
		if( listFiles == true )
		{
			listFiles = false;

			for( int child = 0; child < childs.Count; ++child )
			{
				FileSystemEntry entry = childs[ child ] as FileSystemEntry;

				if( entry.dir == false )
				{
					childs.RemoveRange( child, childs.Count - child );

					break;
				}
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void RemoveAll()
	{
		childs.Clear();

		listDirs  = false;

		listFiles = false;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void BuildFilesList( string paramPath )
	{
		if( parent == null )
		{
			name = paramPath;

			path = paramPath;

			AddDirectories();

			AddFiles();
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void ExpandOverride()   { if( dir ) { AddDirectories(); AddFiles(); } }

	public override void CollapseOverride() { if( dir ) { RemoveAll();                  } }
}

//********************************************************************************************************
//
//********************************************************************************************************

public class UIFileListItem : UIListItem
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private Color m_iconColorDir  = new Color( 1.0f, 0.64f,  0.0f, 1.0f );

	static private Color m_iconColorFile = new Color( 1.0f, 0.81f, 0.48f, 1.0f );

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void Setup( UIItemListBase list, int paramIndex, GameObject paramObj )
	{
		base.Setup( list, paramIndex, paramObj );

		icon = ( obj != null ) ? CORE.HIERARCHY.FindComp< UnityEngine.UI.RawImage >( obj, "Icon" ) : null;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public UnityEngine.UI.RawImage icon { get; set; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override bool ShowExpandButtonOverride()
	{
		FileSystemEntry entry = ( userDatas != null ) && ( userDatas is FileSystemEntry ) ? userDatas as FileSystemEntry : null;

		return ( entry == null ) || ( entry.dir );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void ReflectDatasOverride()
	{
		FileSystemEntry entry = ( userDatas != null ) && ( userDatas is FileSystemEntry ) ? userDatas as FileSystemEntry : null;

		bool            dir   = ( entry == null ) || ( entry.dir );

		if( txt  != null ) txt.text = ( entry != null ) ? entry.name : string.Empty;

		if( icon != null )
		{
			icon.uvRect = new Rect( new Vector2( dir ? 0.0f : 0.5f, 0.0f ), new Vector2( 0.5f, 1.0f ) );

			icon.color  = dir ? m_iconColorDir : m_iconColorFile;
		}
	}
}
