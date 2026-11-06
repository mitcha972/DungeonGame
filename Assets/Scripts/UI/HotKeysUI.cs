using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/* This object updates the inventory UI. */
public class HotKeysUI : MonoBehaviour 
{
	public Inventory inventory;
	public GameObject[] hotKeys; //hotkeyslots
	public GameObject[] imageSlots; //hotkeyslots

	void Start () 
	{
		inventory.onItemAddCallback += UpUI;
		inventory.onItemChangedCallback += UpUI;
		//inventory.onItemRemoveCallback += UpUI; //Fixxx

		//Set Tab Icon Images to Inventory slot images
		for(int i = 0; i < hotKeys.Length; i++)
			hotKeys[i].GetComponent<Image>().sprite = imageSlots[i].GetComponent<Image>().sprite;			
	}

	/*
	Update the inventory UI by:
			- Adding items
			- Clearing empty slot
	*/
    void UpUI()
	{
		StartCoroutine(UpdateUI());
	}  

	public IEnumerator UpdateUI()
	{
		//Debug.Log("ShoulhaveAdded ---- ");
		yield return new WaitForSeconds(0.08f);
		
		for(int i = 0; i <= 4; i++)
		{
			//Debug.Log("Sprite - " + i + " " + imageSlots[i].GetComponent<Image>().sprite);
			if(imageSlots[i].GetComponent<Image>().sprite != null)
			{
				hotKeys[i].GetComponent<Image>().sprite = imageSlots[i].GetComponent<Image>().sprite;
				hotKeys[i].SetActive(true);
			}
			else
			{
				hotKeys[i].GetComponent<Image>().sprite = null;
				hotKeys[i].SetActive(false);
			}
		}

	}

}
