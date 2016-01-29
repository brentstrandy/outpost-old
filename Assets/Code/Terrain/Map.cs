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
    private Dictionary<HexLocation, float> Offsets;

    public float GetOffset(HexLocation location)
    {
        float value = 0.0f;
        Offsets.TryGetValue(location, out value);
        return value;
    }

    public void SetOffset(HexLocation location, float value)
    {
        Offsets[location] = value;
    }
    
    public void Build(int width, int height) {
	}
}