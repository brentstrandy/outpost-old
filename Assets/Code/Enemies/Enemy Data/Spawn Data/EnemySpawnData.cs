using UnityEngine;
using System;
using System.Xml.Serialization;

/// <summary>
/// Enemy spawn data used to place enemies in the level.
/// Owner: John Fitzgerald
/// </summary>
[Serializable]
public class EnemySpawnData
{
    // EnemyName must come first in class so it replaces element tag in Inspector
    [XmlElement("EnemyName")]
    public string EnemyName;
    [XmlElement("StartTime")]
    public float StartTime;
    [XmlElement("StartAngle")]
    public int StartAngle;

    [HideInInspector]
    public bool ShowDebugLogs = true;

	public EnemySpawnData() { }

	public EnemySpawnData(EnemySpawnData obj)
    {
        EnemyName = obj.EnemyName;
        StartTime = obj.StartTime;
        StartAngle = obj.StartAngle;
    }

	public EnemySpawnData(string enemyName, float startTime, int startAngle)
    {
        EnemyName = enemyName;
        StartTime = startTime;
        StartAngle = startAngle;
    }
    #region MessageHandling
    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[TowerData] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[TowerData] " + message);
    }
    #endregion
}