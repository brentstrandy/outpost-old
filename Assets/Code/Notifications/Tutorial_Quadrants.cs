using UnityEngine;
using System.Collections;

public class Tutorial_Quadrants : MonoBehaviour
{
	private float StartTime;
	private float TimeToDestroy = 5;
	private bool QuadrantChanged = false;

	void Start ()
	{
		InputManager.Instance.OnQuadrantRotate += OnQuadrantChanged;

		StartTime = Time.time;

		// Align this Notification to the mining facility
		this.transform.position = GameManager.Instance.ObjMiningFacility.transform.position;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(QuadrantChanged && Time.time - StartTime >= TimeToDestroy)
		{
			Destroy (this.gameObject);
		}
	}

	private void OnQuadrantChanged(string direction)
	{
		QuadrantChanged = true;
	}
}
