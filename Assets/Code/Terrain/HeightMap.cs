using UnityEngine;
using System;
using System.Collections.Generic;
using Settworks.Hexagons;

[Serializable]
public class HeightMap
{
    protected readonly Vector3 Up = new Vector3(0.0f, 0.0f, -1.0f);

    public Texture2D Texture;
    public HexMeshSurfaceStyle SurfaceStyle;
    public float Height = 5.0f;
    public float AttenuationMultiplier = 0.0f;
    public float AttenuationExponent = 0.0f;
    public HexMeshAttentuationStyle AttenuationStyle;

    public HexSurface Build(IEnumerable<HexCoord> coords, float inset)
    {
        CartesianScaler tex = coords.CartesianScalerUV();

        var surface = new HexSurface();
        foreach (var coord in coords)
        {
            surface[coord] = Calculate(coord, tex, inset);
        }
        return surface;
    }

    private HexPlane Calculate(HexCoord coord, CartesianScaler tex, float inset)
    {
        float outer = 1.0f;
        float inner = outer - inset;

        Vector2 p = coord.Position();

        switch (SurfaceStyle)
        {
            case HexMeshSurfaceStyle.FlatCenter:
                float center = Value(tex(p));
                return new HexPlane(coord, center);

            case HexMeshSurfaceStyle.FlatCentroid:
                float centroid = 0.0f;
                for (int s = 0; s < 6; s++)
                {
                    Vector2 sc = HexCoord.CornerVector(s) * inner + p;
                    centroid += Value(tex(sc));
                }
                centroid *= 0.166666666667f; // Divide by six
                return new HexPlane(coord, centroid);

            case HexMeshSurfaceStyle.Oblique:
                // Loosely fit plane based on a sampling of three corners
                // Note: Corner 0 is at the upper right, others proceed counterclockwise.
                Plane surface = new Plane();
                Vector2 c0 = HexCoord.CornerVector(0) * inner + p;
                Vector2 c2 = HexCoord.CornerVector(2) * inner + p;
                Vector2 c4 = HexCoord.CornerVector(4) * inner + p;
                Vector3 p0 = new Vector3(c0.x, c0.y, Value(tex(c0)));
                Vector3 p2 = new Vector3(c2.x, c2.y, Value(tex(c2)));
                Vector3 p4 = new Vector3(c4.x, c4.y, Value(tex(c4)));
                surface.Set3Points(p4, p2, p0);
                return new HexPlane(coord, surface);
            default:
                return HexPlane.Zero;
        }
    }

    private float Value(Vector2 uv)
    {
        // Fetch the value from the height map
        float result = Texture.GetPixelBilinear(uv.x, uv.y).grayscale;

        // Apply the height scale (inverted because up is in negative Z)
        result *= -Height;

        // Apply the attenuation factor
        result *= 1.0f + Attenuation(uv);

        return result;
    }

    private float Attenuation(Vector2 uv)
    {
        // Translate and scale to the coodinate space between (-1,-1) and (1,1)
        Vector2 offset = new Vector2(0.5f, 0.5f);
        Vector2 scale = new Vector2(2.0f, 2.0f);

        Vector2 pos = Vector2.Scale(uv - offset, scale);

        // Calculate attenuation value
        float attenuationValue = 0.0f;
        switch (AttenuationStyle)
        {
            case HexMeshAttentuationStyle.DistanceFromCenter:
                attenuationValue = pos.magnitude;
                break;

            case HexMeshAttentuationStyle.DistanceFromEdge:
                attenuationValue = Mathf.Max(Mathf.Abs(pos.x), Mathf.Abs(pos.y));
                break;
        }

        // Calculate attenuation factor
        return AttenuationMultiplier * Mathf.Pow(attenuationValue, AttenuationExponent);
    }
}
