using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

using System.IO;

using BinaryFormatter = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;

//********************************************************************************************************
//
//********************************************************************************************************

namespace CORE
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public abstract class Parsable< ObjectT >
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		static private  long m_sizeof = ( ushort )Marshal.SizeOf( typeof( ObjectT ) );

		static public   long @sizeof    { get { return m_sizeof; } }

		public abstract bool Parse      ( byte[] bytes, int pos );

		//************************************************************************************************
		//
		//************************************************************************************************

		static public bool READ_08( ref char dst, byte[] bytes, ref int pos )
		{
			if( pos >= bytes.Length ) return false;

			dst = Convert.ToChar( bytes[ pos++ ] );

			return true;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static public bool READ_16( ref ushort dst, byte[] bytes, ref int pos )
		{
			if( pos + 1 >= bytes.Length ) return false;

			dst  = ( ushort )( Convert.ToUInt16( bytes[ pos++ ] ) << 0 );

			dst |= ( ushort )( Convert.ToUInt16( bytes[ pos++ ] ) << 8 );

			return true;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static public bool READ_32( ref uint dst, byte[] bytes, ref int pos )
		{
			if( pos + 3 >= bytes.Length ) return false;

			dst  = ( ushort )( Convert.ToUInt32( bytes[ pos++ ] ) << 0  );

		    dst |= ( ushort )( Convert.ToUInt32( bytes[ pos++ ] ) << 8  );

		    dst |= ( ushort )( Convert.ToUInt32( bytes[ pos++ ] ) << 16 );

		    dst |= ( ushort )( Convert.ToUInt32( bytes[ pos++ ] ) << 24 );

			return true;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static public bool READ_64( ref ulong dst, byte[] bytes, ref int pos )
		{
			if( pos + 7 >= bytes.Length ) return false;

			dst  = ( ushort )( Convert.ToUInt32( bytes[ pos++ ] ) << 0  );

		    dst |= ( ushort )( Convert.ToUInt32( bytes[ pos++ ] ) << 8  );

		    dst |= ( ushort )( Convert.ToUInt32( bytes[ pos++ ] ) << 16 );

			dst |= ( ushort )( Convert.ToUInt32( bytes[ pos++ ] ) << 24 );

			dst |= ( ushort )( Convert.ToUInt32( bytes[ pos++ ] ) << 32 );

			dst |= ( ushort )( Convert.ToUInt32( bytes[ pos++ ] ) << 40 );

			dst |= ( ushort )( Convert.ToUInt32( bytes[ pos++ ] ) << 48 );

			dst |= ( ushort )( Convert.ToUInt32( bytes[ pos++ ] ) << 56 );

			return true;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class Row : Parsable< Row > 
	{
		ushort m_index     = 0;

		ushort m_colFirst  = 0;

		ushort m_colLast   = 0;

		ushort m_height    = 0;

		ushort m_reserved1 = 0;

		ushort m_unused1   = 0;


		public override bool Parse( byte[] bytes, int pos )
		{
			if( pos + @sizeof > bytes.Length ) return false;

			READ_16( ref m_index,		bytes, ref pos );

			READ_16( ref m_colFirst,	bytes, ref pos );

			READ_16( ref m_height,		bytes, ref pos );

			READ_16( ref m_colLast,		bytes, ref pos );

			READ_16( ref m_height,		bytes, ref pos );

			READ_16( ref m_reserved1,	bytes, ref pos );

			READ_16( ref m_unused1,		bytes, ref pos );

			return true;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class Value : Parsable< Value > 
	{
		ushort m_row = 0;

		ushort m_col = 0;


		public override bool Parse( byte[] bytes, int pos )
		{
			if( pos + @sizeof > bytes.Length ) return false;

			READ_16( ref m_row, bytes, ref pos );

			READ_16( ref m_col, bytes, ref pos );

			return true;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class Record // BIFF12 include variable length headers and consist in zips with multipart BINs > switching to CSV
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		public enum SPECS { MAX_SIZE = 8224 }

		private ushort m_type  = 0;

		private ushort m_size  = 0;

		private byte[] m_bytes = new byte[ 0 ];

		//************************************************************************************************
		//
		//************************************************************************************************

		public Record( ushort paramType, ushort paramSize )
		{
			type = paramType;

			size = paramSize;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public void SetSize( int paramSize )
		{
			ushort sze = ( ushort )Mathf.Clamp( paramSize, 0, ( int )SPECS.MAX_SIZE );

			if( m_size != sze )
			{
				m_size  = sze;

				System.Array.Resize( ref m_bytes, sze );
			}
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public ushort type  { get { return m_type;  } set { m_type = value;   } }

		public ushort size  { get { return m_size;  } set { SetSize( value ); } }

		public byte[] bytes { get { return m_bytes; } }
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

namespace CSV
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum COL    { LONGITUDE, LATITUDE, SITE_NAME, DATE_OPENED, DATE_CLOSED, ITEM_NAME, DATE_FOUND, DESC, NB }

	public enum COORDS { WGS84, LAMBERT_93 }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class Options
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		private int[] m_cols = new int[ ( int )CSV.COL.NB ];

		//************************************************************************************************
		//
		//************************************************************************************************

		public Options()
		{
			ResetToDefault();
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public Options( Options o )
		{
			if( o == null ) { ResetToDefault(); return; }

			separator = o.separator;

			coords    = o.coords;

			System.Array.Copy( o.cols, cols, o.cols.Length );
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public void ResetToDefault()
		{
			separator = ';';

			coords    = COORDS.WGS84;

			cols[ ( int )COL.LONGITUDE ] = 0;

			cols[ ( int )COL.LATITUDE ] = 1;

			for( int col = 2; col < cols.Length; ++col ) cols[ col ] = -1;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public bool   valid         { get { return ( colX != colY ) && ( colX != -1 ) && ( colY != -1 ); } }

		public char   separator     { get; set; }

		public COORDS coords        { get; set; }

		public int[]  cols          { get { return m_cols; } }

		public int    colX          { get { return cols[ ( int )COL.LONGITUDE   ]; } set { cols[ ( int )COL.LONGITUDE   ] = value; } }

		public int    colY          { get { return cols[ ( int )COL.LATITUDE    ]; } set { cols[ ( int )COL.LATITUDE    ] = value; } }

		public int    colSiteName   { get { return cols[ ( int )COL.SITE_NAME   ]; } set { cols[ ( int )COL.SITE_NAME   ] = value; } }

		public int    colItemName   { get { return cols[ ( int )COL.ITEM_NAME   ]; } set { cols[ ( int )COL.ITEM_NAME   ] = value; } }

		public int    colDateOpened { get { return cols[ ( int )COL.DATE_OPENED ]; } set { cols[ ( int )COL.DATE_OPENED ] = value; } }

		public int    colDateClosed { get { return cols[ ( int )COL.DATE_CLOSED ]; } set { cols[ ( int )COL.DATE_CLOSED ] = value; } }

		public int    colDateFound  { get { return cols[ ( int )COL.DATE_FOUND  ]; } set { cols[ ( int )COL.DATE_FOUND  ] = value; } }

		public int    colDesc       { get { return cols[ ( int )COL.DESC        ]; } set { cols[ ( int )COL.DESC        ] = value; } }

		//************************************************************************************************
		//
		//************************************************************************************************

		public Options byval { get { Options o = new Options( this ); return o; } }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class File
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		private const bool ASYNC_NO  = false;

		private const bool ASYNC_YES = true;

		//************************************************************************************************
		//
		//************************************************************************************************

		private Options  m_options = new Options();

		private string[] m_header  = null;

		private string[] m_lines   = null;

		public  Options  options { get { return m_options; } }

		//************************************************************************************************
		//
		//************************************************************************************************

		private void DiscardContent()
		{
			m_header = null;

			m_lines  = null;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		private void Trim( string[] values )
		{
			if( values != null )
			{
				for( int val = 0; val < values.Length; ++val ) { values[ val ] = values[ val ].Trim( '"' ); }
			}
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		private bool Read( string path )
		{
			DiscardContent();

			FileInfo info = new FileInfo( path );

			if( info.Exists == false        ) return false;

			if( info.Length <= 0            ) return false;

			if( info.Length > ( 256 << 20 ) ) return false;


			m_lines = System.IO.File.ReadAllLines( path );

			if( ( m_lines != null ) && ( m_lines.Length > 1 ) )
			{
				m_header = m_lines[ 0 ].Split( m_options.separator );

				Trim( m_header );

				return true;
			}

			return false;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		private string MarkDuplicateNames< LocalizableT >( string name ) where LocalizableT : Localizable, new()
		{
			if( string.IsNullOrEmpty( name ) == false )
			{
				List< Localizable > list = ( typeof( LocalizableT ) == typeof( Site ) ) ? DBObjects.instance.sites : DBObjects.instance.items;

				for( int obj = 0; obj < list.Count; ++obj )
				{
					if( list[ obj ].name.CompareTo( name ) == 0 ) { name = "(duplicate) " + name; return name; }
				}
			}

			return name;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		private bool Import()
		{
			if( ( m_header == null ) || ( m_header.Length < 2 ) ) return false;

			if( DBObjects.instance == null )                      return false;

			if( DBObjects.instance.busy    )                      return false;


			object session = DBObjects.instance.BeginEdit();

			if( session == null ) return false;


			List< KeyValuePair< Item, string > > parentNames = new List< KeyValuePair< Item, string > >();

			for( int entry = 1; entry < m_lines.Length; ++entry )
			{
				string[] values = m_lines[ entry ].Split( m_options.separator );

				Trim( values );


				if( values.Length != m_header.Length )
				{
					Debug.Log( string.Format( "CSV > IMPORT > entry {0} of {1} > Skipping", entry + 1, m_lines.Length ) );

					continue;
				}
				else
				{
					Debug.Log( string.Format( "CSV > IMPORT > entry {0} of {1} > Adding", entry + 1, m_lines.Length ) );
				}


				Vector2 coords = Vector2.zero;

				float.TryParse( values[ m_options.colX ], out coords.x );

				float.TryParse( values[ m_options.colY ], out coords.y );

				string siteName   = ( m_options.colSiteName   != -1 ) ? values[ m_options.colSiteName   ] : string.Empty;

				string itemName   = ( m_options.colItemName   != -1 ) ? values[ m_options.colItemName   ] : string.Empty;

				string dateOpened = ( m_options.colDateOpened != -1 ) ? values[ m_options.colDateOpened ] : string.Empty;

				string dateClosed = ( m_options.colDateClosed != -1 ) ? values[ m_options.colDateClosed ] : string.Empty;

				string dateFound  = ( m_options.colDateFound  != -1 ) ? values[ m_options.colDateFound  ] : string.Empty;

				string desc       = ( m_options.colDesc       != -1 ) ? values[ m_options.colDesc       ] : string.Empty;



				Localizable localizable = null;
				
				if( string.IsNullOrEmpty( itemName ) == false ) localizable = DBObjects.instance.Create< Item >( null, null, session );

				else                                            localizable = DBObjects.instance.Create< Site >( null, null, session );
				
				if( localizable == null ) continue;

				localizable.name   = localizable is Site ? MarkDuplicateNames< Site >( siteName ) : MarkDuplicateNames< Item >( itemName );

				localizable.m_desc = desc;

				


				if( m_options.coords == COORDS.LAMBERT_93 ) coords = CORE.CONVERT.LambertToLngLat( coords );

				coords.x = CORE.Angle.Normalize( coords.x, Angle.UNIT.DEG , Angle.NORM.NEG, GPS.TYPE.LONGITUDE );

				coords.y = CORE.Angle.Normalize( coords.y, Angle.UNIT.DEG , Angle.NORM.NEG, GPS.TYPE.LATITUDE  );

				localizable.m_coord.longitude.FromAngle( coords.x, GPS.UNIT.DD );

				localizable.m_coord.latitude.FromAngle ( coords.y, GPS.UNIT.DD );


				if( localizable is Site )
				{
					Site            site  = localizable as Site;

					System.DateTime date1 = System.DateTime.Now; System.DateTime.TryParse( dateOpened, out date1 );

					System.DateTime date2 = System.DateTime.Now; System.DateTime.TryParse( dateClosed, out date2 );

					site.m_dateOpened = date1;

					site.m_dateClosed = date2;
				}
				else
				{
					Item            item  = localizable as Item;

					System.DateTime date1 = System.DateTime.Now; System.DateTime.TryParse( dateFound, out date1 );

					item.m_dateFound = date1;

					parentNames.Add( new KeyValuePair< Item, string >( item, siteName ) );
				}

				localizable.Async_DBPush( null );
			}


			for( int pair = 0; pair < parentNames.Count; ++pair )
			{
				Localizable item   = parentNames[ pair ].Key;

				Localizable parent = Localizable.Get( parentNames[ pair ].Value );

				if( ( item != null ) && ( parent != null ) && ( parent is Site ) )
				{
					item.SetParent( parent );
				}
			}


			DBObjects.instance.EndEdit( session );

			DiscardContent();

			return true;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public void Import( string path )
		{
			if( POPUPCsvConfig.Instance == null ) return;

			if( DBObjects.instance      == null ) return;

			if( DBObjects.instance.busy )         return;

			if( Read( path ) == false   )         return;

			POPUPCsvConfig.Instance.Show( OnPopupCSVConfigButton, m_header );
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		private string QuotedString( string s )
		{
			return ( string.IsNullOrEmpty( s ) == false ) ? ( '\"' + s + '\"' ) : ( "\"\"" );
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		private string DateToString( System.DateTime date )
		{
			return string.Format( "{0}-{1}-{2}", date.Year, date.Month, date.Day );
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public void Export( string path )
		{
			if( ( DBObjects.instance != null ) && ( DBObjects.instance.busy ) )
			{
				return;
			}

			List< Localizable > sites = DBObjects.instance.sites;

			List< Localizable > items = DBObjects.instance.items;


			string header  = string.Empty;

			int    nbCols  = ( int )COL.NB;

			int    lastCol = nbCols - 1;

			for( int col = 0; col < nbCols; ++col ) { header += QuotedString( ( ( COL )col ).ToString() ); if( col < lastCol ) header += ';'; }

			header = CORE.ENCODE.ToUTF8( header );


			using( System.IO.StreamWriter file = System.IO.File.CreateText( path ) )
			{
				if( file != null )
				{
					file.WriteLine( header );


					for( int site = 0; site < sites.Count; ++site )
					{
						Site Site = sites[ site ] as Site;

						string line  = QuotedString( Site.m_coord.longitude.deg.ToString() ) + ';';

						       line += QuotedString( Site.m_coord.latitude.deg.ToString()  ) + ';';

						       line += QuotedString( Site.name ) + ';';

						       line += QuotedString( DateToString( Site.m_dateOpened ) ) + ';';

						       line += QuotedString( DateToString( Site.m_dateClosed ) ) + ';';

						       line += QuotedString( null ) + ';';

						       line += QuotedString( null ) + ';';

						       line += QuotedString( Site.m_desc );


						line = CORE.ENCODE.ToUTF8( line );

						file.WriteLine( line );
					}
					

					for( int item = 0; item < items.Count; ++item )
					{
						Item Item = items[ item ] as Item;

						string line  = QuotedString( Item.m_coord.longitude.deg.ToString() ) + ';';

						       line += QuotedString( Item.m_coord.latitude.deg.ToString()  ) + ';';

						       line += QuotedString( ( Item.m_parent != null ) ? Item.m_parent.name : null ) + ';';

						       line += QuotedString( null ) + ';';

						       line += QuotedString( null ) + ';';

						       line += QuotedString( Item.name ) + ';';

						       line += QuotedString( DateToString( Item.m_dateFound ) ) + ';';

						       line += QuotedString( Item.m_desc );


						line = CORE.ENCODE.ToUTF8( line );

						file.WriteLine( line );
					}
				}
			}
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		private void OnPopupCSVConfigButton( POPUPCsvConfig popup, POPUPCsvConfig.BUTON but )
		{
			if( but == POPUPCsvConfig.BUTON.OK )
			{
				m_options = popup.m_options.byval;

				Import();
			}
		}
	}
}

