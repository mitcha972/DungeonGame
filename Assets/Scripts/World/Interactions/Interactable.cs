using UnityEngine;

// Base component for objects the player can interact with.
public class Interactable : MonoBehaviour 
{
	public float radius = 3f;				// How close do we need to be to interact?
	public Transform interactionTransform;	// The transform from where we interact in case you want to offset it

	public virtual void Interact (Transform interactOwner)
	{
		// This method is meant to be overwritten
		Debug.Log("Interacting with " + transform.name);
	}

	// Called when the object starts being focused
	public virtual void OnFocused (Transform playerTransform)
	{
	}

	// Called when the object is no longer focused
	public virtual void OnDefocused()
	{
	}
}
