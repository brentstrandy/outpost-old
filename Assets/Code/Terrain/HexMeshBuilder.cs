using UnityEngine;
using System.Collections;
using Settworks.Hexagons;

public class HexMeshBuilder : MeshBuilder
{
	public struct Node
	{
		public Vector3 vertex;
		public Vector2 uv;

		public Node(Vector3 vertex, Vector2 uv)
		{
			this.vertex = vertex;
			this.uv = uv;
		}
		
		public Node(Vector2 vertex)
		{
			this.vertex = vertex;
			this.uv = vertex;
		}
		
		public Node(Vector3 vertex)
		{
			this.vertex = vertex;
			this.uv = vertex;
		}
	}

	public delegate Node NodeDelegate(HexCoord coord, int i);

	protected int[] triangles;
	protected NodeDelegate predicate;

	public HexMeshBuilder() : base()
	{
		triangles = new int[] { 0,1,5, 1,2,5, 2,4,5, 2,3,4 };
		predicate = (coord, i) => new Node(coord.Corner(i));
	}

	public void SetTriangles(int[] triangles)
	{
		this.triangles = triangles;
	}

	public void SetPredicate(NodeDelegate predicate)
	{
		this.predicate = predicate;
	}

	public void AddHexagon(HexCoord coord)
	{
		for (int i = 0; i < triangles.Length; i+=3)
		{
			Node n1 = this.predicate(coord, triangles[i]);
			Node n2 = this.predicate(coord, triangles[i+1]);
			Node n3 = this.predicate(coord, triangles[i+2]);
			AddTriangle(n1.vertex, n1.uv, n2.vertex, n2.uv, n3.vertex, n3.uv);
		}
	}
}
