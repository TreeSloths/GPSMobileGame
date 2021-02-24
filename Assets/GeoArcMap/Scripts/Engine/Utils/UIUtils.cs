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

	public static class UI
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		public enum PRESERVE_ASPECT { DEFAULT, W, H }

		//************************************************************************************************
		//
		//************************************************************************************************

		public static Vector2 Resize( float dim, Vector2 src, PRESERVE_ASPECT preserve )
		{
			if( src.x <= 0.0f )                       return Vector2.zero;

			if( src.y <= 0.0f )                       return Vector2.zero;

			if( preserve == PRESERVE_ASPECT.DEFAULT ) return Vector2.zero;


			float scale = ( preserve == PRESERVE_ASPECT.H ) ? dim   / src.x : dim   / src.y;

			float ratio = ( preserve == PRESERVE_ASPECT.H ) ? src.y / src.x : src.x / src.y;

			float x     = ( preserve == PRESERVE_ASPECT.H ) ? src.x * scale : src.y * scale * ratio;

			float y     = ( preserve == PRESERVE_ASPECT.W ) ? src.y * scale : src.x * scale * ratio;

			return new Vector2( x, y );
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public static Vector2 Constrain( Vector2 dst, Vector2 src, PRESERVE_ASPECT preserve = PRESERVE_ASPECT.DEFAULT )
		{
			if( preserve == PRESERVE_ASPECT.DEFAULT )
			{
				if( src.x >= src.y ) return Resize( dst.x, src, PRESERVE_ASPECT.H );

				else				 return Resize( dst.y, src, PRESERVE_ASPECT.W );
			}

			return Resize( dst.y, src, preserve );
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public static void Constrain( RectTransform dst, Texture src, PRESERVE_ASPECT preserve = PRESERVE_ASPECT.DEFAULT )
		{
			if( dst == null ) return;

			if( src == null ) return;

			Vector2  size  = CORE.UI.Constrain( new Vector2( dst.rect.width, dst.rect.height ), new Vector2( src.width, src.height ), preserve );

			Vector2  scale = new Vector2( size.x / dst.rect.width, size.y / dst.rect.height );

			dst.localScale = new Vector3( scale.x, scale.y, 1.0f );
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public static Texture ClampTexture( Texture texture, FilterMode mode )
		{
			texture.wrapMode   = TextureWrapMode.Clamp;

			texture.filterMode = mode;

			return texture;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class UIButton
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		static private Color DEFAULT_DISABLED_TXT_COLOR = new Color( 0.5f, 0.5f, 0.5f, 1.0f );

		//************************************************************************************************
		//
		//************************************************************************************************

		private Color m_enabledTextColor = Color.white;

		private Color m_disabledTxtColor = Color.white;

		public UnityEngine.UI.Button but { get; private set; }

		public UnityEngine.UI.Text   txt { get; private set; }

		//************************************************************************************************
		//
		//************************************************************************************************

		public UIButton( UnityEngine.UI.Button paramButton, Color paramDisabledTxtColor = default( Color ) )
		{
			but              = paramButton;

			txt              = ( but != null ) ? but.gameObject.GetComponentInChildren< UnityEngine.UI.Text >() : null;

			enabledTxtColor  = ( txt != null ) ? txt.color : Color.white;

			disabledTxtColor = ( paramDisabledTxtColor == default( Color ) ) ? DEFAULT_DISABLED_TXT_COLOR : paramDisabledTxtColor;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public void SetListener( UnityEngine.Events.UnityAction listener )
		{
			if( but != null )
			{
				but.onClick.RemoveAllListeners();

				but.onClick.AddListener( listener );
			}
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public bool interactable
		{
			get { return ( but != null ) ? but.interactable : false; }

			set
			{
				if( ( but != null ) && ( but.interactable != value ) )
				{
					but.interactable = value;

					if( txt != null ) txt.color = value ? enabledTxtColor : disabledTxtColor;
				}
			}
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public Color enabledTxtColor  { get { return m_enabledTextColor; } set { m_enabledTextColor = value; if( ( interactable == true  ) && ( txt != null ) ) { txt.color = value; } } }

		public Color disabledTxtColor { get { return m_disabledTxtColor; } set { m_disabledTxtColor = value; if( ( interactable == false ) && ( txt != null ) ) { txt.color = value; } } }
	}
}
