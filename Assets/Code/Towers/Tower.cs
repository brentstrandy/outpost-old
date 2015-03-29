using UnityEngine;
using System.Collections;

public class Tower : MonoBehaviour
{
	public string Name;
	public bool ShowDebugLogs = true;
	public GameObject Pivot;
	protected readonly Vector3 Up = new Vector3(0.0f, 0.0f, -1.0f);

	// Tower Details
	public int Cost;
	public int ShutdownCost;
	public float Range;
	public float StartupTime;
	public float RateOfFire;
	public float ThraceiumDamage;
	public float BallisticDamaage;
	public float TrackingSpeed; // Number of seconds it takes to lock onto a target

	protected Enemy TargetedEnemy = null;
	protected float TimeLastShotFired;

	// Use this for initialization
	public virtual void Start () 
	{
		// Allow the first bullet to be fired immediately after the tower is instantiated
		TimeLastShotFired = Time.time - (RateOfFire * 2);
	}
	
	// Update is called once per frame
	public virtual void Update()
	{
		if(TargetedEnemy)
		{
			// Have the tower's pivot point look at the targeted enemy
			if(Pivot)
				transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation(TargetedEnemy.transform.position - transform.position, Up), Time.deltaTime * TrackingSpeed );
		}
	}
	
	protected virtual void OnTriggerStay(Collider other)
	{
		if(other.tag == "Enemy")
		{
			if(TargetedEnemy == null)
				TargetedEnemy = other.gameObject.GetComponent<Enemy>();
		}
	}

	protected virtual void OnTriggerExit(Collider other)
	{
		if(TargetedEnemy != null)
		{
			if(other.tag == "Enemy")
			{
				// Remove the targeted Enemy when the enemy leaves
				if(other.gameObject.GetComponent<Enemy>().Equals(TargetedEnemy))
					TargetedEnemy = null;
			}
		}
	}

	protected IEnumerator Fire()
	{
		while(this)
		{
			if(TargetedEnemy)
			{
				if(Time.time - TimeLastShotFired >= RateOfFire)
				{
					TargetedEnemy.TakeDamage(BallisticDamaage, ThraceiumDamage);
					TimeLastShotFired = Time.time;
				}
			}

			yield return 0;
		}
	}

	#region MessageHandling
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
