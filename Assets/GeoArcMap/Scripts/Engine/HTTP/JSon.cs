using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CORE;

/*********************************************************************************************************

ref 1: https://tools.ietf.org/html/rfc4627

ref 2: Unity advert against JsonUtility

*********************************************************************************************************/

public abstract class JSonEntry
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public JSonEntry( string Name, JSonEntry Parent )
	{
		parent = Parent;

		name   = ( Name != null ) ? Name : string.Empty;

		if( Parent != null ) { Parent.Add( this ); } 
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public JSonEntry                                 parent         { get; set; }

	public string                                    name           { get; set; }

	public virtual   Dictionary< string, JSonEntry > obj            { get { return null; } protected set { } }

	public virtual   List< JSonEntry >               array          { get { return null; } protected set { } }

	public virtual   string                          value          { get { return null; }           set { } }

	public abstract  int                             count          { get; }

	public abstract  JSonEntry                       this[ int id ] { get; }

	public abstract  void                            Add            ( JSonEntry entry );

	public abstract  JSonEntry                       Resolve        ( string[] pathComps, int pathComp );

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public JSonEntry this[ string id ] { get { return ( string.IsNullOrEmpty( id ) ) ? null : Resolve( id.Split( '.' ), 0 ); } }
}

//********************************************************************************************************
//
//********************************************************************************************************

