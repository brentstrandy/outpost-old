using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Settworks.Hexagons;

[ExecuteInEditMode]
public class HexMesh : MonoBehaviour
{

	// Properties adjustable in the inspector
	//public bool GenerateNoise = true;
	public Texture2D HeightMap;
	public float HeightScale = 5.0f;
	public bool ShowDebugLogs = true;
	public bool FlatShaded = true;
	public int GridWidth = 5;
	public int GridHeight = 5;
	public int FacilityRadius = 5;
	public int PeripheralRadius = 12;
	public float HexagonRadius = 1.0f;
	public float DetailWidth = 0.1f;
	public float OutlineWidth = 0.02f;
	public Color OutlineColor = Color.yellow;
	public float HighlightWidth = 0.1f;
	public Color HighlightColor = Color.red;

	public HexMeshOverlaySet Overlays;

	// Use this for initialization
	void Start ()
	{
		if (GetComponent<MeshFilter>() == null)
		{
			gameObject.AddComponent<MeshFilter>();
		}
		
		if (GetComponent<MeshRenderer>() == null)
		{
			gameObject.AddComponent<MeshRenderer>();
		}
		
		if (GetComponent<MeshCollider>() == null)
		{
			gameObject.AddComponent<MeshCollider>();
		}

		gameObject.layer = LayerMask.NameToLayer("Terrain");

		CreateOverlays();
		UpdateMesh();
		UpdateOutlines();
	}
	
	public Vector3 IntersectPosition(Vector3 pos, float distance = 0f)
	{
		// TODO: Translate pos into local coordinates?
		RaycastHit hit;
		HexCoord coord;
		pos.z = -100.0f; // Be sure to place the source of the ray cast above the mesh
		var down = new Vector3(0f, 0f, 1.0f); // Fire the ray down toward the mesh
		if (IntersectRay(new Ray(pos, down), out hit, out coord))
		{
			return new Vector3(hit.point.x, hit.point.y, hit.point.z - distance); // Note: Up is negative Z
		}
		return pos;
	}
	
	public bool IntersectRay(Ray ray, out RaycastHit hit, out HexCoord coord)
	{
		if (GetComponent<MeshCollider>().Raycast(ray, out hit, Mathf.Infinity))
		{
			// Convert from world space to local space
			var xy = (Vector2)hit.transform.InverseTransformPoint(hit.point);
			
			// Scale to fit the grid
			float scale = 1.0f; // TODO: Base this on the hexagon diameter
			xy *= scale;
			
			// Convert to a hex coordinate
			coord = HexCoord.AtPosition(xy);
			
			return true;
		}
		coord = default(HexCoord);
		return false;
	}

	public void ApplyProperties()
	{
		// Enforce odd numbered dimensions
		if (GridWidth % 2 == 0)
		{
			GridWidth += 1;
		}
		
		if (GridHeight % 2 == 0)
		{
			GridHeight += 1;
		}

		CreateOverlays();
		UpdateMesh();
		UpdateOutlines();
	}
	
	private void CreateOverlays()
	{
		if (Overlays == null)
		{
			Overlays = new HexMeshOverlaySet(gameObject);
		}
		else
		{
			Overlays.Clear();
		}

		Overlays.Add((int)TerrainOverlays.Outline, "TerrainOutline", "Particles/Additive", CreateOverlayBuilder(OutlineWidth));
		Overlays.Add((int)TerrainOverlays.Highlight, "TerrainHighlight", "Standard", CreateOverlayBuilder(HighlightWidth));
		Overlays.Add((int)TerrainOverlays.Selection, "TerrainSelection", "Standard", CreateOverlayBuilder(HighlightWidth));
	}
	
	private void UpdateMesh()
	{
		var mesh = BuildBaseMesh();
		GetComponent<MeshFilter>().sharedMesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;
	}
	
	private void UpdateOutlines()
	{
		var overlay = Overlays[(int)TerrainOverlays.Outline][0];
		overlay.Update(WithinPlacementRange());
		overlay.Color = OutlineColor;
		overlay.Show();
	}
	
	private Mesh BuildBaseMesh()
	{
		float outer = 1.0f;
		float inner = 1.0f - DetailWidth;
		
		var height = GetHeightPredicate();
		var tex = GetUVPredicate();
		
		HexMeshBuilder.NodeDelegate predicate = (HexCoord c, int i) => {
			Vector2 pos = HexCoord.CornerVector(i) * (i < 6 ? outer : inner) + c.Position();
			Vector2 uv = tex(pos);
			return new HexMeshBuilder.Node(new Vector3(pos.x, pos.y, height(uv)), uv);
		};
		
		var bounds = GetHexBounds();
		var builder = new HexMeshBuilder();
		builder.FlatShaded = FlatShaded;
		
		// Note: Corner 0 is at the upper right, others proceed counterclockwise.
		
		builder.SetPredicate(predicate);
		builder.SetTriangles(new int[] {
			0,6,7,		7,1,0,		1,7,8,		8,2,1,		2,8,9,		9,3,2,
			3,9,10,		10,4,3,		4,10,11,	11,5,4,		5,11,6,		6,0,5,
			6,11,7,		7,11,8,		8,11,10,	10,9,8
				/*
			0,1,6,		6,1,7,		1,2,7,		7,2,8,		2,3,8,		8,3,9,
			3,4,9,		9,4,10,		4,5,10,		10,5,11,	5,0,11,		11,0,6,
			6,7,9,		9,7,8,		11,6,10,	10,6,9
			*/
		});
		
		foreach (HexCoord coord in HexKit.WithinRect(bounds[0], bounds[1]))
		{
			builder.AddHexagon(coord);
		}
		
		Log("Base Mesh Summary: " + builder.Summary());
		return builder.Build();
	}
	
