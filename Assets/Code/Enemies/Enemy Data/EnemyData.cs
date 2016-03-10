using System;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// All details and stats for a single Enemy
/// Owner: Brent Strandy
/// </summary>
[Serializable]
public class EnemyData
{
    // Enemy Details
    public string DisplayName;

    public string PrefabName;
    public int EnemyID;
    public PathFindingType PathFinding;
    public Color HighlightColor;
    public bool AttackTowers;
    public bool AttackMiningFacility;
    public bool AttackWhileMoving;

    // Enemy Stats
    public int MaxHealth;

    public float RateOfFire;
    public float Acceleration;
    public float Speed;
    public float Range;
    public float HoverDistance;
    public float BallisticDamage;
    public float ThraceiumDamage;
    public float BallisticDefense;
    public float ThraceiumDefense;
    public float TurningSpeed;
    public int ScoreValue;
	public int MoneyValue;

    [HideInInspector]
    [XmlIgnore]
    public bool ShowDebugLogs = true;

    public EnemyData()
    {
    }

    /// <summary>
    /// Used for creating a new Enemy in the Unity Inspector
    /// </summary>
    public EnemyData(string empty)
    {
        DisplayName = "";

        MaxHealth = 0;
        RateOfFire = Acceleration = Speed = Range = BallisticDefense = 0;
    }

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[EnemyData] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[EnemyData] " + message);
    }

    #endregion MessageHandling
}