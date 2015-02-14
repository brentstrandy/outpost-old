using UnityEngine;
using System.Collections;

public class EnemySpawner
{
	public bool ShowDebugLogs = true;
	public bool PerformSpawnActions = false;
	private float StartTime;

	private float LastSpawnTime;

	public EnemySpawner()
	{
		StartTime = -1;
	}

	// Update is called once per frame
	public void Update ()
	{
		// Only Update if the spawner has started
		if(StartTime > -1)
		{
			// Initialize enemies only if responsible for performing the action, otherwise do nothing
			if(PerformSpawnActions)
			{
				// TO DO: Spawn enemies at different points in time
				if(Time.time - LastSpawnTime > 2)
				{
					LastSpawnTime = Time.time;
					SpawnEnemy();
				}
			}
		}
	}

	/// <summary>
	/// Loads the spawn data from an XML file
	/// </summary>
	private void LoadSpawnData()
	{
		// TO DO: Load data from XML file
	}

	/// <summary>
	/// Starts the Enemy Spawner by loading the spawning details
	/// </summary>
	/// <param name="performSpawnActions">If set to <c>true</c> this spawner can instantiate enemies.</param>
	public void Start(bool performSpawnActions)
	{
		LoadSpawnData();
		StartTime = Time.time;
		LastSpawnTime = StartTime;
		PerformSpawnActions = performSpawnActions;
	}

	public void Stop()
	{
		PerformSpawnActions = false;
		StartTime = -1;
	}

	private void SpawnEnemy()
	{
		SessionManager.Instance.InstantiateObject("Light Speeder", new Vector3(Random.Range(2.0f, 14.0f), 0, Random.Range(2.0f, 9.0f)), Quaternion.identity);
	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[EnemySpawner] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[EnemySpawner] " + message);
	}
	#endregion
}
