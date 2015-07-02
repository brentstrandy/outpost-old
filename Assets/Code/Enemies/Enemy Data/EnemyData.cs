using UnityEngine;
using System;
using System.Xml.Serialization;

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
	public PathFindingType PathFinding;
	public Color HighlightColor;
	public bool AttackTowers;
	public bool AttackMiningFacility;
	public bool AttackWhileMoving;

	// Enemy Stats
	public int MaxHealth;
	public float RateOfFire;
	public float DamageDealt;
	public float Acceleration;
	public float Speed;
	public float Range;
	public float BallisticDamage;
	public float ThraceiumDamage;
	public float BallisticDefense;
	public float ThraceiumDefense;
	public float TurningSpeed;

    [HideInInspector] [XmlIgnore]
    public bool ShowDebugLogs = true;

	public EnemyData() { }
	
    public EnemyData(EnemyData obj)
    {
        DisplayName = obj.DisplayName;
		PrefabName = obj.PrefabName;
		PathFinding = obj.PathFinding;
		HighlightColor = obj.HighlightColor;
		AttackTowers = obj.AttackTowers;
		AttackMiningFacility = obj.AttackMiningFacility;
		AttackWhileMoving = obj.AttackWhileMoving;
	
	    // Enemy Stats
	    MaxHealth = obj.MaxHealth;
	    RateOfFire = obj.RateOfFire;
	    DamageDealt = obj.DamageDealt;
	    Acceleration = obj.Acceleration;
	    Speed = obj.Speed;
	    Range = obj.Range;
		BallisticDamage = obj.BallisticDamage;
		ThraceiumDamage = obj.ThraceiumDamage;
	    BallisticDefense = obj.BallisticDefense;
        ThraceiumDefense = obj.ThraceiumDefense;
		TurningSpeed = obj.TurningSpeed;
    }
	
    /// <summary>
    /// Used for creating a new Enemy in the Unity Inspector
    /// </summary>
    public EnemyData(string empty)
    {
        DisplayName = "";
	
	    MaxHealth = 0;
	    RateOfFire = DamageDealt = Acceleration = Speed = Range = BallisticDefense = 0;
    }

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[EnemyData] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[EnemyData] " + message);
	}
	#endregion
}