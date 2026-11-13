using DungeonGame.Dialogue;
using UnityEngine;

namespace DungeonGame.Quests
{
    public enum QuestDialogueCommand { Start, TurnInStage, ReportNpc }
    [CreateAssetMenu(menuName = "Dungeon Game/Quests/Dialogue Action")]
    public sealed class QuestDialogueAction : DialogueAction
    {
        public QuestDefinition quest;
        public QuestDialogueCommand command;
        public string stageId;
        public string npcId;
        public override bool Execute(DialogueController controller, DialogueNpc npc)
        {
            var service = controller.GetComponent<QuestController>()?.Service;
            if (service == null) { controller.ShowStatus("Quest services are unavailable."); return true; }
            string reason = null;
            bool success;
            switch (command)
            {
                case QuestDialogueCommand.Start: success = service.TryStart(quest, out reason); break;
                case QuestDialogueCommand.TurnInStage: success = service.TryAdvance(quest, stageId, out reason); break;
                case QuestDialogueCommand.ReportNpc: service.ReportNpc(npcId); return false;
                default: success = false; break;
            }
            if (!success) controller.ShowStatus(reason ?? "This quest action is unavailable.");
            else controller.RefreshEntry();
            return true;
        }
    }
}
