using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class WebMapObjectBase : DragDropComponent
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	protected Localizable m_localizable        = null;

    protected Nameplate   m_nameplate          = null;

	protected Bounds      m_bounds             = default( Bounds ); 

    protected bool        m_visible            = true;

	protected float       m_selectionRadiusSqr = 0.0f;

	protected float       m_sqrDistFromCursor  = float.MaxValue;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Bounds bounds            { get { return m_bounds; } }

	public float  sqrDistFromCursor { get { return m_sqrDistFromCursor; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Localizable localizable
	{
		get { return m_localizable; }

		set
		{
			if( m_localizable != value )
			{
				Nameplates nameplates = ( ApplicationMain.instance != null ) && ( ApplicationMain.instance.ui != null ) ? ApplicationMain.instance.ui.nameplates : null;

				bool  deleteNameplate = ( nameplates != null ) && ( value == null ) && ( m_nameplate != null );

				bool  createNameplate = ( nameplates != null ) && ( value != null ) && ( m_nameplate == null );


				if( deleteNameplate       ) { nameplates.Destroy( m_nameplate ); m_nameplate = null; }

				if( m_localizable != null ) { m_localizable.nameplate = null; }

				m_localizable = value;

				if( createNameplate       ) { m_nameplate = nameplates.Create( gameObject, new Vector3( 0.0f, m_bounds.size.y, 0.0f ) );  }

				if( m_nameplate   != null ) { m_nameplate.m_anchor    = gameObject;  }

				if( m_localizable != null ) { m_localizable.nameplate = m_nameplate; }
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Start()
	{
		Collider collider = gameObject.GetComponent< Collider >();

		m_bounds = ( collider != null ) ? collider.bounds : default( Bounds );

		m_selectionRadiusSqr = Mathf.Pow( Mathf.Max( m_bounds.extents.x, m_bounds.extents.z ), 2 ) * 2.0f;

		if( m_nameplate != null )
		{
			m_nameplate.m_Offset = new Vector3( 0.0f, m_bounds.size.y, 0.0f );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnDestroy()
	{
		localizable = null;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateVisibilityStatus( UnityEngine.Plane[] planes )
	{
		m_bounds.center = transform.position;

		bool   visible  = GeometryUtility.TestPlanesAABB( planes, m_bounds );

		if( m_visible != visible )
		{
			m_visible  = visible;

			gameObject.SetActive( m_visible );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateSelectablesList( Vector3 cursorPos, List< WebMapObjectBase > selectables )
	{
		if( m_visible )
		{
			m_sqrDistFromCursor = ( transform.position - cursorPos ).sqrMagnitude;

			if( m_sqrDistFromCursor <= m_selectionRadiusSqr )
			{
				selectables.Add( this );
			}
		}
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class WebMapObject< ObjectT > : WebMapObjectBase where ObjectT : Localizable, new()
{
}