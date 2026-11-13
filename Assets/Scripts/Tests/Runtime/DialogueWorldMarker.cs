using UnityEngine;
namespace DungeonGame.Dialogue
{
    // A visible, scaled-time witness that world simulation continues during dialogue.
    public sealed class DialogueWorldMarker : MonoBehaviour
    {
        public float SimulatedSeconds { get; private set; }
        private void Update() { SimulatedSeconds += Time.deltaTime; transform.Rotate(0, 60f * Time.deltaTime, 0); }
    }
}
