using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Permissions;

//************************************************************************************************
//
//************************************************************************************************

#if UNITY_EDITOR

using UnityEditor;

#endif

//************************************************************************************************
//
//************************************************************************************************

using BinaryFormatter = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;

//************************************************************************************************
//
//************************************************************************************************

namespace SERIALIZATION
{
	//********************************************************************************************
	//
	//********************************************************************************************

	public enum MODE { DEFAULT, ADAPTIVE }

	//********************************************************************************************
	//
	//********************************************************************************************
	
	[ System.Serializable ]public class Entry
	{
		[ SerializeField ] private string                      m_fieldname = string.Empty;
		
		[ SerializeField ] private bool                        m_serialize = false;
		
		                   private System.Reflection.FieldInfo m_fieldinfo = null;
		
		public string                      fieldname { get { return m_fieldname; } }
		
		public bool                        serialize { get { return m_serialize; } }
		
		public System.Reflection.FieldInfo fieldinfo { get { return m_fieldinfo; } set { m_fieldinfo = value; } }
		
		public Entry() {}
		
		public Entry( object instance, System.Reflection.FieldInfo fieldinfo ) 
		{ 
			m_fieldname = ( fieldinfo != null ) ? fieldinfo.Name : string.Empty; 
			
			m_serialize = ( fieldinfo != null ) && ( ( fieldinfo.FieldType.IsClass == false ) || ( fieldinfo.GetValue( instance ) != null ) );
			
			m_fieldinfo = fieldinfo;
		}
		
		public bool Serialize( Stream stream, BinaryFormatter formatter )
		{
			if( stream    == null ) return false;
			
			if( formatter == null ) return false;
		
			formatter.Serialize( stream, m_fieldname );
			
			formatter.Serialize( stream, m_serialize );
			
			return true;
		}
		
		public bool Deserialize( Stream stream, BinaryFormatter formatter )
		{
			if( stream    == null ) return false;
			
			if( formatter == null ) return false;
			
			m_fieldname = formatter.Deserialize( stream ) as string;
			
			m_serialize = ( bool )formatter.Deserialize( stream );
			
			m_fieldinfo = null;
			
			return true;
		}
	}
	
	//********************************************************************************************
	//
	//********************************************************************************************
	
	[ System.Serializable ]public class Manifest< T >
	{
		public List< Entry > m_entries = new List< Entry >();
		
		//****************************************************************************************
		//
		//****************************************************************************************
		
		public Manifest( T instance )
		{
			Build( instance );
		}
		
		//****************************************************************************************
		//
		//****************************************************************************************
		
		public Manifest( Stream stream, BinaryFormatter formatter )
		{
			Deserialize( stream, formatter );
		}
		
		//****************************************************************************************
		//
		//****************************************************************************************
		
		public void Build( T instance )
		{
			m_entries.Clear();
			
			if( instance != null )
			{
				System.Type type = typeof( T );
				
				System.Reflection.BindingFlags bindingFlags  = ( System.Reflection.BindingFlags )( -1 );
				
				System.Reflection.FieldInfo[]  fieldsInfos   = type.GetFields( bindingFlags );
				
				int                            nbFieldsInfos = fieldsInfos  != null ? fieldsInfos.Length : 0;
				
				for( int field = 0; field < nbFieldsInfos; ++field )
				{
					Entry entry = new Entry( instance, fieldsInfos[ field ] );
				
					if( entry.serialize ) m_entries.Add( entry );
				}
			}
		}
		
		//****************************************************************************************
		//
		//****************************************************************************************
		
		public bool Serialize( Stream stream, BinaryFormatter formatter )
		{
			if( stream    == null ) return false;
			
			if( formatter == null ) return false;
			
			formatter.Serialize( stream, m_entries.Count );
			
			for( int entry = 0; entry < m_entries.Count; ++entry )
			{
				m_entries[ entry ].Serialize( stream, formatter );
			}
			
			return true;
		}
		
		//****************************************************************************************
		//
		//****************************************************************************************
		
		public bool Deserialize( Stream stream, BinaryFormatter formatter )
		{
			if( stream    == null ) return false;
			
			if( formatter == null ) return false;
			
			int nbEntries = ( int )formatter.Deserialize( stream );
			
			for( int entry = 0; entry < nbEntries; ++entry )
			{
				Entry pEntry = new Entry();
				
				pEntry.Deserialize( stream, formatter );
			
				m_entries.Add( pEntry );
			}
			
			
			System.Type type = typeof( T );
			
			System.Reflection.BindingFlags bindingFlags = ( System.Reflection.BindingFlags )( -1 );
			
			for( int entry = 0; entry < m_entries.Count; ++entry )
			{
				string                      fieldname = m_entries[ entry ].fieldname;
				
				System.Reflection.FieldInfo fieldinfo = type.GetField( fieldname, bindingFlags );
			
				if( fieldinfo == null )
				{
					Debug.Log( "Field: " + fieldname + " no longer exist in class " + type.ToString() + " current implementation" );
				}
				
				m_entries[ entry ].fieldinfo = fieldinfo;
			}
			
			return true;
		}
	}
}

