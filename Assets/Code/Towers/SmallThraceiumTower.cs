using UnityEngine;

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

	public override void InstantiateExplosion()
	{
		if (ExplodingEffect)
		{
			GameObject effect = Instantiate(ExplodingEffect, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - 1.32f), Quaternion.Euler(0, 180, 0)) as GameObject;

			effect.GetComponentInChildren<Light>().color = PlayerColor;
			effect.GetComponent<ParticleSystemRenderer>().material.SetColor("_Color", PlayerColor);
			effect.GetComponent<ParticleSystemRenderer>().material.SetColor("_EmmissionColor", PlayerColor);
		}
	}

    #endregion Special Effects

    #region MessageHandling

    protected override void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[SmallThraceiumTower] " + message);
    }

    protected override void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[SmallThraceiumTower] " + message);
    }

    #endregion MessageHandling
}