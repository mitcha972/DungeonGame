using System;
using UnityEngine;
using UnityEngine.EventSystems;

// Gameplay callbacks use native pointer events. Existing serialized hover visuals stay on their original component.
[DisallowMultipleComponent]
public sealed class UIPointerEvents : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Action LeftClicked;
    public Action RightClicked;
    public Action Entered;
    public Action Exited;

    private Behaviour legacyPresentation;
    private bool exitOnLeftClick;

    public static UIPointerEvents GetOrAdd(Component target)
    {
        if (target == null) return null;
        var events = target.GetComponent<UIPointerEvents>();
        return events != null ? events : target.gameObject.AddComponent<UIPointerEvents>();
    }

    private void Awake()
    {
        // Optional migration bridge only: no vendor type is needed to compile or dispatch gameplay callbacks.
        foreach (var component in GetComponents<MonoBehaviour>())
        {
            if (component == null || component.GetType().FullName != "CodeMonkey.Utils.Button_UI") continue;
            legacyPresentation = component;
            exitOnLeftClick = component.GetType().GetField("triggerMouseOutFuncOnClick")?.GetValue(component) is bool value && value;
            enabled = component.enabled;
            break;
        }
    }

    public void SetInteractionEnabled(bool value)
    {
        if (legacyPresentation != null) legacyPresentation.enabled = value;
        enabled = value;
    }

    private bool CanInteract => isActiveAndEnabled && (legacyPresentation == null || legacyPresentation.isActiveAndEnabled);

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanInteract || eventData == null) return;
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (exitOnLeftClick) Exited?.Invoke();
            LeftClicked?.Invoke();
        }
        else if (eventData.button == PointerEventData.InputButton.Right) RightClicked?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CanInteract) Entered?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData) => Exited?.Invoke();
    private void OnDisable() => Exited?.Invoke();

    private void OnDestroy()
    {
        LeftClicked = RightClicked = Entered = Exited = null;
    }
}
