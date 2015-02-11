using UnityEngine;
using System;
using System.Collections;

[ExecuteInEditMode]
public class HexMesh : MonoBehaviour
{
	// Properties adjustable in the inspector
	public int GridWidth = 5;
	public int GridHeight = 5;
	public float HexagonDiameter = 1.0f;

	// Use this for initialization
	void Start ()
	{
		if (GetComponent<MeshFilter>() == null)
		{
			gameObject.AddComponent("MeshFilter");
		}
		
		if (GetComponent<MeshRenderer>() == null)
		{
			gameObject.AddComponent("MeshRenderer");
		}

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

		BuildMesh(GridWidth, GridHeight, HexagonDiameter);
	}

	private void BuildMesh(int gridWidth, int gridHeight, float hexagonDiameter)
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
		var mesh = GetComponent<MeshFilter>().mesh;
		
		// Calculate some properties of the mesh
		int numberOfHexagons = GetNumHexagons(gridWidth, gridHeight);
		int numberOfVertices = GetNumVertices(gridWidth, gridHeight);
		int numberOfIndices = GetNumIndices(gridWidth, gridHeight);
		float hexagonRadius = hexagonDiameter * 0.5f;

		Debug.Log("Number of Hexagons: " + numberOfHexagons);
		Debug.Log("Number of Vertices: " + numberOfVertices);
		Debug.Log("Number of Indices: " + numberOfIndices);
		
		// Allocate vertices and indices
		var vertices = new Vector3[numberOfVertices];
		var indices = new int[numberOfIndices];
		//var uv = new Vector2[NumberOfVertices];

		// Calculate vertices
		int numVertexRows = GetNumVertexRows(gridHeight);
		int numVertexColumnsA = (gridWidth + 1) + (gridHeight - 1) / 2;
		int numVertexColumnsB = 3 * (gridWidth + 1) / 2;

		int vertIndex = 0;
		for (int r = 0; r < numVertexRows; r++)
		{
			int numVertexColumns = r % 2 == 0 ? numVertexColumnsA : numVertexColumnsB; // The number of columns varies depending on whether the row index is even or odd
			for (int c = 0; c < numVertexColumns; c++)
			{
				float offset = (r % 2 == 0) ? hexagonDiameter * 0.25f : 0f;
				float x = offset + c * hexagonRadius;
				float y = 0.0f;
				float z = r * hexagonRadius;
				vertices[vertIndex] = new Vector3(x, y, z);
				
				Debug.Log("v" + vertIndex + ": " + vertices[vertIndex]);

				vertIndex++;
			}
		}

		// Calculate indices
		int numHexagonColumnsA = (gridWidth + 1) / 2;
		int numHexagonColumnsB = (gridWidth - 1) / 2;

		int hexIndex = 0;
		for (int r = 0; r < gridHeight; r++)
		{
			Debug.Log("Row " + r);
			int numHexagonColumns = r % 2 == 0 ? numHexagonColumnsA : numHexagonColumnsB; // The number of columns varies depending on whether the row index is even or odd
			for (int c = 0; c < numHexagonColumns; c++)
			{
				Debug.Log("Column " + c);
				// Clockwise order:
				// JS 2015-02-20: This should be correct but it comes out backwards and I'm not sure why
				/*
				for (int p = 1; p < 7; p++)
				{
					indices[hexIndex++] = GetHexagonIndex(gridWidth, gridHeight, r, c, 0);
					indices[hexIndex++] = GetHexagonIndex(gridWidth, gridHeight, r, c, p);
					indices[hexIndex++] = GetHexagonIndex(gridWidth, gridHeight, r, c, p >= 6 ? 1 : p + 1);
					
					Debug.Log("i" + (hexIndex - 3) + ": " + indices[hexIndex - 3] + "," + indices[hexIndex - 2] + "," + indices[hexIndex - 1]);
				}
				*/

				// Counter-clockwise order:
				for (int p = 2; p < 8; p++)
				{
					indices[hexIndex++] = GetHexagonIndex(gridWidth, gridHeight, r, c, 0);
					indices[hexIndex++] = GetHexagonIndex(gridWidth, gridHeight, r, c, p >= 7 ? 1 : p);
					indices[hexIndex++] = GetHexagonIndex(gridWidth, gridHeight, r, c, p - 1);
					
					Debug.Log("i" + (hexIndex - 3) + ": " + indices[hexIndex - 3] + "," + indices[hexIndex - 2] + "," + indices[hexIndex - 1]);
				}
			}
		}

		// Assign data to the mesh
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.SetTriangles(indices, 0);

		// Tell the mesh to recalculate its properties
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();
	}

	/*
	public void Apply(HexMap map)
	{
	}
	*/
	
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
}
