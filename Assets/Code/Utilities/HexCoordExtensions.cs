using UnityEngine;
using System;
using System.Collections.Generic;
using Settworks.Hexagons;

public static class HexCoordExtensions
{
    public static Vector2 CartesianBounds(this IEnumerable<HexCoord> coords)
    {
        Vector2 min = Vector2.zero;
        Vector2 max = Vector2.zero;
        foreach (var coord in coords)
        {
            foreach (var corner in coord.Corners())
            {
                min = Vector2.Min(min, corner);
                max = Vector2.Max(max, corner);
            }
        }
        return max - min;
    }

    public static Vector2 CartesianScaleUV(this IEnumerable<HexCoord> coords)
    {
        var bounds = coords.CartesianBounds();
        if (Mathf.Approximately(bounds.x, 0f) || Mathf.Approximately(bounds.y, 0f))
        {
            return Vector2.one;
        }
        return new Vector2(1.0f / bounds.x, 1.0f / bounds.y);
    }

    public static CartesianScaler CartesianScalerUV(this IEnumerable<HexCoord> coords)
    {
        Vector2 scale = coords.CartesianScaleUV();
        Vector2 offset = new Vector2(0.5f, 0.5f);
        return (Vector2 uv) => Vector2.Scale(uv, scale) + offset;
    }
}

