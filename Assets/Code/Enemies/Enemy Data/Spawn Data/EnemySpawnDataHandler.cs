using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class EnemySpawnDataHandler
{
	public bool ShowDebugLogs = true;

	private List<EnemySpawnData> SpawnActionList;
    private EnemySpawnDataContainer SpawnActionContainer_Inspector;


	public EnemySpawnDataHandler() 
    {
		SpawnActionList = new List<EnemySpawnData>();
        SpawnActionContainer_Inspector = GameObject.Find("Enemy Spawn Manager").GetComponent<EnemySpawnDataContainer>();
    }

    public void LoadActions(string fileName)
    {
        // location of level specific XML spawn data
		string enemySpawnXMLPath = Application.streamingAssetsPath + "/" + fileName + ".xml";

        // Determine if the file exists
        if (File.Exists(enemySpawnXMLPath))
        {
            // FITZGERALD: To Do
            // Make it so Justin doesn't have to restart the scene for the info to be loaded in (ask Brent about how this can work w/Photon)

            // deserialize XML and add each enemy spawn to the lists
            foreach (EnemySpawnData enemy in XMLParser<EnemySpawnData>.XMLDeserializer_List(enemySpawnXMLPath))
            {
                SpawnActionContainer_Inspector.SpawnDataList.Add(enemy);
                SpawnActionList.Add(enemy);
            }

            // Sort list by time to make sure actions are executed in order
            this.SortListsByStartTime();

            Log("Loaded Enemy Spawn XML data");
        }
        else
            LogError("Cannot find Enemy Spawn XML file for " + fileName);
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

    public int SpawnListSize()
    {
        return SpawnActionList.Count;
    }

	public EnemySpawnData SpawnNext()
	{
		EnemySpawnData action = null;

		if(SpawnActionList.Count() > 0)
		{
			action = SpawnActionList[0];
			SpawnActionList.RemoveAt(0);
		}

		return action;
	}

	public void SortListsByStartTime()
	{
        if (SpawnActionList != null)
        {
            SpawnActionList = SpawnActionList.OrderBy(o => o.StartTime).ToList();
            SpawnActionContainer_Inspector.SpawnDataList = SpawnActionContainer_Inspector.SpawnDataList.OrderBy(o => o.StartTime).ToList();
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
