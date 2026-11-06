using UnityEngine;

public class EnemyEquipmentManager : EquipmentManager 
{	
	//add enemy stats
	//get enemy from stats
	[SerializeField] Enemy enemy; // change to get enemy from stats
	
	public override void Setup ()
	{
		//Set up enemy equipment - Choose from default equiptment by chance;
		
		//defaultEquipment = enemy.defaultEquipment;
		DefaultEquipment[] defaultEquipment = enemy.defaultEquipment; //Fixed

		for(int i = 0; i < defaultEquipment.Length; i++)
		{
			//Floors (1-20) * 1:5 + Difficulty(1-4) * 1:10 
			int randomRange = Random.Range(-10,11); //Time level

			int equipLevel = GameController.floor * 5 + GameController.difficulty * 10 + randomRange; 
			int equipChance = Random.Range(0,101);

			if(equipChance <= defaultEquipment[i].equipChance) //Fixed
				if(defaultEquipment[i].equipLevel <= equipLevel)
					Equip(defaultEquipment[i].equipment);
	}
	}

}
