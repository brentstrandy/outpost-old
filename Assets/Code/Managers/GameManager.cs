using UnityEngine;
using System.Collections;

/// <summary>
/// Manages all high-level game functions. Is responsible for starting and stopping Levels
/// Owner: Brent Strandy
/// </summary>
public class GameManager : MonoBehaviour
{
	private static GameManager instance;
	
	public bool GameRunning = false;
	public GameObject OutpostStation;

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
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(GameRunning)
		{
			EndGameCheck();
		}
	}

	private bool EndGameCheck()
	{
		bool endGame = false;

		// Check to see if all the enemies have spawned and if all enemies are dead
		if(EnemySpawnManager.Instance.FinishedSpawning)
			endGame = true;

		return endGame;
	}

	private void OnSwitchMaster(PhotonPlayer player)
	{

	}

	private void OnPlayerLeft(PhotonPlayer player)
	{
		// Respond appropriately to a player leaving
		if(GameRunning)
		{

		}
	}
}
