using UnityEngine;

[CreateAssetMenu(fileName = "New Class", menuName = "Class")]
public class Class : ScriptableObject 
{
    new public string name = "New Class";	// Name of the item
	new public string description = "Class Description";
	public Sprite icon = null;				// Class icon	

	[Header("Stats")]
	public int health;

    public Equipment[] equipment;
}
