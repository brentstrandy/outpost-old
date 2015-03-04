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
	public string DisplayName { get; private set; }
	public string PrefabName { get; private set; }
	
	// Enemy Stats
	public int Health { get; private set; }
	public float RateOfFire { get; private set; }
	public float Cooldown { get; private set; }
	public float DamageDealt { get; private set; }
	
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