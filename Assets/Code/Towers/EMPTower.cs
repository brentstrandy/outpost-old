using UnityEngine;
using System.Collections;

public class EMPTower : Tower
{
	// Use this for initialization
	public override void Start()
	{
		base.Start();

		// Load default attributes from TowerData
		//TowerData towerData = GameDataManager.Instance.FindTowerDataByPrefabName("EMPTower");

		TimeLastShotFired = Time.time;

		// EMP Tower will fire at enemies, start a coroutine to check (and fire) on enemies
		StartCoroutine("Fire");
	}

	#region MessageHandling
	protected override void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[EMPTower] " + message);
	}
	
	protected override void LogError(string message)
	{
		Debug.LogError("[EMPTower] " + message);
	}
	#endregion
}
