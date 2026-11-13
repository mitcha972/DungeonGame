using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class EnemyInteract : Interactable
{
    private CharacterStats _stats;
    private Outline _outline;
    private void Awake()
    {
        _stats = GetComponent<CharacterStats>();
        _outline = GetComponentInChildren<Outline>(true);
    }
    public override void OnFocused(Transform playerTransform)
    {
        if (!isActiveAndEnabled || _stats == null || _stats.IsDead) return;
        base.OnFocused(playerTransform);
        if (_outline != null) _outline.enabled = true;
    }
    public override void OnDefocused()
    {
        base.OnDefocused();
        if (_outline != null) _outline.enabled = false;
    }
    private void OnDisable() { if (_outline != null) _outline.enabled = false; }
}
