using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

public class EnemySpawnActionHandler : EnemySpawnManager
{
	//public bool ShowDebugLogs = true;
	public SpawnActionsManager_ListContainer container;

	public EnemySpawnActionHandler() 
    {
        container = new SpawnActionsManager_ListContainer();
    }

    public void LoadActions(string filename)
    {
		string enemySpawnXMLPath = Application.streamingAssetsPath + "/" + filename + ".xml";

		// Determine if the file exists
		if (File.Exists(enemySpawnXMLPath))
		{
			foreach (EnemySpawnAction action in XMLParser<EnemySpawnAction>.XMLDeserializer_List(enemySpawnXMLPath))
	            container.SpawnAction_List.Add(action);

			// Sort list by time to make sure actions are executed in order
			container.SortListByStartTime();

			Log("Loaded Enemy Spawn XML data");
		}
		else
			LogError("Cannot find Enemy Spawn XML file for level");
    }

	public float GetNextStartTime()
	{
		float startTime = -1;

		if(container.SpawnAction_List.Count() > 0)
			startTime = container.SpawnAction_List[0].StartTime;

		return startTime;
	}

	public bool SpawnListEmpty()
	{
		return container.SpawnAction_List.Count == 0;
	}

	public EnemySpawnAction SpawnNext()
	{
		EnemySpawnAction action = null;

		if(container.SpawnAction_List.Count() > 0)
		{
			action = container.SpawnAction_List[0];
			container.SpawnAction_List.RemoveAt(0);
		}

		return action;
	}

	/*
	/// <summary>
	///  Populates and creates a light speeder spawn action XML
	/// </summary>
	private void CreateXML(string path)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		
		List<SpawnAction> ListOfActions = new List<SpawnAction>();
		
		// create ten random light speeder spawns
		for (float i = 0.0f; i < 10.0f; ++i)
		{
			ListOfActions.Add(new SpawnAction("Light Speeder", i, Random.Range(10, 80)));
			//ListOfActions.Add(new SpawnAction("Light Speeder", i, new Vector3(Random.Range(2.0f, 14.0f), 0f, Random.Range(2.0f, 9.0f))));
		}
		SpawnActionsHandler.PopulateActions(ListOfActions);
		XMLParser<SpawnAction>.XMLSerializer_List(ListOfActions);
	}*/

    [Serializable]
    public class SpawnActionsManager_ListContainer
    {
        public SpawnActionsManager_ListContainer() 
        {
            SpawnAction_List = new List<EnemySpawnAction>();
        }

		public void SortListByStartTime()
		{
			SpawnAction_List = SpawnAction_List.OrderBy(o=>o.StartTime).ToList();
		}

        [XmlArray("SpawnActions"), XmlArrayItem(typeof(EnemySpawnAction), ElementName = "SpawnAction")]
        public List<EnemySpawnAction> SpawnAction_List;
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
