using UnityEngine;

namespace DungeonGame.Quests
{
    [RequireComponent(typeof(EnemyStats))]
    public sealed class QuestEnemyBridge : MonoBehaviour
    {
        private EnemyStats stats;
        private bool reported;
        private void Awake() => stats = GetComponent<EnemyStats>();
        private void OnEnable() { if (stats != null) stats.OnDeath += OnDeath; }
        private void OnDisable() { if (stats != null) stats.OnDeath -= OnDeath; }
        private void OnDeath(Transform killer)
        {
            if (reported) return;
            reported = true;
            if (killer != null) killer.GetComponentInParent<QuestController>()?.Service?.ReportKill(stats.Definition);
        }
    }
}
