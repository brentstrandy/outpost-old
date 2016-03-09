using UnityEngine;

public class HammerTower : Tower
{
    // Use this for initialization
    public override void Start()
    {
        base.Start();

        //EnemyCircleCollider = this.GetComponent<SphereCollider>();

        // Hammer Tower will attack any enemy within its range, start a coroutine to check (and fire) on enemies
        StartCoroutine("Fire");

        //EnemyCircleCollider.radius = TowerAttributes.Range;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

	#region EVENTS

	public void OnHammerImpact()
	{
		InstantiateFire();
	}

	#endregion

    #region Special Effects

	public override void InstantiateFire()
	{
		// Instantiate prefab for firing a shot
		if (FiringEffect)
		{
			Instantiate(FiringEffect, EmissionPoint.transform.position, Quaternion.identity);
		}
	}

    #endregion Special Effects

	#region RPC Calls

	/// <summary>
	/// Hammer tower does not technically "fire" until the hammer hits the pads. The animation system will tell us when to fire
	/// </summary>
	[PunRPC]
	protected override void FireAcrossNetwork()
	{
		// Tell the tower Animator the tower has fired
		ObjAnimator.SetTrigger("Shot Fired");

		// After the tower fires it loses the ability to fire again until after a cooldown period
		ReadyToFire = false;
	}

	#endregion

    #region MessageHandling

    protected override void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[HammerTower] " + message);
    }

    protected override void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[HammerTower] " + message);
    }

    #endregion MessageHandling
}