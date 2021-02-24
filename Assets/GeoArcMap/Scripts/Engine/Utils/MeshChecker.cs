using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//********************************************************************************************************
//
//********************************************************************************************************

#if UNITY_EDITOR

    using UnityEditor;

#endif

//********************************************************************************************************
//
//********************************************************************************************************

[ System.Serializable ] public class MeshChecker : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

    private static GameObject[] EditorSelection()
    {
        #if UNITY_EDITOR

            return Selection.gameObjects;

        #else

            return null;

        #endif
    }

    //****************************************************************************************************
    //
    //****************************************************************************************************

    public static GameObject selection
    {
        get
        {
            GameObject[] selecteds  = EditorSelection();

            int          nbSelected = selecteds != null ? selecteds.Length : 0;

            return nbSelected > 0 ? selecteds[ 0 ] : null;
        }
    }

    //****************************************************************************************************
    //
    //****************************************************************************************************

    public static void AddVertices( Mesh mesh, Matrix4x4 matrix )
    {
        if( mesh == null )              return;

        if( mesh.vertices == null )     return;

        if( mesh.vertices.Length <= 0 ) return;


        Vector3[] worldVertices = new Vector3[ mesh.vertices.Length ];

        for( int v = 0; v < mesh.vertices.Length; ++v ) worldVertices[ v ] = matrix.MultiplyPoint( mesh.vertices[ v ] );


        int prvSize = m_vertices.Length;

        System.Array.Resize< Vector3 >( ref m_vertices, prvSize + mesh.vertices.Length );

        System.Array.Copy( worldVertices, 0, m_vertices, prvSize, worldVertices.Length );
    }

    //****************************************************************************************************
    //
    //****************************************************************************************************

    public static void CreateDebugMesh( GameObject obj )
    {
        System.Array.Resize< Vector3 >( ref m_vertices, 0 );

        if( obj == null ) return;

        MeshFilter[]          meshes        = obj.GetComponentsInChildren< MeshFilter >();

        SkinnedMeshRenderer[] skinnedMeshes = obj.GetComponentsInChildren< SkinnedMeshRenderer >();

        foreach( MeshFilter meshFilter in meshes )                  AddVertices( meshFilter.sharedMesh,  meshFilter.gameObject.transform.localToWorldMatrix  );

        foreach( SkinnedMeshRenderer skinnedMesh in skinnedMeshes ) AddVertices( skinnedMesh.sharedMesh, skinnedMesh.gameObject.transform.localToWorldMatrix );
    }

    //****************************************************************************************************
    //
    //****************************************************************************************************

    #if UNITY_EDITOR

    [ MenuItem( "Tools/Display Mesh Vertices" ) ]

    #endif

    public static void CheckSelection()
    {
        CreateInstance();

        CreateDebugMesh( selection );
    }

    //****************************************************************************************************
    //
    //****************************************************************************************************

    public static MeshChecker CreateInstance()
    {
        if( m_instance == null ) m_instance = new GameObject( "MeshChecker", typeof( MeshChecker ) ).GetComponent< MeshChecker >();

        return m_instance;
    }

    //****************************************************************************************************
    //
    //****************************************************************************************************

    [ NonSerialized ] private static MeshChecker m_instance = null;

    [ NonSerialized ] private static Vector3[]   m_vertices = new Vector3[ 0 ];

    //****************************************************************************************************
    //
    //****************************************************************************************************

    private MeshChecker()    { }

    private void Awake()     { }

    private void OnDestroy() { if( m_instance == this ) m_instance = null; }

    private void Start()     { }

    //****************************************************************************************************
    //
    //****************************************************************************************************

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;

        Vector3 size = Vector3.one * 0.0025f;

        int   nbVerts = m_vertices.Length;

        for( int v = 0; v < nbVerts; ++v ) Gizmos.DrawCube( m_vertices[ v ], size );
	}
}
