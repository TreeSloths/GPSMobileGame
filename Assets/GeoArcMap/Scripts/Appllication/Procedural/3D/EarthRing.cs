using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

[ RequireComponent( typeof( MeshFilter   ) ) ] 

[ RequireComponent( typeof( MeshRenderer ) ) ]
	
public class EarthRing : MonoBehaviour 
{
    //****************************************************************************************************
    //
    //****************************************************************************************************

	[ System.Flags ] private enum CHANGES { NONE = 0X0, MESH = 0X1, TOPOGRAPHY = 0X2 }

    //****************************************************************************************************
    //
    //****************************************************************************************************

	private float	   m_curHeight    = float.MaxValue;

	private float	   m_reqHeight    = float.MaxValue;

	private int		   m_curNbSides   = 0;

	private int		   m_reqNbSides   = 0;

	private float	   m_curThickness = float.MaxValue;

	private float	   m_reqThickness = float.MaxValue;


	private MeshFilter m_filter  = null;

	private Mesh 	   m_mesh    = null;	

	private Vector3[]  m_ring    = new Vector3[ 0 ];

	private Vector3[]  m_tans    = new Vector3[ 0 ];

	private Vector3[]  m_verts   = new Vector3[ 0 ];
	
	private int[]      m_tris    = new int    [ 0 ];
	
	private Vector2[]  m_uv      = new Vector2[ 0 ];

	//****************************************************************************************************
	//
	//****************************************************************************************************
		
