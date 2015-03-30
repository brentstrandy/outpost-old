using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour 
{	
    public bool ShowDebugLogs = true;
	
    public string Name;
	public float Health;
    public float Speed;
	public float Range;
	public float RateOfFire;
	public float DamageDealt;
	public float Acceleration;
	public float BallisticDefense;
	public float ThraceiumDefense;
	protected readonly Vector3 Up = new Vector3(0.0f, 0.0f, -1.0f);

	protected float TimeLastShotFired;
	protected bool Firing;
	protected Vector3 CurAcceleration;
	protected Vector3 CurVelocity;
	protected MiningFacility MiningFacilityObject;

	public virtual void Awake()
	{
		MiningFacilityObject = GameManager.Instance.MiningFacilityObject;

		// Allow the first bullet to be fired when the enemy is instantiated
		TimeLastShotFired = Time.time - (RateOfFire * 2);

		EnemyManager.Instance.AddActiveEnemy(this);
	}

	// Use this for initialization
	public virtual void Start () 
	{

	}

    // Update is called once per frame
    public virtual void Update()
    {

    }

    public virtual void FixedUpdate()
    {

    }

	protected void SetEnemyData(EnemyData enemyData)
	{
		Name = enemyData.DisplayName;
		Health = enemyData.Health;
		Speed = enemyData.Speed;
		Range = enemyData.Range;
		RateOfFire = enemyData.RateOfFire;
		DamageDealt = enemyData.DamageDealt;
		Acceleration = enemyData.Acceleration;
		BallisticDefense = enemyData.BallisticDefense;
		ThraceiumDefense = enemyData.ThraceiumDefense;
	}

	public virtual void TakeDamage(float ballisticsDamage, float thraceiumDamage)
	{
		// Take damage from Ballistics and Thraceium
		Health -= (ballisticsDamage * BallisticDefense);
		Health -= (thraceiumDamage * ThraceiumDefense);

		// Check to see if enemy is dead
		if(Health <= 0)
			KillEnemy();
	}

	/// <summary>
	/// Destroy the enemy (only if the player is the master client)
	/// </summary>
	protected virtual void KillEnemy()
	{
		// Only kill the enemy if this is the master client
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
			SessionManager.Instance.DestroyObject(this.gameObject);
	}

	public void OnDestroy()
	{
		// Tell the enemy manager this enemy is being destroyed
		EnemyManager.Instance.RemoveActiveEnemy(this);
	}

	public virtual void OnTriggerEnter(Collider other)
	{

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
