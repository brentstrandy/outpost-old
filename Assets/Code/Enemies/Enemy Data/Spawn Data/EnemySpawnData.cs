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
    [XmlElement("EnemyName")]
    public string EnemyName;
    [XmlElement("StartTime")]
    public float StartTime;
    [XmlElement("StartAngle")]
    public int StartAngle;

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
}