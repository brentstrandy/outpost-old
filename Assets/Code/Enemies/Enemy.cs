using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour 
{	
    public bool ShowDebugLogs = true;
	
    public string Name;
	public float Health;
    public float Speed = 0.0f;
	public float BallisticDefense;
	public float ThraceiumDefense;

	protected GameObject OutpostObject;

	public virtual void Awake()
	{
		OutpostObject = GameManager.Instance.MiningFacility;

		EnemyManager.Instance.AddActiveEnemy(this);
	}

	// Use this for initialization
	public virtual void Start () 
	{
		//OutpostObject = GameManager.Instance.OutpostObject;
	}

    // Update is called once per frame
    public virtual void Update()
    {

    }

    public virtual void FixedUpdate()
    {

    }

	public virtual void TakeDamage(float ballisticsDamage, float thraceiumDamage)
	{
		// Take damage from Ballistics and Thraceium
		Health -= (ballisticsDamage * BallisticDefense);
		Health -= (thraceiumDamage * ThraceiumDefense);

		// Check to see if enemy is dead
		if(Health <= 0)
			KillEnemy();
	}

	/// <summary>
	/// Destroy the enemy (only if the player is the master client)
	/// </summary>
	protected virtual void KillEnemy()
	{
		// Only kill the enemy if this is the master client
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
			SessionManager.Instance.DestroyObject(this.gameObject);
	}

	public void OnDestroy()
	{
		// Tell the enemy manager this enemy is being destroyed
		EnemyManager.Instance.RemoveActiveEnemy(this);
	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[Enemy] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[Enemy] " + message);
	}
	#endregion
}
