using UnityEngine;

public class Kamikaze : Enemy
{
    private bool Alive;

    // Use this for initialization
    public override void Start()
    {
        base.Start();

        Alive = true;

        // Load default attributes from EnemyData for this enemy
        //SetEnemyData(GameDataManager.Instance.EnemyDataMngr.FindEnemyDataByDisplayName("Light Speeder"));

        this.transform.LookAt(GameManager.Instance.ObjMiningFacility.transform.position, Up);

        // Start the Kamikaze off the ground
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -0.5f);
    }

    public override void Update()
    {
        // Only run through the enemy's updates if it is alive.
        if (Alive)
        {
            base.Update();
        }
    }

    /// <summary>
    /// RPC Call to tell players to kill this enemy. Drones respond in a special way - they careen out of the sky when they die
    /// </summary>
    [PunRPC]
    protected override void DieAcrossNetwork()
	{
        // Change the Tag to "Dead Enemy" so that towers do not target it
        this.tag = "Dead Enemy";

        Alive = false;

        // Hide the Healthbar if one exists
        if (HealthBar)
            HealthBar.HideHealthBar();

		// TODO: Show kamikaze orb at the brink of exploding

        // Tell the enemy to explode after a short wait
        Invoke("DestroyEnemy", 1.0f);
		
        // Stop sending network updates for this object - it is dead
        ObjPhotonView.ObservedComponents.Clear();
    }

    #region MessageHandling
    protected override void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[Kamizake] " + message);
    }

    protected override void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[Kamikaze] " + message);
    }

    #endregion MessageHandling
}