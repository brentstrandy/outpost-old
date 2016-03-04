using UnityEngine;

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
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<GameDataManager>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    #endregion INSTANCE (SINGLETON)

    // Use this for initialization
    private void Start()
    {
        // Instantiate data class for storing all tower data
        TowerDataManager = new DataManager<TowerData>();
        // Instantiate data class for storing all enemy data
        EnemyDataManager = new DataManager<EnemyData>();
        // Instantiate data class for storing all level data
        LevelDataManager = new DataManager<LevelData>();

        // Set to local when starting in the Unity Editor
#if UNITY_EDITOR
        SwitchToLocalData();
#else
		SwitchToServerData();
#endif
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
		StartCoroutine(TowerDataManager.LoadDataFromServer("http://www.diademstudios.com/outpostdata/TowerData.xml"));
        // Run coroutine to download EnemyData from server (This coroutine cannot be called from the DataManager because
        // it must be called from a MonoBehavior class)
		StartCoroutine(EnemyDataManager.LoadDataFromServer("http://www.diademstudios.com/outpostdata/EnemyData.xml"));
        // Run coroutine to download LevelData from server (This coroutine cannot be called from the DataManager because
        // it must be called from a MonoBehavior class)
		StartCoroutine(LevelDataManager.LoadDataFromServer("http://www.diademstudios.com/outpostdata/LevelData_GetData.php"));
		//StartCoroutine(LevelDataManager.LoadDataFromServer("LevelData.xml"));
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

	public LevelData FindLevelDataByLevelID(int ID)
	{
		return LevelDataManager.DataList.Find(x => x.LevelID == ID);
	}

    // Update is called once per frame
    private void Update()
    {
    }
}