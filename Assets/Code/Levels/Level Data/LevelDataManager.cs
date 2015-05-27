using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class LevelDataManager
{
	public bool ShowDebugLogs = true;

	public bool FinishedLoadingData { get; private set; }
	/// <summary>
	/// Gets the LevelData list.
	/// </summary>
	/// <value>The level data list.</value>
	public List<LevelData> LevelDataList { get; private set; }
	public LevelDataContainer LevelDataContainer_Inspector;
	
	public LevelDataManager()
	{
		FinishedLoadingData = false;

		// Instantiate lists to save the data
		LevelDataList = new List<LevelData>();
		LevelDataContainer_Inspector = GameObject.Find("LevelData").GetComponent<LevelDataContainer>();
	}
	
	/// <summary>
	/// Coroutine method used to load XML data from a server location
	/// </summary>
	public IEnumerator LoadDataFromServer()
	{
		WWW www = new WWW("http://www.diademstudios.com/outpostdata/LevelData.xml");
		string myXML;
		
		while(!www.isDone)
		{
			yield return 0;
		}

		myXML = www.text;
		
		// Deserialize XML and add each enemy spawn to the lists
		foreach (LevelData level in XMLParser<LevelData>.XMLDeserializer_Data(myXML))
		{
			LevelDataContainer_Inspector.LevelDataList.Add(level);
			LevelDataList.Add(level);
		}
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
	
	public string FindLevelSceneByDisplayName(string sceneName)
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
