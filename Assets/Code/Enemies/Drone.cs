using UnityEngine;
using System.Collections;

public class Drone : Enemy
{
	private GameObject Target;

	public Drone()
	{

	}

	// Use this for initialization
	public override void Start ()
	{
		base.Start();

		// Load default attributes from EnemyData for this enemy
		SetEnemyData(GameDataManager.Instance.EnemyDataMngr.FindEnemyDataByDisplayName("Drone"));

		this.transform.LookAt(MiningFacilityObject.transform.position, Up);

		// Start the Drone off the ground
		this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -0.7f);

		// TO DO: Implement the loading of EnemyData via XML before uncommenting
		// Load default attributes from EnemyData
		//EnemyData enemyData = GameDataManager.Instance.EnemyDataMngr.FindEnemyDataByPrefabName("Drone");
		//SetEnemyData(enemyData);
	}

	public override void Update()
	{
		// MASTER CLIENT movement
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			if(Target != null)
				this.transform.LookAt(Target.transform.position, Up);
			else
				this.transform.LookAt(MiningFacilityObject.transform.position, Up);

			// Determine Acceleration
			CurAcceleration = this.transform.forward * Acceleration;

			// Determine Velocity
			CurVelocity += CurAcceleration * Time.deltaTime * Time.deltaTime * Speed;
			CurVelocity = Vector3.ClampMagnitude(CurVelocity, Speed);

			// Determine Position
			this.transform.position += CurVelocity * Time.deltaTime;
			//GetComponent<Rigidbody>().AddForce(this.transform.forward * Time.fixedDeltaTime * Speed, ForceMode.Force);
		}
	}

	public override void OnTriggerEnter(Collider other)
	{
		// Only take action if the droid finds an enemy to follow
		if(other.tag == "Enemy")
		{
			// Only take action if the droid does not find another droid
			if(other.GetComponent<Enemy>().Name != "Drone")
			{
				// Only start following the enemy if the Drone isn't already following an enemy
				if(Target == null)
				{
					Target = other.gameObject;
				}
			}
		}
		// Check to see if the Enemy encounters the Mining Facility and - if so - explode on impact
		else if(other.tag == "Mining Facility")
		{
			MiningFacilityObject.TakeDamage(DamageDealt);
		}
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
