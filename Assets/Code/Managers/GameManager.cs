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
	private bool GameRunning;


	public GameObject MiningFacility;

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

		GameRunning = true;
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

	private void EndGameCheck()
	{
		// Check to see if all the enemies have spawned and if all enemies are dead
		if(EnemySpawnManager.Instance.FinishedSpawning && EnemyManager.Instance.ActiveEnemyCount() == 0)
			EndGame_Victory();
		else if(MiningFacility.GetComponent<OutpostStation>().Health <= 0)
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
