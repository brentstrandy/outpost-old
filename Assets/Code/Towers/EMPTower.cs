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

	/// <summary>
	/// RPC call to tell players to fire a shot. EMP tower is unique in that is launches an orb and does not
	/// do immediate direct damage to an enemy
	/// </summary>
	[PunRPC]
	protected override void FireAcrossNetwork()
	{
		// Reset timer for tracking when to fire next
		TimeLastShotFired = Time.time;
		// Instantiate prefab for firing an orb
		GameObject go = Instantiate(FiringEffect, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - 1.32f), this.transform.rotation) as GameObject;
		// Instantiate the orb
		go.GetComponent<EMPTower_Orb>().Target = TargetedEnemy.transform;
		//go.GetComponent<EMPTower_Orb>().SetData(3, 8, 3);
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