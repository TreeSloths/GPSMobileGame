using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class UIPicListItem : UIListItem
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void Setup( UIItemListBase list, int paramIndex, GameObject paramObj )
	{
		base.Setup( list, paramIndex, paramObj );

		userDatas = string.Empty;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override void ReflectDatasOverride()
	{
		if( txt != null ) txt.text = userDatas as string;
	}
}