//************************************************************************************************
//
//************************************************************************************************

public static class Serializer< T > where T : new()
{
	//********************************************************************************************
	//
	//********************************************************************************************

    public delegate void OnSerialized  ( T instance );

    public delegate void OnDeserialized( T instance );
			
	private static BinaryFormatter m_formatter      = new BinaryFormatter();

    private static OnSerialized    m_onSerialized   = null;

    private static OnDeserialized  m_onDeserialized = null;

	//********************************************************************************************
	//
	//********************************************************************************************

	public static void SetDelegates( OnSerialized onSerialized, OnDeserialized onDeserialized )
    {
        m_onSerialized   = onSerialized;

        m_onDeserialized = onDeserialized;
    }

	//********************************************************************************************
	//
	//********************************************************************************************
	
	public static bool SerializeMembers( T instance, Stream stream ) 
	{
		if( m_formatter == null ) return false;
		
		if( instance    == null ) return false;
		
		if( stream      == null ) return false;
	
		Debug.Log( "Serializing: " + instance.ToString() );


		SERIALIZATION.Manifest< T > manifest = new SERIALIZATION.Manifest< T >( instance );
		
		if( manifest.Serialize( stream, m_formatter ) == false )
		{
			return false;
		}
		
				
		for( int entry = 0; entry < manifest.m_entries.Count; ++entry )
		{
			System.Reflection.FieldInfo fieldinfo = manifest.m_entries[ entry ].fieldinfo;
			
			object                      value     = fieldinfo != null ? fieldinfo.GetValue( instance ) : null;
			
			Debug.Log( "Serializing field: " + fieldinfo.Name + " value: " + ( ( value != null ) ? value.ToString() : "null" ) );
			
			m_formatter.Serialize( stream, value );
		}
		
		return true;
	}

	//********************************************************************************************
	//
	//********************************************************************************************
	
	public static bool DeserializeMembers( T instance, Stream stream ) 
	{
		if( m_formatter == null ) return false;
		
		if( instance    == null ) return false;
		
		if( stream      == null ) return false;
		
		Debug.Log( "Deserializing: " + instance.ToString() );
		

		SERIALIZATION.Manifest< T > manifest = new SERIALIZATION.Manifest< T >( stream, m_formatter );
		
		for( int field = 0; field < manifest.m_entries.Count; ++field )
		{
			System.Reflection.FieldInfo fieldinfo = manifest.m_entries[ field ].fieldinfo;
			
			object                      val       = m_formatter.Deserialize( stream );
			
			if( fieldinfo != null )
			{
				fieldinfo.SetValue( instance, val );
				
				Debug.Log( "Deserialized field: " + fieldinfo.Name + " value: " + ( ( val != null ) ? val.ToString() : "null" ) );
			}
		}
		
		return true; 
	}
	
	//********************************************************************************************
	//
	//********************************************************************************************
	
	public static bool Serialize( T instance, Stream stream, SERIALIZATION.MODE smode )
	{
		if( m_formatter == null ) return false;
		
		if( instance    == null ) return false;
	
		if( stream      == null ) return false;
	
		m_formatter.Serialize( stream, smode );

		if( smode == SERIALIZATION.MODE.DEFAULT )
        {
            m_formatter.Serialize( stream, instance ); 
        }
		else                                      
        {
            SerializeMembers( instance, stream );
        }

        if( m_onSerialized != null ) m_onSerialized( instance );

        return true;
	}
	
	//********************************************************************************************
	//
	//********************************************************************************************
	
	public static bool Deserialize( ref T instance, Stream stream )
	{
		if( m_formatter == null ) return false;
		
		if( stream      == null ) return false;
		
		SERIALIZATION.MODE smode = ( SERIALIZATION.MODE )m_formatter.Deserialize( stream );
		
		if( smode == SERIALIZATION.MODE.DEFAULT ) 
        { 
            instance = ( T )m_formatter.Deserialize( stream ); 
        }
		else                                      
        { 
            DeserializeMembers( instance, stream ); 
        }

        if( m_onDeserialized != null ) m_onDeserialized( instance );

        return true;
	}
	
