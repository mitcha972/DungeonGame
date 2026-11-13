using UnityEngine;

[CreateAssetMenu(menuName = "Item/Skillbook")]
public sealed class Skillbook : Item
{
    public RpgSkill skill;
    public int MaxStackSize => 1;
    public bool Stackable => false;
    public const int XP = 100;
    public override void Use(GameObject owner, int slotIndex)
    {
        if (owner == null) return;
        var bag = owner.GetComponent<Inventory>();
        var progress = owner.GetComponent<PlayerSkillProgress>();
        var stats = owner.GetComponent<PlayerStats>();
        if (bag == null || progress == null || (stats != null && (stats.IsDead || stats.currentHealth <= 0)) || slotIndex < 0 || slotIndex >= bag.items.Count || bag.items[slotIndex] != this || !progress.CanAward(skill, XP) || progress.GetXP(skill) > int.MaxValue - XP) return;
        // Remove the exact slot before XP observers can try activating it again.
        bag.items[slotIndex] = null;
        if (!progress.AwardXP(skill, XP)) { bag.items[slotIndex] = this; return; }
        bag.NotifyExchange();
    }
}
