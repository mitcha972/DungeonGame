using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Canvas canvas;
    RectTransform rectTransform;
    Transform parent;
    Vector3 rect;
    public InventorySlot inventorySlot;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData) 
    {
        //Find Start Position
        parent = transform.parent;
        transform.parent = canvas.gameObject.transform;

        gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
        rect = rectTransform.position;
        Debug. Log ("OnBeginDrag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log ("OnDrag");
        //rectTransform.anchoredPosition+=eventData.delta/canvas.scaleFactor;
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag (PointerEventData eventData) 
    {
        gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
        transform.parent = parent;
        rectTransform.position = rect;
        Debug.Log("OnEndDrag");
    }

}
