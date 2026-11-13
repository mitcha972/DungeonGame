using System;
using UnityEngine;

namespace DungeonGame.Quests
{
    [RequireComponent(typeof(Inventory), typeof(PlayerStats))]
    public sealed class QuestController : MonoBehaviour
    {
        public QuestDefinition[] quests = Array.Empty<QuestDefinition>();
        public QuestService Service { get; private set; }
        private Inventory inventory;
        private void Start() => Initialize();
        public void Initialize()
        {
            if (Service != null) return;
            inventory = GetComponent<Inventory>();
            var stats = GetComponent<PlayerStats>();
            Service = new QuestService(quests, inventory, () => stats.QuestLevel, stats.AddXP);
            inventory.onItemChangedCallback += OnInventoryChanged;
        }
        private void OnInventoryChanged() => Service?.InventoryChanged();
        private void OnDestroy() { if (inventory != null) inventory.onItemChangedCallback -= OnInventoryChanged; }
    }
}
