using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class EnemySpawnActionHandler
{
	public bool ShowDebugLogs = true;

	private List<EnemySpawnAction> SpawnActionList;

	public EnemySpawnActionHandler() 
    {
		SpawnActionList = new List<EnemySpawnAction>();
    }

    public void LoadActions(string filename)
    {
		string enemySpawnXMLPath = Application.streamingAssetsPath + "/" + filename + ".xml";

		// Determine if the file exists
		if (File.Exists(enemySpawnXMLPath))
		{
			foreach (EnemySpawnAction action in XMLParser<EnemySpawnAction>.XMLDeserializer_List(enemySpawnXMLPath))
				SpawnActionList.Add(action);

			// Sort list by time to make sure actions are executed in order
			this.SortListByStartTime();

			Log("Loaded Enemy Spawn XML data");
		}
		else
			LogError("Cannot find Enemy Spawn XML file for level");
    }

	public float GetNextStartTime()
	{
		float startTime = -1;

		if(SpawnActionList.Count() > 0)
			startTime = SpawnActionList[0].StartTime;

		return startTime;
	}

	public bool SpawnListEmpty()
	{
		return SpawnActionList.Count == 0;
	}

	public EnemySpawnAction SpawnNext()
	{
		EnemySpawnAction action = null;

		if(SpawnActionList.Count() > 0)
		{
			action = SpawnActionList[0];
			SpawnActionList.RemoveAt(0);
		}

		return action;
	}

	public void SortListByStartTime()
	{
		if(SpawnActionList != null)
			SpawnActionList = SpawnActionList.OrderBy(o=>o.StartTime).ToList();
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
