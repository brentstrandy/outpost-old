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
	public int GridWidth = 5;
	public int GridHeight = 5;
	public float HexagonRadius = 1.0f;
	public float DetailWidth = 0.1f;
	public float OutlineWidth = 0.02f;
	public Color OutlineColor = Color.yellow;
	public Color HighlightColor = Color.red;

	//private GameObject Outlines;

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

		/*
		if (Outlines == null)
		{
			Outlines = new GameObject("HexOutlines");
			Outlines.transform.parent = gameObject.transform;
			Outlines.transform.localRotation = Quaternion.identity;
			Outlines.transform.localPosition = Vector3.zero;
			Outlines.transform.localScale = Vector3.one;

			if (Outlines.GetComponent<LineRenderer> == null)
			{
				Outlines.AddComponent<LineRenderer>();
			}
		}
		*/

		/*
		if (Collide && GetComponent<MeshCollider>() == null)
		{
			gameObject.AddComponent("MeshCollider");
		}
		*/
		
		BuildMesh();
	}
	
	// Update is called once per frame
	void Update ()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		HexCoord coord;

		if (Input.GetMouseButtonDown(0)) {
			if (IntersectRay(ray, out hit, out coord)) {
				Log("HexMesh Collision: " + hit.point + " - " + coord);
			}
		}
	}

	public bool IntersectRay(Ray ray, out RaycastHit hit, out HexCoord coord)
	{
		int mask = 1 << gameObject.layer;
		if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask)) {
			// Note from J.S. 2015-03-29: The intersection seems to occur with a simple plane right now. The original gameobject was a plane, perhaps the MeshCollider is
			// working against that original mesh and not the real one that we generate?
			if (hit.collider == GetComponent<MeshCollider>()) {
				// Convert from world space to local space
				var xy = (Vector2)hit.transform.InverseTransformPoint(hit.point);

				// Scale to fit the grid
				float scale = 1.0f; // TODO: Base this on the hexagon diameter
				xy *= scale;

				// Convert to a hex coordinate
				coord = HexCoord.AtPosition(xy);

				return true;
			}
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

		BuildMesh();
	}
	
	public HexCoord[] GetHexBounds()
	{
		var corner = new Vector2(GridWidth / 2, GridHeight / 2);
		return HexCoord.CartesianRectangleBounds(corner, -corner);
	}

	private void BuildMesh()
	{
		var mesh = BuildBaseMesh();
		GetComponent<MeshFilter>().sharedMesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;
		
		DestroyOutlines();
		CreateOutlines();
	}

	private Func<Vector2, float> GetHeightPredicate()
	{
		if (HeightMap == null)
		{
			Log("No HeightMap Specified");
			return (Vector2 uv) => 0.0f;
		}

		Log("Using HeightMap");
		return (Vector2 uv) => HeightMap.GetPixelBilinear(uv.x, uv.y).grayscale * -HeightScale;
	}
	
	private Func<Vector2, Vector2> GetUVPredicate()
	{
		Vector2 scale = new Vector2(1.0f / (float)GridWidth, 1.0f / (float)GridHeight);
		Vector2 offset = new Vector2(0.5f, 0.5f);
		return (Vector2 uv) => Vector2.Scale(uv, scale) + offset;
	}
	
	private Mesh BuildBaseMesh()
	{
		float outer = 1.0f;
		float inner = 1.0f - DetailWidth;

		var height = GetHeightPredicate();
		var tex = GetUVPredicate();

		HexMeshBuilder.NodeDelegate predicate = (HexCoord coord, int i) => {
			Vector2 pos = HexCoord.CornerVector(i) * (i < 6 ? outer : inner) + coord.Position();
			Vector2 uv = tex(pos);
			return new HexMeshBuilder.Node(new Vector3(pos.x, pos.y, height(uv)), uv);
		};
		
		var bounds = GetHexBounds();
		var builder = new HexMeshBuilder();

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
	
	private Mesh BuildOutlineMesh()
	{
		float offset = OutlineWidth * 0.5f;
		float outer = 1.0f + offset;
		float inner = 1.0f - offset;

		var height = GetHeightPredicate();
		var tex = GetUVPredicate();

		HexMeshBuilder.NodeDelegate predicate = (HexCoord coord, int i) => {
			Vector2 pos = HexCoord.CornerVector(i) * (i < 6 ? outer : inner) + coord.Position();
			Vector2 uv = tex(pos);
			return new HexMeshBuilder.Node(new Vector3(pos.x, pos.y, height(uv)), uv);
		};
		
		var bounds = GetHexBounds();
		var builder = new HexMeshBuilder();
		builder.SetPredicate(predicate);
		builder.SetTriangles(new int[] {
			0,6,7,		7,1,0,		1,7,8,		8,2,1,		2,8,9,		9,3,2,
			3,9,10,		10,4,3,		4,10,11,	11,5,4,		5,11,6,		6,0,5,
		});
		
		foreach (HexCoord coord in HexKit.WithinRect(bounds[0], bounds[1]))
		{
			builder.AddHexagon(coord);
		}
		
		Log("Outline Mesh Summary: " + builder.Summary());
		return builder.Build();
	}
	
	private void CreateOutlines()
	{
		// Prepare the outline game object
		string name = "HexOutlines";
		var outlines = new GameObject(name);
		outlines.layer = LayerMask.NameToLayer("TransparentFX");
		outlines.transform.parent = gameObject.transform;
		outlines.transform.localRotation = Quaternion.identity;
		outlines.transform.localPosition = new Vector3(0.0f, 0.0f, -0.01f);
		outlines.transform.localScale = Vector3.one;
		
		var filter = outlines.AddComponent<MeshFilter>();
		var renderer = outlines.AddComponent<MeshRenderer>();
		
		filter.mesh = BuildOutlineMesh();

		renderer.sharedMaterial = new Material(Shader.Find("Particles/Additive"));
		renderer.sharedMaterial.SetColor("_TintColor", OutlineColor);
	}
	
	private void DestroyOutlines()
	{
		var outlines = new List<GameObject>();
		foreach (Transform child in transform)
		{
			if (child.gameObject.name.StartsWith("HexOutline"))
			{
				outlines.Add(child.gameObject);
			}
		}
		
		foreach (var child in outlines)
		{
			//Log("Destroying " + child.name);
			DestroyImmediate(child);
		}
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
