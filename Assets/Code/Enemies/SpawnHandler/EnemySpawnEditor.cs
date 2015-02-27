using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
public class EnemySpawnEditor : MonoBehaviour 
{
	[XmlArray("EnemySpawnActions"), XmlArrayItem(typeof(EnemySpawnAction), ElementName = "EnemySpawnAction")]
	public List<EnemySpawnAction> SpawnActionList;

	// Use this for initialization
	void Start ()
	{
		SpawnActionList = new List<EnemySpawnAction>();
	}
}
