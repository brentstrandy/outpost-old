using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EMPTower_Orb : MonoBehaviour
{
    public bool ShowDebugLogs = true;

	private readonly Vector3 Up = new Vector3(0.0f, 0.0f, -1.0f);
	private Vector3 TargetPosition;
	private bool HitTarget = false;
	private float StartTime;
	private float MaxSize;
	private float Speed;
	private float Duration;

	public void SetData(Vector3 targetPosition, float speed, float size, float duration)
	{
		TargetPosition = targetPosition;
		this.transform.LookAt(targetPosition, Up);
		Speed = speed;
		MaxSize = size;
		Duration = duration;

		//gameObject.GetComponent<Renderer>().material.SetColor("", PlayerManager.Instance.
	}

	// Update is called once per frame
	void Update ()
	{
		// Don't deal damage while flying through the air
		if(!HitTarget)
		{
			// Check to see if the orb hit its target location
			if(Vector3.Distance(this.transform.position, TargetPosition) <= 1)
			{
				// Expand the EMP and have it sit
				HitTarget = true;
				StartTime = Time.time;
			}
			else
			{
				this.transform.position += (this.transform.forward * Speed * Time.deltaTime);
			}
		}
		else
		{
			// EMP shot explodes when it hits the target
			if(this.transform.localScale.magnitude <= MaxSize)
				this.transform.localScale += (new Vector3(4, 4, 4) * Time.deltaTime);

			// Check to see if the Orb has reached its life span
			if(Time.time - StartTime > Duration)
				Destroy(this.gameObject);
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
                // Only effect Enemies
                if (other.tag == "Enemy")
                {
                    other.gameObject.GetComponent<Enemy>().Stunned(3f);
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
