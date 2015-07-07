using UnityEngine;
using System.Collections;

public class Tutorial_MiningFacility : Notification
{	
	public override void Start ()
	{
		base.Start();

		// Align this Notification to the mining facility
		this.transform.position = GameManager.Instance.ObjMiningFacility.transform.position;
	}
	
	// Update is called once per frame
	public override void Update ()
	{
		if(Time.time - StartTime >= TimeToDestroy)
		{
			Destroy (this.gameObject);
		}
	}
}
