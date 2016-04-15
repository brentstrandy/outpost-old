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

    public float Intersect(Vector2 pos)
    {
        return this[HexCoord.AtPosition(pos)].Intersect(pos);
    }

    // Diff returns the difference in elevation between from and to.
    public float Diff(HexCoord from, HexCoord to)
    {
        float a = this[from].distance;
        float b = this[to].distance;
        return b - a;
    }

    // Diff returns the differences in elevation between from and to when moving
    // on a straight line between them and taking the specified number of steps.
    public IEnumerable<float> Diff(HexCoord from, HexCoord to, int steps)
    {
        if (steps < 1)
        {
            throw new ArgumentOutOfRangeException("steps", steps, "cannot be less than 1");
        }

        Vector2 start = from.Position();
        Vector2 end = to.Position();

        float stride = 1.0f / (float)steps;
        float last = Intersect(start);

        for (float t = stride; t <= 1.00001f; t += stride) // Handles rounding errors
        {
            float current = Intersect(Vector2.Lerp(start, end, t));
            yield return current - last;
            last = current;
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

