using UnityEngine;
using System.Collections.Generic;
using Settworks.Hexagons;

public class HexMesh : MultiMesh
{
    private static IEnumerable<HexCoord> SingleCoordinateSet(HexCoord coord)
    {
        yield return coord;
    }

    public HexMeshBuilder Builder;
    public int Revision { get; private set; }

    public HexMesh(GameObject parent, string name, int layer, HexMeshBuilder builder) : base(parent, name, layer)
    {
        Builder = builder;
    }

    // Performs a Clear and then adds the given coordinate.
    // Returns true if a change occurred.
    public bool Set(HexCoord coord)
    {
        return Set(SingleCoordinateSet(coord));
    }

    // Performs a Clear and then adds the given coordinates.
    // Returns true if a change occurred.
    public bool Set(IEnumerable<HexCoord> coords)
    {
        if ((Builder.CoordIndices.Keys as IEnumerable<HexCoord>).Compare(coords) == 0)
        {
            return false;
        }

        Builder.Clear();
        foreach (var coord in coords)
        {
            Builder.AddHexagon(coord);
        }
        var meshes = Builder.Build();
        PrepareObjects(meshes.Count);
        for (int i = 0; i < meshes.Count; i++)
        {
            var filter = Objects[i].GetComponent<MeshFilter>();
            filter.sharedMesh = null;
            filter.sharedMesh = meshes[i]; 

            var collider = Objects[i].GetComponent<MeshCollider>();
            if (collider != null)
            {
                collider.sharedMesh = null;
                collider.sharedMesh = meshes[i];
            }
        }

        Revision++;

        return true;
    }

    // Updates the given coordinate if it already exists, otherwise the coordinate is added.
    // Returns true if a change occurred.
    public bool AddOrUpdate(HexCoord coord)
    {
        return AddOrUpdate(SingleCoordinateSet(coord));
    }

    // Updates the given coordinates that already exist and adds the coordinates that don't.
    // Returns true if a change occurred.
    public bool AddOrUpdate(IEnumerable<HexCoord> coords)
    {
        if (Builder.CoordIndices.Count == 0)
        {
            return Set(coords);
        }

        var changed = new HashSet<int>();

        foreach (var coord in coords)
        {
            if (Builder.AddOrUpdateHexagon(coord))
            {
                HexMeshBuilder.MeshIndexSet indexSet;
                if (Builder.CoordIndices.TryGetValue(coord, out indexSet))
                {
                    changed.Add(indexSet.MeshIndex);
                }
            }
        }

        if (changed.Count > 0)
        {
            UpdateMesh(changed);
            Revision++;
            return true;
        }

        return false;
    }

    // Adds the given coordinate if it doesn't exist already.
    // Returns true if a change occurred.
    public bool Add(HexCoord coord)
    {
        return Add(SingleCoordinateSet(coord));
    }

    // Adds the given coordinate if it doesn't exist already.
    // Returns true if a change occurred.
    public bool Add(IEnumerable<HexCoord> coords)
    {
        if (Builder.CoordIndices.Count == 0)
        {
            return Set(coords);
        }

        var changed = new HashSet<int>();

        foreach (var coord in coords)
        {
            if (Builder.AddHexagon(coord))
            {
                HexMeshBuilder.MeshIndexSet indexSet;
                if (Builder.CoordIndices.TryGetValue(coord, out indexSet))
                {
                    changed.Add(indexSet.MeshIndex);
                }
            }
        }

        if (changed.Count > 0)
        {
            UpdateMesh(changed);
            Revision++;
            return true;
        }

        return false;
    }

    // Updates all of the coordinate in the mesh.
    // Returns true if a change occurred.
    public bool Update()
    {
        return Update(Builder.CoordIndices.Keys);
    }

    // Updates the given coordinate if it already exists.
    // Returns true if a change occurred.
    public bool Update(HexCoord coord)
    {
        return Update(SingleCoordinateSet(coord));
    }

    // Updates the given coordinate if they already exist.
    // Returns true if a change occurred.
    public bool Update(IEnumerable<HexCoord> coords)
    {
        if (Builder.CoordIndices.Count == 0)
        {
            return Set(coords);
        }

        var changed = new HashSet<int>();

        foreach (var coord in coords)
        {
            HexMeshBuilder.MeshIndexSet indexSet;
            if (Builder.CoordIndices.TryGetValue(coord, out indexSet))
            {
                if (Builder.UpdateHexagon(coord, indexSet))
                {
                    changed.Add(indexSet.MeshIndex);
                }
            }
        }

        if (changed.Count > 0)
        {
            UpdateMesh(changed);
            Revision++;
            return true;
        }

        return false;
    }

    // Removes the given coordinate if it already exists.
    // Returns true if a change occurred.
    public bool Remove(HexCoord coord)
    {
        return Remove(SingleCoordinateSet(coord));
    }

    // Removes the given coordinates if they already exist.
    // Returns true if a change occurred.
    public bool Remove(IEnumerable<HexCoord> coords)
    {
        var changed = new HashSet<int>();

        foreach (var coord in coords)
        {
            HexMeshBuilder.MeshIndexSet indexSet;
            if (Builder.CoordIndices.TryGetValue(coord, out indexSet))
            {
                changed.Add(indexSet.MeshIndex);
                Builder.RemoveHexagon(coord);
            }
        }

        if (changed.Count > 0)
        {
            UpdateMesh(changed);
            Revision++;
            return true;
        }

        return false;
    }

    // Destroys the mesh and releases resources.
    public override void Destroy()
    {
        Builder.Clear();
        base.Destroy();
    }

    // Copies builder data to the existing meshes specified by indices.
    protected void UpdateMesh(IEnumerable<int> indices)
    {
        foreach (int index in indices)
        {
            UpdateMesh(index);
        }
    }

    // Copies builder data to the existing mesh specified by index.
    protected void UpdateMesh(int index)
    {
        PrepareObjects();

        var builder = Builder.MeshBuilders[index];
        var obj = Objects[index];

        var meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter.sharedMesh == null)
        {
            meshFilter.sharedMesh = builder.Build();
        }
        else
        {
            builder.Apply(meshFilter.sharedMesh);
        }

        var collider = obj.GetComponent<MeshCollider>();
        if (collider != null)
        {
            collider.sharedMesh = meshFilter.sharedMesh;
        }
    }

    protected void PrepareObjects()
    {
        PrepareObjects(Builder.MeshBuilders.Count);
    }
}