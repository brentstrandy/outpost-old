using UnityEngine;
using System;
using System.Xml.Serialization;

/// <summary>
/// All details and stats for a single Level
/// Owner: Brent Strandy
/// </summary>
public class LevelData 
{
	// Level Details
	public string DisplayName;
	public string SceneName;

	public bool ShowDebugLogs = true;
	
	public LevelData() { }
	
	public LevelData(LevelData obj)
	{
		DisplayName = obj.DisplayName;
		SceneName = obj.SceneName;
	}

	public LevelData(string displayName, string sceneName)
	{

	}
	
	//public EnemyData(string displayName, string prefabName, int health, float rateOfFire, 
	//                 float coolDown, float damageDealt, float acceleration, float speed, 
	//                 float range, float ballisticDefense, float thraceiumDefense)
	//{
	//    DisplayName = displayName;
	//    PrefabName = prefabName;
	
	//    Health = health;
	//    RateOfFire = rateOfFire;
	//    Cooldown = coolDown;
	//    DamageDealt = damageDealt;
	//    Acceleration = acceleration;
	//    Speed = speed;
	//    Range = range;
	//    BallisticDefense = ballisticDefense;
	//    ThraceiumDefense = thraceiumDefense;
	//}
	
	/// <summary>
	/// Used for creating a new Enemy in the Unity Inspector
	/// </summary>
	public LevelData(string empty)
	{
		DisplayName = "";
		SceneName = "";
	}
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[LevelData] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[LevelData] " + message);
	}
	#endregion
}