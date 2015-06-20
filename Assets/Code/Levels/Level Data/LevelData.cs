using UnityEngine;
using System;
using System.Xml.Serialization;

/// <summary>
/// All details and stats for a single Level
/// Owner: Brent Strandy
/// </summary>
[Serializable]
public class LevelData 
{
	// Level Details
	public string DisplayName;
	public string SceneName;
	public string EnemySpawnFilename;
	public string Description;
	public int MinimumPlayers;
	public int MaximumPlayers;
	public string AvailableQuadrants;
	public Quadrant StartingQuadrant;
	public string AvailableTowers;
	public int MaxTowersPerPlayer;
	// Mining Facility Details
	public float IncomePerSecond;
	public float MiningFacilityHealth;

    [HideInInspector] [XmlIgnore]
	public bool ShowDebugLogs = true;
	
	public LevelData() { }
	
	public LevelData(LevelData obj)
	{
		DisplayName = obj.DisplayName;
		SceneName = obj.SceneName;
		EnemySpawnFilename = obj.EnemySpawnFilename;
		Description = obj.Description;
		MinimumPlayers = obj.MinimumPlayers;
		MaximumPlayers = obj.MaximumPlayers;
		AvailableQuadrants = obj.AvailableQuadrants;
		StartingQuadrant = obj.StartingQuadrant;
		AvailableTowers = obj.AvailableTowers;
		MaxTowersPerPlayer = obj.MaxTowersPerPlayer;
		// Mining Facility Details
		IncomePerSecond = obj.IncomePerSecond;
		MiningFacilityHealth = obj.MiningFacilityHealth;
	}

	public LevelData(string displayName, string sceneName)
	{
        DisplayName = displayName;
        SceneName = sceneName;
	}
	
	/// <summary>
	/// Used for creating a new LevelData in the Unity Inspector
	/// </summary>
	public LevelData(string empty)
	{
		DisplayName = "";
		SceneName = "";
	}
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[LevelData] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[LevelData] " + message);
	}
	#endregion
}