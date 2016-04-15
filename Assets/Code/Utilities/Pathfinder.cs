using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

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
    public int TowerCost = 1000;

    [SerializeField]
    public int TowerCoverageCost = 1;

    [SerializeField]
    [Range(1, 1000)]
    public int BaseCost = 3;

    [SerializeField]
    public bool AvoidEnemies;

    [SerializeField]
    public HexCoord Target;

    /*
    [SerializeField]
    public int MaxDistanceFromTarget = 24;

    [SerializeField]
    public int MaxDistanceFromLocation = 24;
    */

    [SerializeField]
    public float MaxUphill = 0.5f;

    [SerializeField]
    public float MaxDownhill = 2f;

    [SerializeField]
    [Range(1, 12)]
    public int Precision = 6;

    public HexPathNode[] Path;

    public PathfindingFailureHandler OnPathfindingFailure;

    protected HexLocation ObjHexLocation;
    protected int SolvedTowerState = 0;
    protected HexMeshOverlay.Entry Overlay;

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
        bool success = HexKit.Path(out Path, origin, IsTarget, MoveCost);

        if (HexKit.Path(out Path, origin, Target, MoveCost))
        {
            success = true;
        }

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
        if (HexCoord.Distance(ObjHexLocation.location, coord) > MaxPathfindingDistance)
        {
            return 0;
        }

        int cost = 1;

        if (IsObstacle(coord))
        {
            cost += ObstacleCost;
        }

        if (GameManager.Instance.TowerManager.HasTower(coord))
        {
            cost += TowerCost;
        }

        int coverage = GameManager.Instance.TowerManager.Coverage(coord);
        if (coverage > 0)
        {
            cost += TowerCoverageCost * coverage;
        }

        if (node.Ancestor != coord)
        {
            // TODO: Performa a series of HexPlane.Intersect calls along a straight line between
            // the two plots, checking the elevation change between each step. This will allow
            // oblique surfaces that are actually a smooth hill to be evaluated properly.

            // TODO: Cache viable directions for each plot on scene load? This would require
            // everything to use the same max elevation change.

            var surface = GameManager.Instance.TerrainMesh.Map.Surface;
            foreach (float change in surface.Diff(node.Ancestor, coord, Precision))
            {
                float c = -change; // Invert because -z is up
                if (c > MaxUphill || c < -MaxDownhill)
                {
                    cost += ObstacleCost;
                }
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