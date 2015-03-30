using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Settworks.Hexagons;

[ExecuteInEditMode]
public class HexMesh : MonoBehaviour
{
	// Properties adjustable in the inspector
	public bool GenerateNoise = true;
	public bool ShowDebugLogs = true;
	public int GridWidth = 5;
	public int GridHeight = 5;
	public float HexagonRadius = 1.0f;
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
		
		BuildMesh(GridWidth, GridHeight, 1.0f);
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

		BuildMesh(GridWidth, GridHeight, HexagonRadius);
	}

	private void BuildMesh(int gridWidth, int gridHeight, float hexagonRadius)
	{
		if (gridWidth % 2 == 0)
		{
			throw new ArgumentException("Given value must be an odd number", "width");
		}

		if (gridHeight % 2 == 0)
		{
			throw new ArgumentException("Given value must be an odd number", "height");
		}

		// Create a mesh instance
		//var mesh = new Mesh();
		
		// Get the mesh instance
		var mesh = GetComponent<MeshFilter>().sharedMesh;
		
		// Calculate some properties of the mesh
		float apothemRatio = (float)(Math.Sqrt(3.0) * 0.5);

		int numberOfHexagons = GetNumHexagons(gridWidth, gridHeight);
		int numberOfVertices = GetNumVertices(gridWidth, gridHeight);
		int numberOfIndices = GetNumIndices(gridWidth, gridHeight);

		int numVertexRows = GetNumVertexRows(gridHeight);
		int numVertexColumnsA = (gridWidth + 1) + (gridHeight - 1) / 2;
		int numVertexColumnsB = 3 * (gridWidth + 1) / 2;

		float hexagonDiameter = hexagonRadius * 2.0f;
		float hexagonHalfRadius = hexagonRadius * 0.5f;
		float hexagonApothem = hexagonRadius * apothemRatio;
		float hexagonWidth = hexagonDiameter;
		float hexagonHeight = 2.0f * hexagonApothem;

		var meshDimensions = new Vector2(
			x: (float)numVertexColumnsA * hexagonRadius,
			y: hexagonHeight + (float)(gridHeight - 1) * hexagonApothem
		);

		var meshOffset = new Vector3(
			x: meshDimensions.x * -0.5f,
			y: meshDimensions.y * -0.5f,
			z: 0.0f
		);

		var texScale = new Vector2(
			x: 1.0f / meshDimensions.x,
			y: 1.0f / meshDimensions.y
		);

		this.Log("Number of Hexagons: " + numberOfHexagons);
		this.Log("Number of Vertices: " + numberOfVertices);
		this.Log("Number of Indices: " + numberOfIndices);

		this.Log("Mesh Dimensions: " + meshDimensions);
		this.Log("Mesh Offset: " + meshOffset);
		this.Log("Texture Scale: (" + texScale.x + ", " + texScale.y + ")");
		
		// Allocate vertices, indices and UV coordinates
		var vertices = new Vector3[numberOfVertices];
		var indices = new int[numberOfIndices];
		var uv = new Vector2[numberOfVertices];

		// Calculate vertices and UV coordinates
		int vertIndex = 0;
		for (int r = 0; r < numVertexRows; r++)
		{
			int numVertexColumns = r % 2 == 0 ? numVertexColumnsA : numVertexColumnsB; // The number of columns varies depending on whether the row index is even or odd
			for (int c = 0; c < numVertexColumns; c++)
			{
				float offset = (r % 2 == 0) ? hexagonHalfRadius : 0f;
				var vertex = new Vector3(
					x: offset + c * hexagonRadius,
					y: r * hexagonApothem,
					z: 0.0f
				);
				vertices[vertIndex] = vertex + meshOffset;
				uv[vertIndex] = Vector2.Scale((Vector2)vertex, texScale);
				
				//this.Log("v" + vertIndex + ": " + vertices[vertIndex]);

				vertIndex++;
			}
		}

		// Calculate indices
		int numHexagonColumnsA = (gridWidth + 1) / 2;
		int numHexagonColumnsB = (gridWidth - 1) / 2;

		int hexIndex = 0;
		for (int r = 0; r < gridHeight; r++)
		{
			//this.Log("Row " + r);
			int numHexagonColumns = r % 2 == 0 ? numHexagonColumnsA : numHexagonColumnsB; // The number of columns varies depending on whether the row index is even or odd
			for (int c = 0; c < numHexagonColumns; c++)
			{
				//this.Log("Column " + c);
				// Clockwise order:
				// JS 2015-02-20: This should be correct but it comes out backwards and I'm not sure why
				// JS 2015-03-24: It's because positive Z points toward the camera, not away from it
				/*
				for (int p = 1; p < 7; p++)
				{
					indices[hexIndex++] = GetHexagonIndex(gridWidth, gridHeight, r, c, 0);
					indices[hexIndex++] = GetHexagonIndex(gridWidth, gridHeight, r, c, p);
					indices[hexIndex++] = GetHexagonIndex(gridWidth, gridHeight, r, c, p >= 6 ? 1 : p + 1);
					
					//this.Log("i" + (hexIndex - 3) + ": " + indices[hexIndex - 3] + "," + indices[hexIndex - 2] + "," + indices[hexIndex - 1]);
				}
				*/

				// Counter-clockwise order:
				for (int p = 2; p < 8; p++)
				{
					indices[hexIndex++] = GetHexagonIndex(gridWidth, gridHeight, r, c, 0);
					indices[hexIndex++] = GetHexagonIndex(gridWidth, gridHeight, r, c, p >= 7 ? 1 : p);
					indices[hexIndex++] = GetHexagonIndex(gridWidth, gridHeight, r, c, p - 1);
					
					//this.Log("i" + (hexIndex - 3) + ": " + indices[hexIndex - 3] + "," + indices[hexIndex - 2] + "," + indices[hexIndex - 1]);
				}
			}
		}

		// Apply noise
		if (GenerateNoise)
		{
			ApplyNoise(gridWidth, gridHeight, vertices);
		}

		// Assign data to the mesh
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.SetTriangles(indices, 0);

		// Tell the mesh to recalculate its properties
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();

		// Assign the mesh to the collider
		GetComponent<MeshCollider>().sharedMesh = mesh;

		// Destroy any existing outlines
		DestroyOutlines();

		// Build the new outlines
		BuildOutlines(gridWidth, gridHeight, vertices);
	}

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
	
	private void BuildOutlines(int gridWidth, int gridHeight, Vector3[] vertices)
	{
		int numHexagonColumnsA = (gridWidth + 1) / 2;
		int numHexagonColumnsB = (gridWidth - 1) / 2;

		int member = 0;
		for (int r = 0; r < gridHeight; r++)
		{
			int numHexagonColumns = r % 2 == 0 ? numHexagonColumnsA : numHexagonColumnsB; // The number of columns varies depending on whether the row index is even or odd
			for (int c = 0; c < numHexagonColumns; c++)
			{
				// Prepare the outline game object
				string name = "HexOutline" + member.ToString();
				//Log("Creating " + name);
				var outline = new GameObject(name);
				outline.layer = LayerMask.NameToLayer("TransparentFX");

				// Prepare the line renderer
				var lineRenderer = outline.AddComponent<LineRenderer>();
				lineRenderer.material = new Material (Shader.Find("Particles/Additive"));
				lineRenderer.SetColors(OutlineColor, OutlineColor);
				lineRenderer.SetWidth(0.02f, 0.02f);
				lineRenderer.SetVertexCount(7);
				lineRenderer.useWorldSpace = false;

				// Build the vertex list
				for (int p = 0; p < 7; p++)
				{
					int index = GetHexagonIndex(gridWidth, gridHeight, r, c, p > 5 ? 1 : p + 1);
					lineRenderer.SetPosition(p, vertices[index]);
				}

				//var center = vertices[GetHexagonIndex(gridWidth, gridHeight, r, c, 0)];
				
				outline.transform.SetParent(gameObject.transform, false);
				outline.transform.localRotation = Quaternion.identity;
				//outline.transform.localPosition = center;
				outline.transform.localPosition = new Vector3(0.0f, 0.01f, 0.0f); // Hover above the terrain just slightly
				outline.transform.localScale = Vector3.one;
				//outline.transform.position = center;

				member++;
			}
		}
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
	public void Apply(HexMap map)
	{
	}
	*/

	public void GetHexContainingTriangle(int triangleIndex)
	{
		// TODO: Write this
	}
	
	public static int GetNumHexagons(int gridWidth, int gridHeight)
	{
		// Hexagons in hexagon row A
		// Formula: (w + 1) / 2
		// Pattern:
		// 5 = 3
		// 3 = 2
		// 1 = 1

		// Hexagons in hexagon row B
		// Formula: (w - 1) / 2
		// Pattern:
		// 7 = 3
		// 5 = 2
		// 3 = 1
		// 0 = 0

		// Formula: ((w + 1) / 2) * ((h + 1) / 2) + ((w - 1) / 2) * ((h - 1) / 2)
		// Pattern:
		// 3 x 3 = 5
		// 5 x 3 = 8
		// 5 x 5 = 13

		int wa = (gridWidth + 1) / 2;
		int wb = (gridWidth - 1) / 2;
		int ha = (gridHeight + 1) / 2;
		int hb = (gridHeight - 1) / 2;
		return wa * ha + wb * hb;
	}

	public static int GetNumVertices(int gridWidth, int gridHeight)
	{
		// Vertices in vertex row A
		// Formula: 2 * (w + 1) / 2 + (w - 1) / 2
		// Formula: (w + 1) + (w - 1) / 2
		// Pattern:
		// Width 9 = 14 = 2 * 5 + 4 Vertices
		// Width 7 = 11 = 2 * 4 + 3 Vertices
		// Width 5 = 8 = 2 * 3 + 2 Vertices
		// Width 3 = 5 = 2 * 2 + 1 Vertices
		// Width 1 = 2 = 2 * 1 + 0 Vertices
		
		// Vertices in vertex row B
		// Formula: 3 * (w + 1) / 2
		// Pattern:
		// Width 7 = 12 Vertices
		// Width 5 = 9 Vertices
		// Width 3 = 6 Vertices
		// Width 1 = 3 Vertices
		
		// Total number of vertices
		// Formula: (2 + (h - 1) / 2) * a + (1 + (h - 1) / 2) * b
		// Pattern:
		// 1 = 2a, 1b = a, b, a
		// 3 = 3a, 2b = a, b, a, b, a
		// 5 = 4a, 3b = a, b, a, b, a, b, a
		
		int a = (gridWidth + 1) + (gridWidth - 1) / 2;
		int b = 3 * (gridWidth + 1) / 2;
		int m = (gridHeight - 1) / 2;
		return (2 + m) * a + (1 + m) * b;
	}

	public static int GetNumIndices(int gridWidth, int gridHeight)
	{
		// Indices per hexagon: 18
		return GetNumHexagons(gridWidth, gridHeight) * 6 * 3;
	}
	
	public static int GetNumVertexRows(int gridHeight)
	{
		// Formula: w + 2
		// Pattern:
		// 1 = 3
		// 3 = 5
		// 5 = 7
		return gridHeight + 2;
	}
	
	public static int GetNumVertexColumns(int gridWidth, bool isEven)
	{
		// Formula: w + 2
		// Pattern:
		// 1 = 3
		// 3 = 5
		// 5 = 7
		if (isEven)
		{
			return (gridWidth + 1) + (gridWidth - 1) / 2;
		}
		else
		{
			return 3 * (gridWidth + 1) / 2;
		}
	}

	public static int GetHexagonIndex(int gridWidth, int gridHeight, int r, int c, int point)
	{
		// Note: 7 points starting at the center for index 0, moving to the upper left, and then going clockwise

		// Compute the offset (the index of the first vertex of the row containing the upper left point)
		// Formula: (r + 1) / 2 * a + r / 2 * b
		// Pattern:
		// 0 = 0a, 0b
		// 1 = 1a, 0b
		// 2 = 1a, 1b
		// 3 = 2a, 1b
		// 4 = 2a, 2b
		// 5 = 3a, 2b
		// 6 = 3a, 3b
		// 7 = 4a, 3b
		int a = (gridWidth + 1) + (gridWidth - 1) / 2;
		int b = 3 * (gridWidth + 1) / 2;
		
		int na = (r + 1) / 2;
		int nb = r / 2;
		int offset = na * a + nb * b;

		if (r % 2 == 0)
		{
			switch (point)
			{
			case 0:
				return offset + a + c * 3 + 1;
			case 1:
				return offset + c * 3;
			case 2:
				return offset + c * 3 + 1;
			case 3:
				return offset + a + c * 3 + 2;
			case 4:
				return offset + a + b + c * 3 + 1;
			case 5:
				return offset + a + b + c * 3;
			case 6:
				return offset + a + c * 3;
			default:
				throw new ArgumentOutOfRangeException("index");
			}
		}
		else
		{
			switch (point)
			{
			case 0:
				return offset + b + c * 3 + 2;
			case 1:
				return offset + c * 3 + 2;
			case 2:
				return offset + c * 3 + 3;
			case 3:
				return offset + b + c * 3 + 3;
			case 4:
				return offset + a + b + c * 3 + 3;
			case 5:
				return offset + a + b + c * 3 + 2;
			case 6:
				return offset + b + c * 3 + 1;
			default:
				throw new ArgumentOutOfRangeException("index");
			}
		}
	}

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
