using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class EarthGizmos : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	private const int NB_RINGS      = 6;

	private const int NB_ANCHORS    = 12;

	private const int NB_LNG_LABELS = NB_ANCHORS;

	private const int NB_LAT_LABELS = NB_ANCHORS - 2;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private GameObject   m_earth     = null;

	private GameObject   m_longs     = null;

	private GameObject   m_lats      = null;

	private GameObject[] m_latLabels = new GameObject[ NB_LAT_LABELS ];

	private GameObject[] m_lngLabels = new GameObject[ NB_LNG_LABELS ];

	private EarthRing [] m_rings     = new EarthRing [ NB_RINGS ];

	private EarthRing    m_equator   = null;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ResolveDependencies()
	{
		m_earth = GameObject.Find( "Earth" );

		m_longs = CORE.HIERARCHY.Resolve( m_earth, "Canvas.Longs" );

		m_lats  = CORE.HIERARCHY.Resolve( m_earth, "Canvas.Lats"  );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void CreateRings()
	{
		float step = ( 180.0f - ( 180.0f / NB_RINGS ) ) / ( NB_RINGS - 1 );

		if( m_equator == null )
		{
			m_equator          = EarthRing.Create( m_earth, 0.0f, 58, 0.01f, "Materials/Equator" );

			m_equator.name     = "Equator";

			m_equator.enabled = false;

			m_equator.transform.localScale = Vector3.one * 1.003f;
		}


		for( int ring = 0; ring < NB_RINGS; ++ring )
		{
			if( m_rings[ ring ] == null )
			{
				m_rings[ ring ]           = EarthRing.Create( m_earth, m_equator, "Materials/Meridian" );

				m_rings[ ring ].name      = "Meridian";

				m_rings[ ring ].longitude = ring * step;

				m_rings[ ring ].enabled   = false;

				m_rings[ ring ].transform.localScale = Vector3.one * 1.003f;
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void SetupRingCoordLabels()
	{
		Vector3[] latAnchors = new Vector3[ NB_ANCHORS ];

		Vector3[] lngAnchors = new Vector3[ NB_ANCHORS ];

		float step = 360.0f / ( float )NB_LNG_LABELS;

		for( int anchor = 0; anchor < NB_ANCHORS; ++anchor )
		{
			float      angle     = ( float )anchor * step * -1.0f;

			latAnchors[ anchor ] = Quaternion.AngleAxis( angle, Vector3.right ) * new Vector3( 0.0f, 1.05f,  0.0f );

			lngAnchors[ anchor ] = Quaternion.AngleAxis( angle, Vector3.up    ) * new Vector3( 0.0f, 0.0f, -1.05f );
		}


		int sharedLat = ( 1 << ( NB_ANCHORS >> 2 ) - 1 ) | ( 1 << ( ( NB_ANCHORS >> 2 ) + ( NB_ANCHORS >> 1 ) - 2 ) );

		for( int label = 0, anchor = 0; label < NB_LAT_LABELS; ++label, ++anchor )
		{
			if( m_latLabels[ label ] == null )
			{
				m_latLabels[ label ] = CORE.HIERARCHY.Find( m_lats, string.Format( "Lat{0}" , label ) );

				float lat = Angle.Normalize( 90.0f - ( anchor * step ), Angle.UNIT.DEG, Angle.NORM.NEG, GPS.TYPE.LATITUDE );

				m_latLabels[ label ].GetComponent< UnityEngine.RectTransform >().position = latAnchors[ anchor ];

				m_latLabels[ label ].GetComponent< UnityEngine.UI.Text >().text = string.Format( "{0}°", lat );
			}

			if( ( sharedLat & ( 1 << label ) ) != 0 ) ++anchor;
		}


		int sharedLng = ( 1 ) | ( 1 << ( NB_ANCHORS >> 1 ) );

		for( int label = 0; label < NB_LNG_LABELS; ++label )
		{
			if( m_lngLabels[ label ] == null )
			{
				m_lngLabels[ label ] = CORE.HIERARCHY.Find( m_longs, string.Format( "Long{0}", label ) );

				m_lngLabels[ label ].GetComponent< UnityEngine.RectTransform >().position = lngAnchors[ label ];


				float lng = ( ( float )label * step ) - ( ( label <= 6 ) ? 0.0f : 360.0f );

				if( ( sharedLng & ( 1 << label ) ) == 0 ) m_lngLabels[ label ].GetComponent< UnityEngine.UI.Text >().text = string.Format( "{0}°",      lng );
				
				else                                      m_lngLabels[ label ].GetComponent< UnityEngine.UI.Text >().text = string.Format( "0°N,{0}°E", lng );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Start()
	{
		ResolveDependencies();

		CreateRings();

		SetupRingCoordLabels();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnDestroy()
	{
		for( int ring = 0; ring < NB_RINGS; ++ring ) m_rings[ ring ] = null;
	}
}
