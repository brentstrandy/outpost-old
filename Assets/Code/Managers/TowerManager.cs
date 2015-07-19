using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Settworks.Hexagons;

public class TowerManager
{
	public bool ShowDebugLogs = true;

	private List<Tower> ActiveTowerList;
	private Dictionary<HexCoord, GameObject> TowerLocations = new Dictionary<HexCoord, GameObject>();

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
		if(tower.gameObject.GetComponent<HexLocation>())
			TowerLocations[tower.gameObject.GetComponent<HexLocation>().location] = tower.gameObject;
	}

	public void RemoveActiveTower(Tower tower)
	{
		ActiveTowerList.Remove(tower);
		if(tower.gameObject.GetComponent<HexLocation>())
			TowerLocations.Remove(tower.gameObject.GetComponent<HexLocation>().location);
	}

	public bool TryGetTower(HexCoord coord, out GameObject towerObject)
	{
		return TowerLocations.TryGetValue(coord, out towerObject);
	}

	public bool HasTower(HexCoord coord)
	{
		return TowerLocations.ContainsKey(coord);
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
