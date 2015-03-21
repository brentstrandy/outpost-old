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
		this.transform.LookAt(MiningFacilityObject.transform.position, Vector3.up);
	}

	public override void FixedUpdate()
	{
		if(Target != null)
			this.transform.LookAt(Target.transform.position, Vector3.up);
		else
			this.transform.LookAt(MiningFacilityObject.transform.position, Vector3.up);

		GetComponent<Rigidbody>().AddForce(this.transform.forward * Time.fixedDeltaTime * Speed, ForceMode.Force);
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
