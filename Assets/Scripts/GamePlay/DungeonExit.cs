using UnityEngine;

public class DungeonExit : Interactable
{
    public override void Interact(Transform interactOwner)
    {
        var generator = GetComponentInParent<DungeonGenerator>();
        if (generator == null || generator.IsGenerating || !generator.IsReady) return;
        generator.AdvanceFloor();
    }
}
