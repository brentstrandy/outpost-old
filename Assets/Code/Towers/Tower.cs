using UnityEngine;
using System.Collections;

public class Tower : MonoBehaviour
{
	public string Name;
	public bool ShowDebugLogs = true;
	public GameObject Pivot;

	// Tower Details
	public int Cost;
	public int ShutdownCost;
	public float Range;
	public float StartupTime;
	public float RateOfFire;
	public float ThraceiumDamage;
	public float BallisticDamaage;

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
				// Have the tower's pivot point look at the targeted enemy
				if(Pivot)
					Pivot.transform.LookAt(TargetedEnemy.transform.position, Vector3.up);

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
