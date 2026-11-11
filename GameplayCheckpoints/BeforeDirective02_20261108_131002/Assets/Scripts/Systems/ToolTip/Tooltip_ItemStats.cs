using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;

public class Tooltip_ItemStats : MonoBehaviour 
{

    private static Tooltip_ItemStats instance;
    
    [SerializeField]
    private Camera uiCamera;
    [SerializeField]
    private RectTransform canvasRectTransform;

    private Image image;
    private Text nameText ,descriptionText, attackText, defText, blockText, speedText;
    private RectTransform backgroundRectTransform;

    private void Awake() 
    {
        //Item Text
        instance = this;
        backgroundRectTransform = transform.Find("background").GetComponent<RectTransform>();
        nameText = transform.Find("nameText").GetComponent<Text>();
        descriptionText = transform.Find("descriptionText").GetComponent<Text>();
        attackText = transform.Find("attackText").GetComponent<Text>();
        defText = transform.Find("defText").GetComponent<Text>();

        //blockText = transform.Find("blockText").GetComponent<Text>();
        //speedText = transform.Find("speedText").GetComponent<Text>();
        //priceText = transform.Find("priceText").GetComponent<Text>();

        HideTooltip();
    }

    private void Update() 
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, uiCamera, out localPoint);
        //transform.localPosition = localPoint;
        transform.position = Input.mousePosition;

        Vector2 anchoredPosition = transform.GetComponent<RectTransform>().anchoredPosition;
        if (anchoredPosition.x + backgroundRectTransform.rect.width > canvasRectTransform.rect.width) {
            anchoredPosition.x = canvasRectTransform.rect.width - backgroundRectTransform.rect.width;
        }
        if (anchoredPosition.y - backgroundRectTransform.rect.height > canvasRectTransform.rect.height) {
            anchoredPosition.y = canvasRectTransform.rect.height + backgroundRectTransform.rect.height;
        }
        //transform.GetComponent<Transform>().position = localPoint;
        //transform.GetComponent<RectTransform>().anchoredPosition = localPoint;
        //transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
        
    }

    //Change to Send Item Info
    //private void ShowTooltip(Sprite itemSprite, string itemName, string itemDescription) 
    private void ShowTooltip(Item item) 
    {
        if(item == null)
            return;

        gameObject.SetActive(true);
        //transform.SetAsLastSibling();
        nameText.text = item.name;
        descriptionText.text = item.description;

        //attackText.text = item.damageModifier.ToString();
        //defText.text = item.armorModifier.ToString();
       
        //levelText.text = level.ToString();
        //image.sprite = itemSprite;
        Update();
    }

    public void HideTooltip() 
    {
        gameObject.SetActive(false);
    }

    public static void ShowTooltip_Static(Item item) 
    {
        instance.ShowTooltip(item);
    }

    public static void HideTooltip_Static() 
    {
        instance.HideTooltip();
    }    

/*
    public static void AddTooltip(Transform transform, Sprite itemSprite, string itemName, string itemDescription) 
    {
        if (transform.GetComponent<Button_UI>() != null) 
        {
            //If MOuse is over
            //transform.GetComponent<Button_UI>().MouseOverOnceTooltipFunc = () => Tooltip_ItemStats.ShowTooltip_Static(itemSprite, itemName, itemDescription);
            transform.GetComponent<Button_UI>().MouseOutOnceTooltipFunc = () => Tooltip_ItemStats.HideTooltip_Static();
        }
    }
    */

}