using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemySpawnManager
{
	public bool ShowDebugLogs = true;
	
	public bool FinishedSpawning { get; private set; }
	
	//private EnemySpawnDataHandler SpawnDataHandler;
	private List<EnemySpawnData> SpawnDataList;
	private string EnemySpawnFilename;
	private float StartTime;
	private PhotonView ObjPhotonView;

	public EnemySpawnManager(PhotonView photonView)
	{
		SortListByStartTime();

		FinishedSpawning = false;
	
		ObjPhotonView = photonView;
	}

	/// <summary>
	/// Validate that an enemy can be spawned and if so, spawn it across the network
	/// </summary>
	/// <param name="spawnDetails">Enemy Spawn Data</param>
    private void SpawnEnemy(EnemySpawnData spawnDetails)
    {
		// Only spawn an enemy if you are the Master Client. Otherwise, the Master client will tell all clients to spawn an enemy
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Only spawn the enemy if there are the appropriate number of co-op players
			if(SessionManager.Instance.GetRoomPlayerCount() >= spawnDetails.PlayerCount)
			{
				// Tell all other players that an Enemy has spawned (SpawnEnemyAcrossNetwork is currently in GameManager.cs)
				ObjPhotonView.RPC("SpawnEnemyAcrossNetwork", PhotonTargets.All, spawnDetails.EnemyName, spawnDetails.StartAngle, SessionManager.Instance.AllocateNewViewID());
			}
		}
    }

	/// <summary>
	/// Coroutine that handles the spawning of all enemies.
	/// </summary>
	/// <returns>Nothing</returns>
	public IEnumerator SpawnEnemies(List<EnemySpawnData> spawnDataList)
	{
		SpawnDataList = spawnDataList;

		// Remember when spawning started in order to spawn future enemies at the right time
		StartTime = Time.time;

		// Loop until there are no enemies left to spawn
		while(SpawnDataList.Count != 0)
		{
			// Check to see if the next enemy's spawn time has passed. If so, spawn the enemy
			if(GetNextSpawnTime() <= (Time.time - this.StartTime))
				SpawnEnemy(SpawnNext());

			// TO DO: Tell the coroutine to run when the next available enemy is ready
			// SpawnActionHandler.GetNextStartTime() - Time.time
			yield return 0;
		}

		StartTime = -1;

		// Spawning is complete
		FinishedSpawning = true;
	}

	public float GetNextSpawnTime()
	{
		float startTime = -1;
		
		if(SpawnDataList.Count > 0)
			startTime = SpawnDataList[0].StartTime;
		
		return startTime;
	}
	
	private EnemySpawnData SpawnNext()
	{
		EnemySpawnData action = null;
		
		if(SpawnDataList.Count > 0)
		{
			action = SpawnDataList[0];
			SpawnDataList.RemoveAt(0);
		}
		
		return action;
	}
	
	private void SortListByStartTime()
	{
		if (SpawnDataList != null)
			SpawnDataList = SpawnDataList.OrderBy(o => o.StartTime).ToList();
	}
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[EnemySpawnManager] " + message);
	}
	
	protected void LogError(string message)
	{
		Debug.LogError("[EnemySpawnManager] " + message);
	}
	#endregion
}
