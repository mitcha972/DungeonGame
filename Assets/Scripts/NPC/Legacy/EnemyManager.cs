using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour 
{

	#region Singleton

	public EnemyManager enemyInstance;

	void Awake ()
	{
		enemyInstance = this;
	}

	#endregion
}
