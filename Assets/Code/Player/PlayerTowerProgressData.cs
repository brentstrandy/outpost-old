using UnityEngine;
using System.Xml.Serialization;

public class PlayerTowerProgressData
{
	public string TowerID;
	public int KillCount;
	public int XP;
	public float BallisticDamageDealt;
	public float ThraceiumDamageDealt;

	[HideInInspector]
	[XmlIgnore]
	public bool ShowDebugLogs = true;

	public PlayerTowerProgressData()
	{
		
	}

	#region MessageHandling

	protected void Log(string message)
	{
		if (ShowDebugLogs)
			Debug.Log("[PlayerTowerProgressData] " + message);
	}

	protected void LogError(string message)
	{
		Debug.LogError("[PlayerTowerProgressData] " + message);
	}

	#endregion MessageHandling
}
