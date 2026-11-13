using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGame.Quests
{
    public enum QuestStatus { NotStarted, Active, Completed }
    public enum QuestObjectiveType { PossessItem, KillEnemy, DiscoverLocation, TalkToNpc }

    [Serializable]
    public sealed class QuestObjective
    {
        public string id;
        public string description;
        public QuestObjectiveType type;
        [Min(1)] public int count = 1;
        public Item item;
        public Enemy enemy;
        public string targetId;
        public bool consumeOnTurnIn;
    }

    [Serializable]
    public sealed class QuestStage
    {
        public string id;
        [TextArea] public string journal;
        public QuestObjective[] objectives = Array.Empty<QuestObjective>();
    }

    [CreateAssetMenu(menuName = "Dungeon Game/Quests/Quest")]
    public sealed class QuestDefinition : ScriptableObject
    {
        public string id;
        public string title;
        [TextArea] public string summary;
        [Min(1)] public int minimumLevel = 1;
        public QuestDefinition[] prerequisites = Array.Empty<QuestDefinition>();
        public QuestStage[] stages = Array.Empty<QuestStage>();
        [Min(0)] public int experienceReward;
        public InventoryItemAmount[] itemRewards = Array.Empty<InventoryItemAmount>();

        public bool Validate(out string reason)
        {
            reason = "Invalid quest definition: " + name;
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(title) || minimumLevel < 1 || experienceReward < 0 || stages == null || stages.Length == 0) return false;
            var stageIds = new HashSet<string>();
            foreach (var stage in stages)
            {
                if (stage == null || string.IsNullOrWhiteSpace(stage.id) || !stageIds.Add(stage.id) || stage.objectives == null || stage.objectives.Length == 0) return false;
                var ids = new HashSet<string>();
                foreach (var objective in stage.objectives)
                {
                    if (objective == null || string.IsNullOrWhiteSpace(objective.id) || !ids.Add(objective.id) || objective.count < 1 || !Enum.IsDefined(typeof(QuestObjectiveType), objective.type)) return false;
                    if (objective.type == QuestObjectiveType.PossessItem && objective.item == null) return false;
                    if (objective.type == QuestObjectiveType.KillEnemy && objective.enemy == null) return false;
                    if ((objective.type == QuestObjectiveType.DiscoverLocation || objective.type == QuestObjectiveType.TalkToNpc) && string.IsNullOrWhiteSpace(objective.targetId)) return false;
                    if ((objective.type == QuestObjectiveType.DiscoverLocation || objective.type == QuestObjectiveType.TalkToNpc) && objective.count != 1) return false;
                    if (objective.consumeOnTurnIn && objective.type != QuestObjectiveType.PossessItem) return false;
                }
            }
            if (itemRewards == null || prerequisites == null) return false;
            foreach (var reward in itemRewards) if (reward == null || reward.item == null || reward.count < 1) return false;
            var visiting = new HashSet<QuestDefinition>();
            if (!Acyclic(this, visiting, new HashSet<QuestDefinition>())) return false;
            reason = null;
            return true;
        }

        private static bool Acyclic(QuestDefinition quest, HashSet<QuestDefinition> visiting, HashSet<QuestDefinition> visited)
        {
            if (quest == null || quest.prerequisites == null || !visiting.Add(quest)) return false;
            if (!visited.Contains(quest))
                foreach (var prerequisite in quest.prerequisites)
                    if (!Acyclic(prerequisite, visiting, visited)) return false;
            visiting.Remove(quest);
            visited.Add(quest);
            return true;
        }
    }
}
