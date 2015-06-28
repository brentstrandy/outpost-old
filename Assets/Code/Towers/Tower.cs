﻿using UnityEngine;
using System.Collections;

public class Tower : MonoBehaviour
{
	protected readonly Vector3 Up = new Vector3(0.0f, 0.0f, -1.0f);

	public bool ShowDebugLogs = true;

	// Tower Attributes/Details/Data
	public TowerData TowerAttributes;
	private float Health;
	public int NetworkViewID { get; private set; }

	protected Enemy TargetedEnemy = null;
	protected float TimeLastShotFired;
	
	public GameObject TurretPivot;

	// Components
	protected HealthBarController HealthBar;
	protected PhotonView ObjPhotonView;
	// Effects
	public GameObject FiringEffect;
	public GameObject ExplodingEffect;

	#region INITIALIZE
	public void Awake()
	{

    }

	// Use this for initialization
	public virtual void Start () 
	{
		// Allow the first bullet to be fired immediately after the tower is instantiated
		TimeLastShotFired = 0;

		// Update the hex coordinate to reflect the spawned position
		GetComponent<HexLocation>().ApplyPosition();

		// Track the newly added tower in the TowerManager
		GameManager.Instance.TowerManager.AddActiveTower(this);
		
		// Make the Master Client the owner of this object (authoritative server)
		ObjPhotonView.TransferOwnership(SessionManager.Instance.GetMasterClientID());
	}

	/// <summary>
	/// Sets the tower's properties based on TowerData
	/// </summary>
	/// <param name="towerData">Tower data</param>
	public void SetTowerData(TowerData towerData, Color playerColor)
	{
		TowerAttributes = towerData;
		Health = TowerAttributes.MaxHealth;

		ObjPhotonView = PhotonView.Get(this);
		NetworkViewID = ObjPhotonView.viewID;

		gameObject.GetComponent<Renderer>().material.color = playerColor;
	}
	#endregion

