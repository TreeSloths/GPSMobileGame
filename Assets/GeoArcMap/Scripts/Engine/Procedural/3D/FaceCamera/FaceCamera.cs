using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class FaceCamera : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	private AdaptiveScale m_scale = new AdaptiveScale();

	//****************************************************************************************************
	//
	//****************************************************************************************************

	[ SerializeField ] public Camera m_camera              = null;

	[ SerializeField ] public bool   m_invert              = false;

	[ SerializeField ] public bool   m_cstX                = false;

	[ SerializeField ] public bool   m_cstY                = false;

	[ SerializeField ] public bool   m_cstZ                = false;

	[ SerializeField ] public bool   m_positiveRefDistOnly = true;

	[ SerializeField ] public float  m_refScale            = 1.0f;

	[ SerializeField ] public float  m_refDist             = 1.0f;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Start()
	{
		m_scale.preserve = ( m_cstX ? AdaptiveScale.PRESERVE.X : 0 ) | ( m_cstY ? AdaptiveScale.PRESERVE.Y : 0 ) | ( m_cstZ ? AdaptiveScale.PRESERVE.Z : 0 );

		m_scale.refScale = new Vector3( m_refScale, m_refScale, 1.0f );

		m_scale.refDist  = m_refDist;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void LateUpdate()
	{
		Camera cam = ( m_camera != null ) ? m_camera : Camera.main;

		if( cam != null )
		{
			if( m_scale.preserve != 0 )
			{
				Vector3 scale = m_scale.FromDist( Mathf.Abs( Vector3.Dot( transform.position - cam.transform.position, cam.transform.forward ) ) );

				if( m_positiveRefDistOnly )
				{
					if( scale.x < m_scale.refScale.x ) scale.x = m_scale.refScale.x;

					if( scale.y < m_scale.refScale.y ) scale.y = m_scale.refScale.y;

					if( scale.z < m_scale.refScale.z ) scale.z = m_scale.refScale.z;
				}

				transform.localScale = scale;
			}

			if( m_invert == false ) transform.LookAt( transform.position - cam.transform.forward, cam.transform.up );

			else                    transform.LookAt( transform.position + cam.transform.forward, cam.transform.up );
		}
	}
}
