using UnityEngine;

public class ItemInteract : Interactable 
{
	public Item item;	// Item to put in the inventory on pickup
	public MeshCollider meshCollider;

	void Start()
	{
		//Set Mesh
		meshCollider.sharedMesh = transform.Find("ItemPreview").GetComponent<MeshFilter>().sharedMesh;
		//outline = GetComponent<Outline>(); 
	}

	// When the player interacts with the item
	public override void Interact(Transform interactOwner)
	{
		base.Interact(interactOwner);

		//Pick up inventory
		Inventory ownerInventory = interactOwner.GetComponent<Inventory>();
		PickUp(ownerInventory);	// Pick it up!
	}

	// Pick up the item
	void PickUp(Inventory inventory)
	{
		Debug.Log("Picking up " + item.name);

		// Chekck if can be pickued up and Add to inventory
		bool wasPickedUp = inventory.Add(item);	

		// If successfully picked up
		if (wasPickedUp)
			Destroy(gameObject);	// Destroy item from scene
	}

	public void LookingAt(Transform t)
	{
		Debug.Log("Lookinnnnf " + item.name);
	}

	/*
	void OnMouseEnter()
    {
        //Set Outline
		outline.enabled = true;
		
	}

    void OnMouseExit()
    {
        //Remove Outline
		outline.enabled = false;
    }
	*/

}
