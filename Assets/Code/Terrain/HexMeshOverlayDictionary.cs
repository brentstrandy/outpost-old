using UnityEngine;
using System;
using System.Collections.Generic;
using Settworks.Hexagons;

public class HexMeshOverlayDictionary<TKey> : IEnumerable<HexMeshOverlay> where TKey : struct, IConvertible
{
    private Dictionary<TKey, HexMeshOverlay> Overlays;
    private GameObject Parent;

    public int Count
    {
        get { return Overlays.Count; }
    }

    public HexMeshOverlayDictionary(GameObject parent)
    {
        Parent = parent;
        Overlays = new Dictionary<TKey, HexMeshOverlay>();
    }

    public void Add(TKey key, string name, string shader, HexMeshBuilder builder)
    {
        float offset = -0.01f * (float)(Convert.ToInt32(key) + 1);
        Overlays.Add(key, new HexMeshOverlay(name, offset, Parent, shader, builder));
    }

    public void Add(TKey key, string name, string shader, HexMeshBuilder builder, Color defaultColor)
    {
        float offset = -0.01f * (float)(Convert.ToInt32(key) + 1);
        Overlays.Add(key, new HexMeshOverlay(name, offset, Parent, shader, builder, defaultColor));
    }

    public HexMeshOverlay this[TKey key]
    {
        get
        {
            return Overlays[key];
        }
    }

    public IEnumerator<HexMeshOverlay> GetEnumerator()
    {
        return Overlays.Values.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Update()
    {
        bool changed = false;
        foreach (var overlay in Overlays.Values)
        {
            if (overlay.Update())
            {
                changed = true;
            }
        }
        return changed;
    }

    public bool Update(IEnumerable<HexCoord> coords)
    {
        bool changed = false;
        foreach (var overlay in Overlays.Values)
        {
            if (overlay.Update(coords))
            {
                changed = true;
            }
        }
        return changed;
    }

    public void Clear()
    {
        foreach (var overlay in Overlays.Values)
        {
            overlay.Clear();
        }
        Overlays.Clear();
    }
}