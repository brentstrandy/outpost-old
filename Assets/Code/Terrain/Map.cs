using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Settworks.Hexagons;

[System.Serializable]
public class Map
{
    public int GridWidth = 5;
    public int GridHeight = 5;
    public int FacilityRadius = 5;
    public int PeripheralRadius = 12;

    public string SurfaceHeightMap;

    public float SurfaceHeightScale = 5.0f;
    public float AttenuationMultiplier = 0.0f;
    public float AttenuationExponent = 0.0f;
    public HexMeshAttentuationStyle AttenuationStyle;
    public HexMeshSurfaceStyle SurfaceStyle;

    [Range(0.0f, 1.0f)]
    public float SurfaceStyleInterpolation = 1.0f;

    public bool SurfaceStyleAttenuation = false;
    public HexMeshNeighborStyle NeighborStyle;

    [Range(0.0f, 1.0f)]
    public float NeighborStyleInterpolation = 1.0f;

    [Range(0.0f, 1.0f)]
    public float DetailWidth = 0.2f;

    public Dictionary<HexCoord, float> Offsets;

    public float GetOffset(HexCoord coord)
    {
        float value = 0.0f;
        if (Offsets != null)
        {
            Offsets.TryGetValue(coord, out value);
        }
        return value;
    }

    public void SetOffset(HexCoord coord, float value)
    {
        if (Offsets == null)
        {
            Offsets = new Dictionary<HexCoord, float>();
        }
        Offsets[coord] = value;
    }

    public void ChangeOffset(HexCoord coord, float change)
    {
        SetOffset(coord, GetOffset(coord) + change);
    }
}