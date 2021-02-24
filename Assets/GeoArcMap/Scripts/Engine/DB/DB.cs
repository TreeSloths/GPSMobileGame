using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public interface IDBObject
{
	HTTPReq      MakeQuery_DBInsert ();

	HTTPReq      MakeQuery_DBDelete ();

	HTTPReq      MakeQuery_DBPush   ();

	HTTPReq      MakeQuery_DBPull   ();

	void         OnDBDPush          ( HTTPReq req );

	void         OnDBPull           ( HTTPReq req );


	void         Async_DBInsert     ( HTTPReqDelegate onCompletion );

	void         Async_DBDelete     ( HTTPReqDelegate onCompletion );

	void         Async_DBPush       ( HTTPReqDelegate onCompletion );

	void         Async_DBPull       ( HTTPReqDelegate onCompletion );


	QUERY_RESULT DBInsert (); // insert entry in database

	QUERY_RESULT DBDelete (); // delete entry in database

	QUERY_RESULT DBPush   (); // update  database entry

	QUERY_RESULT DBPull   (); // reflect database entry
}

//********************************************************************************************************
//
//********************************************************************************************************

public class DBInfos
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	private string m_server_adr  = string.Empty;

	private int    m_api_version = 1;

	private string m_url         = string.Empty;

	private bool   m_dirty       = true;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public string server_adr  { get { return m_server_adr;  } set { m_server_adr  = value; m_dirty = true; } }

	public int    api_version { get { return m_api_version; } set { m_api_version = value; m_dirty = true; } }

	public string url         { get { if( m_dirty ) {  m_url = "https://" + server_adr + "/api/v" + api_version.ToString(); } return m_url; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public DBInfos byval
	{
		get
		{
			DBInfos o = new DBInfos();

			o.server_adr  = server_adr;

			o.api_version = api_version;

			return o;
		}
	}
}
