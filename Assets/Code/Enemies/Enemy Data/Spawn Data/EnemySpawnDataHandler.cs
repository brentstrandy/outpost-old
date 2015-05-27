using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class EnemySpawnDataHandler
{
	public bool ShowDebugLogs = false;

	private List<EnemySpawnData> SpawnDataList;
    public EnemySpawnDataContainer SpawnDataContainer_Inspector;


	public EnemySpawnDataHandler() 
    {
		SpawnDataList = new List<EnemySpawnData>();
        SpawnDataContainer_Inspector = GameObject.Find("Enemy Spawn Manager").GetComponent<EnemySpawnDataContainer>();
    }

	public void AddSpawnData(EnemySpawnData spawnData)
	{
		SpawnDataContainer_Inspector.EnemySpawnDataList.Add(spawnData);
		SpawnDataList.Add(spawnData);
	}

	public float GetNextStartTime()
	{
		float startTime = -1;

		if(SpawnDataList.Count() > 0)
			startTime = SpawnDataList[0].StartTime;

		return startTime;
	}

	public bool SpawnListEmpty()
	{
		return SpawnDataList.Count == 0;
	}

    public int SpawnListSize()
    {
        return SpawnDataList.Count;
    }

	public EnemySpawnData SpawnNext()
	{
		EnemySpawnData action = null;

		if(SpawnDataList.Count() > 0)
		{
			action = SpawnDataList[0];
			SpawnDataList.RemoveAt(0);
		}

		return action;
	}

	public void SortListsByStartTime()
	{
        if (SpawnDataList != null)
        {
            SpawnDataList = SpawnDataList.OrderBy(o => o.StartTime).ToList();
            SpawnDataContainer_Inspector.EnemySpawnDataList = SpawnDataContainer_Inspector.EnemySpawnDataList.OrderBy(o => o.StartTime).ToList();
        }    
    }
  
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[SpawnActionsHandler] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[SpawnActionsHandler] " + message);
	}
	#endregion
}
