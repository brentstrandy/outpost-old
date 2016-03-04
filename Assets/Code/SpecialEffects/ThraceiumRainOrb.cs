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
	public GameObject ExplosionEffect;

	private Vector3 Target;
	private bool HitTarget = false;
	private bool Exploded = false;
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
			// Check to see if the orb is about to hit the ground
			if(!Exploded && this.gameObject.GetComponent<Rigidbody>().velocity.z > 0 && Mathf.Abs(this.transform.position.z - GameManager.Instance.TerrainMesh.IntersectPosition(this.transform.position).z) <= 3.0f)
			{
				if(ExplosionEffect)
					Instantiate(ExplosionEffect, this.transform.position, Quaternion.identity);

				Exploded = true;
			}

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

/*
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
    public bool IsSetData = false;

    public Transform Target;
    public Vector3 TargetHexLocation;

    public float ProjectileSpeed = 50.0f;   // Doesn't do anything (same with EMP orb)
    public float ProjectileAngle;
    public float ExplosionSize = 4.0f;
    public float ExplosionDuration = 0.5f;

    #region 5TH PATHFINDING (Parabola)

    public float CompletionTime = 3f;
    public float ObjectTimer = 0f;
    public float ParabolaHeight = 5f;
    public int NumberOfParabolaPoints = 30;

    public Vector3 ParabolaStart;
    public Vector3 ParabolaMidpoint;
    public Vector3 ParabolaEnd;
    public Vector3 TravelDirection;

    #endregion 5TH PATHFINDING (Parabola)

    private readonly Vector3 Up = new Vector3(0.0f, 0.0f, -1.0f);
    private bool HitTarget = false;
    private float StartTime;

    public float ExplosionSpeed
    {
        get
        {
            return ExplosionSize / ExplosionDuration;
        }
    }

    public void SetData()
    {
        TargetHexLocation = new Vector3(Target.position.x, Target.position.y, Target.position.z + 0.3f);
        ProjectileAngle = 45f;
        //ProjectileAngleToEnemy = Vector3.Angle(TargetHexLocation - transform.position, transform.forward);

        if (Target != null)
        {
            ParabolaStart = this.transform.position;
            ParabolaEnd = TargetHexLocation;
            ParabolaMidpoint = (ParabolaStart + ParabolaEnd) / 2;
            ParabolaMidpoint.z -= ParabolaHeight;

            // direction eminating from the start vector to the end vector
            TravelDirection = ParabolaEnd - ParabolaStart;
        }

        // Output each orb's start, middle, and end vectors
        //LogError("Start: (" + ParabolaStart.x +  ", " + ParabolaStart.y + ", " + ParabolaStart.z + ")");
        //LogError("Middle: (" + ParabolaMidpoint.x +", " + ParabolaMidpoint.y +", " + ParabolaMidpoint.z +")");
        //LogError("End: (" + ParabolaEnd.x + ", " + ParabolaEnd.y + ", " + ParabolaEnd.z + ")");

        IsSetData = true;
    }

    private void Update()
    {
        if (!IsSetData)
            SetData();
        else
        {
            // Don't deal damage while flying through the air
            if (!HitTarget && Target)
            {
                // Check to see if the orb hit its target location
                if (Vector3.Distance(this.transform.position, TargetHexLocation) <= 0.5f)
                {
                    // Explosion
                    HitTarget = true;
                    StartTime = Time.time;

                    // Enable SphereCollider
                    SphereCollider collider = GetComponent<SphereCollider>();
                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                }
                // If the orb hasn't hit its target location, continuing traveling (modify ProjectileArch here)
                else
                {
                    #region 5TH PATHFINDING (Parabola)

                    // Set how long the projectile takes to reach it's end on a parabolic trajectory
                    //float objectTimer = (Time.time / CompletionTime) % 1;
                    ObjectTimer += 0.01f;
                    this.transform.position = SampleParabola(ObjectTimer);

                    #endregion 5TH PATHFINDING (Parabola)

                    #region 4TH PATHFINDING

                    //float distance = Vector3.Distance (transform.position, TargetHexLocation);

                    //Velocity = (transform.position - Previous).magnitude / Time.deltaTime;
                    //Previous = transform.position;
                    //float angle = (Mathf.Asin((Physics.gravity.magnitude * distance) / (Velocity * Velocity)) / 2) * 100;

                    #endregion 4TH PATHFINDING

                    #region 3RD PATHFINDING

                    //Velocity = (transform.position - Previous).magnitude / Time.deltaTime;
                    //Previous = transform.position;

                    ////MaxDist = ((Velocity * Velocity) * ((Mathf.Sin((-ProjectileAngleToEnemy * Mathf.Deg2Rad) * 2)))) / 9.81f;

                    //RayX = transform.position.x;

                    //RayZ = (0) + (RayX * (Mathf.Tan(-ProjectileAngleToEnemy * Mathf.Deg2Rad))) -
                    //       ((9.81f * (RayX * RayX)) / (((Mathf.Cos(-ProjectileAngleToEnemy * Mathf.Deg2Rad) * Velocity) *
                    //       (Mathf.Cos(-ProjectileAngleToEnemy * Mathf.Deg2Rad) * Velocity)) * 2f));

                    //transform.position = new Vector3();

                    #endregion 3RD PATHFINDING

                    #region 2ND PATHFINDING

                    //Vector3 targetDir = TargetHexLocation - transform.position;
                    //float distanceTraveled = ProjectileSpeed * Time.deltaTime;
                    ////Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);

                    //// Fix so ray eminates from the TRT
                    ////Debug.DrawRay(transform.position, newDir, Color.red);

                    ////transform.rotation = Quaternion.LookRotation(targetDir);
                    ////transform.position += this.transform.forward * distanceTraveled;
                    ////RB.AddForce(transform.forward * 50f);

                    //transform.position = Vector3.MoveTowards(transform.position, TargetHexLocation, distanceTraveled);

                    #endregion 2ND PATHFINDING

                    #region 1ST PATHFINDING

                    //float distanceTraveled = ProjectileSpeed * Time.deltaTime;
                    //if (Vector3.Distance(this.transform.position, Target.position) < distanceTraveled)
                    //{
                    //    // Stop at the target so that we don't go past it and fail to come within the contact threshold
                    //    this.transform.position = TargetHexLocation;
                    //}
                    //else
                    //{
                    //    // Arch towards the target

                    //    //this.transform.LookAt(Target.position, Up);
                    //    this.transform.position += this.transform.forward * distanceTraveled;
                    //}

                    #endregion 1ST PATHFINDING
                }
            }
            // Explodes when it hits the target
            else
            {
                this.transform.localScale += Vector3.one * ExplosionSpeed * Time.deltaTime;

                float duration = Time.time - StartTime;
                float completed = duration / ExplosionDuration;

                // Check to see if the Orb has reached its life span
                if (duration > ExplosionDuration)
                {
                    var collider = GetComponent<SphereCollider>();
                    if (collider != null && collider.radius > 0.01f)
                    {
                        collider.radius = 0.0f;
                    }
                    else
                    {
                        Destroy(this.gameObject);
                    }
                }
            }
        }
    }

    #region 5TH PATHFINDING (Parabola)

    #if UNITY_EDITOR
    /// <summary>
    /// Draw the trajectory parabola in Unity's Scene window
    /// </summary>
    private void OnDrawGizmos()
    {
        Handles.BeginGUI();
        GUI.skin.box.fontSize = 16;
        GUI.Box(new Rect(10, 10, 100, 25), ParabolaHeight + "");
        Handles.EndGUI();

        // Draw the parabola by sample a few times
        Gizmos.color = Color.red;
        Gizmos.DrawLine(ParabolaStart, ParabolaEnd);
        Vector3 lastPoint = ParabolaStart;

        // And attempt to draw a triangle between all three point
        //Gizmos.DrawLine(ParabolaStart, ParabolaMidpoint);
        //Gizmos.DrawLine(ParabolaMidpoint, ParabolaEnd);
        //Gizmos.DrawLine(ParabolaStart, ParabolaEnd);

        for (float i = 0; i < NumberOfParabolaPoints + 1; i++)
        {
            Vector3 point = SampleParabola(i / NumberOfParabolaPoints);
            //point.z = Mathf.Sin((i / NumberOfParabolaPoints) * Mathf.PI) * ParabolaMidpoint_Position.z;
            Gizmos.color = i % 2 == 0 ? Color.blue : Color.green;
            Gizmos.DrawLine(lastPoint, point);
            lastPoint = point;
        }
    }
    #endif

    /// <summary>
    /// Get position from a parabola defined by start and end vectors, height, and time
    /// </summary>
    private Vector3 SampleParabola(float time)
    {
        Vector3 result = ParabolaStart + (time * TravelDirection);
        //float parabolicT = time * 2 - 1;

        // start and end are roughly level
        if (Mathf.Abs(ParabolaStart.z - ParabolaEnd.z) < 0.1f)
        {
            //result.z -= (-parabolicT * parabolicT - 1) * ParabolaHeight;
            result.z -= Mathf.Sin(time * Mathf.PI) * ParabolaHeight;
        }
        // start and end are not level
        else
        {
            // a linear interpolation between (atop the X-Y plane) the start and end vectors, with the same z-coordinates
            Vector3 levelDirection = ParabolaEnd - new Vector3(ParabolaStart.x, ParabolaStart.y, ParabolaEnd.z);
            // a perpendicular line from the X-Y plane (levelDirection) and the travelDirection
            Vector3 right = Vector3.Cross(TravelDirection, levelDirection);
            // a perpendicular line from the
            Vector3 up = Vector3.Cross(right, levelDirection);
            //Vector3 up = Vector3.Cross(right, travelDirection);
            up.z = up.z * Up.z;

            // face up's vector the other direction if the end vector is higher than the start vector
            if (ParabolaEnd.z < ParabolaStart.z)
                up = -up;

            //result += ((-parabolicT * parabolicT + 1) * ParabolaHeight) * up.normalized;
            result += (Mathf.Sin(time * Mathf.PI) * ParabolaHeight) * up.normalized;
        }

        return result;
    }

    #endregion 5TH PATHFINDING (Parabola)

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
*/