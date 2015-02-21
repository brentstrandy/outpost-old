using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

public class SpawnActionsManager : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	public SpawnActionsManager_ListContainer container;

    public SpawnActionsManager() 
    {
        container = new SpawnActionsManager_ListContainer();
    }

    public void PopulateActions(List<SpawnAction> XMLSpawnActions)
    {
        container = new SpawnActionsManager_ListContainer();

        foreach (SpawnAction action in XMLSpawnActions)
            container.SpawnAction_List.Add(action);
    }

    [Serializable]
    public class SpawnActionsManager_ListContainer
    {
        public SpawnActionsManager_ListContainer() 
        {
            SpawnAction_List = new List<SpawnAction>();
        }

        [XmlArray("SpawnActions"), XmlArrayItem(typeof(SpawnAction), ElementName = "SpawnAction")]
        public List<SpawnAction> SpawnAction_List;
    }
}
