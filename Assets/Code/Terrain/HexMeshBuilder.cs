using UnityEngine;
using System.Collections.Generic;
using Settworks.Hexagons;

public class HexMeshBuilder : MultiMeshBuilder
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

    public struct MeshIndexSet
    {
        public int MeshIndex;
        public int[] Indices;

        public MeshIndexSet(int meshIndex, int numberOfIndices)
        {
            MeshIndex = meshIndex;
            Indices = new int[numberOfIndices];
        }
    }

    public delegate Node NodeFactory(HexCoord coord, int i);

    public int[] Triangles;
    public NodeFactory Factory;
    public Dictionary<HexCoord, MeshIndexSet> CoordIndices;

    public HexMeshBuilder() : this(false)
    { }

    public HexMeshBuilder(bool flatShaded)
        : base(flatShaded)
    {
        Triangles = new int[] { 0, 1, 5, 1, 2, 5, 2, 4, 5, 2, 3, 4 };
        Factory = ((coord, i) => new Node(coord.Corner(i)));
        CoordIndices = new Dictionary<HexCoord, MeshIndexSet>();
    }

    // Updates the given coordinate if it already exists, otherwise the coordinate is added.
    // Returns true if a change occurred.
    public bool AddOrUpdateHexagon(HexCoord coord)
    {
        MeshIndexSet indexSet;
        if (CoordIndices.TryGetValue(coord, out indexSet))
        {
            return UpdateHexagon(coord, indexSet);
        }
        else
        {
            return AddHexagon(coord);
        }
    }

    // Adds the given coordinate if it doesn't exist already.
    // Returns true if a change occurred.
    public bool AddHexagon(HexCoord coord)
    {
        if (CoordIndices.ContainsKey(coord))
        {
            return false; // Already added
        }

        if (Current.Vertices.Count + Triangles.Length >= MeshBuilder.VertexLimit)
        {
            AddBuilder(); // Keep individual hexagons in the same mesh
        }

        MeshIndexSet indexSet = new MeshIndexSet(MeshBuilders.Count - 1, Triangles.Length);
        for (int i = 0; i < Triangles.Length; i += 3)
        {
            Node n1 = Factory(coord, Triangles[i]);
            Node n2 = Factory(coord, Triangles[i + 1]);
            Node n3 = Factory(coord, Triangles[i + 2]);
            int i1, i2, i3;
            AddTriangle(n1.vertex, n1.uv, n2.vertex, n2.uv, n3.vertex, n3.uv, out i1, out i2, out i3);
            indexSet.Indices[i] = i1;
            indexSet.Indices[i + 1] = i2;
            indexSet.Indices[i + 2] = i3;
        }
        CoordIndices[coord] = indexSet;

        return true;
    }

    // Updates the given coordinate if it already exists.
    // Returns true if a change occurred.
    public bool UpdateHexagon(HexCoord coord, MeshIndexSet indexSet)
    {
        var builder = MeshBuilders[indexSet.MeshIndex];
        bool changed = false;

        for (int i = 0; i < Triangles.Length; i += 3)
        {
            int i1 = indexSet.Indices[i];
            int i2 = indexSet.Indices[i + 1];
            int i3 = indexSet.Indices[i + 2];
            Node n1 = Factory(coord, Triangles[i]);
            Node n2 = Factory(coord, Triangles[i + 1]);
            Node n3 = Factory(coord, Triangles[i + 2]);
            if (
                (builder.Vertices[i1] != n1.vertex) || (builder.Vertices[i2] != n2.vertex) || (builder.Vertices[i3] != n3.vertex) ||
                (builder.UV[i1] != n1.uv) || (builder.UV[i2] != n2.uv) || (builder.UV[i3] != n3.uv)
            )
            {
                changed = true;
            }
            builder.Vertices[i1] = n1.vertex;
            builder.Vertices[i2] = n2.vertex;
            builder.Vertices[i3] = n3.vertex;
            builder.UV[i1] = n1.uv;
            builder.UV[i2] = n2.uv;
            builder.UV[i3] = n3.uv;
        }

        return changed;
    }

    // Removes the given coordinate if it already exists.
    // Returns true if a change occurred.
    public bool RemoveHexagon(HexCoord coord)
    {
        // FIXME: What if the mesh isn't flat shaded?

        MeshIndexSet indexSet;
        if (CoordIndices.TryGetValue(coord, out indexSet))
        {
            var builder = MeshBuilders[indexSet.MeshIndex];

            for (int i = 0; i < Triangles.Length; i += 3)
            {
                int i1 = indexSet.Indices[i];
                int i2 = indexSet.Indices[i + 1];
                int i3 = indexSet.Indices[i + 2];
                if (i1 + 1 == i2 && i2 + 1 == i3)
                {
                    builder.Vertices.RemoveRange(i1, 3);
                    builder.UV.RemoveRange(i1, 3);
                }
                else
                {
                    builder.Vertices.RemoveAt(i1);
                    builder.Vertices.RemoveAt(i2);
                    builder.Vertices.RemoveAt(i3);
                    builder.UV.RemoveAt(i1);
                    builder.UV.RemoveAt(i2);
                    builder.UV.RemoveAt(i3);
                }
            }

            CoordIndices.Remove(coord);

            return true;
        }

        return false;
    }

    public override void Clear()
    {
        base.Clear();
        CoordIndices.Clear();
    }
}