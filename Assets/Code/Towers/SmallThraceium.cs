using UnityEngine;
using System.Collections;

public class SmallThraceium : Tower
{
	private SphereCollider EnemySphereCollider;

	// Use this for initialization
	public override void Start()
	{
		Name = "Small Thraceium";
		EnemySphereCollider = this.GetComponent<SphereCollider>();
		TimeLastShotFired = Time.time;

		// Small Thraceium Tower will fire at enemies, start a coroutine to check (and fire) on enemies
		StartCoroutine("Fire");

		EnemySphereCollider.radius = Range;
	}
	
	// Update is called once per frame
	public override void Update()
	{

	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[SmallThraceium] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[SmallThraceium] " + message);
	}
	#endregion
}
