using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Manages single game levels. This manager is only persistent within a single game instance. It will be destroyed
/// and recreated whenever a new level is loaded
/// Created By: Brent Strandy
/// </summary>
public class GameManager : MonoBehaviour
{
    public bool ShowDebugLogs = true;

    private static GameManager instance;

    // Gameplay Managers
    public EnemyManager EnemyManager { get; private set; }

    public TowerManager TowerManager { get; private set; }
    public EnemySpawnManager EnemySpawnManager { get; private set; }

	public int GameID = -1;
    public LevelData CurrentLevelData { get; private set; }
    public DataManager<EnemySpawnData> EnemySpawnDataManager { get; private set; }
    public DataManager<NotificationData> LevelNotificationDataManager { get; private set; }

    public bool Victory { get; private set; }
    public bool GameRunning { get; private set; }

    public float LevelStartTime { get; private set; }

    // Components
	public HexMesh TerrainMesh { get; private set; }

	public MiningFacility ObjMiningFacility;
    private PhotonView ObjPhotonView;

    #region INSTANCE (SINGLETON)

    /// <summary>
    /// Singleton - There can only be one
    /// </summary>
    /// <value>The instance.</value>
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<GameManager>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
		PhotonSerializer.Register();
    }

    #endregion INSTANCE (SINGLETON)

    // Use this for initialization
    private void Start()
    {
        // Track events in order to react to Session Manager events as they happen
        SessionManager.Instance.OnSMSwitchMaster += OnSwitchMaster;

        // Store LevelData from MenuManager
        CurrentLevelData = MenuManager.Instance.CurrentLevelData;

		ObjPhotonView = PhotonView.Get(this);

        // Instantiate objects to manage Enemies and Towers
        EnemyManager = new EnemyManager();
        TowerManager = new TowerManager();
        // Instantiate object to manage Enemy spawning
        EnemySpawnManager = new EnemySpawnManager(ObjPhotonView);

        // Begin gathering Enemy Spawn data that the EnemySpawnManager will eventually use
        EnemySpawnDataManager = new DataManager<EnemySpawnData>();
        // Grab the Enemy Spawn data from either the web server or local xml file. The EnemySpawnManager will use this
        // data to spawn enemies once it has been loaded into the game
        if (GameDataManager.Instance.DataLocation == "Local")
            EnemySpawnDataManager.LoadDataFromLocal("EnemySpawn/" + CurrentLevelData.EnemySpawnFilename + ".xml");
        else
			StartCoroutine(EnemySpawnDataManager.LoadDataFromServer("http://www.diademstudios.com/outpostdata/EnemySpawn/" + CurrentLevelData.EnemySpawnFilename + ".xml"));

        // Determine if there is a Notification File for this level
        if (CurrentLevelData.NotificationFilename != "")
        {
            LevelNotificationDataManager = new DataManager<NotificationData>();

            // Grab the Level Notification data from either the web server or local xml file. The NotificationManager will use this
            // data to automatically show Notifications once it has been loaded into the game
            if (GameDataManager.Instance.DataLocation == "Local")
                LevelNotificationDataManager.LoadDataFromLocal("Notifications/" + CurrentLevelData.NotificationFilename + ".xml");
            else
				StartCoroutine(LevelNotificationDataManager.LoadDataFromServer("http://www.diademstudios.com/outpostdata/Notifications/" + CurrentLevelData.NotificationFilename + ".xml"));
        }

        GameRunning = false;
        Victory = false;
    }

    // Update is called once per frame
    private void Update()
    {
        // Only check for the end of the game while it is running
        if (GameRunning)
        {
            // TO DO: This should not be checked every update loop. It can probably just be checked every 1-2 seconds
            if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
                EndGameCheck();
        }
    }

    /// <summary>
    /// The game is essentially on hold until StartGame is called. This allows the player to wait for other
    /// clients with slower machines/connects to gather/load all the level data
    /// </summary>
    public void StartGame()
    {
        GameRunning = true;

        // Set the player's initial quadrant
        //PlayerManager.Instance.CurrentQuadrant = CurrentLevelData.StartingQuadrant;
        // Inform the Camera of the new quadrant
        //CameraManager.Instance.SetStartQuadrant(CurrentLevelData.StartingQuadrant);

        // Start Game is called once all data has been loaded. We can now tell the EnemySpawnManager to start
        // spawning enemies based on the previously loaded spawn data.
        StartCoroutine(EnemySpawnManager.SpawnEnemies(EnemySpawnDataManager.DataList));

        // We can now tell the NotificationManager to start showing notifications based on the previously loaded spawn data.
        if (LevelNotificationDataManager != null)
            StartCoroutine(NotificationManager.Instance.DisplayLevelNotifications(LevelNotificationDataManager.DataList));

        // Initializes Analytics
        string[] assetSuperTypes = { "Tower", "Enemy" };
        AnalyticsManager.Instance.InitializePlayerAnalytics(assetSuperTypes);

        // Initialize the mining facility
        ObjMiningFacility = GameObject.FindGameObjectWithTag("Mining Facility").GetComponent<MiningFacility>();
        ObjMiningFacility.InitializeFromLevelData(CurrentLevelData);
        AnalyticsManager.Instance.SetMiningFacilityLocation(ObjMiningFacility.transform.position);
    }

    public void OnLevelWasLoaded(int level)
    {
        // TerrainMesh must be set when the level is started because the HexMesh object is not created
        // until the level loads. All levels MUST begin with a defined prefix for this to work properly
		if(SceneManager.GetActiveScene().name.StartsWith("Level"))
        {
            TerrainMesh = GameObject.FindGameObjectWithTag("Terrain").GetComponent<HexMesh>() as HexMesh;
        }
    }

    /// <summary>
    /// Check to see if all data associated with this level has been loaded.
    /// </summary>
    /// <returns><c>true</c>, if loading level was finisheded, <c>false</c> otherwise.</returns>
    public bool FinishedLoadingLevel()
    {
        bool success = false;

        // Ensure EnemySpawnDataManager has loaded all the data and that the game has a GameID from the server
        if (EnemySpawnDataManager != null)
		{
			if (EnemySpawnDataManager.FinishedLoadingData && GameID != -1)
            {
                LevelStartTime = Time.time; // Used for Analytics to keep track of level's start time
                success = true;
            }
		}

        return success;
    }

    /// <summary>
    /// Checks to see if the end game state has been met and takes action accordingly
    /// </summary>
    private void EndGameCheck()
    {
        // Check to see if all the enemies have spawned and if all enemies are dead
        if (EnemySpawnManager.FinishedSpawning && GameManager.Instance.EnemyManager.ActiveEnemyCount() == 0)
        {
            EndGame_Victory();
        }
        else if (ObjMiningFacility.Health <= 0)
        {
            EndGame_Loss();
        }
    }

    #region RPC CALLS

    /// <summary>
    /// Spawn's Enemy on all client machines.
    /// An PunRPC option is needed in order to set the Enemy's default data AFTER being created
    /// </summary>
    /// <param name="displayName">Display name.</param>
    /// <param name="startAngle">Start angle.</param>
    /// <param name="viewID">View ID.</param>
    [PunRPC]
    private void SpawnEnemyAcrossNetwork(string displayName, int startAngle, int viewID)
    {
        // Look up the enemy's data
        EnemyData data = GameDataManager.Instance.FindEnemyDataByDisplayName(displayName);
        // Calculate the enemy's position
        Vector3 position = TerrainMesh.IntersectPosition(AngleToPosition(startAngle), data.HoverDistance);
        // Instantiate a new Enemy
        GameObject newEnemy = Instantiate(Resources.Load("Enemies/" + GameDataManager.Instance.FindEnemyPrefabNameByDisplayName(displayName)), position, Quaternion.identity) as GameObject;
        // Add a PhotonView to the Enemy
        newEnemy.AddComponent<PhotonView>();
        // Set Enemy's PhotonView to match the Master Client's PhotonView ID for this GameObject (these IDs must match for networking to work)
        newEnemy.GetComponent<PhotonView>().viewID = viewID;

        // Store the enemy in AnalyticsManager
        if (GameRunning)
        {
            AnalyticsManager.Instance.Assets.AddAsset("Enemy", displayName, viewID, position);
        }

        // The Prefab doesn't contain the correct default data. Set the Enemy's default data now
        newEnemy.GetComponent<Enemy>().SetEnemyData(data);
    }

    [PunRPC]
    private void EndGame_VictoryAcrossNetwork()
    {
        Victory = true;
        GameRunning = false;
        MenuManager.Instance.ShowVictoryMenu();

        // Save player progress
		PlayerManager.Instance.SavePlayerGameDataToServer();

        // Saves, sends, and resets all relevant analytics
        AnalyticsManager.Instance.PerformAnalyticsProcess();
    }

    [PunRPC]
    private void EndGame_LossAcrossNetwork()
    {
        Victory = false;
        GameRunning = false;
        MenuManager.Instance.ShowLossMenu();

		/// Save player progress
		PlayerManager.Instance.SavePlayerGameDataToServer();

        // Saves, sends, and resets all relevant analytics
        AnalyticsManager.Instance.PerformAnalyticsProcess();
    }
    #endregion RPC CALLS

    /// <summary>
    /// Calculates the starting position of an enemy based on a given angle
    /// </summary>
    /// <returns>The to position.</returns>
    /// <param name="angle">Angle.</param>
    public Vector3 AngleToPosition(int angle)
    {
        float radians = (Mathf.PI / 180) * angle; // Mathf.Deg2Rad;

        // Note from J.S. 2015-03-29: Shouldn't the sin and cos be reversed here? I think that would put 0 degrees to the north, which seems to be typical.
        return new Vector3(Mathf.Sin(radians), Mathf.Cos(radians), 0) * 25;
    }

    /// <summary>
    /// End the game in a VICTORY
    /// </summary>
    private void EndGame_Victory()
    {
        ObjPhotonView.RPC("EndGame_VictoryAcrossNetwork", PhotonTargets.All, null);
    }

    /// <summary>
    /// End the game in a LOSS
    /// </summary>
    private void EndGame_Loss()
    {
        ObjPhotonView.RPC("EndGame_LossAcrossNetwork", PhotonTargets.All, null);
    }
    #region EVENTS

    private void OnSwitchMaster(PhotonPlayer player)
    {
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
            AnalyticsManager.Instance.SetIsMaster(true);
    }

    #endregion EVENTS
	
    public void AddAvailableQuadrant(Quadrant newQuadrant)
    {
        CurrentLevelData.AvailableQuadrants += ", " + newQuadrant.ToString();
    }

    private void OnDestroy()
    {
        // Remove all references to delegate events that were created for this script
		if(SessionManager.Instance != null)
		{
			SessionManager.Instance.OnSMSwitchMaster -= OnSwitchMaster;
		}
    }

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[GameManager] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[GameManager] " + message);
    }

    #endregion MessageHandling
}