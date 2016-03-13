using UnityEngine;
using System;
using System.Collections.Generic;
using Settworks.Hexagons;

[Serializable]
public class Map
{
    public Map()
    {
        Coords = new HexSet();
        Surface = new HexSurface();
    }

    public List<HeightMap> HeightMaps = new List<HeightMap>();

    public int SurfaceWidth = 57;
    public int SurfaceHeight = 57;
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

    [HideInInspector]
    public HexSet Coords;

    [HideInInspector]
    public HexSurface Surface;
}