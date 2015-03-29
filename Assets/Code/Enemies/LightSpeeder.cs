using UnityEngine;
using System.Collections;

public class LightSpeeder : Enemy
{
	public LightSpeeder()
	{

	}

	// Use this for initialization
	public override void Start ()
	{
		this.transform.LookAt(MiningFacilityObject.transform.position, Up);

		// Light Speeders can fire on the mining facility
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
			Debug.Log("[LightSpeeder] " + message);
	}
	
	protected override void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[LightSpeeder] " + message);
	}
	#endregion
}
