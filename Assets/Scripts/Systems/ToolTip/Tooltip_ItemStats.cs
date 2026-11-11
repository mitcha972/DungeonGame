using UnityEngine;
using UnityEngine.UI;

public class Tooltip_ItemStats : MonoBehaviour
{
    private static Tooltip_ItemStats instance;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private RectTransform canvasRectTransform;

    private Text nameText, descriptionText;
    private RectTransform backgroundRectTransform;
    private RectTransform tooltipRectTransform;
    private RectTransform parentRectTransform;
    private Canvas canvas;
    private readonly Vector3[] corners = new Vector3[4];

    private void Awake()
    {
        instance = this;
        tooltipRectTransform = transform as RectTransform;
        parentRectTransform = transform.parent as RectTransform;
        canvas = GetComponentInParent<Canvas>();
        if (canvasRectTransform == null && canvas != null)
            canvasRectTransform = canvas.GetComponent<RectTransform>();
        Transform background = transform.Find("background");
        backgroundRectTransform = background != null ? background as RectTransform : tooltipRectTransform;
        Transform title = transform.Find("nameText");
        Transform description = transform.Find("descriptionText");
        nameText = title != null ? title.GetComponent<Text>() : null;
        descriptionText = description != null ? description.GetComponent<Text>() : null;
        HideTooltip();
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Update()
    {
        if (tooltipRectTransform == null || parentRectTransform == null) return;
        Camera eventCamera = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null : uiCamera != null ? uiCamera : canvas != null ? canvas.worldCamera : null;
        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRectTransform, Input.mousePosition, eventCamera, out Vector3 position)) return;
        tooltipRectTransform.position = position;
        if (canvasRectTransform == null || backgroundRectTransform == null) return;

        // Compare all corners in canvas space so scaling, pivots and nested parents are respected.
        backgroundRectTransform.GetWorldCorners(corners);
        Vector2 minimum = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 maximum = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        foreach (Vector3 corner in corners)
        {
            Vector3 local = canvasRectTransform.InverseTransformPoint(corner);
            minimum = Vector2.Min(minimum, new Vector2(local.x, local.y));
            maximum = Vector2.Max(maximum, new Vector2(local.x, local.y));
        }
        Rect bounds = canvasRectTransform.rect;
        float x = minimum.x < bounds.xMin ? bounds.xMin - minimum.x : maximum.x > bounds.xMax ? bounds.xMax - maximum.x : 0f;
        float y = maximum.y > bounds.yMax ? bounds.yMax - maximum.y : minimum.y < bounds.yMin ? bounds.yMin - minimum.y : 0f;
        if (maximum.x - minimum.x > bounds.width) x = bounds.xMin - minimum.x;
        if (maximum.y - minimum.y > bounds.height) y = bounds.yMax - maximum.y;
        tooltipRectTransform.position += canvasRectTransform.TransformVector(new Vector3(x, y, 0f));
    }

    private void ShowTooltip(Item item)
    {
        if (item == null) { HideTooltip(); return; }
        gameObject.SetActive(true);
        if (nameText != null) nameText.text = item.name;
        if (descriptionText != null) descriptionText.text = item.description;
        if (tooltipRectTransform != null) LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRectTransform);
        Update();
    }

    public void HideTooltip() => gameObject.SetActive(false);

    public static void ShowTooltip_Static(Item item)
    {
        if (instance != null) instance.ShowTooltip(item);
    }

    public static void HideTooltip_Static()
    {
        if (instance != null) instance.HideTooltip();
    }
}
