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

	public int GameID = -1;
    public LevelData CurrentLevelData { get; private set; }
    public DataManager<NotificationData> LevelNotificationDataManager { get; private set; }

    public bool Victory { get; private set; }
    public bool GameRunning { get; private set; }

    public float LevelStartTime { get; private set; }

    // Components
	public HexTerrain TerrainMesh { get; private set; }

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
		SessionManager.Instance.OnSMDisconnected += OnDisconnected_Event;

        // Store LevelData from MenuManager
        CurrentLevelData = MenuManager.Instance.CurrentLevelData;

		ObjPhotonView = PhotonView.Get(this);

        // Instantiate objects to manage Enemies and Towers
        EnemyManager = new EnemyManager();
        TowerManager = new TowerManager();

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

		// Initialize the mining facility
		ObjMiningFacility = GameObject.FindGameObjectWithTag("Mining Facility").GetComponent<MiningFacility>();
		ObjMiningFacility.InitializeFromLevelData(CurrentLevelData);
		AnalyticsManager.Instance.SetMiningFacilityLocation(ObjMiningFacility.transform.position);

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

        // Start Game is called once all data has been loaded. We can now tell the EnemySpawnManager to start
        // spawning enemies based on the previously loaded spawn data.
		EnemySpawnManager.Instance.StartSpawning();

        // We can now tell the NotificationManager to start showing notifications based on the previously loaded spawn data.
        if (LevelNotificationDataManager != null)
            StartCoroutine(NotificationManager.Instance.DisplayLevelNotifications(LevelNotificationDataManager.DataList));


		CameraManager.Instance.GetComponent<Animator>().enabled = false;
        // Initializes Analytics
        string[] assetSuperTypes = { "Tower", "Enemy" };
        AnalyticsManager.Instance.InitializePlayerAnalytics(assetSuperTypes);
    }

    public void OnLevelWasLoaded(int level)
    {
        // TerrainMesh must be set when the level is started because the HexTerrain object is not created
        // until the level loads. All levels MUST begin with a defined prefix for this to work properly
        if (SceneManager.GetActiveScene().name.StartsWith("Level"))
        {
            TerrainMesh = GameObject.FindGameObjectWithTag("Terrain").GetComponent<HexTerrain>() as HexTerrain;
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
        if (EnemySpawnManager.Instance != null && EnemySpawnManager.Instance.EnemySpawnDataManager != null)
		{
			if (EnemySpawnManager.Instance.FinishedLoadingData && GameID != -1)
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
        if (EnemySpawnManager.Instance.FinishedSpawning && GameManager.Instance.EnemyManager.ActiveEnemyCount() == 0)
        {
            EndGame_Victory();
        }
        else if (ObjMiningFacility.Health <= 0)
        {
            EndGame_Loss();
        }
    }

    #region RPC CALLS

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
    /// End the game in a VICTORY
    /// </summary>
    private void EndGame_Victory()
    {
		// Master Client tells server that the game is over
		SendEndGameToServer();

        ObjPhotonView.RPC("EndGame_VictoryAcrossNetwork", PhotonTargets.All, null);
    }

    /// <summary>
    /// End the game in a LOSS
    /// </summary>
    private void EndGame_Loss()
    {
		// Master Client tells server that the game is over
		SendEndGameToServer();

        ObjPhotonView.RPC("EndGame_LossAcrossNetwork", PhotonTargets.All, null);
    }

	/// <summary>
	/// Used when the player manually chooses to quit
	/// </summary>
	/// <param name="applicationQuit">If set to <c>true</c> entire application is being quit.</param>
	public void EndGame_Quit(bool applicationQuit = false)
	{
		Log("The player has quit in the middle of the game.");

		// Save the player's data to the server without waiting for a response from the server. 
		PlayerManager.Instance.SavePlayerGameDataToServer(false);
		// If the quitting player is the last player left in the game, close out the game on the server
		if(SessionManager.Instance.GetOtherPlayersInRoom().Length == 0)
			SendEndGameToServer();

		Victory = false;
		GameRunning = false;

		// Go to the main menu if the player chooses to quit
		if(!applicationQuit)
		{
			// Leave the multiplayer room
			SessionManager.Instance.LeaveRoom();
			// Go back to the Main Menu
			MenuManager.Instance.ReturnToMainMenu();
		}
	}

	/// <summary>
	/// Send message to server that this game is over
	/// </summary>
	private void SendEndGameToServer()
	{
		// Master Client will save overall game data to server
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			Log("Informing server that the game has ended");
			WWWForm form = new WWWForm();
			// When creating a game, track who created it and what level they chose for the game
			form.AddField("gameID", GameManager.Instance.GameID.ToString());
			form.AddField("victory", GameManager.Instance.Victory.ToString());
			WWW www = new WWW("http://www.diademstudios.com/outpostdata/Action_EndGame.php", form);
		}
	}

    #region EVENTS

    private void OnSwitchMaster(PhotonPlayer player)
    {
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
            AnalyticsManager.Instance.SetIsMaster(true);
    }

	/// <summary>
	/// Called when the current game scene ends
	/// --Called after OnApplicationQuit and OnDisconnected
	/// </summary>
	private void OnDestroy()
	{
		// Remove all references to delegate events that were created for this script
		if(SessionManager.Instance != null)
		{
			SessionManager.Instance.OnSMSwitchMaster -= OnSwitchMaster;
			SessionManager.Instance.OnSMDisconnected -= OnDisconnected_Event;
		}
	}

	/// <summary>
	/// Called before the application quits
	/// --Called before OnDisconnected and OnDestroy
	/// </summary>
	private void OnApplicationQuit()
	{
		EndGame_Quit(true);
	}

	/// <summary>
	/// Called when the game disconnects from the realtime server.
	/// --Called after OnApplicationQuit and before OnDestroy
	/// </summary>
	private void OnDisconnected_Event()
	{
		//TODO: Handle unexpectedly being disconnected from the realtime network
	}

    #endregion
	
    public void AddAvailableQuadrant(Quadrant newQuadrant)
    {
        CurrentLevelData.AvailableQuadrants += ", " + newQuadrant.ToString();
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