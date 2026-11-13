using UnityEngine;

// Runtime-only adapter: its lifetime follows the owning player or the current scene.
[AddComponentMenu("")]
public sealed class LevelSystemAnimatedUpdater : MonoBehaviour
{
    private LevelSystemAnimated model;

    internal void Initialize(LevelSystemAnimated value) => model = value;
    internal void Detach() => model = null;
    private void Update() => model?.Tick(Time.deltaTime);

    private void OnDestroy()
    {
        var previousModel = model;
        model = null;
        previousModel?.UpdaterDestroyed(this);
    }
}
