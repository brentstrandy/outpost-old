using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages a reference to all currently active Enemies
/// Created By: Brent Strandy
/// </summary>
public class EnemyManager : MonoBehaviour
{
	public bool ShowDebugLogs = true;

	private static EnemyManager instance;

	private List<Enemy> ActiveEnemyList;

	#region INSTANCE (SINGLETON)
	/// <summary>
	/// Singleton - There can only be one
	/// </summary>
	/// <value>The instance.</value>
	public static EnemyManager Instance
	{
		get
		{
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType<EnemyManager>();
			}
			
			return instance;
		}
	}
	
	void Awake()
	{
		instance = this;
	}
	#endregion

	public int ActiveEnemyCount()
	{
		return ActiveEnemyList.Count;
	}

	// Use this for initialization
	void Start ()
	{
		ActiveEnemyList = new List<Enemy>();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void AddActiveEnemy(Enemy enemy)
	{
		ActiveEnemyList.Add(enemy);
	}

	public void RemoveActiveEnemy(Enemy enemy)
	{
		ActiveEnemyList.Remove (enemy);
	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[EnemyManager] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[EnemyManager] " + message);
	}
	#endregion
}
