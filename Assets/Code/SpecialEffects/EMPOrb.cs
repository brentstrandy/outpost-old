using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EMPOrb : MonoBehaviour
{
	public Transform Target;

	public bool ShowDebugLogs = true;
	public float ProjectileSpeed = 8.0f;
	public float ExplosionSize = 3.0f;
	public float ExplosionDuration = 1.0f;
	public float DistortionDissipationStart = 0.6f;

	private readonly Vector3 Up = new Vector3(0.0f, 0.0f, -1.0f);
	private bool HitTarget = false;
	private float StartTime;
	private float StartDistortion;

	public float ExplosionSpeed
	{
		get
		{
			return ExplosionSize / ExplosionDuration;
		}
	}

	public void SetData(float projectileSpeed, float explosionSize, float explosionDuration)
	{
		ProjectileSpeed = projectileSpeed;
		ExplosionSize = explosionSize;
		ExplosionDuration = explosionDuration;

		//gameObject.GetComponent<Renderer>().material.SetColor("", PlayerManager.Instance.
	}

	// Update is called once per frame
	void Update()
	{
		// Don't deal damage while flying through the air
		if (!HitTarget)
		{
			// Check to see if the orb hit its target location
			if (Vector3.Distance(this.transform.position, Target.position) <= 0.1f)
			{
				// Expand the EMP and have it sit
				HitTarget = true;
				StartTime = Time.time;

				var material = GetComponent<MeshRenderer>().material;
				if (material != null)
				{
					StartDistortion = material.GetInt("_BumpAmt");
				}

				var particles = GetComponent<ParticleSystem>();
				if (particles != null && !particles.isPlaying)
				{
					particles.Play();
				}
			}
			else
			{
				float distanceTraveled = ProjectileSpeed * Time.deltaTime;
				if (Vector3.Distance(this.transform.position, Target.position) < distanceTraveled)
				{
					// Stop at the target so that we don't go past it and fail to come within the contact threshold
					// FIXME: What if the target also moves in the same frame, but after this projectile does?
					this.transform.position = Target.position;
				}
				else
				{
					// Follow the target
					this.transform.LookAt(Target.position, Up);
					this.transform.position += this.transform.forward * distanceTraveled;
				}
			}
		}
		else
		{
			// EMP shot explodes when it hits the target
			this.transform.localScale += Vector3.one * ExplosionSpeed * Time.deltaTime;

			float duration = Time.time - StartTime;
			float completed = duration / ExplosionDuration;

			// Dissipate the distortion effect linearly after we're through a certain amount of the effect.
			if (completed > DistortionDissipationStart)
			{
				float dissipation = (completed - DistortionDissipationStart) / (1.0f - DistortionDissipationStart);
				float distortion = Mathf.Lerp(StartDistortion, 0, dissipation);

				var material = GetComponent<MeshRenderer>().material;
				if (material != null)
				{
					material.SetInt("_BumpAmt", (int)distortion);
				}
			}

			// Check to see if the Orb has reached its life span
			if (duration > ExplosionDuration)
			{
				Destroy(this.gameObject);
			}
		}
	}

    private void OnTriggerEnter(Collider other)
    {
        // Only take action if this is the Master Client
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            // Only enact EMP effects when the EMP shot explodes
            if (HitTarget)
            {
                // Only affect Enemies
                if (other.tag == "Enemy")
                {
                    other.gameObject.GetComponent<Enemy>().Stunned(true);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Only take action if this is the Master Client
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            // Only enact EMP effects when the EMP shot explodes
            if (HitTarget)
            {
                // Only affect Enemies
                if (other.tag == "Enemy")
                {
                    // Set how long the enemy will be stunned once it exits the sphere.
                    other.gameObject.GetComponent<Enemy>().Stunned(3f);
                    
                    Log("OnTriggerExit()");
                }
            }
        }
    }

    #region MessageHandling
    private void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[EMPTower_Orb] " + message);
    }

    private void LogError(string message)
    {
        Debug.LogError("[EMPTower_Orb] " + message);
    }
    #endregion
}
