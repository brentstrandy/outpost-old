using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerManager : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	private static TowerManager instance;

	private List<Tower> ActiveTowerList;

	#region INSTANCE (SINGLETON)
	/// <summary>
	/// Singleton - There can only be one
	/// </summary>
	/// <value>The instance.</value>
	public static TowerManager Instance
	{
		get
		{
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType<TowerManager>();
			}
			
			return instance;
		}
	}
	
	void Awake()
	{
		instance = this;
	}
	#endregion

	// Use this for initialization
	void Start ()
	{
		ActiveTowerList = new List<Tower>();
	}
	
	// Update is called once per frame
	void Update ()
	{

	}

	public int ActiveTowerCount()
	{
		return ActiveTowerList.Count;
	}

	public void AddActiveTower(Tower tower)
	{
		ActiveTowerList.Add(tower);
	}

	public void RemoveActiveTower(Tower tower)
	{
		ActiveTowerList.Remove (tower);
	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[TowerManager] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[TowerManager] " + message);
	}
	#endregion
}
