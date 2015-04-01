using UnityEngine;
using System.Collections;

public class HeavySpeeder : Enemy
{
	public HeavySpeeder()
	{

	}

	// Use this for initialization
	public override void Start ()
	{
		base.Start();

		this.transform.LookAt(MiningFacilityObject.transform.position, Up);

		// Heavy Speeders can fire on the mining facility
		StartCoroutine("Fire");
	}

	public override void FixedUpdate()
	{
		/*RaycastHit hit;

		foreach(GameObject obj in HoverLocation)
		{
			if (Physics.Raycast (obj.transform.position, Vector3.down, out hit, 1.0f))
			{
				Log("HIT: " + Vector3.up * (HoverSpeed * (1.0f - hit.distance)) * Time.fixedDeltaTime);
				rigidbody.AddForceAtPosition(Vector3.up * (HoverSpeed * (1.0f - hit.distance)) * Time.fixedDeltaTime, obj.transform.position, ForceMode.Force);
			}
		}*/

		if(!Firing)
			this.transform.position += this.transform.forward * Speed * Time.deltaTime;
			//GetComponent<Rigidbody>().AddForce(this.transform.forward * Time.fixedDeltaTime, ForceMode.Force);
	}
	
	#region MessageHandling
	protected override void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[HeavySpeeder] " + message);
	}
	
	protected override void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[HeavySpeeder] " + message);
	}
	#endregion
}
