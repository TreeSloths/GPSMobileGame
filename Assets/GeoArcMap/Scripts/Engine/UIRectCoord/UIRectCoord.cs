using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class UICoord2D
{
	public UICoord2D() { pixels = Vector2.zero; coords = Vector2.zero; }

	public Vector2   pixels = Vector2.zero;

	public Vector2   coords = Vector2.zero;

	public UICoord2D byval { get { UICoord2D o = new UICoord2D(); o.pixels = pixels; o.coords = coords; return o; } }
}

//********************************************************************************************************
//
//********************************************************************************************************

public class UIRectCoord
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	private RectTransform m_transform = null;

	private Vector2       m_extents   = Vector2.one;

	private Vector2       m_margins   = Vector2.zero;

	private Vector2       m_borders   = Vector2.one;

	private bool          m_dirty     = true;

	private bool          m_active    = false;

	private UICoord2D     m_hovered   = new UICoord2D();

	private UICoord2D     m_pressed   = new UICoord2D();

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public UIRectCoord( RectTransform xform, Vector2 margs )
	{
		transform = xform;

		margins   = margs;

		UpdateContext();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void UpdateContext()
	{
		if( m_transform == null )
		{
			return;
		}

		if( m_dirty )
		{
			m_dirty     = false;

			m_extents   = new Vector2( m_transform.rect.width * 0.5f, m_transform.rect.height * 0.5f );

			m_borders   = m_extents - m_margins;

			if( m_borders.x < 0.0f ) m_borders.x = 0.0f;

			if( m_borders.y < 0.0f ) m_borders.y = 0.0f;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public RectTransform transform     { get { UpdateContext(); return m_transform; } set { m_transform = value; m_dirty = true; } }

	public Vector2       margins       { get { UpdateContext(); return m_margins;   } set { m_margins   = value; m_dirty = true; } }

	public Vector2       extents       { get { UpdateContext(); return m_extents;   } }

	public bool          active        { get { return m_active;  } }

	public UICoord2D     hovered       { get { return m_hovered; } }

	public UICoord2D     pressed       { get { return m_pressed; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Vector2 GetPosInLocalBase( Vector2 p ) { return p - m_extents; }

	public void    SetPixels        ( Vector2 p ) { m_hovered.pixels = m_pressed.pixels = p; m_hovered.coords = m_pressed.coords = new Vector2( p.x / ( m_extents.x * 2.0f ), p.y / ( m_extents.y * 2.0f ) ); }

	public void    SetCoords        ( Vector2 p ) { m_hovered.coords = m_pressed.coords = p; m_hovered.pixels = m_pressed.pixels = new Vector2( p.x * ( m_extents.x * 2.0f ), p.y * ( m_extents.y * 2.0f ) ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Update()
	{
		UpdateContext();

		Vector2 p;

		bool    valid = true;

		RectTransformUtility.ScreenPointToLocalPointInRectangle( m_transform, Input.mousePosition, null, out p );

		if( p.x < -m_borders.x ) { p.x = -m_borders.x; valid = false; } else if( p.x > m_borders.x ) { p.x = m_borders.x; valid = false; }

		if( p.y < -m_borders.y ) { p.y = -m_borders.y; valid = false; } else if( p.y > m_borders.y ) { p.y = m_borders.y; valid = false; }


		m_hovered.pixels   = p + m_extents;

		m_hovered.coords.x = m_hovered.pixels.x / m_transform.rect.width;  Mathf.Clamp( m_hovered.coords.x, 0.0f, 1.0f );

		m_hovered.coords.y = m_hovered.pixels.y / m_transform.rect.height; Mathf.Clamp( m_hovered.coords.y, 0.0f, 1.0f );



		if     ( Input.GetMouseButtonDown( 0 ) ) m_active = valid;

		else if( Input.GetMouseButtonUp  ( 0 ) ) m_active = false;


		if( Input.GetMouseButton( 0 ) )
		{
			if( m_active ) m_pressed = m_hovered.byval;
		}
	}
}
