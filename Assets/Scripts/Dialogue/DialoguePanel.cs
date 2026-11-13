using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DungeonGame.Dialogue
{
    public sealed class DialoguePanel : MonoBehaviour
    {
        public TMP_Text speakerText;
        public TMP_Text npcText;
        public Button responseTemplate;
        public Transform responseParent;
        private DialogueController controller;
        private readonly System.Collections.Generic.List<Button> buttons = new System.Collections.Generic.List<Button>();
        private int openedFrame;
        private static readonly Key[] NumberKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9 };

        public void Show(DialogueController owner, string speaker, DialogueNode node)
        {
            controller = owner;
            openedFrame = Time.frameCount;
            speakerText.text = speaker;
            npcText.text = node.text;
            foreach (var button in buttons) { button.gameObject.SetActive(false); Destroy(button.gameObject); }
            buttons.Clear();
            responseTemplate.gameObject.SetActive(false);
            for (int i = 0; i < node.responses.Length; i++)
            {
                if (node.responses[i] == null) continue;
                int index = i;
                var button = Instantiate(responseTemplate, responseParent);
                button.name = "Response" + (i + 1);
                button.GetComponentInChildren<TMP_Text>(true).text = (i + 1) + ". " + node.responses[i].text;
                button.onClick.AddListener(() => controller.Choose(index));
                button.gameObject.SetActive(true);
                buttons.Add(button);
            }
            for (int i = 0; i < buttons.Count; i++)
            {
                var navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = buttons[(i + buttons.Count - 1) % buttons.Count], selectOnDown = buttons[(i + 1) % buttons.Count] };
                buttons[i].navigation = navigation;
            }
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(buttons.Count > 0 ? buttons[0].gameObject : null);
        }
        public void SetText(string text) => npcText.text = text;
        private void Update()
        {
            if (controller == null || !controller.IsOpen || Time.frameCount <= openedFrame) return;
            // Standard EventSystem handles arrows/Enter/gamepad. Number shortcuts are supplementary.
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            for (int i = 0; i < NumberKeys.Length && i < buttons.Count; i++)
                if (keyboard[NumberKeys[i]].wasPressedThisFrame) { buttons[i].onClick.Invoke(); return; }
        }
        private void OnDisable() { if (controller != null) controller.Close(); }
    }
}
