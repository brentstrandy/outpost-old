//using System.Collections;
//using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// "Dummy" container class to display a List of the game's enemy data in the inspector.
/// This must derive from Monobehaviour for inspector visibility.
/// This is seperate from EnemyData because Monobehaviour cannot serialize to XML.
/// Owner: John Fitzgerald
/// </summary>
public class EnemyDataContainer : MonoBehaviour
{
    // TODO -- (FITZGERALD) hide when custom inspector is done
    //[HideInInspector]
    [XmlArray("Enemies"), XmlArrayItem(typeof(EnemyData), ElementName = "Enemy")]
    public List<EnemyData> EnemyDataList;

    public EnemyDataContainer()
    {
        EnemyDataList = new List<EnemyData>();
    }
}