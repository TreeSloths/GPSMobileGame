using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public struct GridCoords
{
	public int X;

	public int Y;


	public GridCoords( int x = 0, int y = 0 ) { X = x; Y = y; }

	public static   bool operator == ( GridCoords a, GridCoords b ) { return ( a.X == b.X ) && ( a.Y == b.Y ); }

	public static   bool operator != ( GridCoords a, GridCoords b ) { return ( a.X != b.X ) || ( a.Y != b.Y ); }

	public override bool Equals      ( object o )                   { return o is GridCoords ? this == ( GridCoords )o : false; }
	
	public override int  GetHashCode ()                             { return base.GetHashCode(); }


	public GridCoords Clamp( int nbRows, int nbCols )
	{
		X = ( ( nbRows & ( nbRows - 1 ) ) == 0 ) ? X & ( nbRows - 1 ) : X % nbRows;

		Y = ( ( nbCols & ( nbCols - 1 ) ) == 0 ) ? Y & ( nbCols - 1 ) : Y % nbCols;

		return this;
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class WebMapTileSlot
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	private GridCoords    m_gridCoords = new GridCoords( 0,  0 );

	private WebMapTile    m_tile       = null;

	private Vector3       m_layoutPos  = Vector3.zero;

	private Vector3       m_pos        = Vector3.zero;

	private Vector3       m_LatLngZoom = Vector3.zero;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public GridCoords    coordGrid  { get { return m_gridCoords; }  set { m_gridCoords = value; } }

	public Vector3       coordGeo   { get { return m_LatLngZoom; }  set { m_LatLngZoom = value; } }

	public WebMapTile    tile       { get { return m_tile;       }  set { Bind( value );        } }

	public Vector3       layoutPos  { get { return m_layoutPos;  } set  { m_layoutPos = value;  } }

	public Vector3       position   { get { return m_pos;        } set  { m_pos       = value;  } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public float GetImpingement( Vector3 centerA, Vector2 extentsA, Vector3 centerB, Vector2 extentsB )
	{
		Vector3 pos  = centerA - centerB;

		Vector2 h    = new Vector2( Mathf.Clamp( pos.x - extentsA.x, -extentsB.x, extentsB.x ), Mathf.Clamp( pos.x + extentsA.x, -extentsB.x, extentsB.x ) );

		Vector2 v    = new Vector2( Mathf.Clamp( pos.y - extentsA.y, -extentsB.y, extentsB.y ), Mathf.Clamp( pos.y + extentsA.y, -extentsB.y, extentsB.y ) );

		return ( Mathf.Abs( h.y - h.x ) / extentsB.x ) * ( Mathf.Abs( v.y - v.x ) / extentsB.y );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Bind( WebMapTile tile )
	{
		if( m_tile != tile )
		{
			if( tile   != null ) { if( tile.slot != null ) tile.slot.tile = null; tile.slot = this; }

			if( m_tile != null ) { m_tile.slot = null; }

			m_tile = tile;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Unbind()
	{
		Bind( null );
	}
}

