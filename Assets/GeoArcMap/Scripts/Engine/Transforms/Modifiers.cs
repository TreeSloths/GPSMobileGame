using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

namespace CORE
{
	//********************************************************************************************************
	//
	//********************************************************************************************************

	public class XForm
	{
		//****************************************************************************************************
		//
		//****************************************************************************************************

		public enum SPACE { WORLD, LOCAL }

		//****************************************************************************************************
		//
		//****************************************************************************************************

		public Vector3    m_pos   = Vector3.zero;

		public Quaternion m_quat  = Quaternion.identity;

		public Vector3    m_scale = Vector3.one;

		//****************************************************************************************************
		//
		//****************************************************************************************************

		public Vector3    pos   { get { return m_pos;   } set { m_pos   = value; } }

		public Quaternion quat  { get { return m_quat;  } set { m_quat  = value; } }

		public Vector3    scale { get { return m_scale; } set { m_scale = value; } }

		//****************************************************************************************************
		//
		//****************************************************************************************************

		static public Vector3 LocalScaleForWorldScale( Vector3 worldScale, Transform transform )
		{
			Transform parent      = ( transform != null ) ? transform.parent  : null;

			Vector3   parentScale = ( parent    != null ) ? parent.lossyScale : Vector3.one;

			return new Vector3( parentScale.x != 0.0f ? worldScale.x / parentScale.x : worldScale.x, 

								parentScale.y != 0.0f ? worldScale.y / parentScale.y : worldScale.y, 

								parentScale.z != 0.0f ? worldScale.z / parentScale.z : worldScale.z );
		}

		//****************************************************************************************************
		//
		//****************************************************************************************************

		public void FromTransform( Transform transform, SPACE space )
		{
			if( transform != null )
			{
				if( space == SPACE.WORLD )
				{
					m_pos   = transform.position;

					m_quat  = transform.rotation;

					m_scale = transform.lossyScale;
				}
				else
				{
					m_pos   = transform.localPosition;

					m_quat  = transform.localRotation;

					m_scale = transform.localScale;
				}
			}
		}

		//****************************************************************************************************
		//
		//****************************************************************************************************

		public void ToTransform( Transform transform, SPACE space )
		{
			if( transform != null )
			{
				if( space == SPACE.WORLD )
				{
					transform.position   = m_pos;

					transform.rotation   = m_quat;

					transform.localScale = LocalScaleForWorldScale( m_scale, transform );
				}
				else
				{
					transform.localPosition = m_pos;

					transform.localRotation = m_quat;

					transform.localScale    = m_scale;
				}
			}
		}
	}

	//********************************************************************************************************
	//
	//********************************************************************************************************

	public class AdaptiveScale
	{
		//****************************************************************************************************
		//
		//****************************************************************************************************

		[ System.Flags ] public enum PRESERVE { NONE = 0X0, X = 0X1, Y = 0X2, Z = 0X4 }

		//****************************************************************************************************
		//
		//****************************************************************************************************

		private PRESERVE m_preserve   = PRESERVE.NONE;

		private Vector3  m_refScale   = Vector3.one;

		private Vector3  m_usrScale   = Vector3.one;

		private float    m_refDist    = 1.0f;

		private float    m_distFactor = 1.0f;

		//****************************************************************************************************
		//
		//****************************************************************************************************

		public PRESERVE preserve { get { return m_preserve; } set { m_preserve = value; } }

		public Vector3  refScale { get { return m_refScale; } set { m_refScale = value; } }

		public Vector3  usrScale { get { return m_usrScale; } set { m_usrScale = value; } }

		//****************************************************************************************************
		//
		//****************************************************************************************************

		public float refDist
		{
			get { return m_refDist; }

			set { m_refDist = value; m_distFactor = ( m_refDist > 0.0f ) ? ( 1.0f / m_refDist ) : 1.0f; }
		}

		//****************************************************************************************************
		//
		//****************************************************************************************************

		public Vector3 FromDist( float dist )
		{
			float s = dist * m_distFactor;

			float x = m_usrScale.x * ( ( ( m_preserve & PRESERVE.X ) != 0 ) ? m_refScale.x * s : m_refScale.x );

			float y = m_usrScale.y * ( ( ( m_preserve & PRESERVE.Y ) != 0 ) ? m_refScale.y * s : m_refScale.y );

			float z = m_usrScale.z * ( ( ( m_preserve & PRESERVE.Z ) != 0 ) ? m_refScale.z * s : m_refScale.z );

			return new Vector3( x, y, z );
		}
	}
}