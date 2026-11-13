using UnityEngine;

//This component is for all objects that the player cann nteract with 
public class Interactable : MonoBehaviour 
{
	public float radius = 3f;				// How close do we need to be to interact?
	public Transform interactionTransform;	// The transform from where we interact in case you want to offset it

	bool isFocus = false;	// Is this interactable currently being focused?
	Transform player;		// Reference to the player transform

	//bool hasInteracted = false;	// Have we already interacted with the object?

	public virtual void Interact (Transform interactOwner)
	{
		// This method is meant to be overwritten
		Debug.Log("Interacting with " + transform.name);
	}

	// Called when the object starts being focused
	public virtual void OnFocused (Transform playerTransform)
	{
		isFocus = true;
		player = playerTransform;
		//hasInteracted = true;
	}

	// Called when the object is no longer focused
	public virtual void OnDefocused()
	{
		isFocus = false;
		player = null;
		//hasInteracted = false;
	}
}
