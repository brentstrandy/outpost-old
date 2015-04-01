using UnityEngine;
using System;
using System.Collections;

public class EnemySpawnManager : MonoBehaviour
{
	public bool ShowDebugLogs = true;

	public bool FinishedSpawning { get; private set; }

	private static EnemySpawnManager instance;
	private EnemySpawnDataHandler SpawnActionHandler;
	private string LevelName;
	private float StartTime;

	#region INSTANCE (SINGLETON)
	/// <summary>
	/// Singleton - There can only be one
	/// </summary>
	/// <value>The instance.</value>
	public static EnemySpawnManager Instance
	{
		get
		{
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType<EnemySpawnManager>();
			}
			
			return instance;
		}
	}

	void Awake()
	{
		instance = this;
	}
	#endregion

	public void Start()
	{
		SpawnActionHandler = new EnemySpawnDataHandler();
        LevelName = Application.loadedLevelName;
		FinishedSpawning = false;

		LoadSpawnData();

		StartTime = Time.time;
		StartCoroutine("SpawnEnemies");
	}

	// Update is called once per frame
	public void Update ()
	{

	}

	/// <summary>
	/// Loads the spawn data from an XML file
	/// </summary>
	private void LoadSpawnData()
    {
		SpawnActionHandler.LoadActions(LevelName);
    }

	public void Stop()
	{
		StartTime = -1;
		StopCoroutine("SpawnEnemies");
	}

    private void SpawnEnemy(EnemySpawnData spawnDetails)
    {
        try
        {
			// Only spawn an enemy if you are the Master Client. Otherwise, the Master client will tell all clients to spawn an enemy
			if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
			{
				// Only spawn the enemy if there are the appropriate number of co-op players
				if(SessionManager.Instance.GetRoomPlayerCount() >= spawnDetails.PlayerCount)
					SessionManager.Instance.InstantiateObject("Enemies/" + spawnDetails.EnemyName, AngleToPosition(spawnDetails.StartAngle), Quaternion.identity);
			}
        }
        catch (Exception e)
        {
            Debug.LogError("Error Instantiating Enemy: " + e.Message);
        }
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
		return new Vector3(Mathf.Sin(radians), Mathf.Cos(radians), 0) * 30;
	}
					
	private IEnumerator SpawnEnemies()
	{
		// Loop until there are no enemies left to spawn
		while(!SpawnActionHandler.SpawnListEmpty())
		{
			// Check to see if the next enemy's spawn time has passed. If so, spawn the enemy
			if(SpawnActionHandler.GetNextStartTime() <= (Time.time - this.StartTime))
				SpawnEnemy(SpawnActionHandler.SpawnNext());

			// TO DO: Tell the coroutine to run when the next available enemy is ready
			// SpawnActionHandler.GetNextStartTime() - Time.time
			yield return 0;
		}

		// Spawning is complete
		FinishedSpawning = true;
	}
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[EnemySpawnManager] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[EnemySpawnManager] " + message);
	}
	#endregion
}
