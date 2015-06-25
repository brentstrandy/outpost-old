using UnityEngine;
using System.Collections;
using Settworks.Hexagons;
using System.Collections.Generic;

public class Enemy : MonoBehaviour 
{	
    public bool ShowDebugLogs = true;
	protected readonly Vector3 Up = new Vector3(0.0f, 0.0f, -1.0f);

	// Enemy Stats
    public string Name;
	protected float Health;
	private float MaxHealth;
	protected float Speed;
	protected float Range;
	protected float RateOfFire;
	protected float DamageDealt;
	protected float Acceleration;
	protected float BallisticDefense;
	protected float ThraceiumDefense;
	protected float TurningSpeed; // Number of seconds it takes to lock onto a target
	
	protected float TimeLastShotFired;
	protected bool Firing;
	protected Vector3 CurAcceleration;
	protected Vector3 CurVelocity;
	protected Vector2 TargetHex;

	public int NetworkViewID { get; private set; }

	// Pathfinding
	protected PathFindingType PathFinding;
	protected GameObject TargetObject;

	protected MiningFacility MiningFacilityObject;
	protected Quadrant CurrentQuadrant;

	// Components
	public HealthBarController HealthBar;
	protected PhotonView ObjPhotonView;
	protected Pathfinder ObjPathfinder = null;
	protected HexLocation ObjHexLocation = null;

	private GameObject Shot;
    private GameObject Explosion;

	public virtual void Awake()
	{
		// Save a reference to the center mining facility
		MiningFacilityObject = GameManager.Instance.ObjMiningFacility;

		// Allow the first bullet to be fired when the enemy is instantiated
		TimeLastShotFired = Time.time - (RateOfFire * 2);

		// Add the enemy to the EnemyManager object to track the Enemy
		GameManager.Instance.EnemyManager.AddActiveEnemy(this);

        Shot = Resources.Load("SFX/" + this.Name.Replace(" ", string.Empty) + "Shot") as GameObject;
        Explosion = Resources.Load("SFX/" + this.Name.Replace(" ", string.Empty) + "Explosion") as GameObject;
    }

	// Use this for initialization
	public virtual void Start()
	{
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

		// Check to see if the enemy can shoot the mining facility
		if(enemyData.AttackMiningFacility)
			StartCoroutine("Fire");

		// Check to see if the enemy can shoot towers
		if(enemyData.AttackTowers)
		{};

		// Set Enemy Data
		Name = enemyData.DisplayName;
		Health = enemyData.Health;
		MaxHealth = Health;
		Speed = enemyData.Speed;
		Range = enemyData.Range;
		RateOfFire = enemyData.RateOfFire;
		DamageDealt = enemyData.DamageDealt;
		Acceleration = enemyData.Acceleration;
		BallisticDefense = enemyData.BallisticDefense;
		ThraceiumDefense = enemyData.ThraceiumDefense;
		TurningSpeed = enemyData.TurningSpeed;

		gameObject.GetComponent<Renderer>().material.color = enemyData.HighlightColor;

		// Only initialize the health bar if it is used for this enemy
		if(HealthBar)
			HealthBar.InitializeBars(MaxHealth);
	}

	public virtual void Update()
	{
		// Only move units that are not firing
		if(!Firing)
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
						transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation((Vector3)TargetHex - transform.position, Up), Time.deltaTime * TurningSpeed );

