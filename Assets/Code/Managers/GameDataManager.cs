using UnityEngine;
using System.Collections;

public class GameDataManager : MonoBehaviour
{
	private static GameDataManager instance;

	public string DataLocation { get; private set; }

	public DataManager<TowerData> TowerDataMngr { get; private set; }
	public DataManager<EnemyData> EnemyDataMngr { get; private set; }
	public DataManager<LevelData> LevelDataMngr { get; private set; }

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
		TowerDataMngr = new DataManager<TowerData>();
		TowerDataMngr.LoadDataFromLocal("TowerData.xml");
		// Run coroutine to download TowerData from server (This coroutine cannot be called from the DataManager because
		// it must be called from a MonoBehavior class)
		//StartCoroutine(TowerDataMngr.LoadDataFromServer("TowerData.xml"));

		// Instantiate data class for storing all enemy data
		EnemyDataMngr = new DataManager<EnemyData>();
		EnemyDataMngr.LoadDataFromLocal("EnemyData.xml");
		// Run coroutine to download EnemyData from server (This coroutine cannot be called from the DataManager because
		// it must be called from a MonoBehavior class)
		//StartCoroutine(EnemyDataMngr.LoadDataFromServer());

		// Instantiate data class for storing all level data
		LevelDataMngr = new DataManager<LevelData>();
		LevelDataMngr.LoadDataFromLocal("LevelData.xml");
		// Run coroutine to download LevelData from server (This coroutine cannot be called from the DataManager because
		// it must be called from a MonoBehavior class)
		//StartCoroutine(LevelDataMngr.LoadDataFromServer());
	}

	public void SwitchToServerData()
	{
		DataLocation = "Server";

		// Clear all data
		TowerDataMngr.ClearData();
		EnemyDataMngr.ClearData();
		LevelDataMngr.ClearData();

		// Run coroutine to download TowerData from server (This coroutine cannot be called from the DataManager because
		// it must be called from a MonoBehavior class)
		StartCoroutine(TowerDataMngr.LoadDataFromServer("TowerData.xml"));
		// Run coroutine to download EnemyData from server (This coroutine cannot be called from the DataManager because
		// it must be called from a MonoBehavior class)
		StartCoroutine(EnemyDataMngr.LoadDataFromServer("EnemyData.xml"));
		// Run coroutine to download LevelData from server (This coroutine cannot be called from the DataManager because
		// it must be called from a MonoBehavior class)
		StartCoroutine(LevelDataMngr.LoadDataFromServer("LevelData.xml"));
	}

	public void SwitchToLocalData()
	{
		DataLocation = "Local";

		// Clear all data
		TowerDataMngr.ClearData();
		EnemyDataMngr.ClearData();
		LevelDataMngr.ClearData();

		TowerDataMngr.LoadDataFromLocal("TowerData.xml");
		EnemyDataMngr.LoadDataFromLocal("EnemyData.xml");
		LevelDataMngr.LoadDataFromLocal("LevelData.xml");
	}

	public TowerData FindTowerDataByPrefabName(string prefabName)
	{
		return TowerDataMngr.DataList.Find(x => x.PrefabName.Equals(prefabName));
	}

	public EnemyData FindEnemyDataByDisplayName(string displayName)
	{
		return EnemyDataMngr.DataList.Find(x => x.DisplayName.Equals(displayName));
	}

	public string FindEnemyPrefabNameByDisplayName(string displayName)
	{
		return EnemyDataMngr.DataList.Find(x => x.DisplayName.Equals(displayName)).PrefabName;
	}

	// Update is called once per frame
	void Update () {
	
	}
}
