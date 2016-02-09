using UnityEngine;
using System.Collections.Generic;
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

    protected int[] Triangles;
    protected NodeDelegate Predicate;
    protected Dictionary<HexCoord, int[]> CoordIndexMap;

    public HexMeshBuilder()
        : base()
    {
        SetTriangles(new int[] { 0, 1, 5, 1, 2, 5, 2, 4, 5, 2, 3, 4 });
        SetPredicate((coord, i) => new Node(coord.Corner(i)));
        CoordIndexMap = new Dictionary<HexCoord, int[]>();
    }

    public void SetTriangles(int[] triangles)
    {
        this.Triangles = triangles;
    }

    public int[] GetTriangles()
    {
        return Triangles;
    }

    public void SetPredicate(NodeDelegate predicate)
    {
        this.Predicate = predicate;
    }

    public NodeDelegate GetPredicate()
    {
        return Predicate;
    }

    public Dictionary<HexCoord, int[]> GetCoordIndexMap()
    {
        return CoordIndexMap;
    }

    public void AddHexagon(HexCoord coord)
    {
        int[] indices;
        if (!CoordIndexMap.TryGetValue(coord, out indices))
        {
            indices = new int[Triangles.Length];
        }
        for (int i = 0; i < Triangles.Length; i += 3)
        {
            Node n1 = this.Predicate(coord, Triangles[i]);
            Node n2 = this.Predicate(coord, Triangles[i + 1]);
            Node n3 = this.Predicate(coord, Triangles[i + 2]);
            int i1, i2, i3;
            AddTriangle(n1.vertex, n1.uv, n2.vertex, n2.uv, n3.vertex, n3.uv, out i1, out i2, out i3);
            indices[i] = i1;
            indices[i + 1] = i2;
            indices[i + 2] = i3;
        }
        CoordIndexMap[coord] = indices;
    }

    public override void Clear()
    {
        base.Clear();
        CoordIndexMap.Clear();
    }
}