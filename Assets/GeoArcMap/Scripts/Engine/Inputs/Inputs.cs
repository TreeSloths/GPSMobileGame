using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

namespace CORE
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class Inputs
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		static private Inputs m_instance = null;

		static public  Inputs Instance { get { if( m_instance == null ) { m_instance = new Inputs(); } return m_instance; } }

		//************************************************************************************************
		//
		//************************************************************************************************

		public static bool AnyKey     ( params KeyCode[] Params ) { foreach( KeyCode key in Params ) { if( Input.GetKey    ( key  ) == true ) return true; } return false; }

		public static bool AnyKeyDown ( params KeyCode[] Params ) { foreach( KeyCode key in Params ) { if( Input.GetKeyDown( key  ) == true ) return true; } return false; }

		public static bool AnyKeyUp   ( params KeyCode[] Params ) { foreach( KeyCode key in Params ) { if( Input.GetKeyUp  ( key  ) == true ) return true; } return false; }

		public static bool AnyAxis    ( params string [] Params ) { foreach( string axis in Params ) { if( Input.GetAxis   ( axis ) != 0.0f ) return true; } return false; }
	}
}