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
	//private PhotonView ObjPhotonView;
	
	public LevelData CurrentLevelData { get; private set; }

	public bool Victory { get; private set; }
	public bool GameRunning { get; private set; }
	
	public MiningFacility ObjMiningFacility;
	
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

		// Initialize the mining facility's properties based on the level data
		ObjMiningFacility.InitializeFromLevelData(CurrentLevelData);

		// Set the player's initial quadrant
		Player.Instance.CurrentQuadrant = CurrentLevelData.StartingQuadrant;
		// Inform the Camera of the new quadrant
		CameraManager.Instance.SetStartQuadrant(CurrentLevelData.StartingQuadrant);

		// Inform all necessary managers that this game has started
		EnemySpawnManager.Instance.StartSpawning();
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
		if(EnemySpawnManager.Instance.FinishedLoadingData)
			success = true;

		return success;
	}

	/// <summary>
	/// Checks to see if the end game state has been met and takes action accordingly
	/// </summary>
	private void EndGameCheck()
	{
		// Check to see if all the enemies have spawned and if all enemies are dead
		if(EnemySpawnManager.Instance.FinishedSpawning && EnemyManager.Instance.ActiveEnemyCount() == 0)
			EndGame_Victory();
		else if(ObjMiningFacility.Health <= 0)
			EndGame_Loss();
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