using UnityEngine;
using System.Collections;

public class SmallThraceiumTower : Tower
{
	// Use this for initialization
	public override void Start()
	{
		base.Start();

		//EnemyCircleCollider = this.GetComponent<SphereCollider>();

		// Small Thraceium Tower will fire at enemies, start a coroutine to check (and fire) on enemies
		StartCoroutine("Fire");

		//EnemyCircleCollider.radius = TowerAttributes.Range;
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
