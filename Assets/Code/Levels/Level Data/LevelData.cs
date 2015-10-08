using System;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// All details and stats for a single Level
/// Owner: Brent Strandy
/// </summary>
[Serializable]
public class LevelData : IComparable<LevelData>
{
    // Level Details
    public string DisplayName;

    public string SceneName;
    public int LevelID;
    public string EnemySpawnFilename;
    public string NotificationFilename;
    public string Description;
    public int MinimumPlayers;
    public int MaximumPlayers;
    public string AvailableQuadrants;
    public Quadrant StartingQuadrant;
    public string AvailableTowers;
    public int MaxTowersPerPlayer;

    // Mining Facility Details
    public float StartingMoney;

    public float IncomePerSecond;
    public float MiningFacilityHealth;

    [HideInInspector]
    [XmlIgnore]
    public bool ShowDebugLogs = true;

    public LevelData()
    {
    }

    public LevelData(string displayName, string sceneName, int levelID)
    {
        DisplayName = displayName;
        SceneName = sceneName;
        LevelID = levelID;
    }

    /// <summary>
    /// Used for creating a new LevelData in the Unity Inspector
    /// </summary>
    public LevelData(string empty)
    {
        DisplayName = "";
        SceneName = "";
    }

    public int CompareTo(LevelData ld)
    {
        if (this.LevelID < ld.LevelID)
            return -1;
        else if (this.LevelID == ld.LevelID)
            return 0;
        else
            return 1;
    }

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[LevelData] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[LevelData] " + message);
    }

    #endregion MessageHandling
}