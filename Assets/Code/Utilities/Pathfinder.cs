using UnityEngine;
using System;
using System.Collections;
using Settworks.Hexagons;

[RequireComponent(typeof(HexLocation))]
public class Pathfinder : MonoBehaviour
{
	public delegate void PathfindingFailureHandler();

	public bool ShowDebugLogs = false;

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

	public virtual void Awake()
	{
		ObjHexLocation = GetComponent<HexLocation>();
	}

	// Use this for initialization
	public virtual void Start ()
	{
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
	}
	
	public virtual void FixedUpdate()
	{
		
	}
	
	public bool Solve()
	{
		HexCoord origin = ObjHexLocation.location;

		// If we're configured to avoid tower coverage, make an attempt to find a path that avoids both obstacles and tower coverage
		if (AvoidTowerCoverage && HexKit.Path(out Path, origin, Target, IsObstacleOrTowerCoverage))
		{
			Log("Solved: " + PathToString() + " Origin: " + origin + " Target: " + Target);
			return true;
		}
		
		// If we're configured to avoid towers, make an attempt to find a path that avoids both obstacles and towers
		if (AvoidTowerCoverage && HexKit.Path(out Path, origin, Target, IsObstacleOrTower))
		{
			Log("Solved: " + PathToString() + " Origin: " + origin + " Target: " + Target);
			return true;
		}

		// If all else fails, attempt to find a path that at least avoids obstacles
		if (HexKit.Path(out Path, origin, Target, IsObstacle))
		{
			if (HexKit.Path(out Path, ObjHexLocation.location, Target, IsObstacle))
			{
				Log("Solved: " + PathToString() + " Origin: " + origin + " Target: " + Target);
				return true;
			}
		}
		
		Log("No Solution");

		if (OnPathfindingFailure != null)
		{
			OnPathfindingFailure();
		}
		return false;
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
		
		if (GameManager.Instance.TerrainMesh.IsImpassable(coord))
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
	
	#region MessageHandling
	protected virtual void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[Pathfinder] " + message);
	}
	
	protected virtual void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[Pathfinder] " + message);
	}
	#endregion
}
