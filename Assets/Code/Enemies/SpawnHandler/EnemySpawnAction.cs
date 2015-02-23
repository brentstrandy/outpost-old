using UnityEngine;
using System;
using System.Xml.Serialization;

[Serializable]
public class EnemySpawnAction
{
	public EnemySpawnAction() { }

	public EnemySpawnAction(EnemySpawnAction obj)
    {
        EnemyName = obj.EnemyName;
        StartTime = obj.StartTime;
        StartAngle = obj.StartAngle;
    }

	public EnemySpawnAction(string enemyName, float startTime, int startAngle)
    {
        EnemyName = enemyName;
        StartTime = startTime;
        StartAngle = startAngle;
    }

    [XmlElement("EnemyName")]
    public string EnemyName;
    [XmlElement("StartTime")]
    public float StartTime;
    [XmlElement("StartAngle")]
    public int StartAngle;
}