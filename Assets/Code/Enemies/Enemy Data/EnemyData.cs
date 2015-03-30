using UnityEngine;
using System.Collections;

/// <summary>
/// All details and stats for a single Enemy
/// Owner: Brent Strandy
/// </summary>
public class EnemyData 
{
	public bool ShowDebugLogs = true;
	
	// Enemy Details
	public string DisplayName;
	public string PrefabName;
	
	// Enemy Stats
	public int Health;
	public float RateOfFire;
	public float Cooldown;
	public float DamageDealt;
	public float Acceleration;
	public float Speed;
	public float Range;
	public float BallisticDefense;
	public float ThraceiumDefense;
	
	public EnemyData()
	{
		
	}
	
	// DELETE THIS CONSTRUCTOR - IT IS ONLY USED TO TEMPORARY LOAD TOWER NAMES
	public EnemyData(string displayName, string prefabName)
	{
		DisplayName = displayName;
		PrefabName = prefabName;
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