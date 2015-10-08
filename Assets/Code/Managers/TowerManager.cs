using Settworks.Hexagons;
using System.Collections.Generic;
using UnityEngine;

public class TowerManager
{
    public bool ShowDebugLogs = true;
    public int State { get; private set; }

    private List<Tower> ActiveTowerList = new List<Tower>();
    private Dictionary<HexCoord, GameObject> TowerLocations = new Dictionary<HexCoord, GameObject>();
    private Dictionary<HexCoord, int> TowerCoverage = new Dictionary<HexCoord, int>();

    // Use this for initialization
    public TowerManager()
    {
        State = 0;
    }

    public int ActiveTowerCount()
    {
        return ActiveTowerList.Count;
    }

    public void AddActiveTower(Tower tower)
    {
        State++;
        ActiveTowerList.Add(tower);
        if (tower.gameObject.GetComponent<HexLocation>())
            TowerLocations[tower.gameObject.GetComponent<HexLocation>().location] = tower.gameObject;
    }

    public void RemoveActiveTower(Tower tower)
    {
        State++;
        ActiveTowerList.Remove(tower);
        if (tower.gameObject.GetComponent<HexLocation>())
            TowerLocations.Remove(tower.gameObject.GetComponent<HexLocation>().location);
    }

    public int Coverage(HexCoord coord)
    {
        int value = 0;
        TowerCoverage.TryGetValue(coord, out value);
        return value;
    }

    public void AddCoverage(Tower tower)
    {
        State++;
        foreach (var coord in tower.Coverage())
        {
            int value = 0;
            TowerCoverage.TryGetValue(coord, out value);
            TowerCoverage[coord] = value + 1;
        }
    }

    public void RemoveCoverage(Tower tower)
    {
        State++;
        foreach (var coord in tower.Coverage())
        {
            int value;
            if (TowerCoverage.TryGetValue(coord, out value))
            {
                value -= 1;
                if (value > 0)
                {
                    TowerCoverage[coord] = value;
                }
                else
                {
                    TowerCoverage.Remove(coord);
                }
            }
            else
            {
                LogError("RemoveCoverage called for coordinate with existing coverage: " + coord.ToString());
            }
        }
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
        if (ShowDebugLogs)
            Debug.Log("[TowerManager] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[TowerManager] " + message);
    }

    #endregion MessageHandling
}