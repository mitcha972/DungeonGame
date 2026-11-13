using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceBar : MonoBehaviour {

    private float barMaskWidth;
    private RectTransform barMaskRectTransform;
    private RectTransform edgeRectTransform;
    private RawImage barRawImage;

    private void Awake() 
    {
        barMaskRectTransform = transform.Find("barMask").GetComponent<RectTransform>();
        barRawImage = transform.Find("barMask").Find("bar").GetComponent<RawImage>();
        edgeRectTransform = transform.Find("edge").GetComponent<RectTransform>();

        barMaskWidth = barMaskRectTransform.sizeDelta.x;
    }

    private void Start() 
    {
        SetSize(0f);
    }

    public void SetSize(float sizeNormalized) 
    {
        Rect uvRect = barRawImage.uvRect;
        uvRect.x += .2f * Time.deltaTime;
        barRawImage.uvRect = uvRect;

        Vector2 barMaskSizeDelta = barMaskRectTransform.sizeDelta;
        barMaskSizeDelta.x = sizeNormalized * barMaskWidth;
        barMaskRectTransform.sizeDelta = barMaskSizeDelta;

        edgeRectTransform.anchoredPosition = new Vector2(sizeNormalized * barMaskWidth, 0);

        edgeRectTransform.gameObject.SetActive(sizeNormalized < 1f && sizeNormalized > 0f);
    }

}
