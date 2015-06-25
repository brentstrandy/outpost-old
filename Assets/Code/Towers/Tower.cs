using UnityEngine;
using System.Collections;

public class Tower : MonoBehaviour
{
	public string Name;
	public bool ShowDebugLogs = true;
	public GameObject Pivot;
	protected readonly Vector3 Up = new Vector3(0.0f, 0.0f, -1.0f);

	// Tower Details
	public int InstallCost;
	public float Range;
	public float StartupTime;
	public float RateOfFire;
	public float ThraceiumDamage;
	public float BallisticDamaage;
	public float TrackingSpeed; // Number of seconds it takes to lock onto a target

	public int NetworkViewID { get; private set; }

	protected Enemy TargetedEnemy = null;
	protected float TimeLastShotFired;

	// Components
	protected PhotonView ObjPhotonView;
	private GameObject Shot;

	#region Initialize
	public void Awake()
	{
        Shot = Resources.Load("SFX/" + this.Name.Replace(" ", string.Empty) + "Shot") as GameObject;
    }

	// Use this for initialization
	public virtual void Start () 
	{
		// Allow the first bullet to be fired immediately after the tower is instantiated
		TimeLastShotFired = 0;
		
		GetComponent<HexLocation>().ApplyPosition(); // Update the hex coordinate to reflect the spawned position
		
		ObjPhotonView = PhotonView.Get(this);
		NetworkViewID = ObjPhotonView.viewID;
		
		GameManager.Instance.TowerManager.AddActiveTower(this);
		
		// Make the Master Client the owner of this object (authoritative server)
		ObjPhotonView.TransferOwnership(SessionManager.Instance.GetMasterClientID());
	}

	/// <summary>
	/// Sets the tower's properties based on TowerData
	/// </summary>
	/// <param name="towerData">Tower data</param>
	public void SetTowerData(TowerData towerData, Color playerColor)
	{
		Name = towerData.DisplayName;
		InstallCost = towerData.InstallCost;
		Range = towerData.Range;
		StartupTime = towerData.StartupTime;
		RateOfFire = towerData.RateOfFire;
		ThraceiumDamage = towerData.ThraceiumDamage;
		BallisticDamaage = towerData.BallisticDamage;
		TrackingSpeed = towerData.TrackingSpeed;

		gameObject.GetComponent<Renderer>().material.color = playerColor;
	}
	#endregion

	// Update is called once per frame
	public virtual void Update()
	{
		if(TargetedEnemy)
		{
			// Have the tower's pivot point look at the targeted enemy
			if(Pivot)
				transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation(TargetedEnemy.transform.position - transform.position, Up), Time.deltaTime * TrackingSpeed );
		}
	}

	#region Identifying Target
	/// <summary>
	/// Trigger event used to identify when Enemies enter the tower's firing radius
	/// </summary>
	/// <param name="other">Collider definitions</param>
	protected virtual void OnTriggerStay(Collider other)
	{
		// Only Target enemies if this is the Master Client
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Only target a new enemy if an enemy isn't already targeted
			if(TargetedEnemy == null)
			{
				// Only target Enemy game objects
				if(other.tag == "Enemy")
				{
					// Tell all other clients to target a new enemy
					ObjPhotonView.RPC("TargetNewEnemy", PhotonTargets.Others, other.GetComponent<Enemy>().NetworkViewID);

					// Master Client targets the Enemy
					TargetedEnemy = other.gameObject.GetComponent<Enemy>();
				}
			}
		}
	}

	/// <summary>
	/// Trigger event used to identify when Enemies exit the tower's firing radius
	/// </summary>
	/// <param name="other">Collider definitions</param>
	protected virtual void OnTriggerExit(Collider other)
	{
		// Only Target enemies if this is the Master Client
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Only reset the target enemy if one is already targeted
			if(TargetedEnemy != null)
			{
				// Only reset the target if the object exiting is an enemy
				if(other.tag == "Enemy")
				{
					// Remove the targeted Enemy when the enemy leaves
					if(other.gameObject.GetComponent<Enemy>().Equals(TargetedEnemy))
					{
						// Tell all other clients that 
						ObjPhotonView.RPC("TargetNewEnemy", PhotonTargets.Others, -1);

						TargetedEnemy = null;
					}
				}
			}
		}
	}
	#endregion

	#region RPC Calls
	/// <summary>
	/// Tells the client to target a new enemy
	/// </summary>
	/// <param name="viewID">Network ViewID of the enemy to target. Pass -1 to set the target to null.</param>
	[PunRPC]
	protected virtual void TargetNewEnemy(int viewID)
	{
		// A viewID of -1 means there is no targeted enemy. Otherwise, find the enemy by the networkViewID
		if(viewID == -1)
			TargetedEnemy = null;
		else
			TargetedEnemy = GameManager.Instance.EnemyManager.FindEnemyByID(viewID);
	}
	
	/// <summary>
	/// RPC call to tell players to fire a shot
	/// </summary>
	[PunRPC]
	protected virtual void FireAcrossNetwork()
	{
		// Tell enemy to take damage
		TargetedEnemy.TakeDamage(BallisticDamaage, ThraceiumDamage);
		// Reset timer for tracking when to fire next
		TimeLastShotFired = Time.time;
		// Instantiate prefab
		Instantiate(Shot, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - 1.32f), this.transform.rotation);
	}
	#endregion

	#region Attacking
	/// <summary>
	/// Coroutine that constantly checks to see if tower is ready to fire upon an enemy
	/// </summary>
	protected virtual IEnumerator Fire()
	{
		// Infinite Loop FTW
		while(this)
		{
			// Only perform the act of firing if this is the Master Client
			if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
			{
				// Only fire if there is an enemy being targeted
				if(TargetedEnemy != null)
				{
					// Only fire if tower is ready to fire
					if(Time.time - TimeLastShotFired >= RateOfFire)
					{
						// Only fire if the tower is facing the enemy (or if the tower does not need to face the enemy)
						if(Vector3.Angle(this.transform.forward, TargetedEnemy.transform.position - this.transform.position) <= 8 || Pivot == null)
						{
							// Tell all clients to fire upon the enemy
							ObjPhotonView.RPC("FireAcrossNetwork", PhotonTargets.All, null);
						}
					}
				}
			}
			
			yield return 0;
		}
	}
	#endregion

	#region Message Handling
	protected virtual void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[Tower] " + message);
	}
	
	protected virtual void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[Tower] " + message);
	}
	#endregion
}
