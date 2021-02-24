using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class POPUPColor : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum BUTON { CANCEL, OK }

	public delegate void OnButon( POPUPColor popup, BUTON buton );

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private const float FADE_IN_DURATION = 0.3f;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private POPUPColor m_instance = null;

	static public  POPUPColor Instance { get { return m_instance; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private CanvasGroup             m_group      = null;

	private UnityEngine.UI.RawImage m_tint       = null;

	private UnityEngine.UI.RawImage m_hue        = null;

	private UnityEngine.UI.RawImage m_preview    = null;

	private UnityEngine.UI.RawImage m_tintCursor = null;

	private UnityEngine.UI.RawImage m_hueCursor  = null;

	private UnityEngine.UI.Button   m_CANCEL     = null;

	private UnityEngine.UI.Button   m_OK         = null;

	private UIRectCoord				m_tintPicker = null;

	private UIRectCoord             m_huePicker  = null;

	private CORE.Fade				m_fade       = new CORE.Fade();

	private Color					m_color      = new Color( 0.0f, 0.0f, 0.0f, 1.0f );

	private OnButon                 m_onButon    = null;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Color color { get { return m_color; } set { m_color = value; if( m_preview != null ) m_preview.material.color = value; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Awake()
	{
		if( m_instance == null ) m_instance = this;

		m_group         = GetComponent< CanvasGroup >();
		
		GameObject root = GameObject.Find( "POPUPColor" );

		GameObject buts = CORE.HIERARCHY.Find( root, "Butons" );

		m_tint          = CORE.HIERARCHY.FindComp< UnityEngine.UI.RawImage >( root, "Hue"          );

		m_hue           = CORE.HIERARCHY.FindComp< UnityEngine.UI.RawImage >( root, "HueSelection" );

		m_tintCursor    = CORE.HIERARCHY.FindComp< UnityEngine.UI.RawImage >( root, "TintCursor"   );

		m_hueCursor     = CORE.HIERARCHY.FindComp< UnityEngine.UI.RawImage >( root, "HueCursor"    );

		m_preview       = CORE.HIERARCHY.FindComp< UnityEngine.UI.RawImage >( root, "Preview"      );

		m_CANCEL        = CORE.HIERARCHY.FindComp< UnityEngine.UI.Button   >( buts, "CANCEL"       );

		m_OK            = CORE.HIERARCHY.FindComp< UnityEngine.UI.Button   >( buts, "OK"           );


		m_tintPicker = new UIRectCoord( m_tint.rectTransform, Vector2.one * 4.0f        );

		m_huePicker  = new UIRectCoord( m_hue.rectTransform,  new Vector2( 0.0f, 2.0f ) );


		if( m_CANCEL != null ) { m_CANCEL.onClick.RemoveAllListeners(); m_CANCEL.onClick.AddListener( delegate { OnCANCEL(); } ); }

		if( m_OK     != null ) { m_OK.onClick.RemoveAllListeners();     m_OK.onClick.AddListener    ( delegate { OnOK();     } ); }


		m_fade.Begin( FADE_TYPE.FADE_OUT, 0.0f );

		gameObject.SetActive( false );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void OnDestroy() { if( m_instance == this ) m_instance = null; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool Show( OnButon onButon, Color color )
	{
		if( gameObject.activeSelf == false )
		{
			m_onButon = onButon;

			DecomposeColor( color );


			m_fade.Begin( FADE_TYPE.FADE_IN, FADE_IN_DURATION, null );

			Update();

			gameObject.SetActive( true );

			return true;
		}

		return false;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnEnable () {}

	private void OnDisable() { m_fade.Begin( FADE_TYPE.FADE_OUT, 0.0f, null ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public  void OnCANCEL()  { if( m_onButon != null ) { m_onButon( this, BUTON.CANCEL ); m_onButon = null; } gameObject.SetActive( false ); }

	public  void OnOK()      { if( m_onButon != null ) { m_onButon( this, BUTON.OK     ); m_onButon = null; } gameObject.SetActive( false ); }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void DecomposeColor( Color col )
	{
		CORE.COLOR.Properties props = CORE.COLOR.FromColor( col );


		if( ( m_tintPicker == null ) || ( m_tintCursor == null ) ) return;

		if( ( m_huePicker  == null ) || ( m_hueCursor  == null ) ) return;

		m_tintPicker.SetCoords( new Vector2( props.saturation, props.brightness ) );

		m_huePicker.SetCoords ( new Vector2( 0.0f, props.hue ) );

		color = col;


		Vector2 tintCursorPos = m_tintPicker.GetPosInLocalBase( m_tintPicker.pressed.pixels );

		Vector2 hueCursorPos  = m_huePicker.GetPosInLocalBase ( m_huePicker.pressed.pixels  );

		m_tintCursor.rectTransform.localPosition = new Vector3( tintCursorPos.x, tintCursorPos.y, 0.0f );

		m_hueCursor.rectTransform.localPosition  = new Vector3( 0.0f,            hueCursorPos.y,  0.0f );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void Update()
	{
		m_fade.Update();

		if( m_group != null ) m_group.alpha = ( 1.0f - m_fade.alpha );

		if( ( m_tintPicker == null ) || ( m_tintCursor == null  ) ) return;

		if( ( m_huePicker  == null ) || ( m_hueCursor  == null  ) ) return;

		if( ( m_group      == null ) || ( m_group.alpha >= 1.0f ) )
		{
			m_tintPicker.Update();

			m_huePicker.Update ();
		}



		float brightness = m_tintPicker.pressed.coords.y;

		float saturation = m_tintPicker.pressed.coords.x;

		float hue        = m_huePicker.pressed.coords.y;

		Color hueColor   = new Color();

		color            = CORE.COLOR.FromProperties( new CORE.COLOR.Properties( hue, brightness, saturation ), ref hueColor );

		if( m_tint != null )
		{
			m_tint.color = hueColor;
		}


		Vector2 tintCursorPos = m_tintPicker.GetPosInLocalBase( m_tintPicker.pressed.pixels );

		Vector2 hueCursorPos  = m_huePicker.GetPosInLocalBase ( m_huePicker.pressed.pixels  );

		m_tintCursor.rectTransform.localPosition = new Vector3( tintCursorPos.x, tintCursorPos.y, 0.0f );

		m_hueCursor.rectTransform.localPosition  = new Vector3( 0.0f,            hueCursorPos.y,  0.0f );
	}
}