public class JSonObject : JSonEntry
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	private Dictionary< string, JSonEntry > m_value = new Dictionary< string, JSonEntry >();

	public JSonObject( string Name, JSonEntry Parent ) : base( Name, Parent ) {}

	public override Dictionary< string, JSonEntry > obj            { get { return m_value; } protected set { m_value = value; } }

	public override int                             count          { get { return m_value.Count; } }

	public override JSonEntry                       this[ int id ] { get { return null; } }

	public override void                            Add            ( JSonEntry entry ) { m_value.Add( entry.name, entry ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override JSonEntry Resolve( string[] pathComps, int pathComp )
	{
		if( pathComps.Length <= 0 )        return null;

		if( pathComp         <  0 )        return null;

		if( pathComp >= pathComps.Length ) return this;

		JSonEntry o = null;

		if( m_value.TryGetValue( pathComps[ pathComp ], out o ) ) { if( o != null ) return o.Resolve( pathComps, pathComp + 1 ); }

		return null;
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class JSonArray : JSonEntry
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	private List< JSonEntry > m_value = new List< JSonEntry >();

	public JSonArray( string Name, JSonEntry Parent ) : base( Name, Parent ) {}

	public override List< JSonEntry > array          { get { return m_value; } protected set { m_value = value; } }

	public override int               count          { get { return m_value.Count; } }

	public override JSonEntry         this[ int id ] { get { return m_value[ id ]; } }

	public override void              Add            ( JSonEntry entry ) { m_value.Add( entry ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override JSonEntry Resolve( string[] pathComps, int pathComp )
	{
		if( pathComps.Length <= 0 )        return null;

		if( pathComp         <  0 )        return null;

		if( pathComp >= pathComps.Length ) return this;

		int i = 0;

		if( int.TryParse( pathComps[ pathComp ].Trim( '[', ']' ), out i ) )
		{
			if( i < m_value.Count ) { if( m_value[ i ] != null ) return m_value[ i ].Resolve( pathComps, pathComp + 1 ); }
		}

		return null;
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class JSonValue : JSonEntry
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public JSonValue( string Name, JSonEntry Parent ) : base( Name, Parent ) { }

	public override string    value          { get; set; }

	public override int       count          { get { return 0;    } }

	public override JSonEntry this[ int id ] { get { return null; } }

	public override void      Add            ( JSonEntry entry ) { Debug.Assert( false, "JSonValue: cannot be parent of another json elt" ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override JSonEntry Resolve( string[] pathComps, int pathComp )
	{
		if( pathComps.Length <= 0 )        return null;

		if( pathComp         <  0 )        return null;

		if( pathComp >= pathComps.Length ) return this;

		return null;
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class JSon
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public const char BGN_OBJ  = '{';

	public const char END_OBJ  = '}';

	public const char BGN_ARR  = '[';

	public const char END_ARR  = ']';

	public const char QUOTE    = '\"';

	public const char COLON    = ':';

	public const char COMMA    = ',';

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private static class Semantics
	{
		[ System.Flags ] public enum PROP { NONE = 0X0, GRP = 0X1, CTRL = 0X2, EXPR_SEP = 0X4, ALL = ( GRP | CTRL | EXPR_SEP ) }

		static PROP[] sems = new PROP[ 256 ];

		static               Semantics() { addProps( "{}[]", PROP.GRP );  addProps( "}]:,", PROP.CTRL );  addProps( ":,", PROP.EXPR_SEP ); }

		static private void  addProps ( string chrs, PROP props ) { for( int c = 0; c < chrs.Length; ++c ) { sems[ chrs[ c ] ] |= props; } }

		static public bool   hasProps ( char   chr , PROP props ) { return ( chr < 256 ) ? ( sems[ chr ] & props ) != 0 : false; }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private class ControlStack
	{
		public class Expression
		{
			public int    start  = -1;

			public int    end    = -1;

			public int    quotes = 0;

			public bool   literal   { get { return quotes > 0; } }

			public bool   delimited { get { return ( start != -1 ) && ( end != -1 ) && ( start <= end ); } }

			public bool   valid     { get { return ( delimited ) && ( ( literal == false ) || ( ( quotes & 1 ) == 0 ) ); } }

			public void   Reset     () { start = -1; end = -1; quotes = 0; } 
		}

		private const int   MAX_LVL = 128;

		private JSonEntry[] m_stack = new JSonEntry[ MAX_LVL ];

		private int         m_count = 0;

		private Expression  m_expr  = new Expression();


		public void       Flush() { m_count = 0; }

		public bool       push(     JSonEntry entry ) { if( m_count < MAX_LVL ) { m_stack[ m_count++ ] = entry; return true; } return false; }

		public bool       pop ( ref JSonEntry entry ) { if( m_count > 0       ) { entry = m_stack[ --m_count ]; return true; } return false; }

		public bool       empty                       { get { return m_count <= 0; } }

		public Expression expr                        { get { return m_expr;       } }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private JSonEntry   m_root = null;

	public JSonEntry    root              { get { return m_root;             } }

	public bool         empty             { get { return ( m_root == null ); } }

	public int          count             { get { return ( m_root != null ) ? m_root.count : 0;    } }

	public JSonEntry    this[ int    id ] { get { return ( m_root != null ) ? m_root[ id ] : null; } }

	public JSonEntry    this[ string id ] { get { return ( m_root != null ) ? m_root[ id ] : null; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public JSon( string content = null )
	{
		Parse( content );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool Parse ( string json )
	{
		if( string.IsNullOrEmpty( json ) ) return false;

		ControlStack ctrl   = new ControlStack();

		JSonEntry    cur    = null;

		string       name   = null;

		             m_root = null;


		for( int c = 0; c < json.Length; ++c )
		{
			//--------------------------------------------------------------------------------------------
			// register expression
			//--------------------------------------------------------------------------------------------

			if( char.IsWhiteSpace( json[ c ] ) == false )
			{
				if( Semantics.hasProps( json[ c ], Semantics.PROP.ALL  ) == false )
				{
					if( ctrl.expr.start == -1 ) ctrl.expr.end = ctrl.expr.start = c;

					else                        ctrl.expr.end = c;

					if( json[ c ] == QUOTE ) { ++ctrl.expr.quotes; continue; }
				}
			}
			else
			{
				continue;
			}

			//--------------------------------------------------------------------------------------------
			// register name or value
			//--------------------------------------------------------------------------------------------

			if( Semantics.hasProps( json[ c ], Semantics.PROP.CTRL ) == true  )
			{
				if( ( ctrl.expr.quotes & 1 ) == 0 )
				{
					bool isName  = ( json[ c ] == COLON );

					bool isValue = ( isName    == false );

					bool valid   = ctrl.expr.valid;

					if( ( isName ) && ( valid == false ) )             return false;

					if( ( isName ) && ( cur is JSonObject ) == false ) return false;

					if( valid )
					{
						string expr = json.Substring( ctrl.expr.start, ctrl.expr.end - ctrl.expr.start + 1 ).TrimEnd( ' ', QUOTE, ' ' ).TrimStart( ' ', QUOTE, ' ' );

						if     ( isName  ) { name = expr; }

						else if( isValue ) { new JSonValue( name, cur ).value = expr; name = null; }
					}

					ctrl.expr.Reset();

					if( Semantics.hasProps( json[ c ], Semantics.PROP.GRP ) == false ) continue;
				}
			}

			//--------------------------------------------------------------------------------------------
			// register object or array
			//--------------------------------------------------------------------------------------------

			if( ctrl.expr.quotes == 0 )
			{
				if( json[ c ] == BGN_OBJ  ) { if( ( cur == null ) || ctrl.push( cur ) ) { cur = new JSonObject( name, cur ); if( m_root == null ) m_root = cur; name = null; ctrl.expr.Reset(); continue; } else return false; }

				if( json[ c ] == BGN_ARR  ) { if( ( cur == null ) || ctrl.push( cur ) ) { cur = new JSonArray ( name, cur ); if( m_root == null ) m_root = cur; name = null; ctrl.expr.Reset(); continue; } else return false; }

				if( json[ c ] == END_OBJ  ) { if( cur == m_root ) break; if( ( cur is JSonObject ) && ctrl.pop( ref   cur ) ) { ctrl.expr.Reset(); continue; } else return false; }

				if( json[ c ] == END_ARR  ) { if( cur == m_root ) break; if( ( cur is JSonArray  ) && ctrl.pop( ref   cur ) ) { ctrl.expr.Reset(); continue; } else return false; }
			}
		}

		return ( m_root != null ) && ( ctrl.empty );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public void UnitTest()
	{
		JSon json = new JSon();

		bool result = json.Parse( "{ \"menu\":{\"test\":\"expression with \"quotes\"\",\"id\":\"file\",\"value\":\"File\",\"popup\":{\"menuitem\":[{\"value\":\"New\",\"onclick\":\"CreateNewDoc()\"},{\"value\":\"Open\",\"onclick\":\"OpenDoc()\"},{\"value\":\"Close\",\"onclick\":\"CloseDoc()\"}]}}}" );

		Debug.Assert( result == true );

		if( result )
		{
			string value  = json[ "menu.popup.menuitem.[2].onclick" ].value;

			bool  success = ( value != null ) && ( value.CompareTo( "CloseDoc()" ) == 0 );

			Debug.Assert( success );
		}
	}
}