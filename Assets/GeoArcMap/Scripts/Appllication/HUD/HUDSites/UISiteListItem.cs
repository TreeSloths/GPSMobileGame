using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class UISiteListItem : UIListItem
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void Setup( UIItemListBase list, int paramIndex, GameObject paramObj )
	{
		base.Setup( list, paramIndex, paramObj );

		icon = ( obj != null ) ? CORE.HIERARCHY.FindComp< UnityEngine.UI.RawImage >( obj, "Icon" ) : null;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public UnityEngine.UI.RawImage icon { get; set; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void ReflectDatasOverride()
	{
		Localizable localizable = ( userDatas   != null ) && ( userDatas is Localizable ) ? userDatas as Localizable : null;

		bool        site        = ( localizable != null ) && ( localizable is Site );

		if( icon != null ) icon.uvRect = new Rect( new Vector2( site ? 0.0f : 0.5f, 0.0f ), new Vector2( 0.5f, 1.0f ) );

		if( txt  != null ) txt.text    = ( localizable != null ) ? localizable.name : string.Empty;
	}
}