	//********************************************************************************************
	//
	//********************************************************************************************
	
	public static bool Serialize( T instance, string filename, System.IO.FileMode fmode, SERIALIZATION.MODE smode )
	{
		if( m_formatter == null )              return false;
		
		if( instance    == null )              return false;
		
		if( string.IsNullOrEmpty( filename ) ) return false;
		
		Stream stream = System.IO.File.Open( filename, fmode );
		
		bool   result = Serialize( instance, stream,   smode );
		
		if( stream != null ) stream.Close();
		
		return result;
	}
	
	//********************************************************************************************
	//
	//********************************************************************************************
	
	public static bool Deserialize( ref T instance, string filename )
	{
		if( m_formatter == null )              return false;
	
		if( string.IsNullOrEmpty( filename ) ) return false;
		
		Stream stream = System.IO.File.Open( filename, System.IO.FileMode.Open );
		
		bool   result = Deserialize( ref instance, stream );
		
		if( stream != null ) stream.Close();
		
		return result;
	}
}

//************************************************************************************************
//
//************************************************************************************************

public static class FilesUtils
{
	//********************************************************************************************
	//
	//********************************************************************************************
    
    public static FileInfo[] GetFiles( string rootDir, string pattern, SearchOption option )
    {
        if( string.IsNullOrEmpty( rootDir ) ) return null;

        if( string.IsNullOrEmpty( pattern ) ) return null;

        DirectoryInfo   dirInfo = new DirectoryInfo( rootDir );

        FileInfo[]      files   = dirInfo.GetFiles ( pattern, option );

        return files;
    }
    
	//********************************************************************************************
	//
	//********************************************************************************************

    public static bool FileContainsString( string filename, string occurence )
    {
		if( string.IsNullOrEmpty( filename  ) ) return false;

        if( string.IsNullOrEmpty( occurence ) ) return false;

        bool         found  = false;

		Stream       stream = System.IO.File.Open( filename, FileMode.Open );
		
        StreamReader reader = stream != null ? new StreamReader( stream ) : null;

		if( reader != null ) 
        {
            string line = string.Empty;
            
            while( string.IsNullOrEmpty( line = reader.ReadLine() ) == false )
            {
                if( line.Contains( occurence ) ) 
                { 
                    found = true; 
                    
                    break; 
                }
            }

            reader.Close();

            stream.Close();
        }

        return found;
    }
}

//************************************************************************************************
//
//************************************************************************************************

public static class SystemArrayExtensions
{
	public static bool Contains( this Type[] array, Type type )
	{
		int nbElts = array != null ? array.Length : 0;
		
		for( int elt = 0; elt < nbElts; ++elt )
		{
			if( array[ elt ] == type ) return true;
		}
		
		return false;
	}
}

//************************************************************************************************
//
//************************************************************************************************

public class TypesFilter
{
    public enum TYPE { INCLUSIVE, EXCLUSIVE }

    private TYPE           m_filterType = TYPE.INCLUSIVE;

    private Type[]         m_types      = null;

    private List< string > m_typesNames = new List< string >();

    public TypesFilter( TYPE filterType, params Type[] types ) 
    { 
        m_filterType = filterType; 
        
        m_types      = types;

        for( int type = 0; type < m_types.Length; ++type ) m_typesNames.Add( m_types[ type ].Name );
    }

    public bool Contains( Type   type ) { return ( m_types      != null ) && ( m_types.Contains     ( type ) ); }

    public bool Contains( string type ) { return ( m_typesNames != null ) && ( m_typesNames.Contains( type ) ); }

    public bool Include ( Type   type ) { bool containsType = Contains( type ); return m_filterType == TYPE.INCLUSIVE ? containsType : containsType == false; }

    public bool Exclude ( Type   type ) { bool containsType = Contains( type ); return m_filterType == TYPE.EXCLUSIVE ? containsType : containsType == false; }

    public bool Include ( string type ) { bool containsType = Contains( type ); return m_filterType == TYPE.INCLUSIVE ? containsType : containsType == false; }

    public bool Exclude ( string type ) { bool containsType = Contains( type ); return m_filterType == TYPE.EXCLUSIVE ? containsType : containsType == false; }
}

//************************************************************************************************
//
//************************************************************************************************

public struct RecurseDepth
{
    public int  curLevel;

    public int  maxLevel;

    public bool maxLevelReached;

