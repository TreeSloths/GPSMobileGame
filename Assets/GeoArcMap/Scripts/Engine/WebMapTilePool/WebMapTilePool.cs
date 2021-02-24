using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class WebMapTilePool
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	WebMapTile[] m_entries = null;

	int          m_size    = 0;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public WebMapTilePool( int capacity )
	{
		m_entries = new WebMapTile[ Alignement.Align( capacity, 16 ) ];

		System.Array.Resize( ref m_entries, capacity );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public int        size              { get { return m_entries.Length;   } }

	public WebMapTile this[ int entry ] { get { return m_entries[ entry ]; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Release( WebMapTile tile )
	{
		if( tile      == null )			 return;

		if( tile.pool != null )			 return;

		if( m_size >= m_entries.Length ) return;

		m_entries[ m_size++ ] = tile;

		tile.pool = this;


		if( tile.slot != null ) tile.slot.Unbind();

		tile.gameObject.SetActive( false );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public WebMapTile Grab( WebMapTileSlot slot )
	{
		WebMapTile tile = null;

		if( slot != null )
		{
			for( int entry = m_size - 1; entry >= 0; --entry )
			{
				if( m_entries[ entry ].location.coordGrid == slot.coordGrid )
				{
					tile = m_entries[ entry ];

					m_entries[ entry ] = m_entries[ --m_size ];

					break;
				}
			}

			if( tile == null ) tile = ( m_size > 0 ) ? m_entries[ --m_size ] : null;

			if( tile != null )
			{
				slot.Bind( tile );

				tile.pool = null;

				tile.gameObject.SetActive( true );
			}
		}

		return tile;
	}
}