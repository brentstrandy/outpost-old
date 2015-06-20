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

	public bool Victory { get; private set; }
	public bool GameRunning { get; private set; }

	// Components
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
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType<GameManager>();
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

		// Begin gathering Enemy Spawn data that the EnemySpawnManager will eventually use
		EnemySpawnDataManager = new DataManager<EnemySpawnData>();
		// Grab the Enemy Spawn data from either the web server or local xml file. The EnemySpawnManager will use this
		// data to spawn enemies once it has been loaded into the game
		if(GameDataManager.Instance.DataLocation == "Local")
			EnemySpawnDataManager.LoadDataFromLocal(CurrentLevelData.EnemySpawnFilename + ".xml");
		else
			StartCoroutine(EnemySpawnDataManager.LoadDataFromServer(CurrentLevelData.EnemySpawnFilename + ".xml"));

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
		else
		{
			// Instantiate object to manage Enemy spawning
			EnemySpawnManager = new EnemySpawnManager(ObjPhotonView);
		}
	}

	/// <summary>
	/// The game is essentially on hold until StartGame is called. This allows the player to wait for other
	/// clients with slower machines/connects to gather/load all the level data
	/// </summary>
	public void StartGame()
	{
		GameRunning = true;

		// Initialize the mining facility's properties based on the level data
		ObjMiningFacility.InitializeFromLevelData(CurrentLevelData);

		// Set the player's initial quadrant
		Player.Instance.CurrentQuadrant = CurrentLevelData.StartingQuadrant;
		// Inform the Camera of the new quadrant
		CameraManager.Instance.SetStartQuadrant(CurrentLevelData.StartingQuadrant);

		// Start Game is called once all data has been loaded. We can now tell the EnemySpawnManager to start 
		// spawning enemies based on the previously loaded spawn data.
		StartCoroutine(EnemySpawnManager.SpawnEnemies(EnemySpawnDataManager.DataList));
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
	}

	/// <summary>
	/// End the game in a LOSS
	/// </summary>
	private void EndGame_Loss()
	{
		Victory = false;
		GameRunning = false;
		MenuManager.Instance.ShowLossMenu();
	}
	
	private void OnSwitchMaster(PhotonPlayer player)
	{
		
	}
	
	private void OnPlayerLeft(PhotonPlayer player)
	{
		
	}
	
	private void OnCameraQuadrantChanged(string direction)
	{
		Quadrant newQuadrant = Quadrant.North;
		
		if(direction == "left")
			newQuadrant = GetNextCounterClockwiseQuadrant();
		else if(direction == "right")
			newQuadrant = GetNextClockwiseQuadrant();
		
		// Inform the Player of the new quadrant
		Player.Instance.CurrentQuadrant = newQuadrant;
		// Inform the Camera of the new quadrant
		CameraManager.Instance.UpdateCameraQuadrant(newQuadrant);
	}
	
	private Quadrant GetNextClockwiseQuadrant()
	{
		Quadrant quadrant = Player.Instance.CurrentQuadrant;
		
		if(Player.Instance.CurrentQuadrant == Quadrant.North && CurrentLevelData.AvailableQuadrants.Contains("East"))
			quadrant = Quadrant.East;
		else if(Player.Instance.CurrentQuadrant == Quadrant.East && CurrentLevelData.AvailableQuadrants.Contains("South"))
			quadrant = Quadrant.South;
		else if(Player.Instance.CurrentQuadrant == Quadrant.South && CurrentLevelData.AvailableQuadrants.Contains("West"))
			quadrant = Quadrant.West;
		else if(Player.Instance.CurrentQuadrant == Quadrant.West && CurrentLevelData.AvailableQuadrants.Contains("North"))
			quadrant = Quadrant.North;

		return quadrant;
	}
	
	private Quadrant GetNextCounterClockwiseQuadrant()
	{
		Quadrant quadrant = Player.Instance.CurrentQuadrant;
		
		if(Player.Instance.CurrentQuadrant == Quadrant.North && CurrentLevelData.AvailableQuadrants.Contains("West"))
				quadrant = Quadrant.West;
		else if(Player.Instance.CurrentQuadrant == Quadrant.East && CurrentLevelData.AvailableQuadrants.Contains("North"))
				quadrant = Quadrant.North;
		else if(Player.Instance.CurrentQuadrant == Quadrant.South && CurrentLevelData.AvailableQuadrants.Contains("East"))
				quadrant = Quadrant.East;	
		else if(Player.Instance.CurrentQuadrant == Quadrant.West && CurrentLevelData.AvailableQuadrants.Contains("South"))
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