using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public interface IFocusable
{
	void OnGetFocus ();

	void OnFocusLost();
}

//********************************************************************************************************
//
//********************************************************************************************************

static public class Focus
{
	static private IFocusable m_focused = null;

	static public  IFocusable obj
	{
		get { return m_focused; }

		set
		{
			if( m_focused != value )
			{
				if( m_focused != null ) m_focused.OnFocusLost();

				m_focused = value;

				if( m_focused != null ) m_focused.OnGetFocus();
			}
		}
	}
}