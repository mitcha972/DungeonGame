using UnityEngine;

// Explicitly placed only in Gameplay_Test, never on the production player prefab.
public sealed class SkillbookTestSupply : Interactable
{
    public Skillbook[] books;
    public override void Interact(Transform player)
    {
        var bag = player != null ? player.GetComponent<Inventory>() : null;
        if (bag == null || books == null) return;
        var rewards = new InventoryItemAmount[books.Length];
        for (int i = 0; i < books.Length; i++) rewards[i] = new InventoryItemAmount { item = books[i], count = 1 };
        bool added = bag.TryExchange(System.Array.Empty<InventoryItemAmount>(), rewards);
        player.GetComponent<PlayerGathering>()?.ShowFeedback(added ? "Ten skillbooks added. Open inventory and use each book." : "Make room for ten skillbooks first.");
    }
}
