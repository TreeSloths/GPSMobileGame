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

	static public class HIERARCHY
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		static public GameObject Find( GameObject root, string name )
		{
			if( root == null )                      return null;

			if( string.IsNullOrEmpty( name ) )      return null;

			if( root.name.CompareTo ( name ) == 0 ) return root;


			List< GameObject > scope = new List< GameObject >( 256 );

			scope.Add( root );

			for( int cur = 0; cur < scope.Count; ++cur )
			{
				for( int child = 0; child < scope[ cur ].transform.childCount; ++child )
				{
					GameObject Child = scope[ cur ].transform.GetChild( child ).gameObject;

					if( Child.name.CompareTo( name ) == 0 )
					{
						return Child;
					}
					else
					{
						scope.Add( Child );
					}
				}
			}

			return null;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static private GameObject FindChild( GameObject obj, string name )
		{
			if( obj == null )                  return null;

			if( string.IsNullOrEmpty( name ) ) return null;


			for( int child = 0; child < obj.transform.childCount; ++child )
			{
				GameObject Child = obj.transform.GetChild( child ).gameObject;

				if( Child.name.CompareTo( name ) == 0 )
				{
					return Child;
				}
			}

			return null;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static public GameObject Resolve( GameObject root, string path )
		{
			string[] comps   = ( path  != null ) ? path.Split( new char[] { '.' }, System.StringSplitOptions.RemoveEmptyEntries ) : null;

			int      nbComps = ( comps != null ) ? comps.Length : 0;

			if( nbComps <= 0 ) return null;


			GameObject obj = ( root != null ) ? root : GameObject.Find( comps[ 0 ] );

			for( int  comp = ( root != null ) ? 0 : 1; comp < nbComps; ++comp )
			{
				if( ( obj = FindChild( obj, comps[ comp ] ) ) == null )
				{
					return null;
				}
			}


			if( obj == null )
			{
				CORE.UTILS.Log( string.Format( "GameObject::Resolve() > could not resolve \"{0}\"", path ) );
			}

			return obj;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static public MonoBehaviourT FindComp< MonoBehaviourT >( GameObject root, string name ) where MonoBehaviourT : MonoBehaviour
		{
			if( root == null )                      return null;

			if( string.IsNullOrEmpty( name ) )      return null;

			if( root.name.CompareTo ( name ) == 0 ) return root.GetComponent( typeof( MonoBehaviourT ) ) as MonoBehaviourT;


			List< GameObject > scope = new List< GameObject >( 256 );

			scope.Add( root );

			for( int cur = 0; cur < scope.Count; ++cur )
			{
				for( int child = 0; child < scope[ cur ].transform.childCount; ++child )
				{
					GameObject Child = scope[ cur ].transform.GetChild( child ).gameObject;

					if( Child.name.CompareTo( name ) == 0 )
					{
						return Child.GetComponent( typeof( MonoBehaviourT ) ) as MonoBehaviourT;
					}
					else
					{
						scope.Add( Child );
					}
				}
			}

			return null;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static public TransformT FindXForm< TransformT >( GameObject root, string name ) where TransformT : Transform
		{
			if( root == null )                      return null;

			if( string.IsNullOrEmpty( name ) )      return null;

			if( root.name.CompareTo ( name ) == 0 ) return root.GetComponent( typeof( TransformT ) ) as TransformT;


			List< GameObject > scope = new List< GameObject >( 256 );

			scope.Add( root );

			for( int cur = 0; cur < scope.Count; ++cur )
			{
				for( int child = 0; child < scope[ cur ].transform.childCount; ++child )
				{
					GameObject Child = scope[ cur ].transform.GetChild( child ).gameObject;

					if( Child.name.CompareTo( name ) == 0 )
					{
						return Child.GetComponent( typeof( TransformT ) ) as TransformT;
					}
					else
					{
						scope.Add( Child );
					}
				}
			}

			return null;
		}
	}
}
