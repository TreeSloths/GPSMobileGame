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

	static public class COLOR
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		public class Properties
		{
			public Properties( float h = 0.0f, float b = 0.0f, float s = 0.0f ) { hue = h; brightness = b; saturation = s; }

			public float hue        { get; set; }

			public float brightness { get; set; }

			public float saturation { get; set; }

			public Properties byval { get { Properties p = new Properties( hue, brightness, saturation ); return p; } }
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static public int ToInt( Color color )
		{
			int result = 0;

			result |= ( ( int )( color.r * 255.0f ) & 0XFF ) <<  0;

			result |= ( ( int )( color.g * 255.0f ) & 0XFF ) <<  8;

			result |= ( ( int )( color.b * 255.0f ) & 0XFF ) << 16;

			result |= ( ( int )( color.a * 255.0f ) & 0XFF ) << 24;

			return result;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static public Color FromInt( int color )
		{
			float r = ( float )( ( color >>  0 ) & 0XFF ) / 255.0f;

			float g = ( float )( ( color >>  8 ) & 0XFF ) / 255.0f;

			float b = ( float )( ( color >> 16 ) & 0XFF ) / 255.0f;

			float a = ( float )( ( color >> 24 ) & 0XFF ) / 255.0f;

			return new Color( r, g, b, a );
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static public Properties FromColor( Color col )
		{
			float max = col.r; if( max < col.g ) max = col.g; if( max < col.b ) max = col.b;

			float min = col.r; if( min > col.g ) min = col.g; if( min > col.b ) min = col.b;

			float brg = max;

			float sat = max > 0.0f ? 1.0f - ( min / max ) : 0.0f;


			Color satCol;

			if( col.r == max ) satCol.r = 1.0f; else if( col.r == min ) satCol.r = 0.0f; else satCol.r = ( max > 0.0f ) ? col.r * ( 1.0f / max ) : 0.0f;

			if( col.g == max ) satCol.g = 1.0f; else if( col.g == min ) satCol.g = 0.0f; else satCol.g = ( max > 0.0f ) ? col.g * ( 1.0f / max ) : 0.0f;

			if( col.b == max ) satCol.b = 1.0f; else if( col.b == min ) satCol.b = 0.0f; else satCol.b = ( max > 0.0f ) ? col.b * ( 1.0f / max ) : 0.0f;


			float hue = 0.0f;

			bool domR = satCol.r > 0.0f;

			bool domG = satCol.g > 0.0f;

			bool domB = satCol.b > 0.0f;

			if     ( domR && domG ) hue = ( satCol.r >= 1.0f ) ? 0.0f  + ( satCol.g * 0.165f ) : 0.33f - ( satCol.r * 0.165f );

			else if( domG && domB ) hue = ( satCol.g >= 1.0f ) ? 0.33f + ( satCol.b * 0.165f ) : 0.66f - ( satCol.g * 0.165f );

			else if( domB && domR ) hue = ( satCol.b >= 1.0f ) ? 0.66f + ( satCol.r * 0.165f ) : 1.0f  - ( satCol.b * 0.165f );

			return new Properties ( hue, brg, sat );
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		static public  Color FromProperties( Properties p, ref Color hueColor )
		{
			Color col = new Color( 0.0f, 0.0f, 0.0f, 1.0f );

			if     ( p.hue <= 0.33f ) {  col.r = ( p.hue < 0.165f ) ? 1.0f : 1.0f - ( ( p.hue - 0.165f ) / 0.165f ); col.g = ( p.hue < 0.165f ) ? ( p.hue - 0.0f  ) / 0.165f : 1.0f; }

			else if( p.hue <= 0.66f ) {  col.g = ( p.hue < 0.495f ) ? 1.0f : 1.0f - ( ( p.hue - 0.495f ) / 0.165f ); col.b = ( p.hue < 0.495f ) ? ( p.hue - 0.33f ) / 0.165f : 1.0f; }

			else                      {  col.b = ( p.hue < 0.825f ) ? 1.0f : 1.0f - ( ( p.hue - 0.825f ) / 0.165f ); col.r = ( p.hue < 0.825f ) ? ( p.hue - 0.66f ) / 0.165f : 1.0f; }

			hueColor = col;


			float max = col.r; if( max < col.g ) max = col.g; if( max < col.b ) max = col.b;

			if( max != col.r ) col.r = max - ( p.saturation * ( max - col.r ) );

			if( max != col.g ) col.g = max - ( p.saturation * ( max - col.g ) );

			if( max != col.b ) col.b = max - ( p.saturation * ( max - col.b ) );


			col.r *= p.brightness; Mathf.Clamp( col.r, 0.0f, 1.0f );

			col.g *= p.brightness; Mathf.Clamp( col.g, 0.0f, 1.0f );

			col.b *= p.brightness; Mathf.Clamp( col.b, 0.0f, 1.0f );

			return col;
		}
	}
}
