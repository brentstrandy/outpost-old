using UnityEngine;
using System.Collections;
using Settworks.Hexagons;

public class LightSpeeder : Enemy
{
	public LightSpeeder()
	{
	}

	// Use this for initialization
	public override void Start ()
	{
		base.Start();

		this.transform.LookAt(MiningFacilityObject.transform.position, Up);

		// Light Speeders can fire on the mining facility
		StartCoroutine("Fire");

		// Start the Light Speeder off the ground
		this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -0.5f);
	}
	
	public override void FixedUpdate()
	{
		//GetComponent<Pathfinder>().Solve();
	}
	
	public override void Update()
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

		// Land speeders don't move while they are firing
		if (!Firing)
		{
			var pathfinder = GetComponent<Pathfinder>();
			var location = GetComponent<HexLocation>().location;
			var next = pathfinder.Next();

			if (next != location)
			{
				Vector3 target = next.Position();
				// Do not allow the Z position to change or else the speeder will slowly move downward over time
				target.z = this.transform.position.z;
				transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation(target - transform.position, Up), Time.deltaTime * TurningSpeed );
			}
			else
			{
				Log("Arrived... Target: " + pathfinder.Target + " Next: " + pathfinder.Next() + " Location: " + location + " Path: " + pathfinder.PathToString());
			}

			this.transform.position += this.transform.forward * Speed * Time.deltaTime;
			//GetComponent<Rigidbody>().AddForce(this.transform.forward * Time.fixedDeltaTime, ForceMode.Force);
		}
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
