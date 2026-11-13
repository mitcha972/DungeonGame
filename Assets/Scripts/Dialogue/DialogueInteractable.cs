using UnityEngine;

namespace DungeonGame.Dialogue
{
    [RequireComponent(typeof(DialogueNpc))]
    public sealed class DialogueInteractable : Interactable
    {
        public override void Interact(Transform owner) => GetComponent<DialogueNpc>().Interact(owner);
    }
}
