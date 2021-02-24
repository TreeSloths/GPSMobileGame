using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class TextureCache
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public struct Entry
	{
		public string  m_key;

		public Texture m_texture;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private const int CAPACITY  = 256;

	private Entry[]   m_entries = new Entry[ CAPACITY ];

	private int       m_size    = 0;

	private int       m_first   = 0;

	private object    m_lock    = new object();

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Add( string key, Texture texture )
	{
		lock( m_lock )
		{
			if( key     == null ) return;

			if( texture == null ) return;


			for( int entry = 0; entry < m_size; ++entry )
			{
				 if( m_entries[ entry ].m_key.CompareTo( key ) == 0 )
				 {
					 m_entries[ entry ].m_texture = texture;

					 return;
				 }
			}


			if( m_size >= CAPACITY ) m_first = ( m_first + 1 ) & ( CAPACITY - 1 );

			else                     m_size  = ( m_size  + 1 );

			int index = ( m_size >= CAPACITY ) ? ( m_first + m_size - 1 ) & ( CAPACITY - 1 ) : m_size - 1;

			m_entries[ index ].m_key     = key;

			m_entries[ index ].m_texture = texture;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Flush()
	{
		lock( m_lock )
		{
			for( int entry = 0; entry < m_size; ++entry )
			{
				m_entries[ entry ].m_key     = null;

				m_entries[ entry ].m_texture = null;
			}

			m_first = 0;

			m_size  = 0;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Texture Get( string key )
	{
		lock( m_lock )
		{
			if( key != null )
			{
				for( int entry = 0; entry < m_size; ++entry )
				{
					 if( m_entries[ entry ].m_key.CompareTo( key ) == 0 )
					 {
						 return m_entries[ entry ].m_texture;
					 }
				}
			}

			return null;
		}
	}
}

