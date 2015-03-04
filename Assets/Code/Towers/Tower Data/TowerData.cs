using UnityEngine;
using System.Collections;

/// <summary>
/// All details and stats for a single Tower
/// Owner: Brent Strandy
/// </summary>
public class TowerData
{
	public bool ShowDebugLogs = true;

	// Tower Details
	public string DisplayName { get; private set; }
	public string PrefabName { get; private set; }

	// Tower Stats
	public int Health { get; private set; }
	public float RateOfFire { get; private set; }
	public float Cooldown { get; private set; }
	public float ThraceiumDamage { get; private set; }
	public float BallisticDamage { get; private set; }
	public float TransitionTime { get; private set; }
	public float InstallCount { get; private set; }
	public float MaintenanceCost { get; private set; }

	public TowerData()
	{

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
