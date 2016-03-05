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

    #region Special Effects

    #endregion Special Effects

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