using UnityEngine;
using System.Collections;

public class GameDataManager : MonoBehaviour
{
	private static GameDataManager instance;

	public string DataLocation { get; private set; }

	public DataManager<TowerData> TowerDataManager { get; private set; }
	public DataManager<EnemyData> EnemyDataManager { get; private set; }
	public DataManager<LevelData> LevelDataManager { get; private set; }

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
		DataLocation = "Local";

		// Instantiate data class for storing all tower data
		TowerDataManager = new DataManager<TowerData>();
		TowerDataManager.LoadDataFromLocal("TowerData.xml");
		// Run coroutine to download TowerData from server (This coroutine cannot be called from the DataManager because
		// it must be called from a MonoBehavior class)
		//StartCoroutine(TowerDataMngr.LoadDataFromServer("TowerData.xml"));

		// Instantiate data class for storing all enemy data
		EnemyDataManager = new DataManager<EnemyData>();
		EnemyDataManager.LoadDataFromLocal("EnemyData.xml");
		// Run coroutine to download EnemyData from server (This coroutine cannot be called from the DataManager because
		// it must be called from a MonoBehavior class)
		//StartCoroutine(EnemyDataMngr.LoadDataFromServer());

		// Instantiate data class for storing all level data
		LevelDataManager = new DataManager<LevelData>();
		LevelDataManager.LoadDataFromLocal("LevelData.xml");
		// Run coroutine to download LevelData from server (This coroutine cannot be called from the DataManager because
		// it must be called from a MonoBehavior class)
		//StartCoroutine(LevelDataMngr.LoadDataFromServer());
	}

	public void SwitchToServerData()
	{
		DataLocation = "Server";

		// Clear all data
		TowerDataManager.ClearData();
		EnemyDataManager.ClearData();
		LevelDataManager.ClearData();

		// Run coroutine to download TowerData from server (This coroutine cannot be called from the DataManager because
		// it must be called from a MonoBehavior class)
		StartCoroutine(TowerDataManager.LoadDataFromServer("TowerData.xml"));
		// Run coroutine to download EnemyData from server (This coroutine cannot be called from the DataManager because
		// it must be called from a MonoBehavior class)
		StartCoroutine(EnemyDataManager.LoadDataFromServer("EnemyData.xml"));
		// Run coroutine to download LevelData from server (This coroutine cannot be called from the DataManager because
		// it must be called from a MonoBehavior class)
		StartCoroutine(LevelDataManager.LoadDataFromServer("LevelData.xml"));
	}

	public void SwitchToLocalData()
	{
		DataLocation = "Local";

		// Clear all data
		TowerDataManager.ClearData();
		EnemyDataManager.ClearData();
		LevelDataManager.ClearData();

		TowerDataManager.LoadDataFromLocal("TowerData.xml");
		EnemyDataManager.LoadDataFromLocal("EnemyData.xml");
		LevelDataManager.LoadDataFromLocal("LevelData.xml");
	}

	public TowerData FindTowerDataByDisplayName(string displayName)
	{
		return TowerDataManager.DataList.Find(x => x.DisplayName.Equals(displayName));
	}

	public string FindTowerPrefabByDisplayName(string displayName)
	{
		return TowerDataManager.DataList.Find(x => x.DisplayName.Equals(displayName)).PrefabName;
	}

	public EnemyData FindEnemyDataByDisplayName(string displayName)
	{
		return EnemyDataManager.DataList.Find(x => x.DisplayName.Equals(displayName));
	}

	public string FindEnemyPrefabNameByDisplayName(string displayName)
	{
		return EnemyDataManager.DataList.Find(x => x.DisplayName.Equals(displayName)).PrefabName;
	}

	public LevelData FindLevelDataByDisplayName(string displayName)
	{
		return LevelDataManager.DataList.Find(x => x.DisplayName.Equals(displayName));
	}

	// Update is called once per frame
	void Update () {
	
	}
}
