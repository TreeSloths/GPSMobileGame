using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class Localizable : IDBObject
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private Dictionary< long, Localizable > m_instances = new Dictionary< long, Localizable >();

	static private void Register  ( Localizable localizable ) { if( localizable != null ) m_instances.Add   ( localizable.m_id, localizable ); }

	static private void Unregister( Localizable localizable ) { if( localizable != null ) m_instances.Remove( localizable.m_id );              }

	static public  void Clear     () { m_instances.Clear(); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public Localizable Get( long id )
	{
		Localizable instance = null;

		m_instances.TryGetValue( id, out instance );

		return instance;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public Localizable Get( string name )
	{
		foreach( Localizable localizable in m_instances.Values )
		{
			if( localizable.name.CompareTo( name ) == 0 )
			{
				return localizable;
			}
		}

		return null;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private Nameplate          m_nameplate     = null;

	private string	           m_name          = string.Empty;

	private long               m_id            = Guid.@new;

	public System.DateTime	   m_dateCreated   = System.DateTime.Now;

	public string			   m_desc          = string.Empty;

	public MapCoord			   m_coord         = new MapCoord();

	public string              m_pic           = string.Empty;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public  UIListItemData      m_ListItemDatas = null;

	private long                m_parentID      = -1L;

	public  Localizable         m_parent        = null;

	public  List< Localizable > m_childs        = new List< Localizable >();

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Localizable()
	{
		Register( this );

		m_name = defaultName;

		m_ListItemDatas = new UIListItemData( null, this );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	~Localizable()
	{
		SetParent( null );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public long id
	{
		get { return m_id; }

		set { if( m_id != value ) { Unregister( this ); if( ( m_id = value ) != -1L ) Register( this ); } }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public long parentID
	{
		get { return m_parentID; }

		set { if( m_parentID != value ) { SetParent( value ); } }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Nameplate nameplate   { get { return m_nameplate; } set { m_nameplate = value; if( m_nameplate != null ) m_nameplate.m_text.text = m_name; } }

	public string    name        { get { return m_name;      } set { m_name      = value; if( m_nameplate != null ) m_nameplate.m_text.text = value;  } }

	public string    defaultName { get { return GetType().Name; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public virtual HTTPReq MakeQuery_DBInsert () { return null; }

	public virtual HTTPReq MakeQuery_DBDelete () { return null; }

	public virtual HTTPReq MakeQuery_DBPush   () { return null; }

	public virtual HTTPReq MakeQuery_DBPull   () { return null; }

	public virtual void    OnDBDPush          ( HTTPReq req ) {}

	public virtual void    OnDBPull           ( HTTPReq req ) {}


	public void            Async_DBInsert ( HTTPReqDelegate onCompletion ) { HTTPReq req = MakeQuery_DBInsert(); if( req != null ) req.SubmitAsync( onCompletion ); else { if( onCompletion != null ) onCompletion.Invoke( null ); } }

	public void            Async_DBDelete ( HTTPReqDelegate onCompletion ) { HTTPReq req = MakeQuery_DBDelete(); if( req != null ) req.SubmitAsync( onCompletion ); else { if( onCompletion != null ) onCompletion.Invoke( null ); } }

	public void            Async_DBPush   ( HTTPReqDelegate onCompletion ) { HTTPReq req = MakeQuery_DBPush();   if( req != null ) req.SubmitAsync( onCompletion ); else { if( onCompletion != null ) onCompletion.Invoke( null ); } }

	public void            Async_DBPull   ( HTTPReqDelegate onCompletion ) { HTTPReq req = MakeQuery_DBPull();   if( req != null ) req.SubmitAsync( onCompletion ); else { if( onCompletion != null ) onCompletion.Invoke( null ); } }


	public QUERY_RESULT    DBInsert () { HTTPReq req = MakeQuery_DBInsert(); return ( req != null ) ? req.SubmitImmediate() : QUERY_RESULT.ERROR; }

	public QUERY_RESULT    DBDelete () { HTTPReq req = MakeQuery_DBDelete(); return ( req != null ) ? req.SubmitImmediate() : QUERY_RESULT.ERROR; }

	public QUERY_RESULT    DBPush   () { HTTPReq req = MakeQuery_DBPush();   QUERY_RESULT result = ( req != null ) ? req.SubmitImmediate() : QUERY_RESULT.ERROR; if( result == QUERY_RESULT.SUCCESS ) OnDBDPush( req ); return result; }

	public QUERY_RESULT    DBPull   () { HTTPReq req = MakeQuery_DBPull();   QUERY_RESULT result = ( req != null ) ? req.SubmitImmediate() : QUERY_RESULT.ERROR; if( result == QUERY_RESULT.SUCCESS ) OnDBPull ( req ); return result; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	protected virtual bool AcceptChild( Localizable child )
	{
		return true;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void AddChild( Localizable child )
	{
		if( child          == null ) return;

		if( child          == this ) return;

		if( child.m_parent != null ) return;

		m_childs.Add( child );

		child.m_parent   = this;

		child.m_parentID = m_id;

		m_ListItemDatas.AddChild( child.m_ListItemDatas );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void RemoveChild( Localizable child )
	{
		if( child          == null ) return;

		if( child          == this ) return;

		if( child.m_parent != this ) return;

		m_childs.Remove( child );

		child.m_parent   = null;

		child.m_parentID = -1L;

		m_ListItemDatas.RemoveChild( child.m_ListItemDatas ); 
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void SetParent( long paramID )
	{
		if( paramID == m_id       ) return;

		if( paramID == m_parentID ) return;


		Localizable parent = Get( paramID );

		if( ( parent != null ) && ( parent.AcceptChild( this ) ) )
		{
			if( m_parent != null ) m_parent.RemoveChild( this );

			parent.AddChild( this );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void SetParent( Localizable parent )
	{
		if( ( m_parent != parent ) && ( parent != this ) )
		{
			if( m_parent != null ) m_parent.RemoveChild( this );

			if( ( parent != null ) && ( parent.AcceptChild( this ) ) )
			{
				parent.AddChild( this );
			}
		}
	}
}
