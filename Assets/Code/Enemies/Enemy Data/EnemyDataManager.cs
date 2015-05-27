using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class EnemyDataManager
{
	public bool ShowDebugLogs = true;

	public bool FinishedLoadingData { get; private set; }
	/// <summary>
	/// Gets the EnemyData list.
	/// </summary>
	/// <value>The tower data list.</value>
	public List<EnemyData> EnemyDataList { get; private set; }
    public EnemyDataContainer EnemyDataContainer_Inspector;

	public EnemyDataManager()
	{
		FinishedLoadingData = false;

		// Instantiate lists to save the data
        EnemyDataList = new List<EnemyData>();
        EnemyDataContainer_Inspector = GameObject.Find("EnemyData").GetComponent<EnemyDataContainer>();
	}

	/// <summary>
	/// Coroutine method used to load XML data from a server location
	/// </summary>
	public IEnumerator LoadDataFromServer()
	{
		WWW www = new WWW("http://www.diademstudios.com/outpostdata/EnemyData.xml");
		string myXML;
		
		while(!www.isDone)
		{
			yield return 0;
		}

		myXML = www.text;
		
		// Deserialize XML and add each enemy spawn to the lists
		foreach (EnemyData enemy in XMLParser<EnemyData>.XMLDeserializer_Data(myXML))
		{
			EnemyDataContainer_Inspector.EnemyDataList.Add(enemy);
			EnemyDataList.Add(enemy);
		}
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
