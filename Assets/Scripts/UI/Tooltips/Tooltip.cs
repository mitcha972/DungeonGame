using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour 
{
    private static Tooltip instance;
    
    [SerializeField]
    private Camera uiCamera;
    [SerializeField]
    private RectTransform canvasRectTransform;

    private Text tooltipText;
    private RectTransform backgroundRectTransform;
    private Func<string> getTooltipStringFunc;

    private void Awake() 
    {
        instance = this;
        backgroundRectTransform = transform.Find("background").GetComponent<RectTransform>();
        tooltipText = transform.Find("text").GetComponent<Text>();

        HideTooltip();
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Update() 
    {
        if (getTooltipStringFunc == null) { HideTooltip(); return; }
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, uiCamera, out localPoint);
        transform.localPosition = localPoint;

        SetText(getTooltipStringFunc());

        Vector2 anchoredPosition = transform.GetComponent<RectTransform>().anchoredPosition;
        if (anchoredPosition.x + backgroundRectTransform.rect.width > canvasRectTransform.rect.width) {
            anchoredPosition.x = canvasRectTransform.rect.width - backgroundRectTransform.rect.width;
        }
        if (anchoredPosition.y + backgroundRectTransform.rect.height > canvasRectTransform.rect.height) {
            anchoredPosition.y = canvasRectTransform.rect.height - backgroundRectTransform.rect.height;
        }
        transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
    }

    private void ShowTooltip(string tooltipString) 
    {
        ShowTooltip(() => tooltipString);
    }

    private void ShowTooltip(Func<string> getTooltipStringFunc) 
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        this.getTooltipStringFunc = getTooltipStringFunc;
        Update();
    }

    private void SetText(string tooltipString) 
    {
        tooltipText.text = tooltipString;
        float textPaddingSize = 4f;
        Vector2 backgroundSize = new Vector2(tooltipText.preferredWidth + textPaddingSize * 2f, tooltipText.preferredHeight + textPaddingSize * 2f);
        backgroundRectTransform.sizeDelta = backgroundSize;
    }

    private void HideTooltip() 
    {
        gameObject.SetActive(false);
    }

    public static void ShowTooltip_Static(string tooltipString) 
    {
        if (instance != null) instance.ShowTooltip(tooltipString);
    }

    public static void ShowTooltip_Static(Func<string> getTooltipStringFunc) 
    {
        if (instance != null) instance.ShowTooltip(getTooltipStringFunc);
    }

    public static void HideTooltip_Static() 
    {
        if (instance != null) instance.HideTooltip();
    }    

    public static void AddTooltip(Transform transform, string tooltipString) {
        AddTooltip(transform, () => tooltipString);
    }

    public static void AddTooltip(Transform transform, Func<string> getTooltipStringFunc) {
        var button = UIPointerEvents.GetOrAdd(transform);
        if (button == null) return;
        button.Entered = () => ShowTooltip_Static(getTooltipStringFunc);
        button.Exited = () => HideTooltip_Static();
    }

}

public static class Tooltip_Extensions 
{

    public static void AddTooltip(this Transform transform, string tooltipString) {
        transform.AddTooltip(() => tooltipString);
    }
    
    public static void AddTooltip(this Transform transform, Func<string> getTooltipStringFunc) {
        Tooltip.AddTooltip(transform, getTooltipStringFunc);
    }
}
