using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class ObjectPool< ObjectT >
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum OPTION { NONE, CHECK_DUPLICATES }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private int       m_capacity = 0;

	private int       m_size     = 0;

	private ObjectT[] m_slots    = new ObjectT[ 0 ];

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public ObjectPool( int capacity )
	{
		m_capacity = ( capacity > 0 ) ? CORE.Alignement.Align( capacity, 16 ) : 16;

		System.Array.Resize( ref m_slots, m_capacity );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public ObjectT Grab()
	{
		ObjectT o = ( m_size > 0 ) ? m_slots[ --m_size ] : default( ObjectT );

		return  o;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Release( ObjectT o, OPTION opt = OPTION.NONE )
	{
		if( o == null )            return;

		if( m_capacity <= 0 )      return;

		if( m_size >= m_capacity ) return;

		if( opt == OPTION.CHECK_DUPLICATES )
		{
			for( int slot = 0; slot < m_size; ++slot )
			{
				if( object.ReferenceEquals( m_slots[ slot ], o ) )
				{
					return;
				}
			}
		}

		m_slots[ m_size++ ] = o;
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class UIListItemData
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public enum RECURSE { NO, YES }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public UIListItemData( UIListItemData paramParent, object paramUserDatas )
	{
		itemList   = null;

		depth      = -1;

		expanded   = false;

		userDatas  = paramUserDatas;

		parent     = null;

		childs     = new List< UIListItemData >();

		if( paramParent != null ) paramParent.AddChild( this );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public UIItemListBase         itemList   { get; set; }

	public int                    depth      { get; set; }

	public bool                   expanded   { get; set; }

	public object                 userDatas  { get; set; }

	public UIListItemData         parent     { get; set; }

	public List< UIListItemData > childs     { get; set; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void AddChild( UIListItemData child )
	{
		if( child        == null ) return;

		if( child.parent != null ) return;

		childs.Add( child );

		child.parent = this;

		child.depth  = depth + 1;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void RemoveChild( UIListItemData child )
	{
		if( child        == null ) return;

		if( child.parent != this ) return;

		childs.Remove( child );

		child.parent = null;

		child.depth  = -1;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void Collapse( RECURSE recurse )
	{
		if( ( parent != null ) && ( expanded ) )
		{
			expanded = false;

			CollapseOverride();

			if( itemList != null ) itemList.SetUpdateHierarchy();
		}

		if( recurse == RECURSE.YES )
		{
			for( int child = 0; child < childs.Count; ++child )
			{
				childs[ child ].Collapse( recurse );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void Expand( RECURSE recurse, int paramDepth = -1 )
	{
		if( ( parent != null ) && ( expanded == false ) )
		{
			expanded = true;

			ExpandOverride();

			if( itemList != null ) itemList.SetUpdateHierarchy();
		}

		if( paramDepth < 0 )
		{
			for( UIListItemData cur = parent; ( cur != null ) && ( cur.parent != null ); cur = cur.parent )
			{
				parent.Expand( RECURSE.NO, 0 );
			}
		}

		if( recurse == RECURSE.YES )
		{
			for( int child = 0; child < childs.Count; ++child )
			{
				childs[ child ].Expand( recurse, paramDepth + 1 );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Expand( bool expand, RECURSE recurse )
	{
		if( expand ) Expand  ( recurse );

		else         Collapse( recurse );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Sort( RECURSE recurse, Comparison< UIListItemData > comparison )
	{
		if( comparison != null ) 
		{
			childs.Sort( comparison );

			if( itemList != null ) itemList.SetUpdateHierarchy();


			if( recurse == RECURSE.YES )
			{
				for( int child = 0; child < childs.Count; ++child )
				{
					childs[ child ].Sort( recurse, comparison );
				}
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public virtual void ExpandOverride()   {}

	public virtual void CollapseOverride() {}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class UIListItem
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	private UIItemListBase m_itemList     = null;

	private UIListItemData m_listItemData = null;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public virtual void Setup( UIItemListBase paramList, int paramIndex, GameObject paramObj )
	{
		list         = paramList;

		index        = paramIndex;

		datas        = null;

		obj          = paramObj;
		
		xform        = ( obj != null ) ? obj.GetComponent< RectTransform         >() : null;

		but          = ( obj != null ) ? obj.GetComponent< UnityEngine.UI.Button >() : null;

		indent       = CORE.HIERARCHY.FindXForm< RectTransform           >( obj, "ItemIndent" );

		txt          = CORE.HIERARCHY.FindComp < UnityEngine.UI.Text     >( obj, "ItemText"   );

		btExpand     = CORE.HIERARCHY.FindComp < UnityEngine.UI.Button   >( obj, "ItemExpand" );

		btExpandIcon = CORE.HIERARCHY.FindComp < UnityEngine.UI.RawImage >( obj, "ItemExpand" );


		if( btExpand != null ) btExpand.onClick.AddListener( delegate { Expand(); } );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public UIItemListBase          list         { get { return m_itemList;     } set { m_itemList     = value; if( m_listItemData != null ) m_listItemData.itemList = value;      } }

	public UIListItemData          datas        { get { return m_listItemData; } set { m_listItemData = value; if( m_listItemData != null ) m_listItemData.itemList = m_itemList; } }

	public int                     index        { get; set; }

	public object                  userDatas    { get { return ( datas != null ) ? datas.userDatas : null;  } set { if( datas != null ) datas.userDatas = value; } }

	public bool                    expanded     { get { return ( datas != null ) ? datas.expanded  : false; } set { if( datas != null ) datas.expanded  = value; } }

	public int                     depth        { get { return ( datas != null ) ? datas.depth     : 0;     } set { if( datas != null ) datas.depth     = value; } }

	public GameObject              obj          { get; set; }

	public RectTransform           xform        { get; set; }

	public RectTransform           indent       { get; set; }

	public UnityEngine.UI.Button   but          { get; set; }

	public UnityEngine.UI.Text     txt          { get; set; }

	public UnityEngine.UI.Button   btExpand     { get; set; }

	public UnityEngine.UI.RawImage btExpandIcon { get; set; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Expand()
	{
		if( datas != null )
		{
			datas.Expand( ! datas.expanded, UIListItemData.RECURSE.NO );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ReflectDatas( object selection )
	{
		if( btExpandIcon != null )
		{
			btExpandIcon.gameObject.SetActive( ( ( datas != null ) && ( datas.childs.Count > 0 ) ) || ShowExpandButtonOverride() );

			if( btExpandIcon.gameObject.activeSelf )
			{
				btExpandIcon.uvRect = new Rect( new Vector2( expanded ? 0.5f : 0.0f, 0.0f ), new Vector2( 0.5f, 1.0f ) );
			}
		}

		if( indent != null ) indent.localPosition = Vector3.right * ( depth << 4 );

		if( but    != null ) but.interactable     = ( ( datas == null ) || ( datas.userDatas != selection ) );

		ReflectDatasOverride();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public virtual bool ShowExpandButtonOverride() { return false; }

	public virtual void ReflectDatasOverride()     {}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class UIItemListBase
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public delegate void OnSelection( object userDatas );

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public UIItemListBase( GameObject paramRoot, string paramScrollView, string paramViewport, string paramContent, string paramScrollbar, string paramItemTemplate, OnSelection paramOnSelection )
	{
		GameObject scrl = CORE.HIERARCHY.Find( paramRoot, paramScrollView );

		viewport        = CORE.HIERARCHY.FindXForm< RectTransform            >( scrl, paramViewport  );

		content         = CORE.HIERARCHY.FindXForm< RectTransform            >( scrl, paramContent   );

		scrollbar       = CORE.HIERARCHY.FindComp < UnityEngine.UI.Scrollbar >( scrl, paramScrollbar );

		itemTemplate    = Resources.Load< GameObject >( paramItemTemplate );

		onSelection     = paramOnSelection;

		datas           = new List< UIListItemData >();

		items           = new List< UIListItem     >();

		dic             = new Dictionary< int, UIListItem >();

		pool            = new ObjectPool< UIListItem >( 256 );

		selection       = null;

		dirty           = true;


		ConfigureViewport();

		if( scrollbar != null )
		{
			scrollbar.onValueChanged.RemoveAllListeners();

			scrollbar.onValueChanged.AddListener( delegate { dirty = true; } );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void ConfigureViewport()
	{
		if( viewport != null )
		{
			UnityEngine.UI.RawImage img = viewport.GetComponent< UnityEngine.UI.RawImage >();

			UnityEngine.UI.Button   but = viewport.GetComponent< UnityEngine.UI.Button   >();

			if( but == null )
			{
				but = viewport.gameObject.AddComponent< UnityEngine.UI.Button >();

				but.transition = UnityEngine.UI.Selectable.Transition.None;


				UnityEngine.UI.Navigation nav = new UnityEngine.UI.Navigation();

				nav.mode = UnityEngine.UI.Navigation.Mode.None;

				but.navigation = nav;
			}

			if( but != null )
			{
				but.interactable = true;

				but.onClick.RemoveAllListeners();

				but.onClick.AddListener( delegate { SelectItemUserDatas( null ); } );
			}

			if( img != null )
			{ 
				img.raycastTarget = true;
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public virtual UIListItem CreateItem()
	{
		return new UIListItem();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public RectTransform                 content          { get; set; }

	public RectTransform                 viewport         { get; set; }

	public UnityEngine.UI.Scrollbar      scrollbar        { get; set; }

	public GameObject                    itemTemplate     { get; set; }

	public UIListItemData                datasRoot        { get; set; }

	public List< UIListItemData >        datas            { get; set; }

	public List< UIListItem     >        items            { get; set; }

	public Dictionary< int, UIListItem > dic              { get; set; }

	public ObjectPool< UIListItem >      pool             { get; set; }

	public object				         selection        { get; set; }

	public OnSelection                   onSelection      { get; set; }

	public bool                          updateHierarchy  { get; set; }

	public bool                          dirty            { get; set; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public UIListItem this[ int index ]
	{
		get
		{
			UIListItem item = null;

			dic.TryGetValue( index, out item );

			return item;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void ShowItemObject( GameObject o, bool show )
	{
		if( o != null ) o.SetActive( show );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void AddItemData( UIListItemData itemData )
	{
		if( itemData != null )
		{
			datas.Add( itemData );

			itemData.itemList = this;

			if( itemData.expanded )
			{
				for( int data = 0; data < itemData.childs.Count; ++data )
				{
					AddItemData( itemData.childs[ data ] );
				}
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void SetItemsDatas( UIListItemData root )
	{
		Clear();

		if( ( datasRoot = root ) != null )
		{
			for( int data = 0; data < root.childs.Count; ++data )
			{
				AddItemData( root.childs[ data ] );
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateItemsDatas()
	{
		updateHierarchy = false;

		SetItemsDatas( datasRoot );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void SelectItemUserDatas( object userDatas )
	{
		if( selection != userDatas )
		{
			selection  = userDatas;

			for( int item = 0; item < items.Count; ++item ) { items[ item ].ReflectDatas( selection ); }

			if( onSelection != null )
			{
				onSelection( userDatas ); 
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void SelectItem( int index )
	{
		UIListItem     item = ( index >= 0    ) ? this[ index ] : null;

		UIListItemData data = ( item  != null ) ? item.datas    : null;

		if( data != null ) SelectItemUserDatas( data.userDatas );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void SetUpdateHierarchy()
	{
		updateHierarchy = true;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void SetDirty()
	{
		dirty = true;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Refresh()
	{
		for( int item = 0; item < items.Count; ++item )
		{
			ShowItemObject( items[ item ].obj, false );

			pool.Release  ( items[ item ] );
		}

		dic.Clear  ();

		items.Clear();

		dirty = true;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Clear()
	{
		if( selection != null )
		{
			selection  = null;

			if( onSelection != null ) { onSelection( null ); }
		}

		datasRoot = null;

		datas.Clear();

		Refresh    ();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Sort( Comparison< UIListItemData > comparison )
	{
		if( datasRoot != null )
		{
			datasRoot.Sort( UIListItemData.RECURSE.YES, comparison );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Update()
	{
		if( updateHierarchy ) UpdateItemsDatas();

		if ( dirty == false ) return;

		else dirty = false;


		RectTransform xform  = ( itemTemplate != null ) ? itemTemplate.GetComponent< RectTransform >() : null;

		float itemH          = ( xform  !=  null ) ? xform.rect.height                                                     : 0;

		int   aperture       = ( itemH       > 0 ) ? ( int )( viewport.rect.height / itemH )                               : 0;

		int   firstVisible   = ( datas.Count > 0 ) ? ( int )( ( 1.0f - scrollbar.value ) * ( datas.Count - 1 ) )           : 0;

		int   lastVisible    = ( datas.Count > 0 ) ? Mathf.Clamp( firstVisible + aperture, firstVisible, datas.Count - 1 ) : 0;

		int   nbVisible      = ( datas.Count > 0 ) && ( aperture > 0 ) ? lastVisible - firstVisible + 1                    : 0;


		for( int item = 0; item < items.Count; )
		{
			int index = items[ item ].index;

			if( ( nbVisible <= 0 ) || ( index < firstVisible ) || ( index > lastVisible ) )
			{
				ShowItemObject( items[ item ].obj, false );

				pool.Release  ( items[ item ] );

				items.RemoveAt( item  );

				dic.Remove    ( index );
			}
			else
			{
				 ++item;
			}
		}


		for( int item = 0; item < nbVisible; ++item )
		{
			int index = firstVisible + item;

			if( dic.ContainsKey( index ) )
			{
				items[ item ].ReflectDatas( selection );

				continue;
			}

			UIListItem instance = pool.Grab();

			if( instance == null )
			{
				instance = CreateItem();

				instance.Setup( this, -1, GameObject.Instantiate< GameObject >( itemTemplate ) );

				if( instance.obj != null ) instance.obj.hideFlags = HideFlags.DontSave;
			}

			if( instance != null )
			{
				instance.index = index;

				instance.datas = datas[ index ];

				items.Add( instance );

				dic.Add  ( instance.index, instance );


				instance.xform.SetParent( content, false );

				instance.xform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, content.rect.width );

				instance.xform.localPosition = -( Vector3.up * ( ( index * itemH ) + ( itemH * 0.5f ) ) );


				instance.ReflectDatas( selection );

				if( instance.but != null )
				{
					instance.but.onClick.RemoveAllListeners();

					instance.but.onClick.AddListener( delegate { SelectItem( index ); } );
				}

				ShowItemObject( instance.obj, true );
			}
		}

		content.sizeDelta = new Vector2( content.sizeDelta.x, ( datas.Count * itemH ) );
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class UIItemList< ItemT > : UIItemListBase where ItemT : UIListItem, new()
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public UIItemList( GameObject  paramRoot			,
		
					   string      paramScrollView		, 
		
		               string      paramViewport		, 
					   
					   string      paramContent			, 
					   
					   string      paramScrollbar		, 
					   
					   string      paramItemTemplate	, 
					   
					   OnSelection paramOnSelection ) 
	
	: base( paramRoot, paramScrollView, paramViewport, paramContent, paramScrollbar, paramItemTemplate, paramOnSelection ) { }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override UIListItem CreateItem()
	{
		return new ItemT();
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class UIItemListDeferredSelection
{
	private object m_value = null;

	public bool   Active { get; set; }

	public object Value  { get { return m_value; } set { m_value = value; Active = true; } }

	public object Consume() { if( Active ) { Active = false; return Value; }  return null; }
}
