using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
	private static GameManager instance;

	public bool GameRunning = false;
	public GameObject OutpostStation;

	private EnemySpawner Spawner;

	/// <summary>
	/// Persistent Singleton - this object lasts between scenes
	/// </summary>
	/// <value>The instance.</value>
	public static GameManager Instance
	{
		get
		{
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType<GameManager>();
				
				//Tell unity not to destroy this object when loading a new scene!
				DontDestroyOnLoad(instance.gameObject);
			}
			
			return instance;
		}
	}

	// Use this for initialization
	void Start () 
	{
		// Track events in order to react to Session Manager events as they happen
		SessionManager.Instance.OnSMSwitchMaster += OnSwitchMaster;
	
		// Every player has an enemy spawner - but only the master client is responsible for instantiating 
		Spawner = new EnemySpawner();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(GameRunning)
		{
			// When the game is running, the host manages when enemy's spawn
			if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
			{
				if(Spawner != null)
					Spawner.Update();

			}
		}
	}

	public void StartNewGame()
	{
		// Only start the game if the game hasn't been started
		if(!GameRunning)
		{
			GameRunning = true;

			Spawner.Start(SessionManager.Instance.GetPlayerInfo().isMasterClient);
		}
	}

	public void EndGame()
	{
		// Only end the game if one is currently running
		if(GameRunning)
		{
			GameRunning = false;

			Spawner.Stop();
		}
	}

	private void OnSwitchMaster(PhotonPlayer player)
	{
		// Check if the current player has recently become the master client and give them the appropriate controls
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Set the spawner to instantiate enemies
			Spawner.PerformSpawnActions = true;
		}
	}
}
