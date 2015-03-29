using UnityEngine;
//using System.Collections;
//using System;
using System.Collections.Generic;
using System.Xml.Serialization;

/// <summary>
/// "Dummy" container class to display a List of the game's tower data in the inspector.
/// This must derive from Monobehaviour for inspector visibility.
/// This is seperate from TowerData because Monobehaviour cannot serialize to XML.
/// Owner: John Fitzgerald
/// </summary>
public class TowerDataContainer : MonoBehaviour
{
    // FITZGERALD: hide when custom inspector is done
    //[HideInInspector]
    [XmlArray("Towers"), XmlArrayItem(typeof(TowerData), ElementName = "Tower")]
    public List<TowerData> TowerDataList;

    public TowerDataContainer()
    {
        TowerDataList = new List<TowerData>();
    }
}