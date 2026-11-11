using UnityEngine;

[ExecuteInEditMode]
public class MeshFromSkinnedMesh : MonoBehaviour
{
    private void Start()
    {
        var source = GetComponent<SkinnedMeshRenderer>();
        if (source == null || source.sharedMesh == null) return;
        var filter = GetComponent<MeshFilter>();
        if (filter == null) filter = gameObject.AddComponent<MeshFilter>();
        var target = GetComponent<MeshRenderer>();
        if (target == null) target = gameObject.AddComponent<MeshRenderer>();
        if (filter == null || target == null) return;
        filter.sharedMesh = source.sharedMesh;
        target.sharedMaterials = source.sharedMaterials;

        if (Application.isPlaying)
        {
            source.enabled = false;
            enabled = false;
            Destroy(source);
            Destroy(this);
        }
        else
        {
            DestroyImmediate(source);
            DestroyImmediate(this);
        }
    }
}
