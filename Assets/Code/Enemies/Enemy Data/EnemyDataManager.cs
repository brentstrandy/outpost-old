using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class EnemyDataManager
{
	public bool ShowDebugLogs = true;
	/// <summary>
	/// Gets the EnemyData list.
	/// </summary>
	/// <value>The tower data list.</value>
	public List<EnemyData> EnemyDataList { get; private set; }
    public EnemyDataContainer EnemyDataContainer_Inspector;

	public EnemyDataManager()
	{
        // location of level specific XML spawn data
        string enemyDataXMLPath = Application.streamingAssetsPath + "/EnemyData.xml";

        EnemyDataList = new List<EnemyData>();
        EnemyDataContainer_Inspector = GameObject.Find("EnemyData").GetComponent<EnemyDataContainer>();

        if (File.Exists(enemyDataXMLPath))
        {
            // deserialize XML and add each enemy spawn to the lists
            //foreach (EnemyData enemy in XMLParser<EnemyData>.XMLDeserializer_List(enemyDataXMLPath))
            //{
            //    EnemyDataContainer_Inspector.EnemyDataList.Add(enemy);
            //    EnemyDataList.Add(enemy);
            //}
            foreach (EnemyData enemy in EnemyDataContainer_Inspector.EnemyDataList)
            {
                EnemyDataList.Add(enemy);
            }
        }
        else
            LogError("Cannot find Enemy Data XML file");
	}
	
	/// <summary>
	/// Total number of Enemies loaded
	/// </summary>
	/// <returns>Total Count</returns>
	public int EnemyCount()
	{
		return EnemyDataList.Count;
	}
	
	/// <summary>
	/// Returns an array of the Display Names for all the currently loaded Enemies
	/// </summary>
	/// <returns>The display names.</returns>
	public string[] EnemyDisplayNames()
	{
		string[] towerNames = new string[EnemyDataList.Count];
		
		for(int i = 0; i < EnemyDataList.Count; i++)
		{
			towerNames[i] = EnemyDataList[i].DisplayName;
		}
		
		return towerNames;
	}

	/// <summary>
	/// Finds the EnemyData based on a display name
	/// </summary>
	/// <returns>The EnemyData by display name.</returns>
	/// <param name="displayName">Display name.</param>
	public EnemyData FindEnemyDataByDisplayName(string displayName)
	{
		return EnemyDataList.Find(x => x.DisplayName.Equals(displayName));
	}

	public string FindEnemyPrefabNameByDisplayName(string displayName)
	{
		return EnemyDataList.Find(x => x.DisplayName.Equals(displayName)).PrefabName;
	}
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[EnemyDataManager] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[EnemyDataManager] " + message);
	}
	#endregion
}
