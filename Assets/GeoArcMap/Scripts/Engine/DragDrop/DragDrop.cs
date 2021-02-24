using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public interface IDragDrop
{
	bool BeginDrag   ( IDragDrop obj );

	void UpdateDrag  ( IDragDrop obj );

	void CancelDrag  ( IDragDrop obj );

	bool AcceptDrop  ( IDragDrop obj );
}

//********************************************************************************************************
//
//********************************************************************************************************

static public class DragDropOperation
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private IDragDrop m_cur     = null;

	static private IDragDrop m_handler = null;

	static public bool      pending { get { return m_cur != null; } }

	static public IDragDrop obj     { get { return m_cur;         } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public bool Begin( IDragDrop handler, IDragDrop obj )
	{
		if( m_cur == null )
		{
			if( ( handler != null ) && ( obj != null ) && ( handler.BeginDrag( obj ) ) )
			{
				m_cur     = obj;

				m_handler = handler;

				return true;
			}
		}

		return false;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public void Cancel()
	{
		if( ( m_handler != null ) && ( m_cur != null ) )
		{
			m_handler.CancelDrag( m_cur );

			m_handler = null;

			m_cur     = null;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public bool Drop( IDragDrop container )
	{
		if( ( m_handler != null ) && ( m_cur != null ) && ( container != null ) && ( m_cur != container ) )
		{
			if( container.AcceptDrop( m_cur ) )
			{
				m_handler = null;

				m_cur     = null;

				return true;
			}

			Cancel();
		}

		return false;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static public void Update()
	{
		if( ( m_handler != null ) && ( m_cur != null ) )
		{
			m_handler.UpdateDrag( m_cur );
		}
	}
}


