using UnityEngine;
using System;
using System.Xml.Serialization;

[Serializable]
public class SpawnAction
{
    public SpawnAction() { }

    public SpawnAction(SpawnAction obj)
    {
        EnemyName = obj.EnemyName;
        StartTime = obj.StartTime;
        StartAngle = obj.StartAngle;
    }

    public SpawnAction(string _EnemyName, float _StartTime, int _StartAngle)
    {
        EnemyName = _EnemyName;
        StartTime = _StartTime;
        StartAngle = _StartAngle;
    }

    [XmlElement("EnemyName")]
    public string EnemyName;
    [XmlElement("StartTime")]
    public float StartTime;
    [XmlElement("StartAngle")]
    public int StartAngle;
    //[XmlElement("StartPosition")]
    //public Vector3 StartPosition;
}