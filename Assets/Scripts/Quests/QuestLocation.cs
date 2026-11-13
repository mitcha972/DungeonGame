using UnityEngine;

namespace DungeonGame.Quests
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class QuestLocation : MonoBehaviour
    {
        public string locationId;
        private void Reset() => GetComponent<BoxCollider>().isTrigger = true;
        // Stay also covers accepting/restoring a stage while already inside. Discovery is boolean.
        private void OnTriggerStay(Collider other)
        {
            other.GetComponentInParent<QuestController>()?.Service?.ReportLocation(locationId);
        }
    }
}
