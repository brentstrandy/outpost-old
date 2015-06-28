using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Settworks.Hexagons;

public class TowerManager
{
	public bool ShowDebugLogs = true;

	private List<Tower> ActiveTowerList;
	private HashSet<HexCoord> TowerLocations = new HashSet<HexCoord>();

	// Use this for initialization
	public TowerManager()
	{
		ActiveTowerList = new List<Tower>();
	}

	public int ActiveTowerCount()
	{
		return ActiveTowerList.Count;
	}

	public void AddActiveTower(Tower tower)
	{
		ActiveTowerList.Add(tower);
		TowerLocations.Add(tower.gameObject.GetComponent<HexLocation>().location);
	}

	public void RemoveActiveTower(Tower tower)
	{
		ActiveTowerList.Remove(tower);
		TowerLocations.Remove(tower.gameObject.GetComponent<HexLocation>().location);
	}

	public bool HasTower(HexCoord coord)
	{
		return TowerLocations.Contains(coord);
	}

	public Tower FindTowerByID(int viewID)
	{
		return ActiveTowerList.Find(x => x.NetworkViewID == viewID);
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
