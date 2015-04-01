using UnityEngine;
using System.Collections;

public class SmallThraceiumTower : Tower
{
	private SphereCollider EnemySphereCollider;

	// Use this for initialization
	public override void Start()
	{
		base.Start();

		// Load default attributes from TowerData
		TowerData towerData = GameDataManager.Instance.TowerDataMngr.FindTowerDataByPrefabName("SmallThraceiumTower");
		SetTowerData(towerData);

		EnemySphereCollider = this.GetComponent<SphereCollider>();
		TimeLastShotFired = Time.time;

		// Small Thraceium Tower will fire at enemies, start a coroutine to check (and fire) on enemies
		StartCoroutine("Fire");

		EnemySphereCollider.radius = Range;
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
			Debug.Log("[SmallThraceiumTower] " + message);
	}
	
	protected override void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[SmallThraceiumTower] " + message);
	}
	#endregion
}
