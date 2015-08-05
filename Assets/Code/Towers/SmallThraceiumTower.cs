using UnityEngine;
using System.Collections;

public class SmallThraceiumTower : Tower
{
	// Use this for initialization
	public override void Start()
	{
		base.Start();

		//EnemyCircleCollider = this.GetComponent<SphereCollider>();

		// Small Thraceium Tower will fire at enemies, start a coroutine to check (and fire) on enemies
		StartCoroutine("Fire");

		//EnemyCircleCollider.radius = TowerAttributes.Range;
	}
	
	// Update is called once per frame
	public override void Update()
	{
		base.Update();
	}

	#region Special Effects
	public override void InstantiateFire()
	{
		// Instantiate prefab for firing a shot
		if (FiringEffect)
		{
			GameObject effect = Instantiate(FiringEffect, EmissionPoint.transform.position, EmissionPoint.transform.rotation) as GameObject;
			effect.GetComponent<LaserFire>().Target = TargetedEnemy.transform;

			// TO DO: Instead of instantiating a new prefab all the time just use one prefab and reset it after each use
			// Set the color of the laser effect
			effect.GetComponent<Light>().color = PlayerColor;
			effect.GetComponent<LineRenderer>().material.SetColor("_Color", PlayerColor);
			effect.GetComponent<LineRenderer>().material.SetColor("_EmissionColor", PlayerColor);
		}
	}
	#endregion Special Effects

	#region MessageHandling
	protected override void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[SmallThraceiumTower] " + message);
	}
	
	protected override void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[SmallThraceiumTower] " + message);
	}
	#endregion
}
