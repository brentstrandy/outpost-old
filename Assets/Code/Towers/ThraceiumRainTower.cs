using UnityEngine;

public class ThraceiumRainTower : Tower
{
    // Use this for initialization
    public override void Start()
    {
        base.Start();

        // Thraceium Rain Tower will fire at enemies, start a coroutine to check (and fire) on enemies
        StartCoroutine("Fire");
    }

    /// <summary>
    /// RPC call to tell players to fire a shot. Thraceium Rain tower is unique in that is launches an orb and does not
    /// do immediate direct damage to an enemy
    /// </summary>
    [PunRPC]
    protected override void FireAcrossNetwork()
    {
        // After the tower fires it loses the ability to fire again until after a cooldown period
        ReadyToFire = false;
        // Tell the tower Animator the tower has fired
        ObjAnimator.SetTrigger("Shot Fired");

        // Instantiate prefab for firing an orb
        GameObject go = Instantiate(FiringEffect, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - 1.32f), this.transform.rotation) as GameObject;

        // Instantiate the orb
		go.GetComponent<ThraceiumRainOrb>().SetData(go.transform.position, TargetedEnemy.transform.position, PlayerColor);
    }

    #region MessageHandling

    protected override void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[ThraceiumRainTower] " + message);
    }

    protected override void LogError(string message)
    {
        Debug.LogError("[ThraceiumRainTower] " + message);
    }

    #endregion MessageHandling
}