	// Update is called once per frame
	public virtual void Update()
	{
		// Perform actions if the tower is targeting an enemy
		if(TargetedEnemy)
		{
			// Have the tower's pivot point look at the targeted enemy
			if(TurretPivot)
				transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation(TargetedEnemy.transform.position - transform.position, Up), Time.deltaTime * TowerAttributes.TrackingSpeed );
		}
	}

	#region IDENTIFYING TARGETS
	/// <summary>
	/// Trigger event used to identify when Enemies enter the tower's firing radius
	/// </summary>
	/// <param name="other">Collider definitions</param>
	protected virtual void OnTriggerStay(Collider other)
	{
		// Only Target enemies if this is the Master Client
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Only target a new enemy if an enemy isn't already targeted
			if(TargetedEnemy == null)
			{
				// Only target Enemy game objects
				if(other.tag == "Enemy")
				{
					// Tell all other clients to target a new enemy
					ObjPhotonView.RPC("TargetNewEnemy", PhotonTargets.Others, other.GetComponent<Enemy>().NetworkViewID);

					// Master Client targets the Enemy
					TargetedEnemy = other.gameObject.GetComponent<Enemy>();
				}
			}
		}
	}

	/// <summary>
	/// Trigger event used to identify when Enemies exit the tower's firing radius
	/// </summary>
	/// <param name="other">Collider definitions</param>
	protected virtual void OnTriggerExit(Collider other)
	{
		// Only Target enemies if this is the Master Client
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Only reset the target enemy if one is already targeted
			if(TargetedEnemy != null)
			{
				// Only reset the target if the object exiting is an enemy
				if(other.tag == "Enemy")
				{
					// Remove the targeted Enemy when the enemy leaves
					if(other.gameObject.GetComponent<Enemy>().Equals(TargetedEnemy))
					{
						// Tell all other clients that 
						ObjPhotonView.RPC("TargetNewEnemy", PhotonTargets.Others, -1);

						TargetedEnemy = null;
					}
				}
			}
		}
	}
	#endregion

	#region RPC CALLS
	/// <summary>
	/// Tells the client to target a new enemy
	/// </summary>
	/// <param name="viewID">Network ViewID of the enemy to target. Pass -1 to set the target to null.</param>
	[PunRPC]
	protected virtual void TargetNewEnemy(int viewID)
	{
		// A viewID of -1 means there is no targeted enemy. Otherwise, find the enemy by the networkViewID
		if(viewID == -1)
			TargetedEnemy = null;
		else
			TargetedEnemy = GameManager.Instance.EnemyManager.FindEnemyByID(viewID);
	}
	
	/// <summary>
	/// RPC call to tell players to fire a shot
	/// </summary>
	[PunRPC]
	protected virtual void FireAcrossNetwork()
	{
		// Tell enemy to take damage
		TargetedEnemy.TakeDamage(TowerAttributes.BallisticDamage, TowerAttributes.ThraceiumDamage);
		// Reset timer for tracking when to fire next
		TimeLastShotFired = Time.time;
		// Instantiate prefab for firing a shot
		if(FiringEffect)
			Instantiate(FiringEffect, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - 1.32f), this.transform.rotation);
	}

	/// <summary>
	/// RPC Call to tell players the enemy needs to take damage
	/// </summary>
	/// <param name="ballisticsDamage">Ballistics damage</param>
	/// <param name="thraceiumDamage">Thraceium damage</param>
	[PunRPC]
	protected virtual void TakeDamageAcrossNetwork(float damage)
	{
		// Take damage
		Health = Mathf.Max(Health - damage, 0);
		
		// Only update the Health Bar if there is one to update
		if(HealthBar)
			HealthBar.UpdateHealthBar(Health);
	}
	
	/// <summary>
	/// RPC Call to tell players to kill the enemy
	/// </summary>
	[PunRPC]
	protected virtual void DieAcrossNetwork()
	{
		// Simply destroy the tower (show explosion and play sound)
		DestroyTower();
	}
	#endregion
	
	#region TAKE DAMAGE / DIE
	/// <summary>
	/// Tower takes damage and responds accordingly
	/// </summary>
	/// <param name="damage">Damage.</param>
	public virtual void TakeDamage(float damage)
	{
		// Only the master client dictates how to handle damage
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Take damage 
			Health = Mathf.Max(Health - damage, 0);
			
			// Only update the Health Bar if there is one to update
			if(HealthBar)
				HealthBar.UpdateHealthBar(Health);
			
			// Either tell all other clients the enemy is dead, or tell them to have the enemy take damage
			if (Health <= 0)
				ObjPhotonView.RPC("DieAcrossNetwork", PhotonTargets.All, null);
			else
				ObjPhotonView.RPC("TakeDamageAcrossNetwork", PhotonTargets.Others, damage);
		}
	}

	/// <summary>
	/// Destroy the Tower from all areas of the game
	/// </summary>
	protected virtual void DestroyTower()
	{
		// Instantiate a prefab to show the tower exploding
		if(ExplodingEffect)
			Instantiate(ExplodingEffect, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - 1.32f), this.transform.rotation);
		
		// Tell the enemy manager this enemy is being destroyed
		GameManager.Instance.TowerManager.RemoveActiveTower(this);
		
		// The GameObject must be destroyed or else the enemy will stay instantiated
		Destroy (this.gameObject);
	}
	#endregion

	#region ATTACKING
	/// <summary>
	/// Coroutine that constantly checks to see if tower is ready to fire upon an enemy
	/// </summary>
	protected virtual IEnumerator Fire()
	{
		// Infinite Loop FTW
		while(this)
		{
			// Only perform the act of firing if this is the Master Client
			if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
			{
				// Only fire if there is an enemy being targeted
				if(TargetedEnemy != null)
				{
					// Only fire if tower is ready to fire
					if(Time.time - TimeLastShotFired >= TowerAttributes.RateOfFire)
					{
						// Only fire if the tower is facing the enemy (or if the tower does not need to face the enemy)
						if(Vector3.Angle(this.transform.forward, TargetedEnemy.transform.position - this.transform.position) <= 8 || TurretPivot == null)
						{
							// Tell all clients to fire upon the enemy
							ObjPhotonView.RPC("FireAcrossNetwork", PhotonTargets.All, null);
						}
					}
				}
			}
			
			yield return 0;
		}
	}
	#endregion
	
	#region MESSAGE HANDLING
	protected virtual void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[Tower] " + message);
	}
	
	protected virtual void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[Tower] " + message);
	}
	#endregion
}
