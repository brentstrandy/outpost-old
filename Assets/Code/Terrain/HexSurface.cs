using UnityEngine;
using System;
using System.Collections.Generic;
using Settworks.Hexagons;

[Serializable]
public class HexSurface : OrderedDictionary<HexCoord, HexPlane>
{
    public HexSurface() :
        base((HexPlane p) => p.coord)
    { }

    new public HexPlane this[HexCoord coord]
    {
        get
        {
            HexPlane surface;
            if (!TryGetValue(coord, out surface))
            {
                surface = HexPlane.Zero;
            }
            surface.coord = coord;
            return surface;
        }
        set
        {
            if (Mathf.Approximately(value.distance, 0f) && (value.normal == HexPlane.Zero.normal))
            {
                base.Remove(coord);
            }
            else
            {
                value.coord = coord;
                base[coord] = value;
            }
        }
    }

    public float Distance(HexCoord coord)
    {
        return this[coord].distance;
    }

    public IEnumerable<float> Distance(IEnumerable<HexCoord> coords)
    {
        foreach (var coord in coords)
        {
            yield return this[coord].distance;
        }
    }

    public Vector3 Normal(HexCoord coord)
    {
        return this[coord].normal;
    }

    public IEnumerable<Vector3> Normal(IEnumerable<HexCoord> coords)
    {
        foreach (var coord in coords)
        {
            yield return this[coord].normal;
        }
    }

    public void SetDistance(HexCoord coord, float value)
    {
        HexPlane surface;
        if (!TryGetValue(coord, out surface))
        {
            surface = HexPlane.Zero;
        }
        surface.coord = coord;
        surface.distance = value;
        this[coord] = surface;
    }

    public void ChangeDistance(HexCoord coord, float change)
    {
        HexPlane surface;
        if (!TryGetValue(coord, out surface))
        {
            surface = HexPlane.Zero;
        }
        surface.coord = coord;
        surface.distance += change;
        this[coord] = surface;
    }

    public void ChangeDistance(IEnumerable<HexCoord> coords, float change)
    {
        foreach (var coord in coords)
        {
            ChangeDistance(coord, change);
        }
    }
}

