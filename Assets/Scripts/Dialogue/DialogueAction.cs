using UnityEngine;

namespace DungeonGame.Dialogue
{
    // Return true when the action has taken ownership of the transition.
    public abstract class DialogueAction : ScriptableObject
    {
        public abstract bool Execute(DialogueController controller, DialogueNpc npc);
    }
}
