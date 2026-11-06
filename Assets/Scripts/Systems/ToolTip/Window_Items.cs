using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window_Items : MonoBehaviour {

	private void Start () 
    {
        //GetButtons
        //IF moouse if over button Chheck tool tip
        //transform.GetComponent<Button_UI>().MouseOutOnceTooltipFunc = () => Tooltip_ItemStats.HideTooltip_Static();
        //transform.GetComponent<Button_UI>().MouseOverOnceTooltipFunc = () => Tooltip_ItemStats.ShowTooltip_Static(item);
        /*
        for (int i = 1; i <= 6; i++) 
        {
            ItemInfo itemInfo = transform.Find("itemBtn_" + i).GetComponent<ItemInfo>();
            //Tooltip_ItemStats.AddTooltip(transform.Find("itemBtn_" + i), itemInfo.sprite, itemInfo.itemName, itemInfo.itemDescription, itemInfo.DEX, itemInfo.CON, itemInfo.STR);
            Tooltip_ItemStats.AddTooltip(transform.Find("itemBtn_" + i), itemInfo.sprite, itemInfo.itemName, itemInfo.itemDescription);
       
        }
        */
	}

}
