using UnityEngine;
using System.Collections.Generic;

public class MultiMeshBuilder
{
    public bool FlatShaded;

    public List<MeshBuilder> MeshBuilders;

    public MeshBuilder Current
    {
        get { return MeshBuilders[MeshBuilders.Count - 1]; }
    }

    public MultiMeshBuilder() : this(false)
    {}

    public MultiMeshBuilder(bool flatShaded)
    {
        FlatShaded = flatShaded;
        MeshBuilders = new List<MeshBuilder>();
        AddBuilder();
    }

    public virtual void AddBuilder()
    {
        MeshBuilders.Add(new MeshBuilder(reserve: 3, flatShaded: FlatShaded));
    }

    public void AddSubMesh()
    {
        MakeReady();
        Current.AddSubMesh();
    }

    public void AddTriangle(Vector3 v1, Vector2 uv1, Vector3 v2, Vector2 uv2, Vector3 v3, Vector2 uv3)
    {
        MakeReady();
        Current.AddTriangle(v1, uv1, v2, uv2, v3, uv3);
    }

    public void AddTriangle(Vector3 v1, Vector2 uv1, Vector3 v2, Vector2 uv2, Vector3 v3, Vector2 uv3, out int i1, out int i2, out int i3)
    {
        MakeReady();
        Current.AddTriangle(v1, uv1, v2, uv2, v3, uv3, out i1, out i2, out i3);
    }

    public int AddVertex(Vector3 vertex, Vector2 uv)
    {
        MakeReady();
        return Current.AddVertex(vertex, uv);
    }

    public List<Mesh> Build()
    {
        var meshes = new List<Mesh>();
        foreach (var builder in MeshBuilders)
        {
            meshes.Add(builder.Build());
        }
        return meshes;
    }

    public virtual void Clear()
    {
        foreach (var builder in MeshBuilders)
        {
            builder.Clear();
        }
        MeshBuilders.Clear();
        AddBuilder();
    }

    public string Summary()
    {
        int vertices = 0;
        int indices = 0;
        int uv = 0;
        int dedup = 0;
        foreach (var builder in MeshBuilders)
        {
            vertices += builder.Vertices.Count;
            foreach (var subMesh in builder.SubMeshes)
            {
                indices += subMesh.Count;
            }
            uv += builder.UV.Count;
            dedup += builder.DeduplicationCount;
        }
        return "V: " + vertices + " UV: " + uv + " I: " + indices + " DEDUP: " + dedup;
    }

    protected virtual void MakeReady()
    {
        if (Current.Full)
        {
            AddBuilder();
        }
    }
}
