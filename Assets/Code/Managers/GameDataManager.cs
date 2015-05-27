using UnityEngine;
using System.Collections;

public class GameDataManager : MonoBehaviour
{
	private static GameDataManager instance;

	public TowerDataManager TowerDataMngr { get; private set; }
	public EnemyDataManager EnemyDataMngr { get; private set; }
	public LevelDataManager LevelDataMngr { get; private set; }

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
		// Instantiate data class for storing all tower data
		TowerDataMngr = new TowerDataManager();
		// Run coroutine to download TowerData from server (This coroutine cannot be called from the DataManager because
		// it must be called from a MonoBehavior class)
		StartCoroutine(TowerDataMngr.LoadDataFromServer());

		// Instantiate data class for storing all enemy data
		EnemyDataMngr = new EnemyDataManager();
		// Run coroutine to download EnemyData from server (This coroutine cannot be called from the DataManager because
		// it must be called from a MonoBehavior class)
		StartCoroutine(EnemyDataMngr.LoadDataFromServer());

		// Instantiate data class for storing all level data
		LevelDataMngr = new LevelDataManager();
		// Run coroutine to download LevelData from server (This coroutine cannot be called from the DataManager because
		// it must be called from a MonoBehavior class)
		StartCoroutine(LevelDataMngr.LoadDataFromServer());
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
