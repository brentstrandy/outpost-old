using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThraceiumHealingTower : Tower
{
	private bool ReadyToHeal = false;
	public GameObject HealingEffect;

	// Use this for initialization
	public override void Start()
	{
		base.Start();

		//EnemyCircleCollider = this.GetComponent<SphereCollider>();

		//EnemyCircleCollider.radius = TowerAttributes.Range;
	}
	
	// Update is called once per frame
	public override void Update()
	{
		// Only perform the act of healing if this is the Master Client
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Only check to see if the tower is ready to heal again when it is not ready to heal
			if(ReadyToHeal == false)
			{
				// Only heal if tower is ready to heal
				if(Time.time - TimeLastShotFired >= (1 / TowerAttributes.RateOfFire))
				{
					ReadyToHeal = true;
				}
			}
		}
	}
	
	#region IDENTIFYING TARGETS
	protected override void OnTriggerEnter(Collider other)
	{

	}

	protected override void OnTriggerExit(Collider other)
	{

	}

	protected override void OnTriggerStay(Collider other)
	{
		if(ReadyToHeal)
		{
			if(other.tag == "Tower")
			{
				// Check to see if the tower needs to be healed
				if(!other.GetComponent<Tower>().HasFullHealth())
				{
					ReadyToHeal = false;
					// Tell all clients to heal the tower
					ObjPhotonView.RPC("HealAcrossNetwork", PhotonTargets.All, other.GetComponent<Tower>().NetworkViewID);
				}
			}
		}
	}
	#endregion

	#region RPC CALLS
	/// <summary>
	/// RPC call to tell players to heal a tower
	/// </summary>
	[PunRPC]
	private void HealAcrossNetwork(int viewID)
	{
		Tower tower = GameManager.Instance.TowerManager.FindTowerByID(viewID);

		// Heal the tower
		tower.Heal(5.0f);
		// Reset timer for tracking when to heal next
		TimeLastShotFired = Time.time;

		// Instantiate prefab for healing
		if(HealingEffect)
			Instantiate(HealingEffect, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - 1.32f), this.transform.rotation);
	}
	#endregion

	#region MessageHandling
	protected override void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[ThraceiumHealingTower] " + message);
	}
	
	protected override void LogError(string message)
	{
		Debug.LogError("[ThraceiumHealingTower] " + message);
	}
	#endregion
}
