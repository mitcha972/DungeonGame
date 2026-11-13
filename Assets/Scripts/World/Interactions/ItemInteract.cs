using UnityEngine;

public class ItemInteract : Interactable
{
    public Item item;
    public MeshCollider meshCollider;
    private bool pickedUp;

    private void Start()
    {
        Transform preview = transform.Find("ItemPreview");
        var filter = preview != null ? preview.GetComponent<MeshFilter>() : null;
        if (meshCollider != null && filter != null) meshCollider.sharedMesh = filter.sharedMesh;
    }

    public override void Interact(Transform interactOwner)
    {
        if (pickedUp || item == null || interactOwner == null) return;
        var inventory = interactOwner.GetComponent<Inventory>();
        if (inventory == null || !inventory.Add(item)) return;
        // Destroy is deferred, so guard against a second interaction this frame.
        pickedUp = true;
        Destroy(gameObject);
    }

    public void LookingAt(Transform t)
    {
        if (item != null) Debug.Log("Looking at " + item.name);
    }
}
