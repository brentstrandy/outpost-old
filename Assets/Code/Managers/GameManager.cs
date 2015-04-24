using UnityEngine;
using System.Collections;

/// <summary>
/// Manages single game levels. This manager is only persistent within a single game instance. It will be destroyed
/// and recreated whenever a new level is loaded
/// Created By: Brent Strandy
/// </summary>
public class GameManager : MonoBehaviour
{
	private static GameManager instance;
	public bool ShowDebugLogs = true;
	public bool Victory { get; private set; }
	public bool GameRunning { get; private set; }
	
	public MiningFacility MiningFacilityObject;

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
		//InGameCanvas = GameObject.Find ("InGame Canvas") as Canvas;

		GameRunning = true;
		Victory = false;

		Player.Instance.CurrentQuadrant = Quadrant.North;
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

	private void EndGameCheck()
	{
		// Check to see if all the enemies have spawned and if all enemies are dead
		if(EnemySpawnManager.Instance.FinishedSpawning && EnemyManager.Instance.ActiveEnemyCount() == 0)
			EndGame_Victory();
		else if(MiningFacilityObject.Health <= 0)
			EndGame_Loss();
	}

	private void EndGame_Victory()
	{
		Victory = true;
		GameRunning = false;
		MenuManager.Instance.ShowVictoryMenu();
	}

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
		Quadrant quadrant = Quadrant.North;

		if(Player.Instance.CurrentQuadrant == Quadrant.North)
			quadrant = Quadrant.East;
		else if(Player.Instance.CurrentQuadrant == Quadrant.East)
			quadrant = Quadrant.South;
		else if(Player.Instance.CurrentQuadrant == Quadrant.South)
			quadrant = Quadrant.West;
		else if(Player.Instance.CurrentQuadrant == Quadrant.West)
			quadrant = Quadrant.North;

		return quadrant;
	}

	private Quadrant GetNextCounterClockwiseQuadrant()
	{
		Quadrant quadrant = Quadrant.North;
		
		if(Player.Instance.CurrentQuadrant == Quadrant.North)
			quadrant = Quadrant.West;
		else if(Player.Instance.CurrentQuadrant == Quadrant.East)
			quadrant = Quadrant.North;
		else if(Player.Instance.CurrentQuadrant == Quadrant.South)
			quadrant = Quadrant.East;
		else if(Player.Instance.CurrentQuadrant == Quadrant.West)
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
