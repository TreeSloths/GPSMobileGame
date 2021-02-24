using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

static public class QuadUV
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private Vector3   m_c = new Vector3( 0.5f, 0.0f, 0.5f );

	static private Vector3   m_u = new Vector3( 0.0f, 0.0f, 0.5f );

	static private Vector3   m_r = new Vector3( 0.5f, 0.0f, 0.0f );

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private Vector2   m_UV00    = new Vector2( 0.0f, 0.0f );

	static private Vector2   m_UV11    = new Vector2( 1.0f, 1.0f );

	static private Vector2   m_UV10    = new Vector2( 1.0f, 0.0f );

	static private Vector2   m_UV01    = new Vector2( 0.0f, 1.0f );

	static private Vector2[] m_DEFAULT = new Vector2[ 4 ] { m_UV00, m_UV11, m_UV10, m_UV01 };

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public  Vector2   UV00          { get { return m_UV00;    } }

	static public  Vector2   UV11          { get { return m_UV11;    } }

	static public  Vector2   UV10          { get { return m_UV10;    } }

	static public  Vector2   UV01          { get { return m_UV01;    } }

	static public  Vector2[] DEFAULT_BYREF { get { return m_DEFAULT; } }

	static public  Vector2[] DEFAULT_BYVAL { get { Vector2[] uv = new Vector2[ 4 ]; m_DEFAULT.CopyTo( uv, 0 ); return uv; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public Vector2[] Rotate( Vector2[] uv, float deg )
	{
		if( ( deg != 0.0f ) && ( deg != 360.0f ) )
		{
			Quaternion q = Quaternion.AngleAxis( deg, Vector3.up );

			Vector3    u = q * m_u;

			Vector3    r = q * m_r;

			uv[ 0 ].x = m_c.x - r.x - u.x; uv[ 0 ].y = m_c.z - r.z - u.z;

			uv[ 1 ].x = m_c.x + r.x + u.x; uv[ 1 ].y = m_c.z + r.z + u.z;

			uv[ 2 ].x = m_c.x + r.x - u.x; uv[ 2 ].y = m_c.z + r.z - u.z;

			uv[ 3 ].x = m_c.x - r.x + u.x; uv[ 3 ].y = m_c.z - r.z + u.z;
		}

		return uv;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public Vector2[] Scale( Vector2[] uv, float scale )
	{
		Vector3 u = m_u * scale;

		Vector3 r = m_r * scale;

		uv[ 0 ].x = m_c.x - r.x - u.x; uv[ 0 ].y = m_c.z - r.z - u.z;

		uv[ 1 ].x = m_c.x + r.x + u.x; uv[ 1 ].y = m_c.z + r.z + u.z;

		uv[ 2 ].x = m_c.x + r.x - u.x; uv[ 2 ].y = m_c.z + r.z - u.z;

		uv[ 3 ].x = m_c.x - r.x + u.x; uv[ 3 ].y = m_c.z - r.z + u.z;

		return uv;
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class LoadingIcon
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	private Texture   m_texture = null;

	private float     m_angle   = 0.0f;

	private Vector2[] m_uv      = null;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Texture texture { get { return m_texture; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Setup( string resource )
	{
		m_texture = Resources.Load< Texture >( "2D/MapTile/TileLoading" );

		m_uv      = QuadUV.DEFAULT_BYVAL;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Rotate()
	{
		QuadUV.Rotate( m_uv, m_angle -= 360.0f * Time.deltaTime );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ApplyToMesh( MeshFilter meshFilter, bool apply )
	{
		Mesh mesh = ( meshFilter != null ) ? meshFilter.mesh : null;

		if ( mesh != null )
		{
			 mesh.uv = apply ? m_uv : QuadUV.DEFAULT_BYVAL;
		}
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

static public class TextureID_0
{
	static private int m_id = -1;

	static public  int id { get { return ( m_id != -1 ) ? m_id : m_id = Shader.PropertyToID( "_MainTex" ); } }
}

//********************************************************************************************************
//
//********************************************************************************************************

public class WebMapTile : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public object           m_pool            = null;

	public  WebMapTileSlot	m_slot            = null;

	private WebMapParams	m_location        = new WebMapParams ();

	private WebMapRequest   m_loadRequest     = new WebMapRequest();

	private Texture         m_texture         = null;

	private MeshFilter      m_meshFilter      = null;

	private MeshRenderer    m_meshRendrerer   = null;

	private LoadingIcon     m_loadIcon        = null;

	private GameObject      m_loadingFrame    = null;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public object			pool     { get { return m_pool;                  } set { m_pool = value;             } }

	public WebMapTileSlot	slot     { get { return m_slot;                  } set { m_slot = value;             } }

	public Vector3          position { get { return transform.position;      } set { transform.position = value; } }

	public WebMapParams		location { get { return m_location;              } }

	public Texture			texture  { get { return m_texture;               } set { m_texture = value; if( m_meshRendrerer != null ) m_meshRendrerer.material.mainTexture = value; } }

	public float			width    { get { return transform.localScale.x;  } set { transform.localScale = new Vector3( value, transform.localScale.y, transform.localScale.z );   } }

	public float			height   { get { return transform.localScale.y;  } set { transform.localScale = new Vector3( transform.localScale.x, value, transform.localScale.z );   } }

	public Vector2[]		tiling   { get { return GetTiling();             } set { SetTiling( value ); } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ShowLoadingFrame( bool show )
	{
		if( m_loadingFrame != null ) m_loadingFrame.SetActive( show );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void BreakUVConnection( Mesh mesh )
	{
		if( mesh != null )
		{
			mesh.uv  = QuadUV.DEFAULT_BYVAL;

			mesh.uv2 = QuadUV.DEFAULT_BYVAL;

			mesh.uv3 = QuadUV.DEFAULT_BYVAL;

			mesh.uv4 = QuadUV.DEFAULT_BYVAL;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void SetTiling( Vector2[] Tiling )
	{
		if( m_meshRendrerer != null )
		{
			m_meshRendrerer.material.mainTextureOffset = Tiling[ 0 ];

			m_meshRendrerer.material.mainTextureScale  = Tiling[ 1 ];
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public Vector2[] GetTiling()
	{
		Vector2[] Tiling = new Vector2[ 2 ] { Vector2.zero, Vector2.one };

		if( m_meshRendrerer != null )
		{
			Tiling[ 0 ] = m_meshRendrerer.material.mainTextureOffset;

			Tiling[ 1 ] = m_meshRendrerer.material.mainTextureScale;
		}

		return Tiling;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Setup( string name, float size )
	{
	    gameObject.name	 = name;

		m_meshFilter     = GetComponent< MeshFilter   >();

		m_meshRendrerer  = GetComponent< MeshRenderer >();

		if( m_meshFilter != null )
		{
			BreakUVConnection( m_meshFilter.mesh );
		}


		m_location.m_size    = ( int )size;

		transform.localScale = new Vector3( size, size, 1.0f );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void SubmitLoadingRequest( WebMapLoader loader, LoadingIcon loadIcon )
	{
		if( loader != null )
		{
			m_loadRequest.@params = m_location;

			if( loader.Queue( m_loadRequest ) )
			{
				m_loadIcon = loadIcon;

				texture    = loadIcon.texture;

				ShowLoadingFrame( true );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void CancelLoadingRequest()
	{
		m_loadRequest.Cancel(); 
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void FlushTexture()
	{
		m_loadRequest.Invalidate();

		texture = null;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Awake()
	{
		m_loadingFrame = CORE.HIERARCHY.Find( gameObject, "LoadingFrame" );

		ShowLoadingFrame( false );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Update()
	{
		if( ( m_loadRequest.completed ) && ( texture != m_loadRequest.texture ) )
		{
			texture = m_loadRequest.texture;

			if( m_loadIcon != null )
			{
				m_loadIcon.ApplyToMesh( m_meshFilter, false );

				m_loadIcon = null;

				ShowLoadingFrame( false );
			}
		}

		if( m_loadIcon != null )
		{
			m_loadIcon.ApplyToMesh( m_meshFilter, true );
		}
	}
}
