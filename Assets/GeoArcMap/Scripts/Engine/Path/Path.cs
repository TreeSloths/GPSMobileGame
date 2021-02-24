using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using CORE;

//********************************************************************************************************
// kept for reference
//********************************************************************************************************
/*
( dirs[ 0 ].Equals( "Assets", System.StringComparison.OrdinalIgnoreCase ) )
*/
//********************************************************************************************************
//
//********************************************************************************************************

#if UNITY_EDITOR

using UnityEditor;

#endif

//********************************************************************************************************
//
//********************************************************************************************************

namespace CORE
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class Path
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		[ System.Flags ] public enum OPTIONS
		{
			NONE			 = 0X00,

			SEPARATORS       = 0X01,

			SLASH_TERMINATED = 0x02,

			LOWER_CASE       = 0X04,

			UPPER_CASE       = 0X08,

			CREATE           = 0X10
		}

		//************************************************************************************************
		//
		//************************************************************************************************

	    private const char CHAR_SPACE   = ' ';
	
	    private const char CHAR_STD_SEP = '/';
	
	    private const char CHAR_ALT_SEP = '\\';

		//************************************************************************************************
		//
		//************************************************************************************************

		static public bool Normalize( ref string path, OPTIONS options )
		{ 
			#if UNITY_EDITOR

				if( string.IsNullOrEmpty( path ) == false )
				{ 
					path.Trim( CHAR_SPACE );

					if( System.IO.Path.IsPathRooted( path ) == false )
					{
						if     ( ( options & OPTIONS.SLASH_TERMINATED ) != 0 ) AppendMissingSlash( ref path );

						if     ( ( options & OPTIONS.SEPARATORS       ) != 0 ) { path = path.Replace( CHAR_ALT_SEP, CHAR_STD_SEP ); }

						if     ( ( options & OPTIONS.LOWER_CASE       ) != 0 ) { path = path.ToLower(); }

						else if( ( options & OPTIONS.UPPER_CASE       ) != 0 ) { path = path.ToUpper(); }

						return ( ( options & OPTIONS.CREATE           ) != 0 ) ? CreateFolders( path ) : true;
					}
				}

			#endif

			return false;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

	    static public bool StartWith( string path, params char[] chars )
	    {
		    if( string.IsNullOrEmpty( path ) )               return false;
		
		    if( ( chars == null ) || ( chars.Length <= 0 ) ) return false;
		
		    for( int c = 0; c < chars.Length; ++c ) { if( path[ 0 ] == chars[ c ] ) return true; }
		
		    return false;
	    }

		//************************************************************************************************
		//
		//************************************************************************************************

	    static public bool EndsWith( string path, params char[] chars )
	    {
		    if( string.IsNullOrEmpty( path ) )               return false;
		
		    if( ( chars == null ) || ( chars.Length <= 0 ) ) return false;
		
		    for( int c = 0; c < chars.Length; ++c ) { if( path[ path.Length - 1 ] == chars[ c ] ) return true; }
		
		    return false;
	    }

		//************************************************************************************************
		//
		//************************************************************************************************

		static public char GetDominantSeparator( string path )
		{
			if( string.IsNullOrEmpty( path )  == false )
			{
				int nbStd = 0;

				int nbAlt = 0;

				for( int c = 0; c < path.Length; ++c )
				{
					if     ( path[ c ] == CHAR_STD_SEP     ) ++nbStd;

					else if( path[ c ] == CHAR_ALT_SEP ) ++nbAlt;
				}

				if( ( nbStd > 0 ) || ( nbAlt > 0 ) ) return ( nbStd >= nbAlt ) ? CHAR_STD_SEP : CHAR_ALT_SEP;
			}

			return CHAR_STD_SEP;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static public void AppendMissingSlash( ref string path )
		{
			if( string.IsNullOrEmpty( path ) ) return;

			if( EndsWith( path, new char[] { CHAR_STD_SEP, CHAR_ALT_SEP } ) == false )
			{
				char separator = GetDominantSeparator( path );

				path += separator;
			}
			else
			{
				path.TrimEnd( CHAR_STD_SEP, CHAR_ALT_SEP );
			}
		}

		//************************************************************************************************
		//
		//************************************************************************************************

	    static public void Append( ref string path, params string[] components )
	    {
		    int nbComponents = components != null ? components.Length : 0;
		
		    for( int p = 0; p < nbComponents; ++p )
		    {
			    string component = components[ p ].Trim( CHAR_SPACE, CHAR_STD_SEP, CHAR_SPACE, CHAR_ALT_SEP, CHAR_SPACE );
			
			    if( component.Length > 0 ) 
			    {
				    if( path.Length  > 0 ) path += CHAR_STD_SEP;
			
				    path += component;
			    }
		    }
	    }

		//************************************************************************************************
		//
		//************************************************************************************************

	    static public List< KeyValuePair< string, int > > GetComponents( string path )
	    {
		    List< KeyValuePair< string, int > > components = new List< KeyValuePair< string, int > >();
	
		    int len      = ( path != null ) ? path.Length : 0;
		
		    int lastChar = ( len > 0 ) ? ( len - 1 ) : 0;
		
		    int dirStart = ( ASCII.Semantics.IsFolderSeparator( path[ 0 ] ) == false ) ? 0 : -1;
		
		    int dirEnd   = -1;
		
		    for( int c = 0; c < len; ++c )
		    {
			    if( ( c > 0 )         && ( ASCII.Semantics.IsFolderSeparator( path[ c - 1 ] ) ) ) dirStart = c;
			
			    if( ( c >= lastChar ) || ( ASCII.Semantics.IsFolderSeparator( path[ c + 1 ] ) ) ) dirEnd   = c;
			
			    if( ( dirStart != -1 ) && ( dirEnd != -1 ) && ( dirEnd >= dirStart ) )
			    {
				    string dir = path.Substring( dirStart, ( dirEnd - dirStart ) + 1 );
			
				    components.Add( new KeyValuePair< string, int >( dir,  dirStart ) );
				
				    dirStart = -1;
				
				    dirEnd   = -1;
			    }
		    }
		
		    return components;
	    }

		//************************************************************************************************
		//
		//************************************************************************************************

	    static public void SetRelativeTo( ref string path, string root )
	    {
		    if( string.IsNullOrEmpty( root ) == false )
		    {
			    List< KeyValuePair< string, int > > components = GetComponents( path );
			
			    for( int comp = 0; comp < components.Count; ++comp )
			    {
				    KeyValuePair< string, int > component = components[ comp ];
			
				    if( component.Key == root )
				    {
					    path = path.Substring( component.Value );
				
					    break;
				    }
			    } 
		    }
	    }	

		//************************************************************************************************
		//
		//************************************************************************************************

		public static bool CreateFolders( string path )
		{
			#if UNITY_EDITOR

				string[] dirs   = System.IO.Path.GetDirectoryName( path ).Split( new char[] { CHAR_STD_SEP, CHAR_ALT_SEP } );

				string   accum  = ( dirs.Length > 0 ) ? dirs[ 0 ] : string.Empty;

				bool     exist  = ( dirs.Length > 0 ) && AssetDatabase.IsValidFolder( dirs[ 0 ] );

				for( int dir = 1; dir < dirs.Length; ++dir )
				{
					string folder = accum + "/" + dirs[ dir ]; 

					exist = ( AssetDatabase.IsValidFolder( folder ) ) || ( string.IsNullOrEmpty( AssetDatabase.CreateFolder( accum, dirs[ dir ] ) ) == false );

					if( exist == false )
					{
						UnityEngine.Debug.LogWarning( "Could not create folder " + folder );

						break;
					}

					accum = folder;
				}

				return exist;

			#else

				return false;

			#endif
		}
	}
}
