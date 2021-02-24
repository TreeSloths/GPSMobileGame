using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public abstract class DBTask
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum STATUS { CREATED, STARTED, FINISHED }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	protected DBObjects db     { get; set; }

	protected HTTPReq   req    { get; set; }

	public    STATUS    status { get; private set; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public DBTask( DBObjects paramDB ) { db = paramDB; req = null; status = STATUS.CREATED; }

	      ~DBTask() { End(); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	protected virtual void OnStart  () { End(); }

	protected virtual void OnMonitor() {}

	protected virtual void OnEnd    () {}

	protected         void CancelReq() { if( req != null ) { req.Cancel(); req = null; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Start  () { if( status == STATUS.CREATED ) { status = STATUS.STARTED; OnStart(); } }

	public void Monitor() { if( status == STATUS.STARTED ) { OnMonitor(); } }

	public void End    () { if( status == STATUS.STARTED ) { status = STATUS.FINISHED; CancelReq(); OnEnd(); } }
}

//********************************************************************************************************
//
//********************************************************************************************************

public class DBTaskRefreshBegin : DBTask
{
	public DBTaskRefreshBegin( DBObjects paramDB ) : base( paramDB ) {}

	protected override void OnStart() { db.NotifyRefreshBegin(); db.Clear(); End(); }
}

//********************************************************************************************************
//
//********************************************************************************************************

public class DBTaskRefreshEnd : DBTask
{
	public DBTaskRefreshEnd( DBObjects paramDB ) : base( paramDB ) {}

	protected override void OnStart() { db.NotifyRefreshEnd(); End(); }
}

//********************************************************************************************************
//
//********************************************************************************************************

public class DBTaskUpdateSites : DBTask
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public DBTaskUpdateSites( DBObjects paramDB ) : base( paramDB ) {}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	protected override void OnStart()
	{
		Site.DBList( 0, int.MaxValue, new HTTPReqDelegate( OnSitesQueryResponse, null ) );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnSitesQueryResponse( HTTPReq query, params object[] paramsList )
	{
		if( query.result != QUERY_RESULT.SUCCESS ) { End(); return; }


		JSon json = new JSon( query.response );

		if( json[ "error" ] != null )             { End(); return; }

		if( ( json.root is JSonArray ) == false ) { End(); return; }

		for( int entry = 0; entry < json.count; ++entry )
		{
			JSonEntry o =  json[ entry ];

			if( ( o is JSonObject ) == false ) continue;

			long  ID  = -1L;  if( long.TryParse ( o[ "id"   ].value, out ID  ) == false ) continue;

			float LAT = 0.0f; if( float.TryParse( o[ "lat"  ].value, out LAT ) == false ) continue;

			float LNG = 0.0f; if( float.TryParse( o[ "lng"  ].value, out LNG ) == false ) continue;

			float ALT = 0.0f; if( float.TryParse( o[ "alt"  ].value, out ALT ) == false ) continue;


			Site site     = new Site();

			site.parentID = 0L;

			site.id       = ID;

			site.name     = o[ "name" ].value;

			site.m_desc   = o[ "dsc"  ].value;

			site.m_pic    = o[ "pic"  ].value;

			site.m_coord.latitude.FromAngle ( LAT, GPS.UNIT.DD );

			site.m_coord.longitude.FromAngle( LNG, GPS.UNIT.DD );

			site.m_coord.altitude = ALT;

			db.sites.Add( site );
		}

		End();
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class DBTaskUpdateItems : DBTask
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public DBTaskUpdateItems( DBObjects paramDB ) : base( paramDB ) {}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	protected override void OnStart()
	{
		Item.DBList( 0, int.MaxValue, new HTTPReqDelegate( OnItemsQueryResponse, null ) );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnItemsQueryResponse( HTTPReq query, params object[] paramsList )
	{
		if( query.result != QUERY_RESULT.SUCCESS ) { End(); return; }


		JSon json = new JSon( query.response );

		if( json[ "error" ] != null )             { End(); return; }

		if( ( json.root is JSonArray ) == false ) { End(); return; }

		for( int entry = 0; entry < json.count; ++entry )
		{
			JSonEntry o =  json[ entry ];

			if( ( o is JSonObject ) == false ) continue;

			long  ID     = -1L;  if( long.TryParse( o[ "id"      ].value, out ID     ) == false ) continue;

			long  siteID = -1L;  if( long.TryParse( o[ "site_id" ].value, out siteID ) == false ) continue;

			float LAT    = 0.0f; if( float.TryParse( o[ "lat"    ].value, out LAT    ) == false ) continue;

			float LNG    = 0.0f; if( float.TryParse( o[ "lng"    ].value, out LNG    ) == false ) continue;

			float ALT    = 0.0f; if( float.TryParse( o[ "alt"    ].value, out ALT    ) == false ) continue;


			Item item       = new Item();

			item.id         = ID;

			item.parentID   = siteID;

			item.name       = o[ "name" ].value;

			item.m_desc     = o[ "dsc"  ].value;

			item.m_pic      = o[ "pic"  ].value;

			item.m_coord.latitude.FromAngle ( LAT, GPS.UNIT.DD );

			item.m_coord.longitude.FromAngle( LNG, GPS.UNIT.DD );

			item.m_coord.altitude = ALT;

			db.items.Add( item );


			if( item.m_parent == null )
			{
				item.SetParent( db.root );
			}
		}

		End();
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class DBObjects : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum EVT
	{
		EVT_REFRESH_BEGIN	,

		EVT_REFRESH_END		,

		EVT_EDIT_BEGIN		,

		EVT_EDIT_END		,

		EVT_EDIT_CANCEL		,

		EVT_OBJECT_CREATED	,

		EVT_OBJECT_DELETED
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private DBObjects m_instance = null;

	static public  DBObjects instance { get { return m_instance; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private object               m_session   = null;

	private DBTask               m_task      = null;

	private Queue< DBTask >      m_tasks     = new Queue< DBTask >();

	private List < Localizable > m_sites     = new List < Localizable >();

	private List < Localizable > m_items     = new List < Localizable >();

	private Listeners< EVT >     m_listeners = new Listeners< EVT >();

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool                  disabled     { get; set; }

	public object                session      { get { return m_session; } private set { m_session = value; } }

	public bool                  editing      { get { return ( m_session != null ); } }

	public bool                  hasTasks     { get { return ( m_task != null ) || ( m_tasks.Count > 0 ); } }

	public bool                  busy         { get { return ( editing ) || ( hasTasks ); } }

	public Localizable           root         { get; private set; }

	public List< Localizable >   sites        { get { return m_sites;       } }

	public List< Localizable >   items        { get { return m_items;       } }

	public Listeners< EVT >      listeners    { get { return m_listeners;   } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Awake    () { if( m_instance == null ) m_instance = this; } 

	public void OnDestroy() { if( m_instance == this ) m_instance = null; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void CreateRoot()
	{
		if( root == null )
		{
			root      = new Localizable();

			root.name = "ROOT";

			root.id   = 0L;
		}
	}		

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void DeleteRoot()
	{
		if( root != null )
		{
			root.m_childs.Clear();

			root.m_ListItemDatas.childs.Clear();

			root.id = -1L;

			root    = null;
		}
	}		

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Clear()
	{
		Localizable.Clear();

		sites.Clear();

		items.Clear();

		DeleteRoot ();

		CreateRoot ();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void NotifyRefreshBegin() { m_listeners.Notify( EVT.EVT_REFRESH_BEGIN, null ); }

	public void NotifyRefreshEnd  () { m_listeners.Notify( EVT.EVT_REFRESH_END,   null ); }

	public void NotifyEditBegin   () { m_listeners.Notify( EVT.EVT_EDIT_BEGIN,    null ); }

	public void NotifyEditEnd     () { m_listeners.Notify( EVT.EVT_EDIT_END,      null ); }

	public void NotifyEditCancel  () { m_listeners.Notify( EVT.EVT_EDIT_CANCEL,   null ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public object BeginEdit      ()           { if( busy == false )        { m_session = new object(); NotifyEditBegin (); return m_session; } return null;  }

	public bool   EndEdit        ( object o ) { if( IsSessionActive( o ) ) { m_session = null;         NotifyEditEnd   (); return true;      } return false; }

	public bool   CancelEdit     ( object o ) { if( IsSessionActive( o ) ) { m_session = null;         NotifyEditCancel(); return true;      } return false; }

	public bool   IsSessionActive( object o ) { return ( o != null ) && ( ReferenceEquals( m_session, o ) ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Localizable Create< LocalizableT >( Localizable parent, HTTPReqDelegate.Delegate onCompletion, object session ) where LocalizableT : Localizable, new()
	{
		Localizable localizable = ( disabled == false ) && ( IsSessionActive( session ) ) ? new LocalizableT() : null;

		if( localizable != null )
		{
			localizable.SetParent( ( parent != null ) ? parent : root );

			localizable.Async_DBInsert( new HTTPReqDelegate( OnLocalizableDBInserted + onCompletion, localizable ) );
		}

		return localizable;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnLocalizableDBInserted( HTTPReq req, params object[] paramsList )
	{
		Localizable localizable = CORE.Params.Get< Localizable >( 0, paramsList );

		if( localizable != null )
		{
			if( req.result == QUERY_RESULT.SUCCESS )
			{
				if     ( localizable is Site ) sites.Add( localizable );

				else if( localizable is Item ) items.Add( localizable );

				m_listeners.Notify( EVT.EVT_OBJECT_CREATED, localizable );
			}
			else
			{
				OnLocalizableDBDeleted( null, localizable );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool Delete( Localizable localizable, HTTPReqDelegate.Delegate onCompletion, object session )
	{
		if( disabled )                            return false;

		if( localizable == null )                 return false;

		if( IsSessionActive( session ) == false ) return false;


		localizable.Async_DBDelete( new HTTPReqDelegate( OnLocalizableDBDeleted + onCompletion, localizable ) );

		return true;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnLocalizableDBDeleted( HTTPReq req, params object[] paramsList )
	{
		Localizable localizable = CORE.Params.Get< Localizable >( 0, paramsList );

		if( localizable != null )
		{
			if( ( req == null ) || ( req.result == QUERY_RESULT.SUCCESS ) )
			{
				if     ( localizable is Site ) sites.Remove( localizable );

				else if( localizable is Item ) items.Remove( localizable );

				localizable.id = -1L;

				localizable.SetParent( null );

				m_listeners.Notify( EVT.EVT_OBJECT_DELETED, localizable );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool UpdateFromDB()
	{
		if( disabled ) return false;

		if( busy     ) return false;

		m_tasks.Enqueue( new DBTaskRefreshBegin( this ) );

		m_tasks.Enqueue( new DBTaskUpdateSites ( this ) );

		m_tasks.Enqueue( new DBTaskUpdateItems ( this ) );

		m_tasks.Enqueue( new DBTaskRefreshEnd  ( this ) );

		return true;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Start()
	{
		UpdateFromDB();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void EndCurTask()
	{
		if( m_task != null )
		{
			m_task.End();

			m_task = null;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void AbortTasks()
	{
		EndCurTask();

		m_tasks.Clear();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Update()
	{
		if( ( m_task == null ) && ( m_tasks.Count > 0 ) )
		{
			m_task = m_tasks.Dequeue();

			m_task.Start();
		}

		if( m_task != null )
		{
			m_task.Monitor();

			if( m_task.status == DBTask.STATUS.FINISHED )
			{
				m_task = null;
			}
		}
	}
}
