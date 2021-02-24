using UnityEngine;
using System.Collections;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public class DragDropComponent : MonoBehaviour, IDragDrop
{
	public virtual bool BeginDrag   ( IDragDrop obj ) { return false; }

	public virtual void UpdateDrag  ( IDragDrop obj ) {}

	public virtual void CancelDrag  ( IDragDrop obj ) {}

	public virtual bool AcceptDrop  ( IDragDrop obj ) { return false; }
}
