using UnityEngine;
using System.Collections;
using Settworks.Hexagons;
using System.Collections.Generic;

public class Enemy : MonoBehaviour 
{	
    public bool ShowDebugLogs = true;
	protected readonly Vector3 Up = new Vector3(0.0f, 0.0f, -1.0f);

	// Enemy Attributes/Details/Data
	public EnemyData EnemyAttributes;
	protected float Health;

	protected float TimeLastShotFired;
	protected bool FiringOnMiningFacility;
	protected bool FiringOnTower;
	protected Vector3 CurAcceleration;
	protected Vector3 CurVelocity;
	protected Vector2 TargetHex;

	public int NetworkViewID { get; private set; }

	// Pathfinding
	protected PathFindingType PathFinding;
	protected GameObject TargetedObjectToFollow;
	protected GameObject TargetedObjectToAttack;

	//protected Tower TargetedTower;
	//protected MiningFacility MiningFacilityObject;
	protected Quadrant CurrentQuadrant;

	// Components
	public HealthBarController HealthBar;
	public GameObject TurretPivot;
	protected PhotonView ObjPhotonView;
	protected Pathfinder ObjPathfinder = null;
	protected HexLocation ObjHexLocation = null;

	// Effects
	public GameObject FiringEffect;
	public GameObject ExplodingEffect;

	#region INITIALIZATION
	public virtual void Awake()
	{

    }

	// Use this for initialization
	public virtual void Start()
	{
		// Add the enemy to the EnemyManager object to track the Enemy
		GameManager.Instance.EnemyManager.AddActiveEnemy(this);

		var hexLocation = GetComponent<HexLocation>();
		if (hexLocation != null)
		{
			hexLocation.ApplyPosition();
		}
	}

	/// <summary>
	/// Instantiates the Enemy based on EnemyData. The Enemy will create Components where needed
	/// </summary>
	/// <param name="enemyData">EnemyData to be used when instantiating Enemy</param>
	public void SetEnemyData(EnemyData enemyData)
	{
		// Save reference to PhotonView
		ObjPhotonView = PhotonView.Get (this);
		NetworkViewID = ObjPhotonView.viewID;

		// PhotonView does not instantiate the ObservedComponents list - you must instantiate this list before attempting
		// to add any items into it.
		ObjPhotonView.ObservedComponents = new List<Component>();
		ObjPhotonView.synchronization = ViewSynchronization.Off;

		PathFinding = enemyData.PathFinding;

		// Allow the first bullet to be fired when the enemy is instantiated
		TimeLastShotFired = Time.time - (EnemyAttributes.RateOfFire * 2);

		// Initialize the proper pathfinding
		if(PathFinding == PathFindingType.ShortestPath)
		{
			if(gameObject.GetComponent<Pathfinder>() == null)
				ObjPathfinder = gameObject.AddComponent<Pathfinder>();
			else
				ObjPathfinder = gameObject.GetComponent<Pathfinder>();

			// By default, avoid towers
			ObjPathfinder.AvoidTowers = true;

			if(gameObject.GetComponent<HexLocation>() == null)
				ObjHexLocation = gameObject.AddComponent<HexLocation>();
			else
				ObjHexLocation = gameObject.GetComponent<HexLocation>();

			ObjHexLocation.layout = HexCoord.Layout.Horizontal;
			ObjHexLocation.autoSnap = false;
			ObjHexLocation.gridScale = 1;

			TargetHex = ObjPathfinder.Next().Position();
		}
		else
		{
			ObjPhotonView.synchronization = ViewSynchronization.ReliableDeltaCompressed;
			ObjPhotonView.ObservedComponents.Add(this.transform);
			ObjPhotonView.onSerializeTransformOption = OnSerializeTransform.PositionAndRotation;
		}

		// Start a coroutine to attack the mining facility and towers (if applicable)
		if(EnemyAttributes.AttackMiningFacility || EnemyAttributes.AttackTowers)
			StartCoroutine("Fire");

		Health = enemyData.MaxHealth;

		// Only initialize the health bar if it is used for this enemy
		if(HealthBar)
			HealthBar.InitializeBars(EnemyAttributes.MaxHealth);

		// Set the enemy's highlight color
		gameObject.GetComponent<Renderer>().material.color = enemyData.HighlightColor;
	}
	#endregion

