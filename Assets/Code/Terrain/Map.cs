using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Map : ISerializationCallbackReceiver
{
    public Map()
    {
        Coords = new HexSet();
        Surface = new HexSurface();
        Slope = new SlopeCache(Surface, SlopeSampling);
    }

    public const int SlopeSampling = 3;

    public List<HeightMap> HeightMaps = new List<HeightMap>();

    public int SurfaceWidth = 57;
    public int SurfaceHeight = 57;
    public int FacilityRadius = 5;
    public int PeripheralRadius = 12;

    public HexMeshNeighborStyle NeighborStyle;

    [Range(0.0f, 1.0f)]
    public float NeighborStyleInterpolation = 1.0f;

    [Range(0.0f, 1.0f)]
    public float DetailWidth = 0.2f;

    [HideInInspector]
    public HexSet Coords;

    [HideInInspector]
    public HexSurface Surface;

    [HideInInspector]
    [NonSerialized]
    public SlopeCache Slope;

    public void CalculateSlopes()
    {
        Slope.Build(Coords);
        foreach (var coord in Coords)
        {
            Slope.Update(coord);
        }
        Debug.Log("Calculated " + Slope.Count.ToString() + " slopes for " + Coords.Count.ToString() + " coordinates using a cache of " + Slope.SizeInBytes.ToString() + " bytes.");
    }

    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
    {
        CalculateSlopes();
    }
}