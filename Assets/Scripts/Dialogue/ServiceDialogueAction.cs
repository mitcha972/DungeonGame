using UnityEngine;

namespace DungeonGame.Dialogue
{
    public enum DialogueService { OpenShop, OpenBank, StartQuest, OpenService, EndDialogue }

    [CreateAssetMenu(menuName = "Dungeon Game/Dialogue/Service Action")]
    public sealed class ServiceDialogueAction : DialogueAction
    {
        public DialogueService service;
        public string serviceId;
        public override bool Execute(DialogueController controller, DialogueNpc npc)
        {
            if (service == DialogueService.EndDialogue) { controller.Close(); return true; }
            if (service == DialogueService.OpenShop)
            {
                // Activate the shop before releasing dialogue's input lock: no gameplay frame in between.
                if (npc.shop != null && npc.shop.TryOpenShop(controller.transform)) controller.Close();
                else controller.ShowStatus("The shop is unavailable right now.");
                return true;
            }
            // Deliberate integration hooks, not banking/quest implementations.
            // A receiver opens its UI before calling controller.Close() to transfer ownership.
            npc.RequestService(service, serviceId);
            return true;
        }
    }
}
