using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

//********************************************************************************************************
//
//********************************************************************************************************

public class Instances< T > where T : class
{
	//****************************************************************************************************
	//
	//****************************************************************************************************
	
	public delegate void OnRegister  ( T instance );
	
	public delegate void OnUnregister( T instance );
	
	//****************************************************************************************************
	//
	//****************************************************************************************************
	
	private OnRegister   m_onRegister   = null;
	
	private OnUnregister m_onUnregister = null;
	
	private List< T >    m_list         = new List< T >();
	
	//****************************************************************************************************
	//
	//****************************************************************************************************
	
	public void Register  ( T instance ) 
	{ 
		if( ( instance != null ) && ( m_list.Contains( instance ) == false ) ) 
		{
			m_list.Add( instance );
			
			if( m_onRegister != null ) m_onRegister( instance );
		} 
	}
	
	public void Unregister( T instance ) 
	{ 
		if( ( instance != null ) && ( m_list.Contains( instance ) == true  ) ) 
		{
			m_list.Remove( instance ); 
			
			if( m_onUnregister != null ) m_onUnregister( instance );
		}
	}
	
	public ReadOnlyCollection< T > list  { get { return m_list.AsReadOnly(); } }
	
	//****************************************************************************************************
	//
	//****************************************************************************************************
	
	public Instances( OnRegister onRegister, OnUnregister onUnregister ) { m_onRegister = onRegister; m_onUnregister = onUnregister; }
}
