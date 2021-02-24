using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class MapCursor
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	private EarthRing  m_latRing = null;

	private EarthRing  m_lngRing = null;

	private GameObject m_spot    = null;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Setup( string parent, string spot, int nbSides, float thickness, string material )
	{
		GameObject parentObj = GameObject.Find( parent );

		GameObject spotObj   = CORE.HIERARCHY.Resolve( null, spot );

		if( m_latRing == null ) m_latRing = EarthRing.Create( parentObj, 0.0f, nbSides, thickness, material );

		if( m_lngRing == null ) m_lngRing = EarthRing.Create( parentObj, 0.0f, nbSides, thickness, material );

		if( m_spot    == null ) m_spot    = spotObj;


		m_latRing.transform.localScale = Vector3.one * 1.0025f;

		m_lngRing.transform.localScale = Vector3.one * 1.0025f;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Destroy()
	{
		m_latRing = null;

		m_lngRing = null;

		m_spot    = null;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Show( bool show )
	{
		if( m_latRing != null ) m_latRing.gameObject.SetActive( show );

		if( m_lngRing != null ) m_lngRing.gameObject.SetActive( show );

		if( m_spot    != null ) m_spot.SetActive( show );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Update( GPS.Coord latitude, GPS.Coord longitude, Vector3 pos )
	{
		if( m_latRing != null ) m_latRing.latitude  = latitude.deg;

		if( m_lngRing != null ) m_lngRing.longitude = longitude.deg;

		if( m_spot    != null )
		{
			m_spot.transform.position = pos;

			m_spot.transform.LookAt( pos * 2.0f );
		}
	}
}
