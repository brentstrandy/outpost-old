using UnityEngine;
//using System.Collections;
//using System;
using System.Collections.Generic;
using System.Xml.Serialization;

/// <summary>
/// "Dummy" container class to display a List of the level's spawn data in the inspector.
/// This must derive from Monobehaviour for inspector visibility.
/// This is seperate from EnemySpawnData because Monobehaviour cannot serialize to XML.
/// Owner: John Fitzgerald
/// </summary>
public class EnemySpawnDataContainer : MonoBehaviour 
{
    [HideInInspector]
	[XmlArray("EnemySpawnActions"), XmlArrayItem(typeof(EnemySpawnData), ElementName = "EnemySpawnAction")]
	public List<EnemySpawnData> EnemySpawnDataList;

    public EnemySpawnDataContainer()
    {
        EnemySpawnDataList = new List<EnemySpawnData>();
    }
}
