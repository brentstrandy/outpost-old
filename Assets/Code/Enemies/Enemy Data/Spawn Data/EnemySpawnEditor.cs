using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
public class EnemySpawnEditor : MonoBehaviour 
{
	[XmlArray("EnemySpawnActions"), XmlArrayItem(typeof(EnemySpawnData), ElementName = "EnemySpawnAction")]
	public List<EnemySpawnData> SpawnActionList;

	// Use this for initialization
	void Start ()
	{
		SpawnActionList = new List<EnemySpawnData>();
	}
}
