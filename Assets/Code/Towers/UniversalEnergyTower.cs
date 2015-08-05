using UnityEngine;
using System.Collections;

public class UniversalEnergyTower : Tower
{
	// Use this for initialization
	public override void Start()
	{
		base.Start();

		// Load default attributes from TowerData
		//TowerData towerData = GameDataManager.Instance.FindTowerDataByPrefabName("UniversalEnergyTower");

		// Universal Energy Tower will fire at enemies, start a coroutine to check (and fire) on enemies
		StartCoroutine("Fire");
	}
	
	// Update is called once per frame
	public override void Update()
	{
		base.Update();
	}

	#region MessageHandling
	protected override void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[UniversalEnergyTower] " + message);
	}
	
	protected override void LogError(string message)
	{
		Debug.LogError("[UniversalEnergyTower] " + message);
	}
	#endregion
}
