using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

//********************************************************************************************************
//
//********************************************************************************************************

public static class BoundsUtils
{
	//****************************************************************************************************
	//
	//****************************************************************************************************
	
	public static void Extend( ref Bounds bounds, Vector3 p )
	{
		if( bounds == default( Bounds ) ) { bounds.center = p; return; }
		
		bounds.min = new Vector3( Mathf.Min( bounds.min.x, p.x ), Mathf.Min( bounds.min.y, p.y ), Mathf.Min( bounds.min.z, p.z ) );
		
		bounds.max = new Vector3( Mathf.Max( bounds.max.x, p.x ), Mathf.Max( bounds.max.y, p.y ), Mathf.Max( bounds.max.z, p.z ) );
	}
	
	//****************************************************************************************************
    //
    //****************************************************************************************************

    public static void Extend( ref Bounds bounds, Bounds other )
    {
		if( other  == default( Bounds ) ) {                 return; }
		
		if( bounds == default( Bounds ) ) { bounds = other; return; }
    
        Vector3 min = new Vector3( Mathf.Min( bounds.min.x, other.min.x ), Mathf.Min( bounds.min.y, other.min.y ), Mathf.Min( bounds.min.z, other.min.z ) );

        Vector3 max = new Vector3( Mathf.Max( bounds.max.x, other.max.x ), Mathf.Max( bounds.max.y, other.max.y ), Mathf.Max( bounds.max.z, other.max.z ) );

        bounds.center = ( min + max ) * 0.5f;

        bounds.size   = ( max - min );
    }

    //****************************************************************************************************
    //
    //****************************************************************************************************

	public static void Extend( ref Bounds bounds, Mesh mesh, Matrix4x4 matrix )
    {
		if( mesh == null ) return;
				
		int    nbVerts     = mesh.vertices != null ? mesh.vertices.Length : 0;
		
		Bounds worldBounds = default( Bounds );
		
		for( int v = 0; v < nbVerts; ++v ) BoundsUtils.Extend( ref worldBounds, matrix.MultiplyPoint( mesh.vertices[ v ] ) );
    
		Extend( ref bounds, worldBounds );
    }

    //****************************************************************************************************
    //
    //****************************************************************************************************

    public static void Extend( ref Bounds bounds, Collider[] colliders )
    {
        if( colliders == null )     return;

        if( colliders.Length <= 0 ) return;

        for( int collider = 0; collider < colliders.Length; ++collider ) Extend( ref bounds, colliders[ collider ].bounds );
    }

    //****************************************************************************************************
    //
    //****************************************************************************************************

    public static Bounds Get( GameObject obj )
    {
        Bounds result = default( Bounds );

		if( obj != null )
		{
			MeshFilter          meshFilter  = obj.GetComponent< MeshFilter >();

			SkinnedMeshRenderer skinnedMesh = obj.GetComponent< SkinnedMeshRenderer >();

			if( meshFilter  != null ) Extend( ref result, meshFilter.sharedMesh,  meshFilter.transform.localToWorldMatrix  );
		
			if( skinnedMesh != null ) Extend( ref result, skinnedMesh.sharedMesh, skinnedMesh.transform.localToWorldMatrix );
		}

        return result;
    }
}

