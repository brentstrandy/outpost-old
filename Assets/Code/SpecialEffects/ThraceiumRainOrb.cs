#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// An Orb emmited from the Thraceium Rain Tower that follows a parabolic trajectory.
/// Owner: John Fitzgerald
/// </summary>
public class ThraceiumRainOrb : MonoBehaviour
{
    public bool ShowDebugLogs = true;
	public GameObject Explosion;

	private Vector3 Target;
	private bool HitTarget = false;
	private float HitTargetTime;

    private float StartTime;

	public void SetData(Vector3 source, Vector3 target, Color playerColor)
	{
		Target = target;
		this.GetComponent<Renderer>().material.color = playerColor;
		this.GetComponent<Rigidbody>().AddForce(CalculateShotForce(source, target, 1.5f), ForceMode.VelocityChange);
	}

    private void Update()
    {
		// Travelling through the air
		if(!HitTarget)
		{
			// Check to see if orb has hit final target
			if(Vector3.Distance(this.transform.position, Target) <= 0.5f)
			{
				// TO DO: Show explosion
				// Expand orb to represent explosion - in the future this should probably be and explosion 
				// from another prefab that is instantiated.
				this.transform.localScale *= 5;
				this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
				this.gameObject.AddComponent<SphereCollider>();
				HitTarget = true;
				HitTargetTime = Time.time;
	        }
		}
		// Post-explosion
		else
		{
			// Self destruct after a short period of time in order to show the player the orb
			if(Time.time - HitTargetTime >= 0.5f)
				Destroy(this.gameObject);
		}
    }

	private Vector3 CalculateShotForce(Vector3 origin, Vector3 target, float timeToTarget)
	{
		// Vi = (d / t) - ((a * t) / 2)

		Vector3 toTarget = target - origin;
		Vector3 toTargetXY = toTarget;
		toTargetXY.z = 0;
		
		float ViZ = (toTarget.z / timeToTarget) - ((Physics.gravity.magnitude * timeToTarget) / 2);
		float ViXY = toTargetXY.magnitude / timeToTarget;
		
		Vector3 initialForce = toTargetXY.normalized;
		initialForce *= ViXY;
		initialForce.z = ViZ;

		return initialForce;
	}

    private void OnTriggerEnter(Collider other)
    {
        // Only take action if this is the Master Client
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            // Only enact TRT effects when the TRT orb explodes
            if (HitTarget)
            {
                // Only affect Enemies
                if (other.tag == "Enemy")
                {
                    // Tell enemy to take damage (only the Master Client can do this)
                    if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
                    {
                        // load attributes of Thraceium Rain Orb here
                        TowerData _TRT_Data = GameDataManager.Instance.FindTowerDataByDisplayName("Thraceium Rain Tower");
                        other.gameObject.GetComponent<Enemy>().TakeDamage(_TRT_Data.BallisticDamage, _TRT_Data.ThraceiumDamage, SessionManager.Instance.GetPlayerInfo());
                    }
                }
            }
        }
    }

    #region MessageHandling

    private void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[ThraceiumRainOrb] " + message);
    }

    private void LogError(string message)
    {
        Debug.LogError("[ThraceiumRainOrb] " + message);
    }

    #endregion MessageHandling
}