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
		Name = "Drone";
		Speed = 15.0f;
		Health = 5.0f;
		this.transform.LookAt(OutpostObject.transform.position, Vector3.up);
	}
	
	// Update is called once per frame
	public override void Update ()
	{

	}


	public override void FixedUpdate()
	{
		if(Target != null)
			this.transform.LookAt(Target.transform.position, Vector3.up);
		else
			this.transform.LookAt(OutpostObject.transform.position, Vector3.up);

		GetComponent<Rigidbody>().AddForce(this.transform.forward * Time.fixedDeltaTime * Speed, ForceMode.Force);
	}

	public void OnTriggerEnter(Collider other)
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
	}
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[Drone] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[Drone] " + message);
	}
	#endregion
}
