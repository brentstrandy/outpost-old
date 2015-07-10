﻿using UnityEngine;
using System.Collections;

public class Tutorial_AllQuadrants : Notification
{
	private bool QuadrantChanged = false;

	public override void Start ()
	{
		base.Start();

		InputManager.Instance.OnQuadrantRotate += OnQuadrantChanged;

		// Align this Notification to the mining facility
		this.transform.position = GameManager.Instance.ObjMiningFacility.transform.position;

		// Allow the player to navigate to the North Quadrant
		GameManager.Instance.AddAvailableQuadrant(Quadrant.North);
		// Allow the player to navigate to the East Quadrant
		GameManager.Instance.AddAvailableQuadrant(Quadrant.East);
		// Allow the player to navigate to the South Quadrant
		GameManager.Instance.AddAvailableQuadrant(Quadrant.South);
		// Allow the player to navigate to the West Quadrant
		GameManager.Instance.AddAvailableQuadrant(Quadrant.West);
	}
	
	// Update is called once per frame
	public override void Update ()
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