using DungeonGame.Dialogue;
using UnityEngine;

namespace DungeonGame.Quests
{
    public enum QuestDialogueCheck { NotStarted, CanStart, ActiveStage, ReadyToTurnIn, Completed }
    [CreateAssetMenu(menuName = "Dungeon Game/Quests/Dialogue Condition")]
    public sealed class QuestDialogueCondition : DialogueCondition
    {
        public QuestDefinition quest;
        public QuestDialogueCheck check;
        public string stageId;
        public override bool Evaluate(DialogueController controller, DialogueNpc npc)
        {
            var service = controller != null ? controller.GetComponent<QuestController>()?.Service : null;
            if (service == null || quest == null) return false;
            switch (check)
            {
                case QuestDialogueCheck.NotStarted: return service.Status(quest) == QuestStatus.NotStarted;
                case QuestDialogueCheck.CanStart: return service.CanStart(quest, out _);
                case QuestDialogueCheck.ActiveStage: return service.CurrentStage(quest)?.id == stageId;
                case QuestDialogueCheck.ReadyToTurnIn: return service.CurrentStage(quest)?.id == stageId && service.Ready(quest);
                case QuestDialogueCheck.Completed: return service.Status(quest) == QuestStatus.Completed;
                default: return false;
            }
        }
    }
}