						// The enemy's current velocity is always the speed (during this pathfinding)
						CurVelocity = transform.forward * Speed;
					}
				}
				// Track Friendly while Ignoring Pathfinding for movement
				else if(PathFinding == PathFindingType.TrackFriendly_IgnorePath)
				{
					if(TargetObject != null)
						transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation(TargetObject.transform.position - transform.position, Up), Time.deltaTime * TurningSpeed );
					else
						transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation(MiningFacilityObject.transform.position - transform.position, Up), Time.deltaTime * TurningSpeed );
					
					// Determine Acceleration
					CurAcceleration = this.transform.forward * Acceleration;
					
					// Determine Velocity
					CurVelocity += CurAcceleration * Time.deltaTime * Time.deltaTime * Speed;
					CurVelocity = Vector3.ClampMagnitude(CurVelocity, Speed);
					
					// Determine Position
					//this.transform.position += CurVelocity * Time.deltaTime;
				}
			}
			// CLIENT movement
			else
			{
				// Use pathfinding to predict movement
				if(ObjPathfinder != null)
				{
					transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation((Vector3)TargetHex - transform.position, Up), Time.deltaTime * TurningSpeed );

					// The enemy's current velocity is always the speed (during this pathfinding)
					CurVelocity = transform.forward * Speed;
				}
			}

			// Do not allow the Z axis to be updated - that is static
			CurVelocity.z = 0;
			// Manually change the Enemy's position
			this.transform.position += CurVelocity * Time.deltaTime;
		}
	}

	public virtual void TakeDamage(float ballisticsDamage, float thraceiumDamage)
	{
		// Only the master client dictates how to handle damage
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Take damage from Ballistics and Thraceium
			Health -= (ballisticsDamage * BallisticDefense);
			Health -= (thraceiumDamage * ThraceiumDefense);
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
	/// RPC Call to tell players the enemy needs to take damage
	/// </summary>
	/// <param name="ballisticsDamage">Ballistics damage</param>
	/// <param name="thraceiumDamage">Thraceium damage</param>
	[PunRPC]
	protected virtual void TakeDamageAcrossNetwork(float ballisticsDamage, float thraceiumDamage)
	{
		// Take damage from Ballistics and Thraceium
		Health -= (ballisticsDamage * BallisticDefense);
		Health -= (thraceiumDamage * ThraceiumDefense);
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
	/// Forces the Enemy to die regardless of how much health is left
	/// </summary>
	public virtual void ForceInstantDeath()
	{
		// If this is the master client then they tell all other clients to destroy this enemy
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
			ObjPhotonView.RPC("DieAcrossNetwork", PhotonTargets.All, null);
	}

	public virtual void OnTriggerEnter(Collider other)
	{
		// By default, enemies do nothing when their trigger is flagged
	}

	/// <summary>
	/// Coroutine that allows the enemy to fire upon the Mining Facility when in range
	/// </summary>
	protected IEnumerator Fire()
	{
		while(this)
		{
			// Master Client - Responsible for showing effects AND making gameplay decisions
			if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
			{
				// If the enemy is within range of the mining faclity, then open fire
				if(Vector3.Distance(this.transform.position, MiningFacilityObject.transform.position) <= Range)
				{
					if(Time.time - TimeLastShotFired >= RateOfFire)
					{
						// Master client needs to inform the mining facility that it has been hit
						MiningFacilityObject.TakeDamage(DamageDealt);
						TimeLastShotFired = Time.time;
						Instantiate(Shot, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - 1.32f), this.transform.rotation);
					}
					Firing = true;
				}
				else
					Firing = false;
			}
			// Client is only responsible for showing effects - not for making gameplay decisions
			else
			{
				// TO DO: Have the Master Client tell the client when to fire next and have the client "predict" when to fire

				// If the enemy is within range of the mining faclity, then open fire
				if(Vector3.Distance(this.transform.position, MiningFacilityObject.transform.position) <= Range)
				{
					if(Time.time - TimeLastShotFired >= RateOfFire)
					{
						TimeLastShotFired = Time.time;
						Instantiate(Shot, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - 1.32f), this.transform.rotation);
					}
					Firing = true;
				}
				else
					Firing = false;
			}

			yield return 0;
		}
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

	/// <summary>
	/// Destroy the Enemy from all areas of the game
	/// </summary>
	protected virtual void DestroyEnemy()
	{
        // Instantiate a prefab containing an FMOD_OneShot of enemy's explosion sound.
        Instantiate(Explosion, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - 1.32f), this.transform.rotation);

		// Tell the enemy manager this enemy is being destroyed
		GameManager.Instance.EnemyManager.RemoveActiveEnemy(this);

		// The GameObject must be destroyed or else the enemy will stay instantiated
		Destroy (this.gameObject);
	}

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
