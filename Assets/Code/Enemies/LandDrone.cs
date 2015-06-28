using UnityEngine;
using System.Collections;

public class LandDrone : Enemy
{
	public LandDrone()
	{

	}

	// Use this for initialization
	public override void Start ()
	{
		base.Start();

		// Load default attributes from EnemyData for this enemy
		//SetEnemyData(GameDataManager.Instance.EnemyDataMngr.FindEnemyDataByDisplayName("Drone"));

		this.transform.LookAt(GameManager.Instance.ObjMiningFacility.transform.position, Up);

		// Start the Land Drone barely off the ground
		this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -0.1f);
	}

	#region MessageHandling
	protected override void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[LandDrone] " + message);
	}
	
	protected override void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[LandDrone] " + message);
	}
	#endregion
}
