﻿using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Responsible for loading and storing Data for all Towers. 
/// Has ability to find towers based on a single Tower attribute
/// Owner: Brent Strandy
/// </summary>
public class TowerDataManager
{
	public bool ShowDebugLogs = true;
	/// <summary>
	/// Gets the TowerData list.
	/// </summary>
	/// <value>The tower data list.</value>
	public List<TowerData> TowerDataList { get; private set; }
    public TowerDataContainer TowerDataContainer_Inspector;


	public TowerDataManager()
	{
        // location of level specific XML spawn data
        string towerDataXMLPath = Application.streamingAssetsPath + "/TowerData.xml";

        TowerDataList = new List<TowerData>();
        // FITZGERALD: this requires Level 1 to be loaded
        //TowerDataContainer_Inspector = GameObject.Find("Tower Data Manager").GetComponent<TowerDataContainer>();

        if (File.Exists(towerDataXMLPath))
        {
            // deserialize XML and add each enemy spawn to the lists
            foreach (TowerData tower in XMLParser<TowerData>.XMLDeserializer_List(towerDataXMLPath))
            {
                //TowerDataContainer_Inspector.TowerDataList.Add(tower);
                TowerDataList.Add(tower);
            }
        }
        else
            LogError("Cannot find Tower Data XML file");
        
        //// DELETE ALL CODE FROM HERE ... ->
        //TowerDataList = new List<TowerData>();
        //TowerDataList.Add(new TowerData("Small Thraceium Tower", "SmallThraceiumTower"));
        //TowerDataList.Add(new TowerData("Thraceium Rain Tower", "ThraceiumRainTower"));
        //TowerDataList.Add(new TowerData("EMP Tower", "EMPTower"));
        //TowerDataList.Add(new TowerData("Universal Energy Tower", "UniversalEnergyTower"));
        //// -> TO HERE
	}

	/// <summary>
	/// Total number of Towers loaded
	/// </summary>
	/// <returns>Total Count</returns>
	public int TowerCount()
	{
		return TowerDataList.Count;
	}

	/// <summary>
	/// Returns an array of the Display Names for all the currently loaded towers
	/// </summary>
	/// <returns>The display names.</returns>
	public string[] TowerDisplayNames()
	{
		string[] towerNames = new string[TowerDataList.Count];

		for(int i = 0; i < TowerDataList.Count; i++)
		{
			towerNames[i] = TowerDataList[i].DisplayName;
		}

		return towerNames;
	}

	/// <summary>
	/// Finds the TowerData based on a prefab name
	/// </summary>
	/// <returns>The TowerData by prefab name.</returns>
	/// <param name="prefabName">Prefab name.</param>
	public TowerData FindTowerDataByPrefabName(string prefabName)
	{
		return TowerDataList.Find(x => x.PrefabName.Equals(prefabName));
	}

	/// <summary>
	/// Finds the TowerData based on a display name
	/// </summary>
	/// <returns>The TowerData by display name.</returns>
	/// <param name="displayName">Display name.</param>
	public TowerData FindTowerDataByDisplayName(string displayName)
	{
		return TowerDataList.Find(x => x.PrefabName.Equals(displayName));
	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[TowerDataManager] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[TowerDataManager] " + message);
	}
	#endregion
}