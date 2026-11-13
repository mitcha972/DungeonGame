using UnityEngine;
using UnityEngine.EventSystems;

namespace DungeonGame.Dialogue
{
    [RequireComponent(typeof(PlayerControlState))]
    public sealed class DialogueController : MonoBehaviour
    {
        public DialoguePanel panel;
        public bool IsOpen { get; private set; }
        public DialogueNpc Speaker { get; private set; }
        public DialogueNode CurrentNode { get; private set; }
        public PlayerControlState Controls => controls;
        private PlayerControlState controls;
        private GameObject previousSelection;

        private void Awake() => controls = GetComponent<PlayerControlState>();
        public bool Open(DialogueNpc npc)
        {
            if (IsOpen || controls == null || !controls.GameplayAllowed || panel == null || npc == null || npc.conversation == null) return false;
            var node = npc.conversation.Find(npc.conversation.entryNode);
            if (node == null) return false;
            previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            Speaker = npc;
            npc.ActiveController = this;
            IsOpen = true;
            controls.Synchronize();
            GetComponent<PlayerCombat>()?.CancelAttack();
            panel.gameObject.SetActive(true);
            Display(node);
            return true;
        }
        private void Update()
        {
            if (IsOpen && (panel == null || !panel.gameObject.activeInHierarchy || Speaker == null || !Speaker.isActiveAndEnabled || !controls.isActiveAndEnabled || GetComponent<PlayerStats>()?.IsDead == true)) Close();
        }
        private void Display(DialogueNode node)
        {
            CurrentNode = node;
            panel.Show(this, Speaker.speakerName, node);
        }
        public void Choose(int index)
        {
            if (!IsOpen || CurrentNode == null || index < 0 || index >= CurrentNode.responses.Length) return;
            var response = CurrentNode.responses[index];
            if (response == null) return;
            if (response.action != null && response.action.Execute(this, Speaker)) return;
            if (string.IsNullOrEmpty(response.nextNode)) { Close(); return; }
            var next = Speaker.conversation.Find(response.nextNode);
            if (next != null) Display(next);
            else ShowStatus("This conversation branch is unavailable.");
        }
        public void ShowStatus(string message) { if (panel != null) panel.SetText(message); }
        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            if (Speaker != null && Speaker.ActiveController == this) Speaker.ActiveController = null;
            Speaker = null;
            CurrentNode = null;
            if (panel != null) panel.gameObject.SetActive(false);
            if (EventSystem.current != null && (EventSystem.current.currentSelectedGameObject == null || (panel != null && EventSystem.current.currentSelectedGameObject.transform.IsChildOf(panel.transform))))
                EventSystem.current.SetSelectedGameObject(previousSelection != null && previousSelection.activeInHierarchy ? previousSelection : null);
            if (controls != null) controls.Synchronize();
        }
        private void OnDisable() => Close();
    }
}
