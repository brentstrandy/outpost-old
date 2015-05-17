using UnityEngine;
using System.Collections;

public class GameDataManager : MonoBehaviour
{
	private static GameDataManager instance;

	public TowerDataManager TowerDataMngr { get; private set; }
	public EnemyDataManager EnemyDataMngr { get; private set; }

	#region INSTANCE (SINGLETON)
	/// <summary>
	/// Singleton - There can only be one
	/// </summary>
	/// <value>The instance.</value>
	public static GameDataManager Instance
	{
		get
		{
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType<GameDataManager>();
			}
			
			return instance;
		}
	}

	void Awake()
	{
		instance = this;
	}
	#endregion

	// Use this for initialization
	void Start ()
	{
		// Load all Enemy and Tower data for the game
		TowerDataMngr = new TowerDataManager();

		// At this time Enemy data is not needed. This data will be stored in the prefabs
		EnemyDataMngr = new EnemyDataManager();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
