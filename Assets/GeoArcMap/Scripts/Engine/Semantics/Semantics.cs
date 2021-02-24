//********************************************************************************************************
//
//********************************************************************************************************

namespace CORE
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	namespace ASCII
	{
		[ System.Flags ] public enum PROPS
		{
			PROP_NOT_SET			= 0X00000000,

			PROP_ALPHABET			= 0X00000001,

			PROP_ALPHA_NUMERIC		= 0X00000002,

			PROP_FLOAT_LITTERAL		= 0X00000004,

			PROP_OPERATOR			= 0X00000008,

			PROP_SIGN				= 0X00000010,

			PROP_GROUP_LITTERAL		= 0X00000020,

			PROP_GROUP_EXPRESSION	= 0X00000040,

			PROP_GROUP_MARKUP		= 0X00000080,

			PROP_GROUP				= 0X00000100,

			PROP_GROUP_START		= 0X00000200,

			PROP_GROUP_END			= 0X00000400,

			PROP_WILDCARD			= 0X00000800,

			PROP_PATH_SEPARATOR		= 0X00001000,

			PROP_FOLDER_SEPARATOR	= 0X00002000,
		}

		public static class Semantics
		{
			static private PROPS[] m_props = new PROPS[ 256 ];

			static public  void Add     ( string symbols, PROPS props ) { for( int symbol = 0; symbol < symbols.Length; ++symbol ) { m_props[ symbols[ symbol ] ] |= props; } }

			static public  bool HasProps( char chr,       PROPS props ) { return ( chr < 256 ) ? ( m_props[ chr ] & props ) != 0 : false; }

			static Semantics()
			{
				Add( "abcdefghijklmnopqrstuvwxyz",	PROPS.PROP_ALPHABET         );

				Add( "ABCDEFGHIJKLMNOPQRSTUVWXYZ",	PROPS.PROP_ALPHABET			);

				Add( "0123456789",					PROPS.PROP_ALPHA_NUMERIC	);

				Add( "-+.0123456789ef",				PROPS.PROP_FLOAT_LITTERAL	);

				Add( "=+-*/^",						PROPS.PROP_OPERATOR			);

				Add( "+-",							PROPS.PROP_SIGN				);

				Add( "(){}<>\"",					PROPS.PROP_GROUP			);

				Add( "({<\"",						PROPS.PROP_GROUP_START		);

				Add( ")}>\"",						PROPS.PROP_GROUP_END		);

				Add( "\"",							PROPS.PROP_GROUP_LITTERAL	);

				Add( "()",							PROPS.PROP_GROUP_EXPRESSION	);

				Add( "<>",							PROPS.PROP_GROUP_MARKUP		);

				Add( "?*",							PROPS.PROP_WILDCARD			);

				Add( ":/\\",						PROPS.PROP_PATH_SEPARATOR	);

				Add( "/\\",							PROPS.PROP_FOLDER_SEPARATOR	);
			}

			static public bool IsAlphabet       ( char symbol ) { return HasProps( symbol, PROPS.PROP_ALPHABET         ); }

			static public bool IsAlphaNum       ( char symbol ) { return HasProps( symbol, PROPS.PROP_ALPHA_NUMERIC    ); }

			static public bool IsFloatLitteral  ( char symbol ) { return HasProps( symbol, PROPS.PROP_FLOAT_LITTERAL   ); }

			static public bool IsOperator       ( char symbol ) { return HasProps( symbol, PROPS.PROP_OPERATOR         ); }

			static public bool IsSign           ( char symbol ) { return HasProps( symbol, PROPS.PROP_SIGN             ); }

			static public bool IsGroup          ( char symbol ) { return HasProps( symbol, PROPS.PROP_GROUP            ); }

			static public bool IsGroupStart     ( char symbol ) { return HasProps( symbol, PROPS.PROP_GROUP_START      ); }

			static public bool IsGroupEnd       ( char symbol ) { return HasProps( symbol, PROPS.PROP_GROUP_END        ); }

			static public bool IsGroupLitteral  ( char symbol ) { return HasProps( symbol, PROPS.PROP_GROUP_LITTERAL   ); }

			static public bool IsGroupExpression( char symbol ) { return HasProps( symbol, PROPS.PROP_GROUP_EXPRESSION ); }

			static public bool IsGroupMarkup    ( char symbol ) { return HasProps( symbol, PROPS.PROP_GROUP_MARKUP     ); }

			static public bool IsWildcard       ( char symbol ) { return HasProps( symbol, PROPS.PROP_WILDCARD         ); }

			static public bool IsPathSeparator  ( char symbol ) { return HasProps( symbol, PROPS.PROP_PATH_SEPARATOR   ); }

			static public bool IsFolderSeparator( char symbol ) { return HasProps( symbol, PROPS.PROP_FOLDER_SEPARATOR ); }
		}
	}
}
