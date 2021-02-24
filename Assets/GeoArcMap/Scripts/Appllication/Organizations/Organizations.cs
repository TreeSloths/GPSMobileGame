using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class Organization : Localizable, IDBObject
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public System.DateTime m_dateFunded = System.DateTime.Now;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Organization() {}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override HTTPReq MakeQuery_DBInsert()
	{
		HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/organization_create.php", HTTP_METHOD.POST );

		query.AddParameter( "id", id.ToString(), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "date_created", string.Format( "{0}-{1}-{2}", m_dateCreated.Year, m_dateCreated.Month, m_dateCreated.Day ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "date_funded",  string.Format( "{0}-{1}-{2}", m_dateFunded.Year,  m_dateFunded.Month,  m_dateFunded.Day  ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "name", name,   QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "dsc",  m_desc, QUERY_PARAM_TYPE.STRING );

		return query;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override HTTPReq MakeQuery_DBDelete()
	{
		HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/generic_delete.php", HTTP_METHOD.POST );

		query.AddParameter( "id",	 id.ToString() ,   QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "table", "organizations" , QUERY_PARAM_TYPE.VALUE );

		return query;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override HTTPReq MakeQuery_DBPush()
	{
		HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/organization_update.php", HTTP_METHOD.POST );

		query.AddParameter( "id",           id.ToString() , QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "date_funded",  string.Format( "{0}-{1}-{2}", m_dateFunded.Year, m_dateFunded.Month, m_dateFunded.Day ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "name",         name,   QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "dsc",	        m_desc, QUERY_PARAM_TYPE.STRING );

		return query;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override HTTPReq MakeQuery_DBPull()
	{
		HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/generic_retrieve.php", HTTP_METHOD.GET );

		query.AddParameter( "id",	 id.ToString(),   QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "table", "organizations", QUERY_PARAM_TYPE.VALUE );

		return query;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void OnDBPull( HTTPReq req )
	{
		JSon json = new JSon( req.response );

		if( json[ "error" ] != null )              return;

		if( ( json.root is JSonObject ) == false ) return;


		JSonObject o    = json.root as JSonObject;

		System.DateTime.TryParse( o[ "date_funded" ].value, out m_dateFunded );

		name   = o[ "name" ].value;

		m_desc = o[ "dsc"  ].value;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public void DBList( int start = 0, int end = int.MaxValue, HTTPReqDelegate handler = null )
	{
		if( handler != null )
		{
			HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/organization_list.php", HTTP_METHOD.GET );

			query.SubmitAsync( handler );
		}
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class Employee : Localizable, IDBObject
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum RANK { UNKNOWN, ASSISTANT, EMPLOYEE, DIRECTOR }

	public enum TYPE { UNKNOWN, SERVICE_PROVIDER, CORPORATE   }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public string m_loginUsername = string.Empty;

	public string m_loginPassword = string.Empty;

	public string m_organization  = string.Empty;

	public RANK   m_rank          = RANK.UNKNOWN;

	public TYPE   m_type          = TYPE.UNKNOWN;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Employee() {}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override HTTPReq MakeQuery_DBInsert()
	{
		HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/account_create.php", HTTP_METHOD.POST );

		query.AddParameter( "id",               id.ToString(),      QUERY_PARAM_TYPE.VALUE  );

		query.AddParameter( "date_created",     string.Format( "{0}-{1}-{2}", m_dateCreated.Year, m_dateCreated.Month, m_dateCreated.Day ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "login_username",	m_loginUsername,	QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "login_password",	m_loginPassword,	QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "name",				name,				QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "rank",				m_rank.ToString(),	QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "type",				m_type.ToString(),	QUERY_PARAM_TYPE.STRING );

		return query;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override HTTPReq MakeQuery_DBDelete()
	{
		HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/generic_delete.php", HTTP_METHOD.POST );

		query.AddParameter( "id",	 id.ToString(), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "table", "employees"  , QUERY_PARAM_TYPE.VALUE );

		return query;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override HTTPReq MakeQuery_DBPush()
	{
		HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/account_update.php", HTTP_METHOD.POST );

		query.AddParameter( "id",               id.ToString(),      QUERY_PARAM_TYPE.VALUE  );

		query.AddParameter( "login_username",	m_loginUsername,	QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "login_password",	m_loginPassword,	QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "name",				name,				QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "rank",				m_rank.ToString(),	QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "type",				m_type.ToString(),	QUERY_PARAM_TYPE.STRING );

		return query;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override HTTPReq MakeQuery_DBPull()
	{
		HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/generic_retrieve.php", HTTP_METHOD.GET );

		query.AddParameter( "id",	 id.ToString(), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "table", "employees"  , QUERY_PARAM_TYPE.VALUE );

		return query;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void OnDBPull( HTTPReq req )
	{
		JSon json = new JSon( req.response );

		if( json[ "error" ] != null )              return;

		if( ( json.root is JSonObject ) == false ) return;


		JSonObject o    = json.root as JSonObject;

		m_loginUsername = o[ "login_username" ].value;

		m_loginPassword = o[ "login_password" ].value;

		name            = o[ "name"           ].value;

		m_rank          = ( RANK ) System.Enum.Parse( typeof( RANK ), o[ "rank" ].value, true );

		m_type          = ( TYPE ) System.Enum.Parse( typeof( TYPE ), o[ "type" ].value, true );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public void DBList( int start = 0, int end = int.MaxValue, HTTPReqDelegate handler = null )
	{
		if( handler != null )
		{
			HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/account_list.php", HTTP_METHOD.GET );

			query.SubmitAsync( handler );
		}
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class Site : Localizable, IDBObject
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public System.DateTime	m_dateOpened = System.DateTime.Now;

	public System.DateTime	m_dateClosed = System.DateTime.Now;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Site() {}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	protected override bool AcceptChild( Localizable child )
	{
		return child is Item;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override HTTPReq MakeQuery_DBInsert()
	{
		HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/site_create.php", HTTP_METHOD.POST );

		query.AddParameter( "id",           id.ToString(), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "date_created", string.Format( "{0}-{1}-{2}", m_dateCreated.Year, m_dateCreated.Month, m_dateCreated.Day ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "date_opened",  string.Format( "{0}-{1}-{2}", m_dateOpened.Year,  m_dateOpened.Month,  m_dateOpened.Day  ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "date_closed",  string.Format( "{0}-{1}-{2}", m_dateClosed.Year,  m_dateClosed.Month,  m_dateClosed.Day  ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "name",         name,   QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "dsc",          m_desc, QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "lat",          string.Format( "{0:F9}", m_coord.latitude.deg  ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "lng",          string.Format( "{0:F9}", m_coord.longitude.deg ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "alt",          string.Format( "{0:F9}", m_coord.altitude      ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "pic",          m_pic , QUERY_PARAM_TYPE.STRING );

		return query;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override HTTPReq MakeQuery_DBDelete()
	{
		HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/generic_delete.php", HTTP_METHOD.POST );

		query.AddParameter( "id",	 id.ToString(), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "table", "sites"      , QUERY_PARAM_TYPE.VALUE );

		return query;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override HTTPReq MakeQuery_DBPush()
	{
		HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/site_update.php", HTTP_METHOD.POST );

		query.AddParameter( "id",           id.ToString(), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "date_opened",  string.Format( "{0}-{1}-{2}", m_dateOpened.Year,  m_dateOpened.Month,  m_dateOpened.Day  ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "date_closed",  string.Format( "{0}-{1}-{2}", m_dateClosed.Year,  m_dateClosed.Month,  m_dateClosed.Day  ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "name",         name,   QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "dsc",          m_desc, QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "lat",          string.Format( "{0:F9}", m_coord.latitude.deg  ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "lng",          string.Format( "{0:F9}", m_coord.longitude.deg ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "alt",          string.Format( "{0:F9}", m_coord.altitude      ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "pic",          m_pic , QUERY_PARAM_TYPE.STRING );

		return query;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override HTTPReq MakeQuery_DBPull()
	{
		HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/generic_retrieve.php", HTTP_METHOD.GET );

		query.AddParameter( "id",	 id.ToString(), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "table", "sites"      , QUERY_PARAM_TYPE.VALUE );

		return query;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void OnDBPull( HTTPReq req )
	{
		JSon json = new JSon( req.response );

		if( json[ "error" ] != null )              return;

		if( ( json.root is JSonObject ) == false ) return;


		JSonObject o = json.root as JSonObject;

		System.DateTime.TryParse( o[ "date_opened" ].value, out m_dateOpened );

		System.DateTime.TryParse( o[ "date_closed" ].value, out m_dateClosed );

		name = o[ "name" ].value;

		m_coord.latitude.FromAngle ( float.Parse( o[ "lat" ].value ), GPS.UNIT.DD );

		m_coord.longitude.FromAngle( float.Parse( o[ "lng" ].value ), GPS.UNIT.DD );

		m_coord.altitude = float.Parse( o[ "alt" ].value );

		m_pic            = o[ "pic" ].value;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public void DBList( int start = 0, int end = int.MaxValue, HTTPReqDelegate handler = null )
	{
		if( handler != null )
		{
			HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/site_list.php", HTTP_METHOD.GET );

			query.SubmitAsync( handler );
		}
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class Item : Localizable, IDBObject
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public System.DateTime m_dateFound = System.DateTime.Now;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Item() {}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	protected override bool AcceptChild( Localizable child )
	{
		return false;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override HTTPReq MakeQuery_DBInsert()
	{
		HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/item_create.php", HTTP_METHOD.POST );

		query.AddParameter( "id",           id.ToString(),       QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "site_id",      parentID.ToString(), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "date_created", string.Format( "{0}-{1}-{2}", m_dateCreated.Year, m_dateCreated.Month, m_dateCreated.Day ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "date_found",   string.Format( "{0}-{1}-{2}", m_dateFound.Year,   m_dateFound.Month,   m_dateFound.Day   ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "name",         name,   QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "dsc",          m_desc, QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "lat",          string.Format( "{0:F9}", m_coord.latitude.deg  ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "lng",          string.Format( "{0:F9}", m_coord.longitude.deg ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "alt",          string.Format( "{0:F9}", m_coord.altitude      ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "pic",          m_pic , QUERY_PARAM_TYPE.STRING );

		return query;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override HTTPReq MakeQuery_DBDelete()
	{
		HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/generic_delete.php", HTTP_METHOD.POST );

		query.AddParameter( "id",	 id.ToString(), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "table", "items"      , QUERY_PARAM_TYPE.VALUE );

		return query;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override HTTPReq MakeQuery_DBPush()
	{
		HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/item_update.php", HTTP_METHOD.POST );

		query.AddParameter( "id",          id.ToString(),       QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "site_id",     parentID.ToString(), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "date_found",  string.Format( "{0}-{1}-{2}", m_dateFound.Year, m_dateFound.Month, m_dateFound.Day ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "name",        name,   QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "dsc",         m_desc, QUERY_PARAM_TYPE.STRING );

		query.AddParameter( "lat",         string.Format( "{0:F9}", m_coord.latitude.deg  ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "lng",         string.Format( "{0:F9}", m_coord.longitude.deg ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "alt",         string.Format( "{0:F9}", m_coord.altitude      ), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "pic",         m_pic , QUERY_PARAM_TYPE.STRING );

		return query;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override HTTPReq MakeQuery_DBPull()
	{
		HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/generic_retrieve.php", HTTP_METHOD.GET );

		query.AddParameter( "id",	 id.ToString(), QUERY_PARAM_TYPE.VALUE );

		query.AddParameter( "table", "items"      , QUERY_PARAM_TYPE.VALUE );

		return query;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void OnDBPull( HTTPReq req )
	{
		JSon json = new JSon( req.response );

		if( json[ "error" ] != null )              return;

		if( ( json.root is JSonObject ) == false ) return;


		JSonObject o = json.root as JSonObject;

		System.DateTime.TryParse( o[ "date_found" ].value, out m_dateFound );


		name     = o[ "name" ].value;

		parentID = long.Parse( o[ "site_id" ].value );

		m_coord.latitude.FromAngle ( float.Parse( o[ "lat" ].value ), GPS.UNIT.DD );

		m_coord.longitude.FromAngle( float.Parse( o[ "lng" ].value ), GPS.UNIT.DD );

		m_coord.altitude = float.Parse( o[ "alt" ].value );

		m_pic            = o[ "pic" ].value;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public void DBList( int start = 0, int end = int.MaxValue, HTTPReqDelegate handler = null )
	{
		if( handler != null )
		{
			HTTPReq query = new HTTPReq( ApplicationMain.instance.options.DBInfos.url + "/item_list.php", HTTP_METHOD.GET );

			query.SubmitAsync( handler );
		}
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

static public class SitesItemDatasComparison
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public int CompareAscending( UIListItemData a, UIListItemData b )
	{
		object A = ( a != null ) ? a.userDatas : null;

		object B = ( b != null ) ? b.userDatas : null;

		if( ( A == null ) && ( B == null ) ) return 0;

		if( ( A == null ) || ( B == null ) ) return ( A != null ) ? -1 : 1;

		if( ( A is Site ) ^  ( B is Site ) ) return ( A is Site ) ? -1 : 1;


		Localizable LocA = A as Localizable;

		Localizable LocB = B as Localizable;

		return LocA.name.CompareTo( LocB.name );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public int CompareDescending( UIListItemData a, UIListItemData b )
	{
		object A = ( a != null ) ? a.userDatas : null;

		object B = ( b != null ) ? b.userDatas : null;

		if( ( A == null ) && ( B == null ) ) return 0;

		if( ( A == null ) || ( B == null ) ) return ( A != null ) ? -1 : 1;

		if( ( A is Site ) ^  ( B is Site ) ) return ( A is Site ) ? -1 : 1;


		Localizable LocA = A as Localizable;

		Localizable LocB = B as Localizable;

		return LocB.name.CompareTo( LocA.name );
	}
}
