using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class Nameplates : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	private List< Nameplate > m_nameplates = new List< Nameplate >( 128 );

	private RectTransform     m_transform  = null;

	private Canvas            m_canvas     = null;

	private Font              m_font       = null;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Awake()
	{
		m_transform = GetComponent< RectTransform >();

		m_canvas    = GetComponent< Canvas        >();

		m_font      = Resources.GetBuiltinResource<Font>( "Arial.ttf" );

		if( m_canvas != null ) m_canvas.sortingOrder = 0;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Start()
	{
		if( m_canvas != null ) m_canvas.sortingOrder = -1;

		if( DBObjects.instance != null ) DBObjects.instance.listeners.Add( OnDBEvent );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnDestroy()
	{
		if( DBObjects.instance != null ) DBObjects.instance.listeners.Remove( OnDBEvent );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnDBEvent( DBObjects.EVT evt, params object[] paramsList )
	{
		if     ( evt == DBObjects.EVT.EVT_REFRESH_BEGIN ) { gameObject.SetActive( false ); }

		else if( evt == DBObjects.EVT.EVT_REFRESH_END   ) { gameObject.SetActive( true  ); }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnEnable()
	{
		if( m_canvas != null )
		{
			int order = m_canvas.sortingOrder;

			m_canvas.sortingOrder = 0;

			m_canvas.sortingOrder = order;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Nameplate Create( GameObject anchor, Vector3 Offset )
	{
		GameObject instance  = new GameObject( "Nameplate", new System.Type[] { typeof( Nameplate ), typeof( UnityEngine.UI.Text ), typeof( UnityEngine.UI.Shadow ) } );

		instance.transform.SetParent( transform, false );

		instance.transform.localPosition = Vector3.zero;

		instance.transform.localScale    = Vector3.one;

		instance.transform.localRotation = Quaternion.identity;


		Nameplate nameplate   = instance.GetComponent< Nameplate >();

		nameplate.m_transform = instance.GetComponent< RectTransform         >();

		nameplate.m_text      = instance.GetComponent< UnityEngine.UI.Text   >();

		nameplate.m_shadow    = instance.GetComponent< UnityEngine.UI.Shadow >();

		nameplate.m_anchor    = anchor;


		if( nameplate             != null ) { nameplate.m_Offset = Offset; }

		if( nameplate.m_transform != null ) { nameplate.m_transform.sizeDelta = new Vector2( 256.0f, 32.0f ); }

		if( nameplate.m_text      != null ) { nameplate.m_text.font = m_font; nameplate.m_text.alignment = TextAnchor.MiddleCenter; }

		if( nameplate.m_shadow    != null ) { nameplate.m_shadow.effectDistance = new Vector2( -1.0f, 1.0f ); nameplate.m_shadow.effectColor = new Color( 0.0f, 0.0f, 0.0f, 1.0f ); }


		m_nameplates.Add( nameplate );

		return nameplate;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Destroy( Nameplate n )
	{
		if( n != null )
		{
			m_nameplates.Remove( n );

			GameObject.Destroy ( n.gameObject );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateNameplate( Nameplate n )
	{
		n.gameObject.SetActive( n.m_anchor.activeInHierarchy );

		if( n.gameObject.activeInHierarchy )
		{
			Vector2 s = Vector2.zero;

			Vector2 p = RectTransformUtility.WorldToScreenPoint( Camera.main, n.m_anchor.transform.localPosition + n.m_Offset );

			RectTransformUtility.ScreenPointToLocalPointInRectangle( m_transform, p, null, out s );

			n.m_transform.localPosition = new Vector3( s.x, s.y, 100.0f );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Update()
	{
		for( int n = 0; n < m_nameplates.Count; ++n )
		{
			UpdateNameplate( m_nameplates[ n ] );
		}
	}
}