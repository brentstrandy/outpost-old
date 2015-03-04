using UnityEngine;
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
	
	public EnemyDataManager()
	{
		// TO DO: Load EnemyDataList from serialized XML
		
		// DELETE ALL CODE FROM HERE ... ->
		EnemyDataList = new List<EnemyData>();
		EnemyDataList.Add(new EnemyData("Light Speeder", "Light Speeder"));
		EnemyDataList.Add(new EnemyData("Heavy Speeder", "Heavy Speeder"));
		EnemyDataList.Add(new EnemyData("Drone", "Drone"));
		// -> TO HERE
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
	/// Finds the EnemyData based on a prefab name
	/// </summary>
	/// <returns>The EnemyData by prefab name.</returns>
	/// <param name="prefabName">Prefab name.</param>
	public EnemyData FindEnemyDataByPrefabName(string prefabName)
	{
		return EnemyDataList.Find(x => x.PrefabName.Equals(prefabName));
	}
	
	/// <summary>
	/// Finds the EnemyData based on a display name
	/// </summary>
	/// <returns>The EnemyData by display name.</returns>
	/// <param name="displayName">Display name.</param>
	public EnemyData FindEnemyDataByDisplayName(string displayName)
	{
		return EnemyDataList.Find(x => x.PrefabName.Equals(displayName));
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
