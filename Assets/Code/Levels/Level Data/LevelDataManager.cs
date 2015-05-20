using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class LevelDataManager
{
	public bool ShowDebugLogs = true;
	/// <summary>
	/// Gets the LevelData list.
	/// </summary>
	/// <value>The level data list.</value>
	public List<LevelData> LevelDataList { get; private set; }
	//public LevelDataContainer LevelDataContainer_Inspector;
	
	public LevelDataManager()
	{
		/* location of level specific XML spawn data
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
		*/

		// Manually add 3 levels to the list of possible levels in the game
		// These lines will be replaced with code that loads data from XML
		LevelDataList.Add(new LevelData("Awesome Level", "Level1"));
		LevelDataList.Add(new LevelData("Sad Level", "Level2"));
		LevelDataList.Add(new LevelData("Dumb Level", "Level3"));
	}
	
	/// <summary>
	/// Total number of Levels
	/// </summary>
	/// <returns>Total Count</returns>
	public int LevelCount()
	{
		return LevelDataList.Count;
	}
	
	/// <summary>
	/// Returns an array of the Display Names for all the currently loaded Levels
	/// </summary>
	/// <returns>The display names.</returns>
	public string[] LevelDisplayNames()
	{
		string[] levelNames = new string[LevelDataList.Count];
		
		for(int i = 0; i < LevelDataList.Count; i++)
		{
			levelNames[i] = LevelDataList[i].DisplayName;
		}
		
		return levelNames;
	}
	
	/// <summary>
	/// Finds the LevelData based on a display name
	/// </summary>
	/// <returns>The LevelData by display name.</returns>
	/// <param name="displayName">Display name.</param>
	public LevelData FindLevelDataByDisplayName(string displayName)
	{
		return LevelDataList.Find(x => x.DisplayName.Equals(displayName));
	}
	
	public string FindLevelSceneNameByDisplayName(string sceneName)
	{
		return LevelDataList.Find(x => x.DisplayName.Equals(sceneName)).SceneName;
	}
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[LevelDataManager] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[LevelDataManager] " + message);
	}
	#endregion
}
