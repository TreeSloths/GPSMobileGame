using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public enum UICMD
{
	NONE						,

	SWITCH_TO_GLOBE				,

	SWITCH_TO_MAP				,

	SWITCH_TO_OPTIONS			,

	SWITCH_TO_2D_MAP			,

	SWITCH_TO_3D_MAP			,

	MAP_ZOOM_INC				,

	MAP_ZOOM_DEC				,

	MAP_ADD_FLAG				,

	MAP_ADD_PIN					,

	DB_REFRESH					,

	GO_TO_SELECTION				,

	EDIT_SELECTION				,

	UI_SITES_EXPAND_ALL			,

	UI_SITES_COLLAPSE_ALL		,

	UI_SITES_SORT_ASCENDING		,

	UI_SITES_SORT_DESCENDING	,

	IMPORT_CSV					,

	EXPORT_CSV					,

	APPLICATION_EXIT			,

	NB
}

//********************************************************************************************************
//
//********************************************************************************************************

public class UICmd : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public delegate void OnUICmd( UICMD cmd );

	static private OnUICmd[] m_handler = new OnUICmd[ ( int )UICMD.NB ];

	static private UICMD     m_cmd     = UICMD.NONE;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public void SetUniqueHandler( UICMD cmd, OnUICmd handler )
	{
		m_handler[ ( int )cmd ] = handler;
	}

	static public void SetUniqueHandler( UICMD[] cmds, OnUICmd handler )
	{
		foreach( UICMD cmd in cmds ) m_handler[ ( int )cmd ] = handler;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Set( string cmd )
	{
		object val = System.Enum.Parse( typeof( UICMD ), cmd, true );

		m_cmd = ( val != null ) ? ( UICMD )val : UICMD.NONE;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public void Set( UICMD cmd )
	{
		m_cmd = cmd;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void LateUpdate()
	{
		if( m_cmd != UICMD.NONE )
		{
			if( m_handler[ ( int )m_cmd ] != null ) m_handler[ ( int )m_cmd ]( m_cmd );

			m_cmd = UICMD.NONE;
		}
	}
}