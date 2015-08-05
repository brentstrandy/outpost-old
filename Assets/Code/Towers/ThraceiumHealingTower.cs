using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThraceiumHealingTower : Tower
{
	private bool ReadyToHeal = true;
	public GameObject HealingEffect;

	// Use this for initialization
	public override void Start()
	{
		base.Start();
	}

	public override void Update()
	{
		// Do nothing
	}

	public override void OnCooldownAnimFinished()
	{
		// Only perform the act of healing if this is the Master Client
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
			ReadyToHeal = true;
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
		// Only perform the act of healing if this is the Master Client
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Only heal when the tower is ready to heal
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

		// Tell the tower Animator the tower has fired
		ObjAnimator.SetTrigger("Shot Fired");

		// Heal the tower
		tower.Heal(5.0f);

		// After healing, this tower cannot heal again until it has finished its cooldown
		ReadyToHeal = false;

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
