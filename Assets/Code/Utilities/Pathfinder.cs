using UnityEngine;
using System.Collections;
using Settworks.Hexagons;

public class Pathfinder : MonoBehaviour
{
	public bool ShowDebugLogs = false;

	[SerializeField]
	public bool AvoidTowers;

	[SerializeField]
	public bool AvoidEnemies;

	[SerializeField]
	public HexCoord Target;

	public HexPathNode[] Path;

	public virtual void Awake()
	{
	}

	// Use this for initialization
	public virtual void Start () {
		//this.Location = GetComponent<HexLocation>();
	}
	
	// Update is called once per frame
	public virtual void Update()
	{
		
	}
	
	public virtual void FixedUpdate()
	{
		
	}
	
	public void Solve()
	{
		HexKit.Path(out Path, GetComponent<HexLocation>().location, Target, IsObstacle);
		if (Path != null)
		{
			Log("Solved: " + PathToString() + " Origin: " + GetComponent<HexLocation>().location + " Target: " + Target);
		}
		else
		{
			Log("No Solution");
		}
	}

	public bool IsObstacle(HexCoord coord)
	{
		// TODO: Check bounds

		if (AvoidTowers)
		{
			if (GameManager.Instance.TowerManager.HasTower(coord))
			{
				return true;
			}
		}
		
		if (GameManager.Instance.TerrainMesh.IsImpassable(coord))
		{
			return true;
		}

		return false;
	}

	public HexCoord Next()
	{
		var location = GetComponent<HexLocation>().location;
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