	public HexCoord[] GetHexBounds()
	{
		var corner = new Vector2(GridWidth / 2, GridHeight / 2);
		return HexCoord.CartesianRectangleBounds(corner, -corner);
	}
	
	public bool InPlacementRange(HexCoord coord)
	{
		// TODO: Consider moving this to the facility object
		int distance = HexCoord.Distance(HexCoord.origin, coord);
		return distance >= FacilityRadius && distance <= PeripheralRadius;
	}

	private IEnumerable<HexCoord> WithinPlacementRange()
	{
		var bounds = GetHexBounds();
		foreach (HexCoord coord in HexKit.WithinRect(bounds[0], bounds[1]))
		{
			if (InPlacementRange(coord))
			{
				yield return coord;
			}
		}
	}
	
	private HexMeshBuilder CreateOverlayBuilder(float lineWidth)
	{
		float offset = lineWidth * 0.5f;
		float outer = 1.0f + offset;
		float inner = 1.0f - offset;
		
		var height = GetHeightPredicate();
		var tex = GetUVPredicate();
		
		HexMeshBuilder.NodeDelegate predicate = (HexCoord c, int i) => {
			Vector2 pos = HexCoord.CornerVector(i) * (i < 6 ? outer : inner) + c.Position();
			Vector2 uv = tex(pos);
			return new HexMeshBuilder.Node(new Vector3(pos.x, pos.y, height(uv)), uv);
		};
		
		var builder = new HexMeshBuilder();
		builder.FlatShaded = false;
		builder.SetPredicate(predicate);
		builder.SetTriangles(new int[] {
			0,6,7,		7,1,0,		1,7,8,		8,2,1,		2,8,9,		9,3,2,
			3,9,10,		10,4,3,		4,10,11,	11,5,4,		5,11,6,		6,0,5,
		});
		
		return builder;
	}
	
	private Func<Vector2, float> GetHeightPredicate()
	{
		if (HeightMap == null)
		{
			//Log("No HeightMap Specified");
			return (Vector2 uv) => 0.0f;
		}
		
		//Log("Using HeightMap");
		return (Vector2 uv) => HeightMap.GetPixelBilinear(uv.x, uv.y).grayscale * -HeightScale;
	}
	
	private Func<Vector2, Vector2> GetUVPredicate()
	{
		Vector2 scale = new Vector2(1.0f / (float)GridWidth, 1.0f / (float)GridHeight);
		Vector2 offset = new Vector2(0.5f, 0.5f);
		return (Vector2 uv) => Vector2.Scale(uv, scale) + offset;
	}

	/*
	private void ApplyNoise(int gridWidth, int gridHeight, Vector3[] vertices)
	{
		int numHexagonColumnsA = (gridWidth + 1) / 2;
		int numHexagonColumnsB = (gridWidth - 1) / 2;

		for (int r = 0; r < gridHeight; r++)
		{
			int numHexagonColumns = r % 2 == 0 ? numHexagonColumnsA : numHexagonColumnsB; // The number of columns varies depending on whether the row index is even or odd
			for (int c = 0; c < numHexagonColumns; c++)
			{
				float baseline = (float)(r * c) * 0.001f;
				float variation = 0.8f;
				float target = baseline * baseline + UnityEngine.Random.Range(0.0f, variation);
				float neighbor = 0.0f;
				for (int p = 0; p < 7; p++)
				{
					int index = GetHexagonIndex(gridWidth, gridHeight, r, c, p);
					neighbor = Mathf.Max(neighbor, vertices[index].z);
				}
				float hexDiff = neighbor - target;
				target += hexDiff * 0.5f;
				for (int p = 0; p < 7; p++)
				{
					int index = GetHexagonIndex(gridWidth, gridHeight, r, c, p);
					float vertexDiff = p > 0 ? vertices[index].z - target : 0.0f;
					vertices[index].z = target + vertexDiff * 0.5f;
				}
			}
		}
	}
	*/

	/*
	public void Apply(HexMap map)
	{
	}
	*/

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[HexMesh] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[HexMesh] " + message);
	}
	#endregion
}
