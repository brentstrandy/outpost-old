using System;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// All information about the progress of a given level
/// Owner: Brent Strandy
/// </summary>
[Serializable]
public class PlayerLevelProgressData
{
    // Level Details
    public int LevelID;

    public int Score;

    [HideInInspector]
    [XmlIgnore]
    public bool ShowDebugLogs = true;

    public PlayerLevelProgressData()
    {
    }

    public PlayerLevelProgressData(int levelID, int score)
    {
        LevelID = levelID;
        Score = score;
    }

    public PlayerLevelProgressData(PlayerLevelProgressData obj)
    {
        LevelID = obj.LevelID;
    }

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[LevelProgressData] " + message);
    }

    protected void LogError(string message)
    {
        Debug.LogError("[LevelProgressData] " + message);
    }

    #endregion MessageHandling
}