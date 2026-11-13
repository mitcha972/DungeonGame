using UnityEngine;

namespace DungeonGame.Dialogue
{
    public abstract class DialogueCondition : ScriptableObject
    {
        public abstract bool Evaluate(DialogueController controller, DialogueNpc npc);
        public static bool All(DialogueCondition[] conditions, DialogueController controller, DialogueNpc npc)
        {
            if (conditions == null) return true;
            foreach (var condition in conditions) if (condition == null || !condition.Evaluate(controller, npc)) return false;
            return true;
        }
    }
}
