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
	// Tower Details
    // DisplayName must come first in class so it replaces element tag in Inspector
    [XmlElement("DisplayName")] 
	public string DisplayName;
    [XmlElement("PrefabName")]
    public string PrefabName;

	// Tower Stats
    [XmlElement("Health")]
	public int Health;
    [XmlElement("RateOfFire")]
    public float RateOfFire;
    [XmlElement("Cooldown")]
    public float Cooldown;
    [XmlElement("ThraceiumDamage")]
    public float ThraceiumDamage;
    [XmlElement("BallisticDamage")]
    public float BallisticDamage;
    [XmlElement("TransitionTime")]
    public float TransitionTime;
    [XmlElement("InstallCount")]
    public float InstallCount;
    [XmlElement("MaintenanceCost")]
    public float MaintenanceCost;

    [HideInInspector]
    public bool ShowDebugLogs = true;

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

	#region MessageHandling
    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[TowerData] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[TowerData] " + message);
    }
	#endregion
}
