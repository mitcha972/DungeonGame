using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    public TMP_Text floorText;
    public Transform target;
    public Camera mapCamera;
    public RectTransform mapPanel;
    public RectTransform playerMarker;
    public RawImage display;
    public float height = 45f;
    public float localRange = 30f;
    public float expandedRange = 100f;
    private RenderTexture texture;
    private int lastFloor = int.MinValue;
    public bool IsExpanded { get; private set; }
    private void Awake()
    {
        if (mapCamera == null || display == null) return;
        texture = new RenderTexture(512,512,16) { name = "Minimap Runtime", antiAliasing = 1 };
        texture.Create();
        mapCamera.targetTexture = texture;
        display.texture = texture;
        mapCamera.orthographic = true;
        mapCamera.nearClipPlane = .1f;
        mapCamera.farClipPlane = height + 30;
        mapCamera.allowHDR = mapCamera.allowMSAA = mapCamera.useOcclusionCulling = false;
        mapCamera.backgroundColor = new Color(.055f,.065f,.055f,1);
        mapCamera.clearFlags = CameraClearFlags.SolidColor;
        SetExpanded(false);
    }
    public void SetExpanded(bool expanded)
    {
        IsExpanded = expanded;
        if (mapPanel == null) return;
        mapPanel.anchorMin = mapPanel.anchorMax = expanded ? new Vector2(.5f,.5f) : Vector2.one;
        mapPanel.pivot = expanded ? new Vector2(.5f,.5f) : Vector2.one;
        mapPanel.anchoredPosition = expanded ? Vector2.zero : new Vector2(-24,-24);
        UpdateSize();
    }
    private void UpdateSize()
    {
        if (mapPanel == null) return;
        var canvas = mapPanel.GetComponentInParent<Canvas>();
        var rect = canvas != null ? canvas.transform as RectTransform : null;
        float side = IsExpanded && rect != null ? Mathf.Min(rect.rect.width,rect.rect.height)*.82f : 220f;
        mapPanel.sizeDelta = new Vector2(side,side);
        if (mapCamera != null) mapCamera.orthographicSize = IsExpanded ? expandedRange : localRange;
    }
    private void LateUpdate()
    {
        if (floorText != null && lastFloor != GameController.floor) { lastFloor = GameController.floor; floorText.text = "Floor " + lastFloor; }
        if (mapCamera == null || target == null) return;
        mapCamera.transform.SetPositionAndRotation(target.position + Vector3.up*height,Quaternion.Euler(90,0,0));
        if (playerMarker != null) playerMarker.localRotation = Quaternion.Euler(0,0,-target.eulerAngles.y);
        UpdateSize();
    }
    private void OnDisable() { IsExpanded = false; }
    private void OnDestroy()
    {
        if (texture == null) return;
        if (mapCamera != null && mapCamera.targetTexture == texture) mapCamera.targetTexture = null;
        texture.Release();
        Destroy(texture);
    }
}
