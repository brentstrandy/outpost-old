using UnityEngine;
using System.Collections;

public class Drone : Enemy
{
	public Drone()
	{

	}

	// Use this for initialization
	public override void Start ()
	{
		base.Start();

		// Load default attributes from EnemyData for this enemy
		//SetEnemyData(GameDataManager.Instance.EnemyDataMngr.FindEnemyDataByDisplayName("Drone"));

		this.transform.LookAt(GameManager.Instance.ObjMiningFacility.transform.position, Up);

		// Start the Drone off the ground
		this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -1.0f);

		// TO DO: Implement the loading of EnemyData via XML before uncommenting
		// Load default attributes from EnemyData
		//EnemyData enemyData = GameDataManager.Instance.EnemyDataMngr.FindEnemyDataByPrefabName("Drone");
		//SetEnemyData(enemyData);
	}

	public override void OnTriggerEnter(Collider other)
	{
		// Only take action if the droid finds an enemy to follow
		if(other.tag == "Enemy")
		{
			// Only take action if the droid does not find another droid
			if(other.GetComponent<Enemy>().EnemyAttributes.DisplayName != "Drone")
			{
				// Only start following the enemy if the Drone isn't already following an enemy
				if(TargetedObjectToFollow == null)
				{
					TargetedObjectToFollow = other.gameObject;
				}
			}
		}
		// Check to see if the Enemy encounters the Mining Facility and - if so - explode on impact
		else if(other.tag == "Mining Facility")
		{
			GameManager.Instance.ObjMiningFacility.TakeDamage(EnemyAttributes.DamageDealt);
		}
	}

	public override void Update()
	{
		base.Update();

		if(this.transform.position.z >= 0)
			DestroyEnemy();
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
		
		// Stop sending network updates for this object - it is dead
		ObjPhotonView.ObservedComponents.Clear();
	}
	
	#region MessageHandling
	protected override void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[Drone] " + message);
	}
	
	protected override void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[Drone] " + message);
	}
	#endregion
}
