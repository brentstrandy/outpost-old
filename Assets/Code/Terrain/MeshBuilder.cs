using UnityEngine;
using System.Collections.Generic;

public class MeshBuilder
{
    public const int VertexLimit = 65000;

    public int Reserve;
    public bool FlatShaded;

    public int DeduplicationCount { get; protected set; }
    public List<Vector3> Vertices;
    public List<Vector2> UV;
    public List<int> Indices;
    public List<List<int>> SubMeshes;
    public Dictionary<Vector3, int> VertexLookup;
    public Dictionary<Vector3, Vector2> UVLookup;

    public MeshBuilder() : this(0, false)
    { }

    public MeshBuilder(int reserve) : this(reserve, false)
    { }

    public MeshBuilder(int reserve, bool flatShaded)
    {
        Reserve = reserve;
        DeduplicationCount = 0;
        FlatShaded = true;
        Vertices = new List<Vector3>();
        UV = new List<Vector2>();
        SubMeshes = new List<List<int>>();
        VertexLookup = new Dictionary<Vector3, int>();
        UVLookup = new Dictionary<Vector3, Vector2>();
        AddSubMesh();
    }

    public void AddSubMesh()
    {
        Indices = new List<int>();
        SubMeshes.Add(Indices);
    }

    public bool Full
    {
        get { return Vertices.Count + Reserve >= VertexLimit; }
    }

    public void AddTriangle(Vector3 v1, Vector2 uv1, Vector3 v2, Vector2 uv2, Vector3 v3, Vector2 uv3)
    {
        Indices.Add(AddVertex(v1, uv1));
        Indices.Add(AddVertex(v2, uv2));
        Indices.Add(AddVertex(v3, uv3));
    }

    public void AddTriangle(Vector3 v1, Vector2 uv1, Vector3 v2, Vector2 uv2, Vector3 v3, Vector2 uv3, out int i1, out int i2, out int i3)
    {
        i1 = AddVertex(v1, uv1);
        i2 = AddVertex(v2, uv2);
        i3 = AddVertex(v3, uv3);
        Indices.Add(i1);
        Indices.Add(i2);
        Indices.Add(i3);
    }

    public int AddVertex(Vector3 vertex, Vector2 uv)
    {
        int index;
        Vector2 existingUV;
        if (!FlatShaded && VertexLookup.TryGetValue(vertex, out index) && UVLookup.TryGetValue(vertex, out existingUV) && existingUV.Equals(uv))
        {
            DeduplicationCount++;
            return index;
        }
        index = Vertices.Count;
        Vertices.Add(vertex);
        UV.Add(uv);
        if (!FlatShaded)
        {
            VertexLookup.Add(vertex, index);
            UVLookup.Add(vertex, uv);
        }
        return index;
    }

    public Mesh Build()
    {
        var mesh = new Mesh();
        mesh.vertices = Vertices.ToArray();
        mesh.uv = UV.ToArray();
        mesh.subMeshCount = SubMeshes.Count;
        for (int subMesh = 0; subMesh < SubMeshes.Count; subMesh++)
        {
            mesh.SetTriangles(SubMeshes[subMesh].ToArray(), subMesh);
        }
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        TangentSolver.Solve2(mesh);
        return mesh;
    }

    public void Apply(Mesh mesh)
    {
        mesh.vertices = Vertices.ToArray();
        mesh.uv = UV.ToArray();
        mesh.subMeshCount = SubMeshes.Count;
        for (int subMesh = 0; subMesh < SubMeshes.Count; subMesh++)
        {
            mesh.SetTriangles(SubMeshes[subMesh].ToArray(), subMesh);
        }
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        TangentSolver.Solve2(mesh);
    }

    public virtual void Clear()
    {
        DeduplicationCount = 0;
        Vertices.Clear();
        UV.Clear();
        SubMeshes.Clear();
        VertexLookup.Clear();
        UVLookup.Clear();
        AddSubMesh();
    }

    public string Summary()
    {
        int i = 0;
        foreach (var indices in SubMeshes)
        {
            i += indices.Count;
        }
        return "V: " + Vertices.Count + " UV: " + UV.Count + " I: " + i + " DEDUP: " + DeduplicationCount;
    }
}