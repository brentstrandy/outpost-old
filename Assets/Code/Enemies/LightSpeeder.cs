using UnityEngine;
using System.Collections;
using Settworks.Hexagons;

public class LightSpeeder : Enemy
{
	public LightSpeeder()
	{
	}

	// Use this for initialization
	public override void Start ()
	{
		base.Start();

		// Load default attributes from EnemyData for this enemy
		//SetEnemyData(GameDataManager.Instance.EnemyDataMngr.FindEnemyDataByDisplayName("Light Speeder"));

		this.transform.LookAt(MiningFacilityObject.transform.position, Up);

		// Start the Light Speeder off the ground
		this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -0.5f);
	}
	
	#region MessageHandling
	protected override void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[LightSpeeder] " + message);
	}
	
	protected override void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[LightSpeeder] " + message);
	}
	#endregion
}