	public virtual void Update()
	{
		// Units cannot move if they are firing on Mining Facility - AND - 
		// Units can only move while firing on a tower IF that ability has been enabled
		if((TargetedObjectToAttack != null && EnemyAttributes.AttackWhileMoving) || TargetedObjectToAttack == null)
		{
			// MASTER CLIENT movement
			if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
			{
				// Use Shortest Path for movement
				if(PathFinding == PathFindingType.ShortestPath)
				{
					// SJODING: What does this check accomplish?
					if(ObjPathfinder.Next() != ObjHexLocation.location)
					{
						// Check to see if the pathfinder is targetting a new hex in its path
						if(TargetHex != ObjPathfinder.Next().Position())
						{
							// Tell all players (including the MASTER CLIENT) that there is a new target hex
							ObjPhotonView.RPC("UpdateEnemyTargetHex", PhotonTargets.All, ObjPathfinder.Next().Position());
						}

						// Slerp the Enemy's rotation to look at the new Hex location
						transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation((Vector3)TargetHex - transform.position, Up), Time.deltaTime * EnemyAttributes.TurningSpeed );

						// The enemy's current velocity is always the speed (during this pathfinding) - it does not use acceleration
						CurVelocity = transform.forward * EnemyAttributes.Speed;
					}
				}
				// Track Friendly while Ignoring Pathfinding for movement
				else if(PathFinding == PathFindingType.TrackFriendly_IgnorePath)
				{
					if(TargetedObjectToFollow != null)
						transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation(TargetedObjectToFollow.transform.position - transform.position, Up), Time.deltaTime * EnemyAttributes.TurningSpeed );
					else
						transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation(GameManager.Instance.ObjMiningFacility.transform.position - transform.position, Up), Time.deltaTime * EnemyAttributes.TurningSpeed );
					
					// Determine Acceleration
					CurAcceleration = this.transform.forward * EnemyAttributes.Acceleration;
					
					// Determine Velocity
					CurVelocity += CurAcceleration * Time.deltaTime * Time.deltaTime * EnemyAttributes.Speed;
					CurVelocity = Vector3.ClampMagnitude(CurVelocity, EnemyAttributes.Speed);
				}
			}
			// CLIENT movement
			else
			{
				// Use pathfinding to predict movement
				if(ObjPathfinder != null)
				{
					transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation((Vector3)TargetHex - transform.position, Up), Time.deltaTime * EnemyAttributes.TurningSpeed );

					// The enemy's current velocity is always the speed (during this pathfinding)
					CurVelocity = transform.forward * EnemyAttributes.Speed;
				}
			}

			// Do not allow the Z axis to be updated - that is static
			CurVelocity.z = 0;
			// Manually change the Enemy's position
			this.transform.position += CurVelocity * Time.deltaTime;
		}
	}

	#region IDENTIFYING TARGETS
	public virtual void OnTriggerStay(Collider other)
	{
		// Only Target enemies if this is the Master Client
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Only target a new object if an object isn't already targeted
			if(TargetedObjectToAttack == null)
			{
				// Only target a tower game object if the enemy has that ability
				if(other.tag == "Tower" && EnemyAttributes.AttackTowers)
				{
					// Tell all other clients to target a new object
					ObjPhotonView.RPC("TargetNewObjectToAttack", PhotonTargets.Others, other.GetComponent<Enemy>().NetworkViewID);
					
					// Master Client targets the object
					TargetedObjectToAttack = other.gameObject;
				}
				/*else if(other.tag == "MiningFacility" && EnemyAttributes.AttackMiningFacility)
				{
					// Tell all other clients to target a new enemy
					ObjPhotonView.RPC("TargetNewObjectToAttack", PhotonTargets.Others, other.GetComponent<Enemy>().NetworkViewID);
					
					// Master Client targets the Enemy
					TargetedObjectToAttack = GameManager.Instance.ObjMiningFacility;
				}*/
			}
		}
	}
	
	/// <summary>
	/// Trigger event used to identify when objects exit the enemy's firing radius
	/// </summary>
	/// <param name="other">Collider definitions</param>
	protected virtual void OnTriggerExit(Collider other)
	{
		// Only Target objects if this is the Master Client
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Only reset the targeted object if one is already targeted
			if(TargetedObjectToAttack != null)
			{
				// Only reset the target if the object exiting is a tower (mining facilities will never exit the enemy's trigger)
				if(other.tag == "Tower")
				{
					// Remove the targeted Tower when the enemy tower leaves the enemies range
					if(other.gameObject.GetComponent<Tower>().Equals(TargetedObjectToAttack.GetComponent<Tower>()))
					{
						// Tell all other clients that the enemy isnot targeting anyone
						ObjPhotonView.RPC("TargetNewObjectToAttack", PhotonTargets.Others, -1);
						
						TargetedObjectToAttack = null;
					}
				}
			}
		}
	}
	#endregion

	#region RPC CALLS
	/// <summary>
	/// Tells the client to target a new GameObject
	/// </summary>
	/// <param name="viewID">Network ViewID of the enemy to target. Pass -1 to set the target to null.</param>
	[PunRPC]
	protected virtual void TargetNewObjectToAttack(int viewID)
	{
		// A viewID of -1 means there is no targeted enemy. Otherwise, find the enemy by the networkViewID
		if(viewID == -1)
			TargetedObjectToAttack = null;
		else
			TargetedObjectToAttack = GameManager.Instance.TowerManager.FindTowerByID(viewID).gameObject;
	}

	/// <summary>
	/// RPC call to tell players to fire a shot
	/// </summary>
	[PunRPC]
	protected virtual void FireAcrossNetwork()
	{
		// Tell enemy to take damage
		TargetedObjectToAttack.GetComponent<Tower>().TakeDamage(EnemyAttributes.DamageDealt);
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
	protected virtual void TakeDamageAcrossNetwork(float ballisticsDamage, float thraceiumDamage)
	{
		// Take damage from Ballistics and Thraceium
		Health -= (ballisticsDamage * EnemyAttributes.BallisticDefense);
		Health -= (thraceiumDamage * EnemyAttributes.ThraceiumDefense);
		Health = Mathf.Max(Health, 0);
		
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
		// Simply destroy the enemy (show explosion and play sound)
		DestroyEnemy();
	}

	/// <summary>
	/// CLIENT: Tells the client the next hex to enemy is traveling towards
	/// </summary>
	/// <param name="newTargetHex">New target hex.</param>
	[PunRPC]
	protected void UpdateEnemyTargetHex(Vector2 newTargetHex)
	{
		TargetHex = newTargetHex;
	}
	#endregion

	#region TAKE DAMAGE / DIE
	public virtual void TakeDamage(float ballisticsDamage, float thraceiumDamage)
	{
		// Only the master client dictates how to handle damage
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Take damage from Ballistics and Thraceium
			Health -= (ballisticsDamage * EnemyAttributes.BallisticDefense);
			Health -= (thraceiumDamage * EnemyAttributes.ThraceiumDefense);
			Health = Mathf.Max(Health, 0);
			
			// Only update the Health Bar if there is one to update
			if(HealthBar)
				HealthBar.UpdateHealthBar(Health);
			
			// Either tell all other clients the enemy is dead, or tell them to have the enemy take damage
			if (Health <= 0)
				ObjPhotonView.RPC("DieAcrossNetwork", PhotonTargets.All, null);
			else
				ObjPhotonView.RPC("TakeDamageAcrossNetwork", PhotonTargets.Others, ballisticsDamage, thraceiumDamage);
		}
	}
	
	/// <summary>
	/// Forces the Enemy to die regardless of how much health is left
	/// </summary>
	public virtual void ForceInstantDeath()
	{
		// If this is the master client then they tell all other clients to destroy this enemy
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
			ObjPhotonView.RPC("DieAcrossNetwork", PhotonTargets.All, null);
	}

	/// <summary>
	/// Destroy the Enemy from all areas of the game
	/// </summary>
	protected virtual void DestroyEnemy()
	{
		// Instantiate a prefab containing an FMOD_OneShot of enemy's explosion sound.
		if(ExplodingEffect)
			Instantiate(ExplodingEffect, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - 1.32f), this.transform.rotation);
		
		// Tell the enemy manager this enemy is being destroyed
		GameManager.Instance.EnemyManager.RemoveActiveEnemy(this);
		
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
				if(TargetedObjectToAttack != null)
				{
					// Only fire if tower is ready to fire
					if(Time.time - TimeLastShotFired >= EnemyAttributes.RateOfFire)
					{
						// Only fire if the tower is facing the enemy (or if the tower does not need to face the enemy)
						if(Vector3.Angle(this.transform.forward, TargetedObjectToAttack.transform.position - this.transform.position) <= 8 || TurretPivot == null)
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
	
	public virtual void OnTriggerEnter(Collider other)
	{ }


	#region MessageHandling
	protected virtual void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[Enemy] " + message);
	}
	
	protected virtual void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[Enemy] " + message);
	}
	#endregion
}
