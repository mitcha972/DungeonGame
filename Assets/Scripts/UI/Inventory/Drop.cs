using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Drop : MonoBehaviour , IDropHandler
{
	public void OnDrop(PointerEventData eventData)
	{
		//When Dropping Moving slot
		Debug.Log("Droped on Slot -- " + this.gameObject.name);

    }
}
