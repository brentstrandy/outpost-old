﻿using UnityEngine;
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
	public int PlayerCount;

    [HideInInspector] [XmlIgnore]
    public bool ShowDebugLogs = true;

	public EnemySpawnData() { }

	public EnemySpawnData(EnemySpawnData obj)
    {
        EnemyName = obj.EnemyName;
        StartTime = obj.StartTime;
        StartAngle = obj.StartAngle;
		PlayerCount = obj.PlayerCount;
    }

	public EnemySpawnData(string enemyName, float startTime, int startAngle, int playerCount)
    {
        EnemyName = enemyName;
        StartTime = startTime;
        StartAngle = startAngle;
		PlayerCount = playerCount;
    }

    #region MessageHandling
    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[EnemySpawnData] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[EnemySpawnData] " + message);
    }
    #endregion
}