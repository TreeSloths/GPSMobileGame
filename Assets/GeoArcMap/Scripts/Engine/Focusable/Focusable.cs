using UnityEngine;
using System.Collections;

//********************************************************************************************************
//
//********************************************************************************************************

public class Focusable : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private int m_nbFocused = 0;

	static private int m_nbPopups  = 0;

	static public bool active { get { return ( m_nbFocused > 0 ); } }

	static public bool popup  { get { return ( m_nbPopups  > 0 ); } }

	static public bool any    { get { return ( m_nbFocused > 0 ) || ( m_nbPopups  > 0 ); } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	[ SerializeField ] public bool           m_isPopup = false;

	                   private RectTransform m_xForm   = null;

					   private bool          m_hovered = false;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void Awake()
	{
		m_xForm = GetComponent< RectTransform >();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnEnable()
	{
		if( m_isPopup ) ++m_nbPopups;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnDisable()
	{
		if( m_isPopup ) { if( m_nbPopups > 0 ) --m_nbPopups; }

		if( m_hovered ) { OnMouseExit();  m_hovered = false; }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnMouseEnter() { ++m_nbFocused; }

	private void OnMouseExit () { if( m_nbFocused > 0 ) --m_nbFocused; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void Update()
	{
		Vector2 p = Vector2.zero;

		RectTransformUtility.ScreenPointToLocalPointInRectangle( m_xForm, Input.mousePosition, null, out p );

		bool hovered = ( ( p.x >= m_xForm.rect.xMin ) && ( p.x <= m_xForm.rect.xMax ) ) && ( ( p.y >= m_xForm.rect.yMin ) && ( p.y <= m_xForm.rect.yMax ) );

		if( m_hovered != hovered )
		{
			m_hovered  = hovered;

			if( m_hovered ) OnMouseEnter();

			else            OnMouseExit ();
		}
	}
}
