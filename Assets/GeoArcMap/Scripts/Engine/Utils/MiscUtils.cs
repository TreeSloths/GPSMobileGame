using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

//********************************************************************************************************
//
//********************************************************************************************************

namespace CORE
{
    //****************************************************************************************************
    //
    //****************************************************************************************************

    public class TimeOut
	{
		private float m_start    = 0.0f;
		
		private float m_duration = 0.0f;
		
		public  TimeOut( float duration ) { m_duration = duration; m_start = Time.time; }
		
		public static implicit operator bool( TimeOut t ) { return( ( Time.time - t.m_start ) >= t.m_duration ); }
	}

    //****************************************************************************************************
    //
    //****************************************************************************************************

    public static class Params
    {
	    public static T Get< T >( T defaultValue, int index, params object[] paramsList )
	    {
		    if( ( paramsList != null ) && ( index < paramsList.Length ) )
		    {
			    if( paramsList[ index ] != null )
			    {
				    Type expectedType = typeof( T );
				
				    Type paramType    = paramsList[ index ].GetType();
				
				    if( expectedType.IsAssignableFrom( paramType ) )
				    {
					    return ( T )paramsList[ index ];
				    }
				    else
				    {
					    UnityEngine.Debug.LogError( "Param " + index + " invalid type - had:" + paramType + " expected:" + expectedType );
				    }
			    }
		    }
		
		    return defaultValue;
	    }
	
	    public static T Get< T >( int index, params object[] paramsList )
	    {
		    return Get< T >( default( T ), index, paramsList );
	    }
    }
}