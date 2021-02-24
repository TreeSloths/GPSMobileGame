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

	public class Mode< ParentT > where ParentT : new()
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		protected ParentT m_parent = default( ParentT );

		public    ParentT o { get { return m_parent; } }

		//************************************************************************************************
		//
		//************************************************************************************************

		public void SetParent( ParentT parent ) { if( m_parent == null ) { m_parent = parent; } }

		virtual public void Init   () { }

		virtual public void Enter  () { }

		virtual public void Exit   () { }

		virtual public void Start  () { }

		virtual public void Update () { }

		virtual public void OnGUI  () { }

		virtual public void Destroy() { }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public class Modes< ParentT > where ParentT : new()
	{
		//************************************************************************************************
		//
		//************************************************************************************************

		protected ParentT m_parent = default( ParentT );

		public    ParentT o { get { return m_parent; } }

		//************************************************************************************************
		//
		//************************************************************************************************

		Mode< ParentT >[] m_modes    = new Mode< ParentT >[ 0 ];

		int			      m_size     = 0;

		int               m_previous = -1;

		int               m_active   = -1;

		int               m_started  = -1;

		//************************************************************************************************
		//
		//************************************************************************************************

		public Modes( ParentT parent = default( ParentT ) ) { m_parent = parent; }

		//************************************************************************************************
		//
		//************************************************************************************************

		public Mode< ParentT > this[ int mode ]
		{
			get
			{
				return ( mode < m_size ) ? m_modes[ mode ] : null;
			}
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public int PreviousMode
		{
			get { return m_previous; }
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public int ActiveMode
		{
			get { return m_active; }

			set { Select( value ); }
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public void Create< ModeT >() where ModeT : Mode< ParentT >, new() 
		{
			System.Array.Resize( ref m_modes, CORE.Alignement.Align( m_size + 1, 4 ) );

			m_modes[ m_size   ] = new ModeT();

			m_modes[ m_size   ].SetParent( m_parent );

			m_modes[ m_size++ ].Init();
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public void Destroy()
		{
			for( int mode = 0; mode < m_size; ++mode )
			{
				if( m_modes[ mode ] != null )
				{
					if( mode == m_active )
					{
						m_modes[ mode ].Exit();
					}

					m_modes[ mode ].Destroy();

					m_modes[ mode ] = null;
				}
			}

			System.Array.Clear( m_modes, 0, m_modes.Length );

			m_previous = m_active = -1;
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public void Select( int selection )
		{
			if( selection <  0 )        return;

			if( selection >= m_size   ) return;

			if( selection == m_active )	return;


			Mode< ParentT > actMode = ( m_active != -1 ) ? m_modes[ m_active ] : null;

			Mode< ParentT > reqMode = m_modes[ selection ];

			if( reqMode != null )
			{
				if( actMode != null )
				{
					actMode.Exit ();
				}

				reqMode.Enter();

				m_previous = ( m_active >= 0 ) ? m_active : selection;

				m_active   = selection;

				m_started  = -1;
			}
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public void Update()
		{
			if( m_active < 0 )       return;

			if( m_active >= m_size ) return;


			Mode< ParentT > active = m_modes[ m_active ];

			if( active != null )
			{
				if( m_started != m_active )
				{
					m_started  = m_active;

					active.Start();
				}

				active.Update();
			}
		}

		//************************************************************************************************
		//
		//************************************************************************************************

		public void OnGUI()
		{
			if( m_active < 0 )       return;

			if( m_active >= m_size ) return;


			Mode< ParentT > active = m_modes[ m_active ];

			if( active != null )
			{
				if( m_started != m_active )
				{
					m_started  = m_active;

					active.Start();
				}

				active.OnGUI();
			}
		}
	}
}