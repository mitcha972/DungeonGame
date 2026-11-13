using UnityEngine;

/// <summary>Presentation only; uses the player's existing Canvas and interaction prompt.</summary>
public class GatheringProgressHUD : MonoBehaviour
{
    public PlayerGathering owner;
    public GameObject progressRoot;
    public UnityEngine.UI.Image progressFill;
    private void LateUpdate()
    {
        bool visible = owner != null && owner.IsGathering;
        if (progressRoot != null && progressRoot.activeSelf != visible) progressRoot.SetActive(visible);
        if (visible && progressFill != null) progressFill.rectTransform.anchorMax = new Vector2(owner.Progress, 1);
    }
}
