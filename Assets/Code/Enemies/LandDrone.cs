using UnityEngine;
using System.Collections;

public class LandDrone : Enemy
{
	private bool Alive;

	// Use this for initialization
	public override void Start ()
	{
		base.Start();

		Alive = true;

		// Load default attributes from EnemyData for this enemy
		//SetEnemyData(GameDataManager.Instance.EnemyDataMngr.FindEnemyDataByDisplayName("Drone"));

		this.transform.LookAt(GameManager.Instance.ObjMiningFacility.transform.position, Up);

		// Start the Land Drone barely off the ground
		this.transform.position = GameManager.Instance.TerrainMesh.IntersectPosition(this.transform.position, 0.1f);
	}

	public override void Update()
	{
		// Only run through the enemy's updates if it is alive.
		if(Alive)
		{
			base.Update();
		}
	}

	/// <summary>
	/// RPC Call to tell players to kill this enemy. Drones respond in a special way - they careen out of the sky when they die
	/// </summary>
	[PunRPC]
	protected override void DieAcrossNetwork()
	{
		Rigidbody rb = this.GetComponent<Rigidbody>();
		
		rb.constraints = RigidbodyConstraints.None;
		// TO DO: Fix this - Brent lost the original code and now he is too dumb to make it work again
		Vector3 temp = new Vector3(transform.position.x - 1, transform.position.y - 1, transform.position.z - 2);
		rb.AddForceAtPosition(new Vector3(0, 0, -20), temp);

		// Change the Tag to "Dead Enemy" so that towers do not target it
		this.tag = "Dead Enemy";

		Alive = false;

		// Hide the Healthbar if one exists
		if(HealthBar)
			HealthBar.HideHealthBar();

		// Tell the enemy to die in 1.5 seconds
		Invoke("DestroyEnemy", 1.5f);

		// Stop sending network updates for this object - it is dead
		ObjPhotonView.ObservedComponents.Clear();
	}
	
	#region Special Effects
	public override void InstantiateFire()
	{
		// Instantiate prefab for firing a shot
		if (FiringEffect)
		{
			GameObject effect = Instantiate(FiringEffect, EmissionPoint.transform.position, EmissionPoint.transform.rotation) as GameObject;
			if (TargetedObjectToAttack != null)
			{
				effect.GetComponent<LaserFire>().Target = TargetedObjectToAttack.transform;
			}
		}
	}
	#endregion Special Effects

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
