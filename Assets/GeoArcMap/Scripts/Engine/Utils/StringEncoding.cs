using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

namespace CORE
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public class ENCODE
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		static public string Using( System.Text.Encoding encoding, string src )
		{
			if( ( encoding != null ) && ( string.IsNullOrEmpty( src ) == false ) )
			{
				byte[] bytes  = encoding.GetBytes ( src.ToCharArray() );

				string result = encoding.GetString( bytes );

				return result;
			}

			return string.Empty;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static public string ToASCII  ( string src ) { return Using( System.Text.ASCIIEncoding.Default,   src ); }

		static public string ToUTF8   ( string src ) { return Using( System.Text.UTF8Encoding.Default,    src ); }

		static public string ToUnicode( string src ) { return Using( System.Text.UnicodeEncoding.Default, src ); }

		//************************************************************************************************
		//
		//************************************************************************************************

		static public string To< EncodingT >( string src ) where EncodingT : System.Text.Encoding, new()
		{
			if( string.IsNullOrEmpty( src ) == false )
			{
				EncodingT encoding = new EncodingT();

				byte[]    bytes    = encoding.GetBytes ( src.ToCharArray() );

				string    result   = encoding.GetString( bytes );

				return    result;
			}

			return string.Empty;
		}
	}
}