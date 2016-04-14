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

    public bool ShowDebugLogs = false;
    public bool ShowPath = false;

    [SerializeField]
    public bool AvoidTowers;

    [SerializeField]
    public bool AvoidTowerCoverage;

    [SerializeField]
    public bool AvoidEnemies;

    [SerializeField]
    public HexCoord Target;

    [SerializeField]
    public int MaxDistanceFromTarget = 24;

    [SerializeField]
    public int MaxDistanceFromLocation = 24;

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
        if (AvoidTowers || AvoidTowerCoverage)
        {
            int towerState = GameManager.Instance.TowerManager.State;
            if (SolvedTowerState != towerState)
            {
                SolvedTowerState = towerState;
                Log("Solving after tower change");
                Solve();
            }
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
        bool success = false;

        // If we're configured to avoid tower coverage, make an attempt to find a path that avoids both obstacles and tower coverage
        if (AvoidTowerCoverage && HexKit.Path(out Path, origin, Target, IsObstacleOrTowerCoverage))
        {
            success = true;
        }
        // If we're configured to avoid towers, make an attempt to find a path that avoids both obstacles and towers
        else if (AvoidTowers && HexKit.Path(out Path, origin, Target, IsObstacleOrTower))
        {
            success = true;
        }
        // If all else fails, attempt to find a path that at least avoids obstacles
        else if (HexKit.Path(out Path, origin, Target, IsObstacle))
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

        if (success)
        {
            if (OnPathfindingFailure != null)
            {
                OnPathfindingFailure();
            }
        }

        return success;
    }

    public bool IsObstacle(HexCoord coord)
    {
        if (HexCoord.Distance(ObjHexLocation.location, coord) > MaxDistanceFromLocation)
        {
            return true;
        }

        if (HexCoord.Distance(coord, Target) > MaxDistanceFromTarget)
        {
            return true;
        }

        if (!GameManager.Instance.TerrainMesh.IsPassable(coord))
        {
            return true;
        }

        return false;
    }

    public bool IsObstacleOrTower(HexCoord coord)
    {
        if (IsObstacle(coord))
        {
            return true;
        }

        if (GameManager.Instance.TowerManager.HasTower(coord))
        {
            return true;
        }

        return false;
    }

    public bool IsObstacleOrTowerCoverage(HexCoord coord)
    {
        if (AvoidTowers)
        {
            if (IsObstacleOrTower(coord))
            {
                return true;
            }
        }
        else
        {
            if (IsObstacle(coord))
            {
                return true;
            }
        }

        if (GameManager.Instance.TowerManager.Coverage(coord) > 0)
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