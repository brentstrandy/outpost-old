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
    public string EnemyName;
    public float StartTime;
    public int StartAngle;
    public EnemyType Type;

    // used for Inspector (Reorderable List)
    [HideInInspector]
    public enum EnemyType 
    {
        Normal,
        Boss
    }

    [HideInInspector] [XmlIgnore]
    public bool ShowDebugLogs = true;

	public EnemySpawnData() { }

	public EnemySpawnData(EnemySpawnData obj)
    {
        EnemyName = obj.EnemyName;
        StartTime = obj.StartTime;
        StartAngle = obj.StartAngle;
        Type = obj.Type;
    }

	public EnemySpawnData(string enemyName, float startTime, int startAngle, EnemyType type)
    {
        EnemyName = enemyName;
        StartTime = startTime;
        StartAngle = startAngle;
        Type = type;
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