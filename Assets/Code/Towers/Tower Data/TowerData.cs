﻿using UnityEngine;
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
	public string DisplayName;
    public string PrefabName;

	// Tower Stats
	public int Health;
    public int PlayerCount; // how many players required to spawn this enemy
    public float RateOfFire;
    public float Cooldown;
    public float ThraceiumDamage;
    public float BallisticDamage;
    public float TransitionTime;
    public float InstallCount;
    public float MaintenanceCost;

    [HideInInspector] [XmlIgnore]
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
