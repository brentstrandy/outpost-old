using UnityEngine;
using System.Collections;

public class EMPTower : Tower
{
	private SphereCollider EnemySphereCollider;

	// Use this for initialization
	public override void Start()
	{
		Name = "EMP Tower";
		EnemySphereCollider = this.GetComponent<SphereCollider>();
		TimeLastShotFired = Time.time;

		// EMP Tower will fire at enemies, start a coroutine to check (and fire) on enemies
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
			Debug.Log("[EMPTower] " + message);
	}
	
	protected override void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[EMPTower] " + message);
	}
	#endregion
}
