using UnityEngine;
using System.Collections;

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
	
	public LevelData CurrentLevelData { get; private set; }
	public DataManager<EnemySpawnData> EnemySpawnDataManager { get; private set; }
	public DataManager<NotificationData> LevelNotificationDataManager { get; private set; }
	
	public bool Victory { get; private set; }
	public bool GameRunning { get; private set; }

    public float LevelStartTime { get; private set; }

	// Components
	public HexMesh TerrainMesh;
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
	
	void Awake()
	{
		instance = this;
		PhotonSerializer.Register();
	}
	#endregion
	
	// Use this for initialization
	void Start ()
	{
		// Track events in order to react to Session Manager events as they happen
		SessionManager.Instance.OnSMSwitchMaster += OnSwitchMaster;
		SessionManager.Instance.OnSMPlayerLeftRoom += OnPlayerLeft;
		InputManager.Instance.OnQuadrantRotate += OnCameraQuadrantChanged;
		
		// Store LevelData from MenuManager
		CurrentLevelData = MenuManager.Instance.CurrentLevelData;
		// Store a reference to the PhotonView
		ObjPhotonView = PhotonView.Get(this);

		// Instantiate objects to manage Enemies and Towers
		EnemyManager = new EnemyManager();
		TowerManager = new TowerManager();
		// Instantiate object to manage Enemy spawning
		EnemySpawnManager = new EnemySpawnManager(ObjPhotonView);

		// Initialize the mining facility
		ObjMiningFacility = GameObject.FindGameObjectWithTag("Mining Facility").GetComponent<MiningFacility>();
		ObjMiningFacility.InitializeFromLevelData(CurrentLevelData);

		// Begin gathering Enemy Spawn data that the EnemySpawnManager will eventually use
		EnemySpawnDataManager = new DataManager<EnemySpawnData>();
		// Grab the Enemy Spawn data from either the web server or local xml file. The EnemySpawnManager will use this
		// data to spawn enemies once it has been loaded into the game
		if(GameDataManager.Instance.DataLocation == "Local")
			EnemySpawnDataManager.LoadDataFromLocal("EnemySpawn/" + CurrentLevelData.EnemySpawnFilename + ".xml");
		else
			StartCoroutine(EnemySpawnDataManager.LoadDataFromServer("EnemySpawn/" + CurrentLevelData.EnemySpawnFilename + ".xml"));

		// Determine if there is a Notification File for this level
		if(CurrentLevelData.NotificationFilename != "")
		{
			LevelNotificationDataManager = new DataManager<NotificationData>();

			// Grab the Level Notification data from either the web server or local xml file. The NotificationManager will use this
			// data to automatically show Notifications once it has been loaded into the game
			if(GameDataManager.Instance.DataLocation == "Local")
				LevelNotificationDataManager.LoadDataFromLocal("Notifications/" + CurrentLevelData.NotificationFilename + ".xml");
			else
				StartCoroutine(LevelNotificationDataManager.LoadDataFromServer("Notifications/" + CurrentLevelData.NotificationFilename + ".xml"));
		}

		GameRunning = false;
		Victory = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Only check for the end of the game while it is running
		if(GameRunning)
		{
			// TO DO: This should not be checked every update loop. It can probably just be checked every 1-2 seconds
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

        LevelStartTime = Time.time;//Used for Analytics -- keep track of level's start time

		// Set the player's initial quadrant
		PlayerManager.Instance.CurrentQuadrant = CurrentLevelData.StartingQuadrant;
		// Inform the Camera of the new quadrant
		CameraManager.Instance.SetStartQuadrant(CurrentLevelData.StartingQuadrant);
		
		// Start Game is called once all data has been loaded. We can now tell the EnemySpawnManager to start
		// spawning enemies based on the previously loaded spawn data.
		StartCoroutine(EnemySpawnManager.SpawnEnemies(EnemySpawnDataManager.DataList));
		
		// We can now tell the NotificationManager to start showing notifications based on the previously loaded spawn data.
		if(LevelNotificationDataManager != null)
			StartCoroutine(NotificationManager.Instance.DisplayLevelNotifications(LevelNotificationDataManager.DataList));
	}
	
	public void OnLevelWasLoaded(int level)
	{
		// TerrainMesh must be set when the level is started because the HexMesh object is not created
		// until the level loads. All levels MUST begin with a defined prefix for this to work properly
		if(Application.loadedLevelName.StartsWith("Level"))
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

		// At the moment the only data that needs to be checked is the EnemySpawnManager
		// Add additional checks for loaded data here if necessary
		if(EnemySpawnDataManager != null)
			if(EnemySpawnDataManager.FinishedLoadingData)
				success = true;
		
		return success;
	}
	
	/// <summary>
	/// Checks to see if the end game state has been met and takes action accordingly
	/// </summary>
	private void EndGameCheck()
	{
		// Check to see if all the enemies have spawned and if all enemies are dead
		if(EnemySpawnManager.FinishedSpawning && GameManager.Instance.EnemyManager.ActiveEnemyCount() == 0)
			EndGame_Victory();
		else if(ObjMiningFacility.Health <= 0)
			EndGame_Loss();
	}
	
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
		// Instantiate a new Enemy
		GameObject newEnemy = Instantiate(Resources.Load("Enemies/" + GameDataManager.Instance.FindEnemyPrefabNameByDisplayName(displayName)), AngleToPosition(startAngle), Quaternion.identity) as GameObject;
		// Add a PhotonView to the Enemy
		newEnemy.AddComponent<PhotonView>();
		// Set Enemy's PhotonView to match the Master Client's PhotonView ID for this GameObject (these IDs must match for networking to work)
		newEnemy.GetComponent<PhotonView>().viewID = viewID;
		// The Prefab doesn't contain the correct default data. Set the Enemy's default data now
		newEnemy.GetComponent<Enemy>().SetEnemyData(GameDataManager.Instance.FindEnemyDataByDisplayName(displayName));
	}
	
	/// <summary>
	/// Calculates the starting position of an enemy based on a given angle
	/// </summary>
	/// <returns>The to position.</returns>
	/// <param name="angle">Angle.</param>
	private Vector3 AngleToPosition(int angle)
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
		Victory = true;
		GameRunning = false;
		MenuManager.Instance.ShowVictoryMenu();

		// Save player progress (won level)
		PlayerManager.Instance.SaveLevelProgress(CurrentLevelData.DisplayName, true, 10);

        PlayerAnalytics.Instance.GameLength = Time.time - LevelStartTime;
        PlayerAnalytics.Instance.LastLevelReached = CurrentLevelData.LevelID;
        // DO NOT call SendAnalytics() until this message is gone. It will flood our analytics with unnecessary and unremovable data =(. Gracias!
        //SendAnalytics()
        //ResetLevelAnalytics();
	}
	
	/// <summary>
	/// End the game in a LOSS
	/// </summary>
	private void EndGame_Loss()
	{
		Victory = false;
		GameRunning = false;
		MenuManager.Instance.ShowLossMenu();

		// Save player progress (lost level)
		PlayerManager.Instance.SaveLevelProgress(CurrentLevelData.DisplayName, false, 10);

        // DO NOT call SendAnalytics() until this message is gone. It will flood our analytics with unnecessary and unremovable data =(. Gracias!
        //SendAnalytics();
        //ResetLevelAnalytics();
	}
	
    // DO NOT activate until this message is gone. It will flood our analytics with unnecessary and unremovable data =(. Gracias!
    private void SendAnalytics()
    {
        //PlayerAnalytics.Instance.SendLevelStats();
        //PlayerAnalytics.Instance.ResetLevelStats();
    }

    // Reset the PlayerAnalytics relevant to the ending of a game
    private void ResetLevelAnalytics()
    {
        PlayerAnalytics.Instance.ResetLevelStats();
    }

	#region EVENTS
	private void OnSwitchMaster(PhotonPlayer player)
	{
		// TODO -- Analytics -- indicate who the master client is (they will send the global data for the game)
	}
	
	private void OnPlayerLeft(PhotonPlayer player)
	{
        PlayerAnalytics.Instance.PlayerCountChanged = true;
	}
	
	private void OnCameraQuadrantChanged(string direction)
	{
		Quadrant newQuadrant = Quadrant.North;
		
		if(direction == "left")
			newQuadrant = GetNextCounterClockwiseQuadrant();
		else if(direction == "right")
			newQuadrant = GetNextClockwiseQuadrant();
		
		// Inform the Player of the new quadrant
		PlayerManager.Instance.CurrentQuadrant = newQuadrant;
		// Inform the Camera of the new quadrant
		CameraManager.Instance.UpdateCameraQuadrant(newQuadrant);
	}
	#endregion

	public void AddAvailableQuadrant(Quadrant newQuadrant)
	{
		CurrentLevelData.AvailableQuadrants += ", " + newQuadrant.ToString();
	}

	private Quadrant GetNextClockwiseQuadrant()
	{
		Quadrant quadrant = PlayerManager.Instance.CurrentQuadrant;
		
		if(PlayerManager.Instance.CurrentQuadrant == Quadrant.North && CurrentLevelData.AvailableQuadrants.Contains("East"))
			quadrant = Quadrant.East;
		else if(PlayerManager.Instance.CurrentQuadrant == Quadrant.East && CurrentLevelData.AvailableQuadrants.Contains("South"))
			quadrant = Quadrant.South;
		else if(PlayerManager.Instance.CurrentQuadrant == Quadrant.South && CurrentLevelData.AvailableQuadrants.Contains("West"))
			quadrant = Quadrant.West;
		else if(PlayerManager.Instance.CurrentQuadrant == Quadrant.West && CurrentLevelData.AvailableQuadrants.Contains("North"))
			quadrant = Quadrant.North;
		
		return quadrant;
	}
	
	private Quadrant GetNextCounterClockwiseQuadrant()
	{
		Quadrant quadrant = PlayerManager.Instance.CurrentQuadrant;
		
		if(PlayerManager.Instance.CurrentQuadrant == Quadrant.North && CurrentLevelData.AvailableQuadrants.Contains("West"))
			quadrant = Quadrant.West;
		else if(PlayerManager.Instance.CurrentQuadrant == Quadrant.East && CurrentLevelData.AvailableQuadrants.Contains("North"))
			quadrant = Quadrant.North;
		else if(PlayerManager.Instance.CurrentQuadrant == Quadrant.South && CurrentLevelData.AvailableQuadrants.Contains("East"))
			quadrant = Quadrant.East;
		else if(PlayerManager.Instance.CurrentQuadrant == Quadrant.West && CurrentLevelData.AvailableQuadrants.Contains("South"))
			quadrant = Quadrant.South;
		
		return quadrant;
	}
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[GameManager] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[GameManager] " + message);
	}
	#endregion
}