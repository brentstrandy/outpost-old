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
	public TowerDataManager TowerDataMngr { get; private set; }

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
	#endregion

	// Use this for initialization
	void Start () 
	{
		// Track events in order to react to Session Manager events as they happen
		SessionManager.Instance.OnSMSwitchMaster += OnSwitchMaster;
		SessionManager.Instance.OnSMPlayerLeftRoom += OnPlayerLeft;

		// Load all tower data for the game
		TowerDataMngr = new TowerDataManager();
	}
	
	// Update is called once per frame
	void Update () 
	{


	}

	/// <summary>
	/// Loads specified Level
	/// </summary>
	/// <param name="level">Level Name</param>
	public void LoadLevel(string level)
	{
		// TO DO: Load a new scene
		Application.LoadLevel("Level1");
	}

	public void StartNewGame()
	{
		// Only start the game if the game hasn't been started
		if(!GameRunning)
		{
			GameRunning = true;
		}
	}

	public void EndGame()
	{
		// Only end the game if one is currently running
		if(GameRunning)
		{
			GameRunning = false;
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		// Save a reference to the Mining Outpost for GameObjects to use later
		OutpostStation = GameObject.FindWithTag("OutpostStation");

		StartNewGame();
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
