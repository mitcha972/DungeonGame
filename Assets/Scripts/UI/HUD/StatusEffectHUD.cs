using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class StatusEffectHUD : MonoBehaviour
{
    public StatusEffectController controller;
    private readonly Dictionary<string, TextMeshProUGUI> labels = new Dictionary<string, TextMeshProUGUI>();
    private ActiveStatusEffect hovered;
    private TextMeshProUGUI tooltip;
    private GameObject tooltipPanel;
    private float messageUntil;
    private void Awake() { if (controller == null) controller = GetComponentInParent<StatusEffectController>(); }
    private void OnEnable() { if (controller != null) { controller.ActiveEffectsChanged += Refresh; Refresh(); } }
    private void OnDisable()
    {
        if (controller != null) controller.ActiveEffectsChanged -= Refresh;
        HideTooltip();
    }
    private void Refresh()
    {
        foreach (var label in labels.Values) if (label != null) { label.transform.parent.gameObject.SetActive(false); Destroy(label.transform.parent.gameObject); }
        labels.Clear(); HideTooltip();
        int index = 0;
        foreach (var effect in controller.ActiveEffects)
        {
            var go = new GameObject(effect.Id, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(40, 48); rect.anchoredPosition = new Vector2(index++ * 46, 0);
            go.GetComponent<UnityEngine.UI.Image>().color = new Color(.18f, .14f, .1f, .95f);
            var icon = new GameObject("Icon", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            icon.transform.SetParent(go.transform, false);
            var ir = (RectTransform)icon.transform; ir.sizeDelta = new Vector2(28, 28); ir.anchoredPosition = new Vector2(0, 7);
            var image = icon.GetComponent<UnityEngine.UI.Image>(); image.sprite = effect.Definition.icon;
            image.color = effect.Definition.icon != null ? Color.white : new Color(.8f, .65f, .25f); image.raycastTarget = false; image.preserveAspect = true;
            var textGo = new GameObject("Countdown", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            var label = textGo.GetComponent<TextMeshProUGUI>(); label.fontSize = 15; label.color = new Color(1, .87f, .58f);
            label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
            label.rectTransform.sizeDelta = new Vector2(40, 20); label.rectTransform.anchoredPosition = new Vector2(0, -15);
            labels.Add(effect.Id, label);
            var pointer = UIPointerEvents.GetOrAdd(go.transform);
            pointer.Entered = () => ShowTooltip(effect);
            pointer.Exited = HideTooltip;
        }
        Update();
    }
    private void HideTooltip()
    {
        hovered = null;
        messageUntil = 0;
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }
    private void ShowTooltip(ActiveStatusEffect effect)
    {
        EnsureTooltip();
        messageUntil = 0; hovered = effect; tooltip.text = Description(effect);
        tooltipPanel.SetActive(true); tooltipPanel.transform.SetAsLastSibling();
    }
    public void ShowMessage(string message)
    {
        EnsureTooltip(); hovered = null; tooltip.text = message;
        messageUntil = Time.unscaledTime + 2;
        tooltipPanel.SetActive(true); tooltipPanel.transform.SetAsLastSibling();
    }
    private void EnsureTooltip()
    {
        if (tooltipPanel == null)
        {
            tooltipPanel = new GameObject("StatusTooltip", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            tooltipPanel.transform.SetParent(transform, false);
            var rect = (RectTransform)tooltipPanel.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(0, -56); rect.sizeDelta = new Vector2(300, 145);
            var background = tooltipPanel.GetComponent<UnityEngine.UI.Image>();
            background.color = new Color(.12f, .09f, .06f, .97f); background.raycastTarget = false;
            var text = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            text.transform.SetParent(tooltipPanel.transform, false); tooltip = text.GetComponent<TextMeshProUGUI>();
            tooltip.rectTransform.anchorMin = Vector2.zero; tooltip.rectTransform.anchorMax = Vector2.one;
            tooltip.rectTransform.offsetMin = new Vector2(9, 8); tooltip.rectTransform.offsetMax = new Vector2(-9, -8);
            tooltip.fontSize = 17; tooltip.color = new Color(1, .87f, .58f); tooltip.raycastTarget = false;
        }
    }
    public static string Description(ActiveStatusEffect effect)
    {
        var d = effect.Definition;
        string text = d.displayName + "\n" + d.description + "\n" + Mathf.CeilToInt(effect.RemainingDuration) + "s remaining";
        if (effect.damage > 0) text += "\nTakes " + effect.damage + " damage every " + effect.interval + " seconds (before armor).";
        if (effect.movement != 1) text += "\nMovement speed: " + (effect.movement * 100).ToString("0") + "%";
        if (effect.outgoing != 1) text += "\nOutgoing damage: " + (effect.outgoing * 100).ToString("0") + "%";
        if (effect.incoming != 1) text += "\nIncoming damage: " + (effect.incoming * 100).ToString("0") + "%";
        return text;
    }
    private void Update()
    {
        if (controller == null) return;
        if (messageUntil > 0 && Time.unscaledTime >= messageUntil) HideTooltip();
        if (hovered != null && tooltip != null) tooltip.text = Description(hovered);
        foreach (var effect in controller.ActiveEffects)
            if (labels.TryGetValue(effect.Id, out var label)) label.SetText("{0}s", Mathf.CeilToInt(effect.RemainingDuration));
    }
}
