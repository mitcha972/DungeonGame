using System;
using UnityEngine;
using UnityEngine.Events;

namespace DungeonGame.Dialogue
{
    public sealed class DialogueNpc : MonoBehaviour
    {
        public string speakerName = "Traveller";
        public DialogueConversation conversation;
        public ShopScript shop;
        public DialogueController ActiveController { get; internal set; }
        public ServiceEvent onServiceRequested = new ServiceEvent();
        [Serializable] public sealed class ServiceEvent : UnityEvent<DialogueService, string> { }

        public void Interact(Transform owner)
        {
            var controller = owner != null ? owner.GetComponent<DialogueController>() : null;
            if (controller != null) controller.Open(this);
        }

        public void RequestService(DialogueService service, string id) => onServiceRequested.Invoke(service, id);
    }
}
