using System;
using UnityEngine;

namespace DungeonGame.Dialogue
{
    [CreateAssetMenu(menuName = "Dungeon Game/Dialogue/Conversation")]
    public sealed class DialogueConversation : ScriptableObject
    {
        public string entryNode = "greeting";
        public DialogueNode[] nodes = Array.Empty<DialogueNode>();

        public DialogueNode Find(string id) => Array.Find(nodes, n => n != null && n.id == id);
    }

    [Serializable]
    public sealed class DialogueNode
    {
        public string id;
        [TextArea(3, 8)] public string text;
        public DialogueResponse[] responses = Array.Empty<DialogueResponse>();
    }

    [Serializable]
    public sealed class DialogueResponse
    {
        public string text;
        [Tooltip("Empty ends the conversation after the action.")]
        public string nextNode;
        public DialogueAction action;
    }
}