    public RecurseDepth( int max ) 
    { 
        curLevel        = 0;

        maxLevel        = max; 
        
        maxLevelReached = curLevel >= maxLevel; 
    }

    public RecurseDepth( RecurseDepth depth ) 
    { 
        curLevel        = depth.curLevel + 1;

        maxLevel        = depth.maxLevel;

        maxLevelReached = curLevel >= maxLevel;
    }
}

//************************************************************************************************
//
//************************************************************************************************

public enum ASSET_OPTION { NONE, CREATE_SUBDIR }

//************************************************************************************************
//
//************************************************************************************************

public static class Asset< T > where T : ScriptableObject
{
	//********************************************************************************************
	//
	//********************************************************************************************

    private const string m_resourcesDir = "Resources";

	//********************************************************************************************
	//
	//********************************************************************************************
    
    public static bool GetResourceInfos( T instance, ref string dir, ref string name, string relativeTo )
    {
        #if UNITY_EDITOR

			string path = instance != null ? AssetDatabase.GetAssetPath( instance ) : string.Empty;

			CORE.Path.Normalize    ( ref path, CORE.Path.OPTIONS.SEPARATORS );

			CORE.Path.SetRelativeTo( ref path, relativeTo );

			dir  = System.IO.Path.GetDirectoryName( path );
			
			name = System.IO.Path.GetFileNameWithoutExtension( path );
		
			return ( string.IsNullOrEmpty( path ) == false );

        #else

            dir = name = string.Empty;

            return false;

        #endif
    }

	//********************************************************************************************
	//
	//********************************************************************************************
    
	public static string CreateDirectory( ASSET_OPTION option )
	{
		#if UNITY_EDITOR
		
			string classname       = typeof( T ).ToString();
			
			string directory       = Application.dataPath + "/" + m_resourcesDir; if( option == ASSET_OPTION.CREATE_SUBDIR ) directory += ( "/" + classname ); 
			
			string unity_directory = UnityEditor.FileUtil.GetProjectRelativePath( directory );
			
			if( System.IO.Directory.Exists( directory ) == false )
			{
				System.IO.Directory.CreateDirectory( directory );
	        }
	        
	        return unity_directory;
        
        #else
        
	        return string.Empty;
        
        #endif
	}
	
	//********************************************************************************************
	//
	//********************************************************************************************
    
	public static bool Create( ASSET_OPTION option ) 
	{
		#if UNITY_EDITOR
		
			string classname = typeof( T ).ToString();
			
			string filename  = classname + ".asset";
			
	        string directory = CreateDirectory( option );
			
			AssetDatabase.CreateAsset( ScriptableObject.CreateInstance< T >(), directory + "/" + filename );
			
			AssetDatabase.SaveAssets ();

            UnityEngine.Debug.Log( "Created asset " + filename );
			
			return true;
		
		#else
		
			return false;
		
		#endif
	}

	//********************************************************************************************
	//
	//********************************************************************************************
    
	public static bool Exist()
	{
		#if UNITY_EDITOR
		
			string   classname = typeof( T ).ToString();
			
			string   filename  = classname + ".asset";
		
			string[] guids     = AssetDatabase.FindAssets( classname );
			
			int      nbAssets  = guids != null ? guids.Length : 0;
			
			for( int asset = 0; asset < nbAssets; ++asset )
			{ 
				string path = AssetDatabase.GUIDToAssetPath( guids[ asset ] );
				
				string name = System.IO.Path.GetFileName( path );
				
				if( name == filename )
				{
					return true;
				}
			}
			
			return false;
		
		#else
		
			return true;
		
		#endif
	} 
	
	//********************************************************************************************
	//
	//********************************************************************************************
    
	public static T Load( ASSET_OPTION option )
	{
		T instance = null;

        if( ( Exist() ) || Create( option ) )
        {
    		string classname = typeof( T ).ToString();

            string resname   = ( option == ASSET_OPTION.CREATE_SUBDIR ) ? classname + "/" + classname : classname;

            UnityEngine.Debug.Log( "Loading res " + resname );

            instance = Resources.Load< T >( resname );
        }

		return instance;
	}
}

//************************************************************************************************
//
//************************************************************************************************

[ System.Serializable ]public class Config< T > : ScriptableObject where T : ScriptableObject
{
    private static T m_instance = default( T );

	public  static T Instance { get { if( m_instance == null ) { m_instance = ( T )Asset< T >.Load( ASSET_OPTION.CREATE_SUBDIR ); } return m_instance; } } 
}