	private void OnDestroy() 
	{
		if( m_ring  != null ) System.Array.Clear( m_ring,  0, m_ring.Length  );

		if( m_tans  != null ) System.Array.Clear( m_tans,  0, m_tans.Length  );

		if( m_verts != null ) System.Array.Clear( m_verts, 0, m_verts.Length );

		if( m_tris  != null ) System.Array.Clear( m_tris,  0, m_tris.Length  );

		if( m_uv    != null ) System.Array.Clear( m_uv,    0, m_uv.Length    );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************
		
	public static EarthRing Create( GameObject parent, float height, int nbSides, float thickness, string material )
	{
		EarthRing ring = new GameObject( "Ring", typeof( EarthRing ) ).GetComponent< EarthRing >();

		if( ring != null ) 
		{
			ring.m_filter = ring.GetComponent< MeshFilter >(); 

			ring.m_mesh   = ring.m_filter != null ? ring.m_filter.mesh : null;

			ring.m_mesh.MarkDynamic();


			ring.material  = material;

			ring.latitude  = height;

			ring.nbSides   = nbSides;

			ring.thickness = thickness;


			if( parent != null )
			{
				ring.transform.parent = parent.transform;

				ring.transform.localPosition = new Vector3( 0.0f, 0.0f, 0.0f );

				ring.transform.localRotation = Quaternion.identity;

				ring.transform.localScale    = Vector3.one;
			}
		}
		
		return ring;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************
		
	public static EarthRing Create( GameObject parent, EarthRing model, string material )
	{
		EarthRing ring = new GameObject( "Ring", typeof( EarthRing ) ).GetComponent< EarthRing >();

		if( ( ring != null ) && ( model != null ) )
		{
			ring.m_filter = ring.GetComponent< MeshFilter >(); 

			ring.m_mesh   = ring.m_filter != null ? ring.m_filter.mesh : null;

			ring.m_mesh.MarkDynamic();


			model.Update();

			System.Array.Resize( ref ring.m_ring,  model.m_ring.Length	 );

			System.Array.Resize( ref ring.m_tans,  model.m_tans.Length	 );

			System.Array.Resize( ref ring.m_verts, model.m_verts.Length );

			System.Array.Resize( ref ring.m_tris,  model.m_tris.Length	 );

			System.Array.Resize( ref ring.m_uv,    model.m_uv.Length	 );
			
				
			System.Array.Copy( model.m_ring,  ring.m_ring, model.m_ring.Length   );

			System.Array.Copy( model.m_tans,  ring.m_tans, model.m_tans.Length   );

			System.Array.Copy( model.m_verts, ring.m_verts, model.m_verts.Length );
	
			System.Array.Copy( model.m_tris,  ring.m_tris, model.m_tris.Length   );
	
			System.Array.Copy( model.m_uv,    ring.m_uv, model.m_uv.Length       );

			ring.ApplyGeometry();

			ring.material = material;



			if( parent != null )
			{
				ring.transform.parent = parent.transform;

				ring.transform.localPosition = new Vector3( 0.0f, 0.0f, 0.0f );

				ring.transform.localRotation = Quaternion.identity;

				ring.transform.localScale    = Vector3.one;
			}
		}
		
		return ring;
	}

    //****************************************************************************************************
    //
    //****************************************************************************************************

	private CHANGES RebuilGeometry()
	{
		if( m_curNbSides != m_reqNbSides )
		{
			int   nbVerts = m_reqNbSides << 1;

			int   nbTris  = m_reqNbSides << 1;

			float step    = 360.0f / ( float )m_reqNbSides;


			System.Array.Resize( ref m_ring,  m_reqNbSides );

			System.Array.Resize( ref m_tans,  m_reqNbSides );

			System.Array.Resize( ref m_verts, nbVerts      );

			System.Array.Resize( ref m_tris,  nbTris * 3   );

			System.Array.Resize( ref m_uv,    nbVerts      );


			Vector3 lead = new Vector3( 0.0f, 0.0f, -1.0f );

			for( uint side = 0; side < m_reqNbSides; ++side )
			{
				m_ring[ side ] = Quaternion.AngleAxis( ( float )side * step * -1.0f, Vector3.up ) * lead;
			}

			for( int side = 0, poly = 0; side < m_reqNbSides; ++side )
			{
				int TL = side;

				int TR = ( side + 1 ) % m_reqNbSides;

				int BR = TR + m_reqNbSides;

				int BL = ( side + m_reqNbSides );


				m_tris[ poly++ ] = TL; 

				m_tris[ poly++ ] = TR;

				m_tris[ poly++ ] = BR;
				
				m_tris[ poly++ ] = BR;

				m_tris[ poly++ ] = BL;

				m_tris[ poly++ ] = TL;


				m_uv  [ TL ].x = ( ( side & 1 ) == 0 ) ? 0.0f : 1.0f;

				m_uv  [ TL ].y = ( ( side & 1 ) == 0 ) ? 0.0f : 0.0f;

				m_uv  [ BL ].x = ( ( side & 1 ) == 0 ) ? 0.0f : 1.0f;

				m_uv  [ BL ].y = ( ( side & 1 ) == 0 ) ? 1.0f : 1.0f;
			}


			m_curNbSides = m_reqNbSides;

			return CHANGES.MESH;
		}

		return CHANGES.NONE;
	}

    //****************************************************************************************************
    //
    //****************************************************************************************************

	private CHANGES MorphGeometry()
	{
		bool changeHeight    = Mathf.Abs( m_curHeight    - m_reqHeight    ) > 0.005f;

		bool changeThickness = Mathf.Abs( m_curThickness - m_reqThickness ) > 0.005f;

		if( changeHeight || changeThickness )
		{
			if( changeHeight )
			{
				float   step  = 360.0f / ( float )m_reqNbSides;

				float   angle = Mathf.Asin ( m_reqHeight );

				Vector3 tan   = new Vector3( 0.0f, Mathf.Cos( angle ), Mathf.Sin( angle ) );

				for( uint side = 0; side < m_reqNbSides; ++side ) { m_tans[ side ] = Quaternion.AngleAxis( ( float )side * step * -1.0f, Vector3.up ) * tan; }
			}


			float radius = Mathf.Cos( Mathf.Asin( m_reqHeight ) );

			for( uint side = 0; side < m_reqNbSides; ++side )
			{
				m_verts[ side ]                = ( m_ring [ side ] * radius ) + ( m_tans[ side ] * ( m_reqThickness * 0.5f ) ) + ( Vector3.up * m_reqHeight );

				m_verts[ side + m_reqNbSides ] = ( m_verts[ side ] ) - ( m_tans[ side ] * ( m_reqThickness ) );
			}


			m_curHeight    = m_reqHeight;

			m_curThickness = m_reqThickness;

			return CHANGES.TOPOGRAPHY;
		}

		return CHANGES.NONE;
	}

    //****************************************************************************************************
    //
    //****************************************************************************************************

    public void ApplyGeometry()
	{
		if( m_mesh == null ) return;
	
		m_mesh.Clear();
		
		m_mesh.vertices  = m_verts;
		
		m_mesh.triangles = m_tris;

		m_mesh.uv        = m_uv;

        m_mesh.RecalculateNormals();
		
		m_mesh.RecalculateBounds ();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public string material  { set { GetComponent< Renderer >().material = Resources.Load< Material >( value ); } }

	public float  thickness { set { m_reqThickness = CORE.Math.Clamp ( value ); } }

	public int    nbSides   { set { m_reqNbSides   = Mathf.Min( 512, Mathf.Max( 8, Alignement.Align( value, 4 ) ) ); } }

	public float  latitude  { set { m_reqHeight    = CORE.Math.Clamp ( Mathf.Sin( value * Mathf.Deg2Rad ), -1.0f, 1.0f ); } }

	public float  longitude { set { transform.localRotation = Quaternion.AngleAxis( -value, Vector3.up ) * Quaternion.AngleAxis( 90.0f, Vector3.forward ); } }

	//****************************************************************************************************
	//
	//****************************************************************************************************
		
	private void Update() 
	{
		CHANGES changes  = RebuilGeometry();

		        changes |= MorphGeometry ();

		if( changes != 0 )
		{
			ApplyGeometry();
		}
	}
}
