using UnityEngine;
using System.Collections;

public class Drone : Enemy
{
	public Drone()
	{

	}

	// Use this for initialization
	public override void Start ()
	{
		Name = "Drone";
		Speed = 2.0f;
		this.transform.LookAt(OutpostObject.transform.position, Vector3.up);
	}
	
	// Update is called once per frame
	public override void Update ()
	{

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

		GetComponent<Rigidbody>().AddForce(this.transform.forward * Time.fixedDeltaTime, ForceMode.Force);
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
