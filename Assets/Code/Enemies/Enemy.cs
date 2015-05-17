using UnityEngine;
using System.Collections;
using Settworks.Hexagons;

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

	protected MiningFacility MiningFacilityObject;
	protected Quadrant CurrentQuadrant;

	// Components
	public HealthBarController HealthBar;
	private PhotonView ObjPhotonView;
	private Pathfinder ObjPathfinder = null;
	private HexLocation ObjHexLocation = null;

	public virtual void Awake()
	{
		// Save a reference to the center mining facility
		MiningFacilityObject = GameManager.Instance.MiningFacilityObject;

		// Save reference to PhotonView
		ObjPhotonView = gameObject.GetComponent<PhotonView>();

		// Allow the first bullet to be fired when the enemy is instantiated
		TimeLastShotFired = Time.time - (RateOfFire * 2);

		// Add the enemy to the EnemyManager object to track the Enemy
		EnemyManager.Instance.AddActiveEnemy(this);
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
	protected void SetEnemyData(EnemyData enemyData)
	{
		// Establish Pathfinding (if needed)
		if(enemyData.UsePathfinding)
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
				// Use Pathfinding for movement
				if(ObjPathfinder != null)
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
					}
				}
			}
			// CLIENT movement
			else
			{
				// Use pathfinding to predict movement
				if(ObjPathfinder != null)
				{
					transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation((Vector3)TargetHex - transform.position, Up), Time.deltaTime * TurningSpeed );
				}
			}

			// Manually change the Enemy's position
			this.transform.position += this.transform.forward * Speed * Time.deltaTime;
		}
	}

	public virtual void FixedUpdate()
	{

	}

	public virtual void TakeDamage(float ballisticsDamage, float thraceiumDamage)
	{
		// Take damage from Ballistics and Thraceium
		Health -= (ballisticsDamage * BallisticDefense);
		Health -= (thraceiumDamage * ThraceiumDefense);
		Health = Mathf.Max(Health, 0);

		// Only update the Health Bar if there is one to update
		if(HealthBar)
		{
			HealthBar.UpdateHealthBar(Health);

			// Save previous size in order to reposition the health bar to the left
			//float previousSize = HealthBar.transform.position.;
			//HealthBar.transform.localScale = new Vector3(Health / MaxHealth, HealthBar.transform.localScale.y, HealthBar.transform.localScale.z);
			// Move health bar in order to give the illusion that the health bar is losing width from right-to-left (not outside-to-inside)
			//HealthBar.transform.localPosition = new Vector3(HealthBar.transform.localPosition.x - (previousSize - HealthBar.transform.localScale.x), HealthBar.transform.localPosition.y, HealthBar.transform.localPosition.z);
		}

		// Check to see if enemy is dead
		if(Health <= 0)
			Die();
	}

	/// <summary>
	/// Destroy the enemy (only if the player is the master client)
	/// </summary>
	protected virtual void Die()
	{
		Rigidbody rb = this.GetComponent<Rigidbody>();

		rb.constraints = RigidbodyConstraints.None;
		rb.AddForce(new Vector3(100, 100, -100), ForceMode.Force);

		// Stop sending network updates for this object - it is dead
		ObjPhotonView.ObservedComponents.Clear();

		// Only kill the enemy if this is the master client
		//if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		//	SessionManager.Instance.DestroyObject(this.gameObject);
	}

	public void OnDestroy()
	{
		// Tell the enemy manager this enemy is being destroyed
		EnemyManager.Instance.RemoveActiveEnemy(this);
	}

	public virtual void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Terrain")
		{
			// Only kill the enemy if this is the master client
			if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
				SessionManager.Instance.DestroyObject(this.gameObject);
		}
	}

	/// <summary>
	/// Coroutine that allows the enemy to fire upon the Mining Facility when in range
	/// </summary>
	protected IEnumerator Fire()
	{
		while(this)
		{
			// If the enemy is within range of the mining faclity, then open fire
			if(Vector3.Distance(this.transform.position, MiningFacilityObject.transform.position) <= Range)
			{
				if(Time.time - TimeLastShotFired >= RateOfFire)
				{
					MiningFacilityObject.TakeDamage(DamageDealt);
					TimeLastShotFired = Time.time;
				}
				Firing = true;
			}
			else
				Firing = false;

			yield return 0;
		}
	}

	[RPC]
	protected void UpdateEnemyTargetHex(Vector2 newTargetHex)
	{
		Debug.Log("New Target: " + newTargetHex);
		TargetHex = newTargetHex;
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
