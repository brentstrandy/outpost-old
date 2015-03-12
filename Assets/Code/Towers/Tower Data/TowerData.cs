using UnityEngine;
using System;
using System.Collections;
using System.Xml.Serialization;

/// <summary>
/// All details and stats for a single Tower
/// Owner: Brent Strandy
/// </summary>
[Serializable]
public class TowerData
{
	public bool ShowDebugLogs = true;

	// Tower Details
    [XmlElement("DisplayName")]
	public string DisplayName { get; private set; }
    [XmlElement("PrefabName")]
    public string PrefabName { get; private set; }

	// Tower Stats
    [XmlElement("Health")]
	public int Health { get; private set; }
    [XmlElement("RateOfFire")]
    public float RateOfFire { get; private set; }
    [XmlElement("Cooldown")]
    public float Cooldown { get; private set; }
    [XmlElement("ThraceiumDamage")]
    public float ThraceiumDamage { get; private set; }
    [XmlElement("BallisticDamage")]
    public float BallisticDamage { get; private set; }
    [XmlElement("TransitionTime")]
    public float TransitionTime { get; private set; }
    [XmlElement("InstallCount")]
    public float InstallCount { get; private set; }
    [XmlElement("MaintenanceCost")]
    public float MaintenanceCost { get; private set; }

	public TowerData() { }

    public TowerData(TowerData obj)
    {
        DisplayName = obj.DisplayName;
        PrefabName = obj.PrefabName;
        Health = obj.Health;
        RateOfFire = obj.RateOfFire;
        Cooldown = obj.Cooldown;
        ThraceiumDamage = obj.ThraceiumDamage;
        BallisticDamage = obj.BallisticDamage;
        TransitionTime = obj.TransitionTime;
        InstallCount = obj.InstallCount;
        MaintenanceCost = obj.MaintenanceCost;
    }

	// DELETE THIS CONSTRUCTOR - IT IS ONLY USED TO TEMPORARY LOAD TOWER NAMES
	public TowerData(string displayName, string prefabName)
	{
		DisplayName = displayName;
		PrefabName = prefabName;
	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[TowerData] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[TowerData] " + message);
	}
	#endregion
}
