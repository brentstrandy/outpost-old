using UnityEngine;
using System.Collections;

public class Tutorial_MiningFacility : MonoBehaviour
{
	public float TimeToDestroy;

	private float StartTime;
	
	void Start ()
	{
		StartTime = Time.time;

		// Align this Notification to the mining facility
		this.transform.position = GameManager.Instance.ObjMiningFacility.transform.position;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(Time.time - StartTime >= TimeToDestroy)
		{
			Destroy (this.gameObject);
		}
	}
}
