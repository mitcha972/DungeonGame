using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGame.Quests
{
    [Serializable] public sealed class QuestCounter { public string id; public int count; }
    [Serializable] public sealed class QuestProgress
    {
        public string questId;
        public string stageId;
        public QuestStatus status;
        public bool rewardsClaimed;
        public List<string> completedStages = new List<string>();
        public List<QuestCounter> counters = new List<QuestCounter>();
    }
    [Serializable] public sealed class QuestSaveData
    {
        public int version = 1;
        public List<QuestProgress> quests = new List<QuestProgress>();
    }

    // Character-owned state engine. Definitions are read-only; adapters provide world facts.
    public sealed class QuestService
    {
        private readonly Dictionary<string, QuestDefinition> definitions = new Dictionary<string, QuestDefinition>();
        private readonly Dictionary<string, QuestProgress> progress = new Dictionary<string, QuestProgress>();
        private readonly Inventory inventory;
        private readonly Func<int> level;
        private readonly Action<int> awardXp;
        private bool mutating;
        public event Action Changed;
        public IEnumerable<QuestDefinition> Definitions => definitions.Values;

        public QuestService(IEnumerable<QuestDefinition> catalog, Inventory inventory, Func<int> level, Action<int> awardXp)
        {
            this.inventory = inventory;
            this.level = level;
            this.awardXp = awardXp;
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            foreach (var quest in catalog)
            {
                if (quest == null) throw new ArgumentException("Missing quest definition.");
                if (!quest.Validate(out var reason)) throw new ArgumentException(reason);
                if (definitions.ContainsKey(quest.id)) throw new ArgumentException("Duplicate quest ID: " + quest.id);
                definitions.Add(quest.id, quest);
            }
            foreach (var quest in definitions.Values)
                foreach (var prerequisite in quest.prerequisites)
                    if (!Known(prerequisite)) throw new ArgumentException("Prerequisite missing from quest catalog: " + prerequisite.name);
        }

        private bool Known(QuestDefinition quest) => quest != null && definitions.TryGetValue(quest.id, out var registered) && registered == quest;
        private QuestProgress State(QuestDefinition quest) => Known(quest) && progress.TryGetValue(quest.id, out var state) ? state : null;
        public QuestStatus Status(QuestDefinition quest) => State(quest)?.status ?? QuestStatus.NotStarted;
        public bool RewardsClaimed(QuestDefinition quest) => State(quest)?.rewardsClaimed == true;
        public QuestStage CurrentStage(QuestDefinition quest)
        {
            var state = State(quest);
            return state != null && state.status == QuestStatus.Active ? Array.Find(quest.stages, stage => stage.id == state.stageId) : null;
        }
        public bool CanStart(QuestDefinition quest, out string reason)
        {
            reason = "This quest is unavailable.";
            if (!Known(quest) || Status(quest) != QuestStatus.NotStarted) return false;
            if ((level?.Invoke() ?? 1) < quest.minimumLevel) { reason = "Requires level " + quest.minimumLevel + "."; return false; }
            foreach (var prerequisite in quest.prerequisites)
                if (Status(prerequisite) != QuestStatus.Completed) { reason = "Complete " + prerequisite.title + " first."; return false; }
            reason = null;
            return true;
        }
        public bool TryStart(QuestDefinition quest, out string reason)
        {
            if (mutating) { reason = "A quest update is already in progress."; return false; }
            if (!CanStart(quest, out reason)) return false;
            progress.Add(quest.id, new QuestProgress { questId = quest.id, stageId = quest.stages[0].id, status = QuestStatus.Active });
            Changed?.Invoke();
            return true;
        }
        public int Count(QuestDefinition quest, QuestObjective objective)
        {
            var stage = CurrentStage(quest);
            if (stage == null || objective == null || Array.IndexOf(stage.objectives, objective) < 0) return 0;
            if (objective.type == QuestObjectiveType.PossessItem) return Mathf.Min(objective.count, inventory != null ? inventory.Count(objective.item) : 0);
            return State(quest).counters.Find(counter => counter.id == objective.id)?.count ?? 0;
        }
        public bool Ready(QuestDefinition quest)
        {
            var stage = CurrentStage(quest);
            if (stage == null) return false;
            var consumption = new Dictionary<Item, int>();
            foreach (var objective in stage.objectives)
            {
                if (Count(quest, objective) < objective.count) return false;
                if (objective.consumeOnTurnIn)
                {
                    consumption.TryGetValue(objective.item, out int total);
                    long required = (long)total + objective.count;
                    if (required > int.MaxValue) return false;
                    consumption[objective.item] = (int)required;
                }
            }
            foreach (var pair in consumption) if (inventory == null || inventory.Count(pair.Key) < pair.Value) return false;
            return true;
        }
        public void InventoryChanged() { if (!mutating) Changed?.Invoke(); }
        public void ReportKill(Enemy enemy) => Report(QuestObjectiveType.KillEnemy, enemy, null);
        public void ReportLocation(string id) => Report(QuestObjectiveType.DiscoverLocation, null, id);
        public void ReportNpc(string id) => Report(QuestObjectiveType.TalkToNpc, null, id);
        private void Report(QuestObjectiveType type, Enemy enemy, string target)
        {
            if (mutating || (type == QuestObjectiveType.KillEnemy ? enemy == null : string.IsNullOrWhiteSpace(target))) return;
            bool changed = false;
            foreach (var quest in definitions.Values)
            {
                var stage = CurrentStage(quest);
                if (stage == null) continue;
                foreach (var objective in stage.objectives)
                {
                    if (objective.type != type || (type == QuestObjectiveType.KillEnemy ? objective.enemy != enemy : objective.targetId != target)) continue;
                    var state = State(quest);
                    var counter = state.counters.Find(value => value.id == objective.id);
                    if (counter == null) { counter = new QuestCounter { id = objective.id }; state.counters.Add(counter); }
                    if (counter.count < objective.count) { counter.count++; changed = true; }
                }
            }
            if (changed) Changed?.Invoke();
        }

        // expectedStage protects stale dialogue actions. Only explicit author actions advance stages.
        public bool TryAdvance(QuestDefinition quest, string expectedStage, out string reason)
        {
            reason = "Finish the current objectives before returning.";
            var stage = CurrentStage(quest);
            if (mutating || stage == null || stage.id != expectedStage || !Ready(quest)) return false;
            int index = Array.IndexOf(quest.stages, stage);
            bool final = index == quest.stages.Length - 1;
            if (final && quest.experienceReward > 0 && awardXp == null) { reason = "Experience rewards are unavailable."; return false; }
            var removals = new List<InventoryItemAmount>();
            foreach (var objective in stage.objectives)
                if (objective.consumeOnTurnIn) removals.Add(new InventoryItemAmount { item = objective.item, count = objective.count });
            var rewards = final ? quest.itemRewards : Array.Empty<InventoryItemAmount>();
            if (inventory == null && (removals.Count > 0 || rewards.Length > 0)) { reason = "Inventory is unavailable."; return false; }
            if (inventory != null && !inventory.CanExchange(removals, rewards)) { reason = "Make room in your inventory for the reward."; return false; }
            mutating = true;
            try
            {
                // Inventory commits without notification; state and XP commit before observers run.
                if (inventory != null && !inventory.TryExchange(removals, rewards, false)) return false;
                var state = State(quest);
                state.completedStages.Add(stage.id);
                state.counters.Clear();
                if (final) { state.status = QuestStatus.Completed; state.rewardsClaimed = true; }
                else state.stageId = quest.stages[index + 1].id;
                if (final && quest.experienceReward > 0) awardXp(quest.experienceReward);
                inventory?.NotifyExchange();
                reason = null;
            }
            finally { mutating = false; }
            Changed?.Invoke();
            return true;
        }

        public string CaptureJson()
        {
            var data = new QuestSaveData();
            data.quests.AddRange(progress.Values);
            return JsonUtility.ToJson(data, true);
        }

        // Quest-only DTO contract. Caller must restore matching inventory/XP in a coordinated save.
        public bool TryRestoreJson(string json, out string reason)
        {
            reason = "Invalid or incompatible quest save.";
            if (mutating || string.IsNullOrWhiteSpace(json)) return false;
            QuestSaveData data;
            try { data = JsonUtility.FromJson<QuestSaveData>(json); } catch (ArgumentException) { return false; }
            if (data == null || data.version != 1 || data.quests == null) return false;
            var restored = new Dictionary<string, QuestProgress>();
            foreach (var state in data.quests)
            {
                if (state == null || string.IsNullOrWhiteSpace(state.questId) || restored.ContainsKey(state.questId) || !definitions.TryGetValue(state.questId, out var quest)) return false;
                int index = Array.FindIndex(quest.stages, stage => stage.id == state.stageId);
                if (index < 0 || (state.status != QuestStatus.Active && state.status != QuestStatus.Completed) || state.counters == null || state.completedStages == null) return false;
                bool completed = state.status == QuestStatus.Completed;
                if (completed != state.rewardsClaimed || (completed && index != quest.stages.Length - 1)) return false;
                int completedCount = completed ? quest.stages.Length : index;
                if (state.completedStages.Count != completedCount) return false;
                for (int i = 0; i < completedCount; i++) if (state.completedStages[i] != quest.stages[i].id) return false;
                var ids = new HashSet<string>();
                if (completed && state.counters.Count > 0) return false;
                foreach (var counter in state.counters)
                {
                    if (counter == null || !ids.Add(counter.id)) return false;
                    var objective = Array.Find(quest.stages[index].objectives, value => value.id == counter.id);
                    if (objective == null || objective.type == QuestObjectiveType.PossessItem) return false;
                    counter.count = Mathf.Clamp(counter.count, 0, objective.count);
                }
                restored.Add(state.questId, state);
            }
            progress.Clear();
            foreach (var pair in restored) progress.Add(pair.Key, pair.Value);
            reason = null;
            Changed?.Invoke();
            return true;
        }
    }
}
