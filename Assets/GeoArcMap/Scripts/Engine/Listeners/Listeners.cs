using System.Collections;
using System.Collections.Generic;

//********************************************************************************************************
//
//********************************************************************************************************

public class Listeners< eventT >
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public delegate void Listener( eventT evt, params object[] paramsList );

	private static List< Listener > m_list = new List< Listener >();

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public static bool ListenerValid( Listener listener )
	{
		if(   listener        == null )                                            return false;

		if(   listener.Method == null )                                            return false;
				
		if( ( listener.Target == null ) && ( listener.Method.IsStatic == false ) ) return false;

		return true;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Add   ( Listener listener ) { if( ( ListenerValid( listener ) ) && ( m_list.Contains( listener ) == false ) ) m_list.Add   ( listener ); }

	public void Remove( Listener listener ) { if( ( listener != null )          && ( m_list.Contains( listener ) == true  ) ) m_list.Remove( listener ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Clear()
	{
	    m_list.Clear();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Clean()
	{
	    for( int listener = 0; listener < m_list.Count; )
	    {
	        if( ListenerValid( m_list[ listener ] ) )
			{
				++listener;
			}
	        else
			{
				m_list.RemoveAt( listener );
			}
	    }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Notify( eventT evt, params object[] Params )
	{
	    for( int listener = 0; listener < m_list.Count; )
	    {
	        Listener instance = m_list[ listener ];

	        if( ListenerValid( instance ) )
			{
				++listener;

				instance( evt, Params );
			}
			else
			{
				m_list.RemoveAt( listener );
			}
	    }
	}
}
