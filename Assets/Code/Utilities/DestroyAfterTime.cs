using UnityEngine;
using System.Collections;

public class DestroyAfterTime : MonoBehaviour
{
	public float TimeToDestroy = 2.0f;
	private float StartTime;

	// Use this for initialization
	void Start ()
	{
		StartTime = Time.time;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(Time.time - StartTime > TimeToDestroy)
			Destroy (this);
	}
}
