using System;
using System.Xml.Serialization;
using UnityEngine;

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
    public int TowerID;

    // Tower Stats
    public int MaxHealth;

    public float RateOfFire;
    public float Cooldown;
    public float Range;

    public float AdjustedRange
    {
        get
        {
            // Note from J.D.S 2015-07-20:
            // This is used as a scaling factor for the tower range rings.
            // I don't know why everything beyond size 1 is cut in half.
            if (Range <= 1.0f)
            {
                return Range;
            }

            return 1.0f + ((Range - 1.0f) * 0.5f);
        }
    }

    public float BallisticDamage;
    public float ThraceiumDamage;
    public float BallisticDefense;
    public float ThraceiumDefense;
    public float TransitionTime;
    public int InstallCost;
    public float MaintenanceCost;
    public float StartupTime;
    public float TrackingSpeed;

    [HideInInspector]
    [XmlIgnore]
    public bool ShowDebugLogs = true;

    public TowerData()
    {
    }

    public TowerData(string blank)
    {
        DisplayName = PrefabName = "";

        MaxHealth = 0;
        RateOfFire = Cooldown = Range = ThraceiumDamage = BallisticDamage = TransitionTime = 0;
        InstallCost = 0;
        MaintenanceCost = StartupTime = TrackingSpeed = 0;
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

    #endregion MessageHandling
}