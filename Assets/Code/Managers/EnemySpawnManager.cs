using UnityEngine;
using System;
using System.Collections;

public class EnemySpawnManager : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	public string LevelName;
	public bool FisnishedSpawning { get; private set; }
	private EnemySpawnDataHandler SpawnActionHandler;
	private float StartTime;

	public void Start()
	{
		SpawnActionHandler = new EnemySpawnDataHandler();
        LevelName = Application.loadedLevelName;
		FisnishedSpawning = false;
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
            	SessionManager.Instance.InstantiateObject("Enemies/" + spawnDetails.EnemyName, AngleToPosition(spawnDetails.StartAngle), Quaternion.identity);
        }
        catch (Exception e)
        {
            Debug.LogError("Exception: " + e);
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
        
		return new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians)) * 10;
	}
					
	IEnumerator SpawnEnemies()
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
		FisnishedSpawning = true;
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
