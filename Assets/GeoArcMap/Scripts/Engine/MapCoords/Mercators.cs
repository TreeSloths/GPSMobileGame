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

	static public class ESPG
	{
		public const float semiMajorAxisLength = 6378137.0f;

		public const float shift               = Mathf.PI * semiMajorAxisLength;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public interface WebMapProj
	{
		float LatToY( float lat );

		float YToLat( float y   );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class WGS_84_WEB : WebMapProj
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		public float LatToY( float lat )
		{
			float    y = Mathf.Log( Mathf.Tan( ( ( 90.0f + lat ) * Mathf.PI ) / 360.0f ) ) * Mathf.Rad2Deg;

			return ( y * ESPG.shift ) / 180.0f;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public float YToLat( float y )
		{
			float  lat = ( y / ESPG.shift ) * 180.0f;

				   lat = Mathf.Rad2Deg * ( ( 2.0f * Mathf.Atan( Mathf.Exp( lat * Mathf.Deg2Rad ) ) ) - ( Mathf.PI * 0.5f ) );

			return lat;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class WebMapProvider
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		private string    m_name                  = null;

		private float     m_pixels                = 0;

		private float     m_meters                = 0.0f;

		private int       m_zoomMax               = 0;

		private float[]	  m_metersPerPixelForZoom = null;

		WebMapProj        m_proj                  = null;

		private string    m_copyRights            = string.Empty;

		//************************************************************************************************
		//
		//************************************************************************************************

		public WebMapProvider( string paramName, float paramPixels, float paramMeters, int paramZoomMax, WebMapProj paramProj, string paramCopyRights )
		{
			m_name       = string.IsNullOrEmpty( paramName ) ? "NOT_SPECIFIED" : paramName;

			m_pixels     = ( paramPixels  >  0    ) ? paramPixels     : 1;

			m_meters     = ( paramMeters  >  0    ) ? paramMeters     : 1;

			m_zoomMax    = ( paramZoomMax >  0    ) ? paramZoomMax    : 1;

			m_proj       = ( paramProj    != null ) ? paramProj       : new WGS_84_WEB();

			m_copyRights = ( m_copyRights != null ) ? paramCopyRights : string.Empty;


			m_metersPerPixelForZoom = new float[ m_zoomMax + 1 ];

			m_metersPerPixelForZoom[ 0 ] = m_meters;

			for( int zoom = 1; zoom <= m_zoomMax; ++zoom ) { m_metersPerPixelForZoom[ zoom ] = m_metersPerPixelForZoom[ zoom - 1 ] * 0.5f; }
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public string  name       { get { return m_name;                  } }

		public string  copyrights { get { return m_copyRights;            } }

		public float   zoomMax    { get { return m_zoomMax;               } }

		public float[] meters     { get { return m_metersPerPixelForZoom; } }

		//************************************************************************************************
		//
		//************************************************************************************************

		public float GetMetersPerPixel( int zoom )                              { return m_metersPerPixelForZoom[ zoom ];         }

		public float MetersToPixel    ( int zoom, float dist  )                 { return dist  / m_metersPerPixelForZoom[ zoom ]; }

		public float PixelToMeters    ( int zoom, float pixel )                 { return pixel * m_metersPerPixelForZoom[ zoom ]; }

		public float MetersToPixel    ( int zoom, float tileSize, float dist  ) { return MetersToPixel( zoom, dist   ) * ( tileSize / m_pixels ); }

		public float PixelToMeters    ( int zoom, float tileSize, float pixel ) { return PixelToMeters( zoom, pixel  ) / ( tileSize / m_pixels ); }

		//************************************************************************************************
		//
		//************************************************************************************************

		public float LatToY( int zoom, float res, float lat ) { return MetersToPixel( zoom, res, m_proj.LatToY( lat ) ); }

		public float YToLat( int zoom, float res, float y   ) { return m_proj.YToLat( PixelToMeters( zoom, res, y ) );   }
	}
}