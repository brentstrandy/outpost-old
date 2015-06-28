using UnityEngine;
using System.Collections;
using Settworks.Hexagons;

public class HeavySpeeder : Enemy
{
	public HeavySpeeder()
	{

	}

	// Use this for initialization
	public override void Start ()
	{
		base.Start();

		// Load default attributes from EnemyData for this enemy
		//SetEnemyData(GameDataManager.Instance.EnemyDataMngr.FindEnemyDataByDisplayName("Heavy Speeder"));

		this.transform.LookAt(GameManager.Instance.ObjMiningFacility.transform.position, Up);

		// Start the Heavy Speeder off the ground
		this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -0.5f);
	}

	public override void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Terrain")
		{
			// Destroy the enemy when it crashes to the ground
			DestroyEnemy();
		}
	}

	/// <summary>
	/// RPC Call to tell players to kill this enemy. Heavy Speeders respond in a special way - they careen out of the sky when they die
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
			Debug.Log("[HeavySpeeder] " + message);
	}
	
	protected override void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[HeavySpeeder] " + message);
	}
	#endregion
}
