using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SpawnEnemyManager : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	public bool PerformSpawnActions = false;

	private SpawnActionsManager SpawnActionsHandler = new SpawnActionsManager();
	private float StartTime;
    int EnemyCount = 0;

	public void Start()
	{
		LoadSpawnData();
		StartTime = Time.time;
	}

	// Update is called once per frame
	public void Update ()
	{
		// Only Update if the spawner has started
		if(StartTime > -1)
		{
			// Initialize enemies only if responsible for performing the action, otherwise do nothing
			if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
			{
				// TO DO: Determine if an enemy should be instantiated
			}
		}
	}

	/// <summary>
	/// Loads the spawn data from an XML file
	/// </summary>
	private void LoadSpawnData()
    {
        if (ShowDebugLogs)
            this.Log("LoadSpawnData()");

        string enemySpawnXMLPath = Application.streamingAssetsPath + "/EnemySpawnInfo.xml";

        // determines if XML file exists
        if (File.Exists(enemySpawnXMLPath))
			SpawnActionsHandler.PopulateActions(XMLParser<SpawnAction>.XMLDeserializer_List());  // XMLParser populates EnemySpawnActions with List<SpawnAction>
        else
            CreateXML(enemySpawnXMLPath);  // create and populate non-existant XML
    }

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
    }

	/// <summary>
	/// Starts the Enemy Spawner by loading the spawning details
	/// </summary>
	/// <param name="performSpawnActions">If set to <c>true</c> this spawner can instantiate enemies.</param>
	public void Start(bool performSpawnActions)
	{

	}

	public void Stop()
	{
		PerformSpawnActions = false;
		StartTime = -1;
	}

	private void SpawnEnemy()
	{
        if (EnemyCount < 9)
        {
			SessionManager.Instance.InstantiateObject(SpawnActionsHandler.container.SpawnAction_List[++EnemyCount]);
            //SessionManager.Instance.InstantiateObject("Light Speeder", new Vector3(Random.Range(2.0f, 14.0f), 0, Random.Range(2.0f, 9.0f)), Quaternion.identity);
        }
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
