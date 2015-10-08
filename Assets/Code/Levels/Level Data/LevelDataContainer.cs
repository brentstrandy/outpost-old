using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// "Dummy" container class to display a List of the game's level data in the inspector.
/// This must derive from Monobehaviour for inspector visibility.
/// This is seperate from LevelData because Monobehaviour cannot serialize to XML.
/// Owner: John Fitzgerald
/// </summary>
public class LevelDataContainer : MonoBehaviour
{
    // FITZGERALD: hide when custom inspector is done
    //[HideInInspector]
    [XmlArray("Levels"), XmlArrayItem(typeof(LevelData), ElementName = "Level")]
    public List<LevelData> LevelDataList;

    public LevelDataContainer()
    {
        LevelDataList = new List<LevelData>();
    }
}