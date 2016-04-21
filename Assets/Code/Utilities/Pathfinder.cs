using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

using System;
using System.Collections.Generic;
using Settworks.Hexagons;

[RequireComponent(typeof(HexLocation))]
public class Pathfinder : MonoBehaviour
{
    public delegate void PathfindingFailureHandler();

    public const int MaxPathfindingDistance = 96;

    public bool ShowDebugLogs = false;
    public bool ShowPath = false;

    [SerializeField]
    public int ObstacleCost = 100000;

    [SerializeField]
    public int SlopeCost = 1000;

    [SerializeField]
    public int TowerCost = 100;

    [SerializeField]
    public int TowerCoverageCost = 0;

    [SerializeField]
    [Range(1, 1000)]
    public int BaseCost = 1;

    [SerializeField]
    public bool AvoidEnemies;

    [SerializeField]
    public HexCoord Target;

    [SerializeField]
    public float MaxSlope = 3f;

    public HexPathNode[] Path;

    public PathfindingFailureHandler OnPathfindingFailure;

    protected HexLocation ObjHexLocation;
    protected int SolvedTowerState = 0;
    protected HexMeshOverlay.Entry Overlay;

    [NonSerialized]
    protected int CalculationCount;

    public virtual void Awake()
    {
        ObjHexLocation = GetComponent<HexLocation>();
    }

    // Use this for initialization
    public virtual void OnDestroy()
    {
        if (ShouldShowPath())
        {
            RemovePathOverlay();
        }
    }

    // Update is called once per frame
    public virtual void Update()
    {
        int towerState = GameManager.Instance.TowerManager.State;
        if (SolvedTowerState != towerState)
        {
            SolvedTowerState = towerState;
            Solve();
        }

        // If the state of the pathfinder overlay has changed, update the overlay
        // immediately instead of waiting for the next run of the solver
        bool hasPath = Overlay != null;
        if (ShouldShowPath() != hasPath)
        {
            UpdatePathOverlay();
        }
    }

    public virtual void FixedUpdate()
    {
    }

    public bool Solve()
    {
        HexCoord origin = ObjHexLocation.location;
        //bool success = HexKit.Path(out Path, origin, IsTarget, MoveCost);
        CalculationCount = 0;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        bool success = HexKit.Path(out Path, origin, Target, MoveCost, true);
        watch.Stop();
        Log("Move Cost: " + CalculationCount.ToString() + " calcs : " + watch.ElapsedMilliseconds.ToString() + " ms : " + watch.ElapsedTicks.ToString() + " ticks");

        // Log success/failure
        if (success)
        {
            Log("Solved: " + PathToString() + " Origin: " + origin + " Target: " + Target);
        }
        else
        {
            Log("No Solution");
        }

        // Update our pathfinding overlay
        UpdatePathOverlay();

        if (!success)
        {
            if (OnPathfindingFailure != null)
            {
                OnPathfindingFailure();
            }
        }

        return success;
    }

    public bool IsTarget(HexCoord coord)
    {
        return coord == Target;
    }

    public uint MoveCost(HexPathNode node, HexCoord coord)
    {
        var map = GameManager.Instance.TerrainMesh.Map;
        if (!map.Coords.Contains(coord))
        {
            return 0;
        }

        if (HexCoord.Distance(ObjHexLocation.location, coord) > MaxPathfindingDistance)
        {
            return 0;
        }

        CalculationCount++;

        int cost = BaseCost;

        if (IsObstacle(coord))
        {
            cost += ObstacleCost;
        }

        if (GameManager.Instance.TowerManager.HasTower(coord))
        {
            cost += TowerCost;
        }

        if (TowerCoverageCost != 0)
        {
            int coverage = GameManager.Instance.TowerManager.Coverage(coord);
            if (coverage > 0)
            {
                cost += TowerCoverageCost * coverage;
            }
        }

        if (node.Ancestor != coord)
        {
            float slope = map.Slope.Get(node.Ancestor, node.FromDirection - 3);
            if (slope > MaxSlope)
            {
                cost += SlopeCost * Mathf.Min(Mathf.FloorToInt(slope / MaxSlope), 10);
            }
        }

        return (uint)cost;
    }

    public bool IsObstacle(HexCoord coord)
    {
        if (!GameManager.Instance.TerrainMesh.IsPassable(coord))
        {
            return true;
        }

        return false;
    }

    public HexCoord Next()
    {
        var location = ObjHexLocation.location;
        if (location == Target)
        {
            return location;
        }

        if (Path == null || Path.Length == 0 || Path[0].Location == location)
        {
            Solve();
        }

        if (Path != null && Path.Length > 0)
        {
            return Path[0].Location;
        }
        else
        {
            return location;
        }
    }

    public void UpdatePathOverlay()
    {
        if (ShouldShowPath())
        {
            if (Overlay == null)
            {
                AddPathOverlay();
            }
            Overlay.Set(EnumeratePath());
            Overlay.Show();
        }
        else if (Overlay != null)
        {
            RemovePathOverlay();
        }
    }

    public void AddPathOverlay()
    {
        int id = gameObject.GetInstanceID();
        Overlay = GameManager.Instance.TerrainMesh.Overlays[TerrainOverlay.Pathfinding][id];
        Overlay.Color = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);
        Overlay.Show();
    }

    public void RemovePathOverlay()
    {
        int id = gameObject.GetInstanceID();
        GameManager.Instance.TerrainMesh.Overlays[TerrainOverlay.Pathfinding].Remove(id);
        Overlay = null;
    }

    public IEnumerable<HexCoord> EnumeratePath()
    {
        foreach (var node in Path)
        {
            yield return node.Location;
        }
    }

    public string PathToString()
    {
        if (Path == null)
        {
            return "(NONE)";
        }

        string output = "";
        for (int i = 0; i < Path.Length; i++)
        {
            if (i > 0)
            {
                output += " ";
            }
            output += Path[i].Location.ToString();
            output += ":";
            output += Path[i].PathCost.ToString();
        }
        return output;
    }

    protected virtual bool ShouldShowPath()
    {
        if (ShowPath)
        {
            return true;
        }

#if UNITY_EDITOR
        if (Selection.activeGameObject == gameObject)
        {
            return true;
        }
#endif

        return false;
    }

    #region MessageHandling

    protected virtual void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[Pathfinder] " + message);
    }

    protected virtual void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[Pathfinder] " + message);
    }

    #endregion MessageHandling
}